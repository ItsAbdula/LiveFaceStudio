using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CallNativeCode : MonoBehaviour
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private const string dllName = "OpenCV_Windows";
#elif UNITY_ANDROID
    private const string dllName = "OpenCV_Android";
#else
    private const string dllName = "_";
#endif

    public Button btnFlipImage;
    public Button btnDetectImage;
    public Image originalImage;

    public RawImage detectedImage;

    [DllImport(dllName)]
    private static extern void FlipImage(ref Color32[] rawImage, int width, int height);

    [DllImport(dllName)]
    private static extern void DetectFace(ref Color32[] rawImage, int width, int height);

    [DllImport(dllName)]
    private static extern float Foopluginmethod();

    private void Start()
    {
        // btnFlipImage.onClick.AddListener(CallFlipImage);
        // btnDetectImage.onClick.AddListener(CallDetectFace);
    }

    private float timer = 0;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > 0.05f)
        {
            timer = 0;

            if (WebCam.canUseDevices == false) return;

            detectedImage.texture = GetDetectedTexture();
        }
    }

    private Texture2D GetDetectedTexture()
    {
        DetectFace(ref WebCam.image, WebCam.Width, WebCam.Height);

        Texture2D newTexture = new Texture2D(WebCam.Width, WebCam.Height, TextureFormat.RGB24, false, false);
        newTexture.SetPixels32(WebCam.image, 0);
        newTexture.Apply();
        detectedImage.texture = newTexture;

        return newTexture;
    }

    private void OnGUI()
    {
        // This Line should display "Foopluginmethod: 10"
        GUI.Label(new Rect(15, 125, 450, 100), "Foopluginmethod: " + Foopluginmethod());
    }
}