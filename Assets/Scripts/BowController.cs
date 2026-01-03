using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BowController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Animator bowAnimator;
    [SerializeField] private Transform nockPoint;
    [SerializeField] private Transform restPointForLeftHandHolder; 
    [SerializeField] private Transform restPointForRightHandHolder;
    [SerializeField] private Collider[] bowColliders;

    // ▼▼▼ 追加箇所 1: オーディオ設定 ▼▼▼
    [Header("オーディオ")]
    [SerializeField] private AudioSource bowAudioSource; // AudioSourceコンポーネント
    [SerializeField] private AudioClip drawSoundClip;    // ギリギリ音（ループ用）
    [SerializeField, Range(0f, 2f)] private float maxPitch = 1.5f; // 最大まで引いた時のピッチ
    // ▲▲▲▲▲▲

    [Header("パラメータ")]
    [SerializeField] private float maxPullDistance = 0.5f;
    [SerializeField] private float shotPower = 20f;
    [SerializeField] private string drawAnimStateName = "DrawBow";
    [SerializeField] private Vector3 arrowRotationOffset = new Vector3(90, 0, 0);
    [Header("アローレスト設定")]

    private float distanceoffset = 0.4f;

    private Transform _holdingHand;
    private Transform _pullingHand;
    private Transform _activeRestPoint;
    private ArrowController _currentArrow;
    private int _animStateHash;

    private bool _isPulled = false;

    private void Start()
    {
        _animStateHash = Animator.StringToHash(drawAnimStateName);

        // ▼▼▼ 追加箇所 2: AudioSourceの初期設定 ▼▼▼
        if (bowAudioSource != null && drawSoundClip != null)
        {
            bowAudioSource.clip = drawSoundClip;
            bowAudioSource.loop = true; // ループ再生にする
            bowAudioSource.playOnAwake = false;
            bowAudioSource.Stop();
        }
        // ▲▲▲▲▲▲
    }

    private void Update()
    {
        if (_pullingHand != null && _holdingHand != null)
        {
            UpdatePullingProcess();
        }
        else if (_isPulled)
        {
            ResetBowString();
        }
    }

    private void UpdatePullingProcess()
    {
        _isPulled = true;

        Vector3 pullVector = _pullingHand.position - _activeRestPoint.position;
        Vector3 bowForward = ( _activeRestPoint.position - nockPoint.position ).normalized;
        float pullValue = Vector3.Dot(pullVector, -bowForward);

        if(pullValue < 0) pullValue = 0;

        float pullProgress = Mathf.Clamp01((pullValue - distanceoffset) / maxPullDistance);

        bowAnimator.Play(_animStateHash, 0, pullProgress);
        bowAnimator.speed = 0; 

        if (_currentArrow != null)
        {
            UpdateArrowTransform(pullProgress);
        }

        // ▼▼▼ 追加箇所 3: 音の制御 ▼▼▼
        if (bowAudioSource != null && drawSoundClip != null)
        {
            // 少しでも引いていれば音を鳴らす
            if (pullProgress > 0.01f)
            {
                if (!bowAudioSource.isPlaying)
                {
                    bowAudioSource.Play();
                }

                // 引き具合に応じて音量とピッチを変える（演出）
                bowAudioSource.volume = pullProgress; // 深く引くほど音が大きくなる
                bowAudioSource.pitch = Mathf.Lerp(1.0f, maxPitch, pullProgress); // 深く引くと音が高くなる
            }
            else
            {
                // 引いていない（戻した）状態なら止める
                if (bowAudioSource.isPlaying)
                {
                    bowAudioSource.Stop();
                }
            }
        }
        // ▲▲▲▲▲▲
    }

    private void UpdateArrowTransform(float pullProgress)
    {
        _currentArrow.transform.position = nockPoint.position;
        Vector3 targetPos = (_activeRestPoint != null) ? _activeRestPoint.position : transform.position;
        Vector3 direction = targetPos - nockPoint.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            _currentArrow.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowRotationOffset);
        }
    }

    private void ResetBowString()
    {
        _isPulled = false;
        bowAnimator.Play(_animStateHash, 0, 0f);

        // ▼▼▼ 追加箇所 4: 強制停止 ▼▼▼
        if (bowAudioSource != null)
        {
            bowAudioSource.Stop();
        }
        // ▲▲▲▲▲▲
    }

    // （以下、イベント系メソッドは変更なしのため省略）
    public void OnBowGrabbed(SelectEnterEventArgs args)
    {
        _holdingHand = args.interactorObject.transform;
        if (_holdingHand.CompareTag("LeftHand")) _activeRestPoint = restPointForLeftHandHolder;
        else if (_holdingHand.CompareTag("RightHand")) _activeRestPoint = restPointForRightHandHolder;
        else _activeRestPoint = restPointForLeftHandHolder;
    }
    public void OnBowReleased(SelectExitEventArgs args) => _holdingHand = null;

    public void OnStringPulled(SelectEnterEventArgs args)
    {
        _pullingHand = args.interactorObject.transform;
        var handManager = args.interactorObject.transform.GetComponentInParent<ArrowHandManager>();
        if (handManager != null && handManager.TryGetArrow(out GameObject arrowObj))
        {
            if (arrowObj.TryGetComponent<ArrowController>(out var arrowCtrl)) NockArrow(arrowCtrl);
        }
    }

    public void OnStringReleased(SelectExitEventArgs args)
    {
        FireArrow();
        _pullingHand = null;
        ResetBowString();
    }
    
    // （NockArrow, FireArrow, IgnoreCollisionsForArrow なども変更なし）
    private void NockArrow(ArrowController arrow)
    {
        if (_currentArrow != null) return;
        _currentArrow = arrow;
        _currentArrow.transform.SetParent(nockPoint);
        _currentArrow.transform.localPosition = Vector3.zero;
        _currentArrow.transform.localRotation = Quaternion.identity;
        _currentArrow.OnNock();
    }

    private void FireArrow()
    {
        AudioManager.Instance.PlaySE("shot");
        if (_currentArrow == null) return;
        _currentArrow.transform.SetParent(null);
        Vector3 targetPos = (_activeRestPoint != null) ? _activeRestPoint.position : transform.position;
        Vector3 aimDirection = (targetPos - nockPoint.position).normalized;
        float currentDist = Vector3.Distance(targetPos, nockPoint.position);
        float powerMultiplier = Mathf.Clamp01((currentDist - distanceoffset) / maxPullDistance);
        Vector3 launchVelocity = aimDirection * (shotPower * powerMultiplier);
        IgnoreCollisionsForArrow(_currentArrow);
        _currentArrow.Launch(launchVelocity);
        
        _currentArrow = null;
    }

    private void IgnoreCollisionsForArrow(ArrowController arrow)
    {
        Collider[] arrowCols = arrow.GetComponentsInChildren<Collider>();
        foreach (var arrowCol in arrowCols)
        {
            foreach (var bowCol in bowColliders) Physics.IgnoreCollision(arrowCol, bowCol, true);
            if (_holdingHand != null && _holdingHand.TryGetComponent<Collider>(out var handCol))
                Physics.IgnoreCollision(arrowCol, handCol, true);
        }
    }
}