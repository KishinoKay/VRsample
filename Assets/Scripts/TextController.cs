using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextController : MonoBehaviour
{
    public GameObject timerText;
    float time = 0.0f;

    // Update is called once per frame
    void Update()
    {
        this.time += Time.deltaTime;
        this.timerText.GetComponent<TextMeshProUGUI>().text = this.time.ToString("F1");
    }
}
