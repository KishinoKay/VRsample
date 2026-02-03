using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class VRForceDirection : MonoBehaviour
{
    [Header("どこに立つ？")]
    [SerializeField] private Transform targetPoint;

    [Header("どっちを見る？")]
    [SerializeField] private Transform lookAtTarget;

    [Header("強制補正（ここで微調整してください）")]
    [SerializeField, Range(-180f, 180f)] 
    private float manualAdjustment = 0f; // ★これをいじれば絶対に向きが変わります

    private void Start()
    {
        StartCoroutine(RecenterSequence());
    }

    // 値を変えたらリアルタイムで反映させる（デバッグ用）
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyRotation();
        }
    }

    private IEnumerator RecenterSequence()
    {
        // 起動時のHMD認識ズレを防ぐため、しつこく待つ
        yield return new WaitForSeconds(0.5f);
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        var xrOrigin = GetComponentInChildren<XROrigin>();
        if (xrOrigin == null) return;

        // 1. 基本となる「向きたい方向」を決定
        Vector3 desiredForward = Vector3.forward;

        if (lookAtTarget != null)
        {
            // 特定のターゲットを見る
            desiredForward = (lookAtTarget.position - targetPoint.position).normalized;
        }
        else if (targetPoint != null)
        {
            // ターゲット地点の矢印方向を見る
            desiredForward = targetPoint.forward;
        }

        desiredForward.y = 0; // 水平化

        // 2. 目標の方角（0～360度）
        float goalAngle = Quaternion.LookRotation(desiredForward).eulerAngles.y;

        // 3. 現在のカメラの首のひねり（ローカル）
        float currentHeadAngle = xrOrigin.Camera.transform.localEulerAngles.y;

        // 4. 計算： (目標 - 首) + 手動補正値
        float finalAngle = goalAngle - currentHeadAngle + manualAdjustment;

        // 5. 適用
        // Rigの傾き(X,Z)もここで強制リセット
        xrOrigin.transform.rotation = Quaternion.Euler(0f, finalAngle, 0f);

        // 6. 位置合わせ
        if (targetPoint != null)
        {
            Vector3 offset = xrOrigin.transform.position - xrOrigin.Camera.transform.position;
            offset.y = 0f;
            Vector3 finalPos = targetPoint.position + offset;
            finalPos.y = targetPoint.position.y;
            xrOrigin.transform.position = finalPos;
        }
        
        Physics.SyncTransforms();
    }
}