using UnityEngine;

/// <summary>
/// 親のスケール影響を受けずに、特定の位置・回転に張り付くスクリプト
/// </summary>
public class ArrowAnchor : MonoBehaviour
{
    private Transform targetTransform;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private bool isAnchored = false;

    public void StickTo(Transform target)
    {
        targetTransform = target;

        // 親のローカル座標系における自分の位置・回転を計算して保存
        // これにより「親が動いても相対的な位置」を維持できる
        initialLocalPosition = target.InverseTransformPoint(transform.position);
        initialLocalRotation = Quaternion.Inverse(target.rotation) * transform.rotation;

        isAnchored = true;
    }

    private void LateUpdate()
    {
        if (!isAnchored) return;

        if (targetTransform == null)
        {
            // くっついていた相手が消滅したら、矢も消す（あるいは物理演算を復活させて落とす）
            Destroy(gameObject);
            return;
        }

        // 毎フレーム、ターゲットの現在位置・回転をもとに自分の位置を更新
        // SetParentしていないので、Scaleの影響は受けない！
        transform.position = targetTransform.TransformPoint(initialLocalPosition);
        transform.rotation = targetTransform.rotation * initialLocalRotation;
    }
}