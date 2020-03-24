using UnityEngine;
using UnityEngine.UI;

public class Test01 : MonoBehaviour
{
    public Button btnFlip;
    public Button btnDetect;
    public Image inputImg;
    public Image resultImg;
    private Texture2D inputTex;
    private Texture2D resultTex;

    private void Start()
    {
        btnFlip.onClick.AddListener(CallFlipImage);
        btnDetect.onClick.AddListener(CallDetectFace);

        inputTex = inputImg.sprite.texture;
        resultTex = resultImg.sprite.texture;
    }

    private void CallFlipImage()
    {
        var inputData = inputTex.GetPixels32();
        NativeCodes.FlipImage(ref inputData, inputTex.width, inputTex.height);
        inputTex.SetPixels32(inputData);
        inputTex.Apply();
    }

    private void CallDetectFace()
    {
        var inputData = inputTex.GetPixels32();

        NativeCodes.DetectFace(ref inputData, inputTex.width, inputTex.height);
        System.Array.Reverse(inputData);

        resultTex.Apply();
    }

    private void OnGUI()
    {
        // This Line should display "Foopluginmethod: 10"
        GUI.Label(new Rect(15, 125, 450, 100), "Foopluginmethod: " + NativeCodes.Foopluginmethod());
    }
}