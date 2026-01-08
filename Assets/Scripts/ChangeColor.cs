using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    Renderer _renderer;
    private Material originalMaterial;
    public Material transparent;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        originalMaterial = _renderer.material;
    }

    // Hover Entered に設定する用（入ったら透明にするだけ）
    public void ChangeTransparent()
    {
        _renderer.material = transparent;
        // デバッグログは確認用。動いたら消してもOK
        // UnityEngine.Debug.Log("ChangedTransparent!"); 
    }

    // Hover Exited に設定する用（出たら戻すだけ）
    public void ChangeOriginalColor()
    {
        _renderer.material = originalMaterial;
    }

    // ★重要：このメソッドはもう使いません！
    // Hoverイベントは「入る」「出る」がハッキリしているので、
    // ここで「どっちかな？」と迷う必要がないからです。
    /*
    bool isOriginal = true;
    public void ChangeColorsForSelect()
    {
       // この中身のロジックが邪魔をしていました
    }
    */
}