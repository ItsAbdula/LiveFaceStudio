using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.UI;

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
    static public int devicesCount = -1;
    static public Color32[] image = new Color32[Screen.width * Screen.height];

    public GameObject objScreen;
    public Text logText;

    private ScreenOrientation _screenOrientation = ScreenOrientation.Portrait;
    private CameraClearFlags _cameraClearFlags;

    private void Awake()
    {
#if UNITY_ANDROID
        if(Permission.HasUserAuthorizedPermission(Permission.Camera) == false)
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif

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
        if (devices.Length == devicesCount) return;

        devicesCount = devices.Length;

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
#if UNITY_ANDROID
            canUseDevices = true;

            string frontCamName = "";

            foreach (var camDevice in devices)
            {
                if (camDevice.isFrontFacing)
                {
                    frontCamName = camDevice.name;
                    break;
                }
            }
            _webCamTexture = new WebCamTexture(frontCamName);

            objScreen.GetComponent<Renderer>().material.mainTexture = _webCamTexture;

            objScreen.transform.localRotation = Quaternion.Euler(0f, 90f, -90f);
#endif
            logText.text = "camera count" + devices.Length;
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