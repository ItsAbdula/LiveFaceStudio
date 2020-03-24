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
    public Image imageBlack;
    public Image detectImage;

    [DllImport(dllName)]
    private static extern void FlipImage(ref Color32[] rawImage, int width, int height);

    [DllImport(dllName)]
    private static extern void DetectFace(ref Color32[] rawImage, int width, int height);

    [DllImport(dllName)]
    private static extern float Foopluginmethod();

    // Start is called before the first frame update
    private void Start()
    {
        btnFlipImage.onClick.AddListener(CallFlipImage);
        btnDetectImage.onClick.AddListener(CallDetectFace);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void CallFlipImage()
    {
        var blackImage = imageBlack.sprite.texture.GetPixels32();
        FlipImage(ref blackImage, 504, 670);
        imageBlack.sprite.texture.SetPixels32(blackImage);
        imageBlack.sprite.texture.Apply();
    }

    private void CallDetectFace()
    {
        var blackImage = imageBlack.sprite.texture.GetPixels32();
        DetectFace(ref blackImage, 504, 670);
        detectImage.sprite.texture.SetPixels32(blackImage);
        detectImage.sprite.texture.Apply();
    }

    private void OnGUI()
    {
        // This Line should display "Foopluginmethod: 10"
        GUI.Label(new Rect(15, 125, 450, 100), "Foopluginmethod: " + Foopluginmethod());
    }
}