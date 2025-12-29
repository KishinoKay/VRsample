using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    Renderer renderer;
    private Material originalMaterial;
    public Material transparent;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.material;
    }

    public void ChangeTransparent()
    {
        renderer.material = transparent;
        UnityEngine.Debug.Log("ChangedTransparent!");
    }

    public void ChangeOriginalColor()
    {
        renderer.material = originalMaterial;
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
