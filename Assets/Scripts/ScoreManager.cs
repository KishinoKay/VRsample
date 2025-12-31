using UnityEngine;
using TMPro; // 文字表示に必要

public class ScoreManager : MonoBehaviour
{
    // これを書くと、他のスクリプトから 'ScoreManager.instance' でいつでも呼べるようになる（シングルトン）
    public static ScoreManager instance;
    
    public TextMeshProUGUI scoreUIText; // ここにさっき作った文字をセットする
    private int currentScore = 0;

    void Awake()
    {
        instance = this; // 「私が管理者です」と宣言
    }

    // 矢や的から呼ばれる関数
    public void AddScore(int points)
    {
        currentScore += points;
        // 画面の文字を更新
        scoreUIText.text = "Score: " + currentScore.ToString();
    }
    // ★追加: スコアをリセットする関数
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreText();
        Debug.Log("Score Reset!");
    }

    // 表示更新処理をまとめた関数
    void UpdateScoreText()
    {
        if (scoreUIText != null)
        {
            scoreUIText.text = "Score: " + currentScore.ToString();
        }
    }
}