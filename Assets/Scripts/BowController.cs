using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BowController : MonoBehaviour
{
    [Header("アニメーション設定")]
    public Animator bowAnimator;
    public string stateName = "DrawBow";
    
    [Header("物理・パラメータ設定")]
    public float maxPullDistance = 0.5f;
    public float shotPower = 20f;
    public Transform nockPoint;

    [Header("ハンドリング参照")]
    public Transform holdingHand;
    
    // 内部変数
    private Transform pullingHand; 
    private GameObject currentArrow; 
    private bool isNocked = false;
    private int stateHash;

    void Start()
    {
        stateHash = Animator.StringToHash(stateName);
    }

    void Update()
    {
        // 【修正点1】 && isNocked を削除しました。
        // これにより、矢を持っていなくても弦を引くアニメーションが動くようになります。
        if (holdingHand != null && pullingHand != null)
        {
            UpdateBowPull();
        }
        else
        {
            // 引っ張っていない時は弦を戻す
            bowAnimator.Play(stateHash, 0, 0f);
        }
    }

    private void UpdateBowPull()
    {
        float dist = Vector3.Distance(holdingHand.position, pullingHand.position);
        float pullValue = Mathf.Clamp01(dist / maxPullDistance);
        bowAnimator.Play(stateHash, 0, pullValue);
        bowAnimator.speed = 0; 

        // 矢がある場合のみ、矢の位置を弦に追従させる
        if(currentArrow != null)
        {
            currentArrow.transform.position = nockPoint.position;
            currentArrow.transform.rotation = nockPoint.rotation;
        }
    }

    // ---------------------------------------------------------
    // XR Events
    // ---------------------------------------------------------

    public void OnBowGrabbed(SelectEnterEventArgs args)
    {
        holdingHand = args.interactorObject.transform;
    }

    public void OnBowReleased(SelectExitEventArgs args)
    {
        holdingHand = null;
    }

    public void OnStringPulled(SelectEnterEventArgs args)
    {
        pullingHand = args.interactorObject.transform;

        // 手に矢を持っていればセットする処理（ここはそのまま）
        ArrowHandManager handManager = args.interactorObject.transform.GetComponentInParent<ArrowHandManager>();

        if (handManager != null && handManager.HasArrow)
        {
            GameObject arrowFromHand = handManager.GiveArrow();
            NockArrow(arrowFromHand);
        }
    }

    public void OnStringReleased(SelectExitEventArgs args)
    {
        Fire();
        pullingHand = null;
        
        // 離した瞬間にアニメーションをリセット（バネのように戻る表現）
        bowAnimator.Play(stateHash, 0, 0f);
    }

    // ---------------------------------------------------------
    // 矢のロジック
    // ---------------------------------------------------------

    public void NockArrow(GameObject arrow)
    {
        if (isNocked) return;

        currentArrow = arrow;
        isNocked = true;

        Rigidbody rb = currentArrow.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        Collider col = currentArrow.GetComponent<Collider>();
        if (col) col.enabled = false;

        currentArrow.transform.SetParent(nockPoint);
        currentArrow.transform.localPosition = Vector3.zero;
        currentArrow.transform.localRotation = Quaternion.identity;
    }

// (前略...Fireメソッドのみ抜粋して修正)

    private void Fire()
    {
        // 矢がない、またはセットされていないなら終了（空撃ち防止）
        if (!isNocked || currentArrow == null) return;

        currentArrow.transform.SetParent(null);

        Rigidbody rb = currentArrow.GetComponent<Rigidbody>();
        
        // ★ここで ArrowController を取得しておく
        ArrowController arrowCtrl = currentArrow.GetComponent<ArrowController>();

        if (rb)
        {
            rb.isKinematic = false;
            Collider col = currentArrow.GetComponent<Collider>();
            if (col) col.enabled = true;

            // 威力の計算
            float currentDist = 0f;
            if (holdingHand != null)
            {
                currentDist = Vector3.Distance(holdingHand.position, transform.position);
            }
            float powerMultiplier = Mathf.Clamp01(currentDist / maxPullDistance);
            
            // 物理的な力を加える
            rb.AddForce(nockPoint.forward * (shotPower * powerMultiplier), ForceMode.Impulse);

            // 【追加】矢に対して「発射されたよ！」と伝える
            // これにより、落下や手放しでは刺さらず、この瞬間だけ「刺さるモード」になる
            if (arrowCtrl != null)
            {
                arrowCtrl.Launch();
            }
        }

        currentArrow = null;
        isNocked = false;
    }
}