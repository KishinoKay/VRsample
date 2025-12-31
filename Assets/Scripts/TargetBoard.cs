using UnityEngine;
using UnityEngine.Events; // 点数表示イベント用

public class TargetBoard : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("手順1で作ったCenterPointをここにドラッグ&ドロップ")]
    public Transform centerPoint;

    [Header("点数の判定基準（単位：メートル）")]
    public float radius10Points = 0.25f; // 10cm以内
    public float radius9Points = 0.4f;  // 20cm以内
    public float radius8Points = 0.6f;  // 30cm以内

    public float radius7Points = 0.8f; // 10cm以内
    public float radius6Points = 1.0f;  // 20cm以内
    public float radius5Points = 1.2f;  // 30cm以内
    public float radius4Points = 1.4f;  // 50cm以内
    public float radius3Points = 1.6f;  // 20cm以内
    public float radius2Points = 1.8f;  // 30cm以内
    public float radius1Points = 2.0f;  // 50cm以内
    // デバッグ用：シーンビューで判定範囲を可視化する
    private void OnDrawGizmos()
    {
        if (centerPoint == null) return;
        
        // 赤い線で10点圏内を表示
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPoint.position, radius10Points);
        
        // オレンジ色の線で9点圏内を表示
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(centerPoint.position, radius9Points);
        // 黄色い線で8点圏内を表示
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPoint.position, radius8Points);
        // 緑色の線で7点圏内を表示
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPoint.position, radius7Points);
        // 青色の線で6点圏内を表示
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(centerPoint.position, radius6Points);
        // 紫色の線で5点圏内を表示
        Gizmos.color = new Color(0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(centerPoint.position, radius5Points);
        // ピンク色の線で4点圏内を表示
        Gizmos.color = new Color(1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(centerPoint.position, radius4Points);
        // 水色の線で3点圏内を表示
        Gizmos.color = new Color(0f, 1f, 1f);
        Gizmos.DrawWireSphere(centerPoint.position, radius3Points);
        // 灰色の線で2点圏内を表示
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(centerPoint.position, radius2Points);
        // 白色の線で1点圏内を表示
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(centerPoint.position, radius1Points);
    }

// 矢から呼ばれる関数
    public int CalculateScore(Vector3 hitPosition)
    {
        // 修正点: Scaleの影響を受けないよう、ワールド座標系で計算します

        // 1. ヒット地点から中心点へのベクトル（ワールド座標）
        Vector3 vectorToHit = hitPosition - centerPoint.position;

        // 2. 「的の表面」上の距離だけを取り出す
        // centerPoint.forward（青い軸）を法線（面の向き）として、ベクトルを面に投影します。
        // これにより、矢が深く刺さりすぎたり手前だったりする「奥行きのズレ」を無視できます。
        Vector3 onPlaneVector = Vector3.ProjectOnPlane(vectorToHit, centerPoint.forward);

        // 3. そのベクトルの長さ（距離）を測る
        float distance = onPlaneVector.magnitude;

        // ログで距離を確認（これが実際のメートル単位の距離になります）
        Debug.Log($"中心からの補正済み距離: {distance:F2} m");

        // --- ここから下は判定ロジック（変更なし） ---
        if (distance <= radius10Points) return 10;
        if (distance <= radius9Points) return 9;
        if (distance <= radius8Points) return 8;   
        if (distance <= radius7Points) return 7;
        if (distance <= radius6Points) return 6;
        if (distance <= radius5Points) return 5;
        if (distance <= radius4Points) return 4;
        if (distance <= radius3Points) return 3;
        if (distance <= radius2Points) return 2;
        if (distance <= radius1Points) return 1;
        
        return 0; 
    }
}