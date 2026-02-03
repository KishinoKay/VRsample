using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class VRRestartManager : MonoBehaviour
{
    [Header("入力設定")]
    [SerializeField] private InputActionProperty restartAction;
    [SerializeField] private float holdDuration = 3.0f;

    // 内部変数
    private float _timer = 0f;
    private bool _isPressed = false;
    private bool _isRestarting = false;

    private void OnEnable()
    {
        if (restartAction.action != null)
        {
            restartAction.action.started += OnRestartStarted;
            restartAction.action.canceled += OnRestartCanceled;
            restartAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (restartAction.action != null)
        {
            restartAction.action.started -= OnRestartStarted;
            restartAction.action.canceled -= OnRestartCanceled;
            restartAction.action.Disable();
        }
    }

    private void Update()
    {
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

    private void OnRestartStarted(InputAction.CallbackContext ctx) => _isPressed = true;
    
    private void OnRestartCanceled(InputAction.CallbackContext ctx)
    {
        _isPressed = false;
        _timer = 0f;
    }

    private void RestartScene()
    {
        if (_isRestarting) return;
        _isRestarting = true;
        
        // シンプルにシーンをロードするだけ！
        // 位置合わせは、ロード後に動き出す「VRAutoRecenter」に任せます。
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}