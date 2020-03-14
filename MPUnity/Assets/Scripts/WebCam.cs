using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCam : MonoBehaviour
{
    public Camera cam;
    public GameObject objScreen;

    private WebCamTexture _webCamTexture = null;
    private ScreenOrientation _screenOrientation = ScreenOrientation.Portrait;
    private CameraClearFlags _cameraClearFlags;

    private void Awake()
    {
        foreach (Camera c in Camera.allCameras)
        {
            if (c != cam)
            {
                c.cullingMask = ~(1 << objScreen.layer);
            }
        }

        cam.gameObject.SetActive(false);
        objScreen.SetActive(false);
        cam.farClipPlane = cam.nearClipPlane + 1f;
        objScreen.transform.localPosition = new Vector3(0f, 0f, cam.farClipPlane * .5f);
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length > 0)
        {
            _webCamTexture = new WebCamTexture(Screen.width, Screen.height);
            objScreen.GetComponent<Renderer>().material.mainTexture = _webCamTexture;
        }

        _screenOrientation = Screen.orientation;
        setOrientation(_screenOrientation);
        StartCoroutine(coroutineOrientation());

        show(true);
    }

    private void setOrientation(ScreenOrientation sc)
    {
        float h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * .5f) * objScreen.transform.localPosition.z * .2f;

        if (cam.orthographic)
        {
            h = Screen.height / cam.pixelHeight;
        }

        if (ScreenOrientation.Landscape == sc)
        {
            objScreen.transform.localRotation = Quaternion.Euler(180f, 180f, 0f);
            objScreen.transform.localScale = new Vector3(cam.aspect * h, 1f, h) / 2;
        }
        else if (ScreenOrientation.LandscapeLeft == sc)
        {
            objScreen.transform.localRotation = Quaternion.Euler(180f, 180f, 0f);
            objScreen.transform.localScale = new Vector3(cam.aspect * h, 1f, h) / 2;
        }
        else if (ScreenOrientation.LandscapeRight == sc)
        {
            objScreen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            objScreen.transform.localScale = new Vector3(cam.aspect * h, 1f, h) / 2;
        }
        else if (ScreenOrientation.Portrait == sc)
        {
            objScreen.transform.localRotation = Quaternion.Euler(90f, -90f, 90f);
            objScreen.transform.localScale = new Vector3(h, 1f, cam.aspect * h) / 2;
        }
        else if (ScreenOrientation.PortraitUpsideDown == sc)
        {
            objScreen.transform.localRotation = Quaternion.Euler(90f, 90f, -90f);
            objScreen.transform.localScale = new Vector3(h, 1f, cam.aspect * h) / 2;
        }
    }

    private IEnumerator coroutineOrientation()
    {
        while (true)
        {
            if (_screenOrientation != Screen.orientation)
            {
                _screenOrientation = Screen.orientation;
                setOrientation(_screenOrientation);
            }
            yield return new WaitForSeconds(.5f);
        }
    }

    public void show(bool flag)
    {
        if (null == _webCamTexture)
        {
            return;
        }

        if (flag)
        {
            if (Camera.main != cam)
            {
                _cameraClearFlags = Camera.main.clearFlags;
                Camera.main.clearFlags = CameraClearFlags.Depth;
            }

            cam.gameObject.SetActive(true);
            objScreen.SetActive(true);
            _webCamTexture.Play();
        }
        else
        {
            if (Camera.main != cam)
            {
                Camera.main.clearFlags = _cameraClearFlags;
            }

            _webCamTexture.Pause();
            objScreen.SetActive(false);
            cam.gameObject.SetActive(false);
        }
    }
}