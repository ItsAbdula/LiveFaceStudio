using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCam : MonoBehaviour
{
    static public int Width
    {
        get
        {
            return _webCamTexture.width;
        }
    }

    static public int Height
    {
        get
        {
            return _webCamTexture.height;
        }
    }

    static private WebCamTexture _webCamTexture = null;
    static public bool canUseDevices = false;
    static public Color32[] image = new Color32[Screen.width * Screen.height];

    public GameObject objScreen;

    private ScreenOrientation _screenOrientation = ScreenOrientation.Portrait;
    private CameraClearFlags _cameraClearFlags;

    private void Awake()
    {
        objScreen.SetActive(false);

        updateDevices();

        _screenOrientation = Screen.orientation;
        StartCoroutine(coroutineOrientation());

        if (canUseDevices == false) return;
        show(true);
    }

    private void Update()
    {
        updateDevices();
        if (canUseDevices == false) return;

        if (_webCamTexture.isPlaying)
        {
            image = _webCamTexture.GetPixels32();
        }
    }

    private void updateDevices()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            canUseDevices = false;
        }
        else if (devices.Length == 1)
        {
            canUseDevices = true;

            _webCamTexture = new WebCamTexture(Screen.width, Screen.height);

            objScreen.GetComponent<Renderer>().material.mainTexture = _webCamTexture;
        }
        else // 여러개라면, frontcam을 알아내서 적용
        {
            canUseDevices = true;
        }
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
        if (_webCamTexture == null) return;

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