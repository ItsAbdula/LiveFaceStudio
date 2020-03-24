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
        Color32[] temp = new Color32[inputTex.width];
        for (int i = 0; i < inputTex.height / 2; i++)
        {
            System.Array.ConstrainedCopy(inputData, i * inputTex.width, temp, 0, inputTex.width);
            System.Array.ConstrainedCopy(inputData, (inputTex.height - i - 1) * inputTex.width, inputData, i * inputTex.width, inputTex.width);
            System.Array.ConstrainedCopy(temp, 0, inputData, (inputTex.height - i - 1) * inputTex.width, inputTex.width);
        }

        NativeCodes.DetectFace(ref inputData, inputTex.width, inputTex.height);
        for (int i = 0; i < inputTex.height / 2; i++)
        {
            System.Array.ConstrainedCopy(inputData, i * inputTex.width, temp, 0, inputTex.width);
            System.Array.ConstrainedCopy(inputData, (inputTex.height - i - 1) * inputTex.width, inputData, i * inputTex.width, inputTex.width);
            System.Array.ConstrainedCopy(temp, 0, inputData, (inputTex.height - i - 1) * inputTex.width, inputTex.width);
        }

        resultTex.SetPixels32(inputData);
        resultTex.Apply();
    }

    private void OnGUI()
    {
        // This Line should display "Foopluginmethod: 10"
        GUI.Label(new Rect(15, 125, 450, 100), "Foopluginmethod: " + NativeCodes.Foopluginmethod());
    }
}