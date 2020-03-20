using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCam : MonoBehaviour
{
    public GameObject objScreen;

    private WebCamTexture _webCamTexture = null;
    private ScreenOrientation _screenOrientation = ScreenOrientation.Portrait;
    private CameraClearFlags _cameraClearFlags;

    private void Awake()
    {
        objScreen.SetActive(false);
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length > 0)
        {
            _webCamTexture = new WebCamTexture(Screen.width, Screen.height);
            objScreen.GetComponent<Renderer>().material.mainTexture = _webCamTexture;
        }

        _screenOrientation = Screen.orientation;
        StartCoroutine(coroutineOrientation());

        show(true);
    }

    private IEnumerator coroutineOrientation()
    {
        while (true)
        {
            if (_screenOrientation != Screen.orientation)
            {
                _screenOrientation = Screen.orientation;
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
            objScreen.SetActive(true);
            _webCamTexture.Play();
        }
        else
        {
            _webCamTexture.Pause();
            objScreen.SetActive(false);
        }
    }
}