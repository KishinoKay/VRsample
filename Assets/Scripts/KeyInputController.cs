using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyInputController : MonoBehaviour
{
    public GameObject bulletPrefab;

    private Camera mainCamera;
    private Camera secondCamera;
    private bool isMainCameraActive = true;

    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        secondCamera = GameObject.FindGameObjectWithTag("SecondCamera").GetComponent<Camera>();
        mainCamera.enabled = true;
        secondCamera.enabled = false;
    }

    public void SwitchCamera()
    {
        Debug.Log("Switching Cameras");
        isMainCameraActive = !isMainCameraActive;
        mainCamera.enabled = isMainCameraActive;
        secondCamera.enabled = !isMainCameraActive;
    }

    public void OnClick()
    {
        UnityEngine.Debug.Log("enter OnClick \n");
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = this.transform.position;
        bullet.transform.rotation = this.transform.rotation;
        bullet.transform.Rotate(new Vector3(90, 0, 0));
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * 1000.0f);
    }
}
