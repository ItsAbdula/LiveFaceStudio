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

        NativeLogger.SetUpLogger();
    }

    private void CallFlipImage()
    {
        var inputData = inputTex.GetPixels32();
        var inputByte = Utils.ColorToByte(inputData);

        NativeCodes.FlipImage(ref inputByte[0], inputTex.width, inputTex.height);
        var resultPixels = Utils.ByteToColor(inputByte);
        inputTex.SetPixels32(resultPixels);
        inputTex.Apply();
    }

    private void CallDetectFace()
    {
        var inputData = inputTex.GetPixels32();
        inputData = Utils.reorderColor32(inputData, inputTex.width, inputTex.height);

        var inputByte = Utils.ColorToByte(inputData);

        var cascadeXml = Resources.Load<TextAsset>("data/haarcascades/haarcascade_frontalface_alt");
        var nestedCascadeXml = Resources.Load<TextAsset>("data/haarcascades/haarcascade_eye_tree_eyeglasses");

        if (cascadeXml == null)
        {
            Debug.Log("Can't load cascade");

            return;
        }
        if (nestedCascadeXml == null)
        {
            Debug.Log("Can't load nestedcascade");

            return;
        }

        NativeCodes.DetectFace(cascadeXml.text, nestedCascadeXml.text, ref inputByte[0], inputTex.width, inputTex.height);

        var resultColors = Utils.ByteToColor(inputByte);
        resultColors = Utils.reorderColor32(resultColors, inputTex.width, inputTex.height);

        resultTex.SetPixels32(resultColors);
        resultTex.Apply();
    }

    private void OnGUI()
    {
        // This Line should display "Foopluginmethod: 10"
        GUI.Label(new Rect(15, 125, 450, 100), "Foopluginmethod: " + NativeCodes.Foopluginmethod());
    }
}