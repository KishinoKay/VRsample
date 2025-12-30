using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    Renderer _renderer;
    private Material originalMaterial;
    public Material transparent;
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        originalMaterial = _renderer.material;
    }

    public void ChangeTransparent()
    {
        _renderer.material = transparent;
        UnityEngine.Debug.Log("ChangedTransparent!");
    }

    public void ChangeOriginalColor()
    {
        _renderer.material = originalMaterial;
        UnityEngine.Debug.Log("ChangedOriginalColor!");
    }
    bool isOriginal = true;
    public void ChangeColorsForSelect()
    {
        if (isOriginal)
        {
            ChangeTransparent();
            isOriginal = false;
        }
        else
        {
            ChangeOriginalColor();
            isOriginal = true;
        }
    }
}
