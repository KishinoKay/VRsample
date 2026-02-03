using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class VRRestartManager : MonoBehaviour
{
    [Header("入力設定")]
    [SerializeField] private InputActionProperty restartAction;

    [Header("設定")]
    [SerializeField] private float holdDuration = 3.0f;

    // 内部変数
    private float _timer = 0f;
    private bool _isPressed = false;
    private bool _isRestarting = false; // ★追加: リスタート処理中かどうかのフラグ
    
    // 初期位置を記録する変数
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    [SerializeField] private Transform playerRoot;

    private void Awake()
    {
        if (playerRoot == null)
        {
            playerRoot = transform.root;
        }

        _initialPosition = playerRoot.position;
        _initialRotation = playerRoot.rotation;
    }

    private void OnEnable()
    {
        var action = restartAction.action;
        if (action != null)
        {
            action.started += OnRestartStarted;
            action.canceled += OnRestartCanceled;
            action.Enable();
        }
    }

    private void OnDisable()
    {
        var action = restartAction.action;
        if (action != null)
        {
            action.started -= OnRestartStarted;
            action.canceled -= OnRestartCanceled;
            action.Disable();
        }
    }

    private void Update()
    {
        // ★追加: リスタート中なら何もしない
        if (_isRestarting) return;

        if (_isPressed)
        {
            _timer += Time.deltaTime;
            if (_timer >= holdDuration)
            {
                _isPressed = false;
                RestartScene();
            }
        }
    }

    private void OnRestartStarted(InputAction.CallbackContext ctx)
    {
        if (_isRestarting) return;
        _isPressed = true;
    }

    private void OnRestartCanceled(InputAction.CallbackContext ctx)
    {
        _isPressed = false;
        _timer = 0f;
    }

    private void RestartScene()
    {
        if (_isRestarting) return;
        _isRestarting = true; // ★フラグを立てる

        Debug.Log("Restarting Scene...");

        // ★重要: Inputによる干渉を防ぐため、イベント購読を解除して無効化しておく
        OnDisable();

        // 補足: 通常のシーンロードであれば、ロード時にオブジェクトが破棄→再生成されるため
        // ForceResetPosition() は不要です。
        // もしプレイヤーが DontDestroyOnLoad (シーンを跨いで破壊されない)設定なら必要です。
        // 今回は念のため残しますが、通常のシーンリロードなら削除しても動作は同じです。
        ForceResetPosition();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ForceResetPosition()
    {
        // オブジェクトがすでに破棄されかけている場合は実行しない
        if (playerRoot == null) return;

        var characterController = playerRoot.GetComponent<CharacterController>();
        if (characterController != null) characterController.enabled = false;

        playerRoot.position = _initialPosition;
        playerRoot.rotation = _initialRotation;

        if (characterController != null) characterController.enabled = true;
        
        Debug.Log($"Player reset to initial position: {_initialPosition}");
    }
}