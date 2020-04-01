using UnityEngine;
using UnityEngine.UI;

public class Test01 : MonoBehaviour
{
    public Button btnFlip;
    public Button btnDetect;
    public Image inputImg;
    public Image resultImg;
    public Transform faceSphere;
    public Transform canvas;

    private Texture2D inputTex;
    private Texture2D resultTex;

    private int maxFaceDetectCount = 5;
    private CvCircle[] faces;

    private void Start()
    {
        btnFlip.onClick.AddListener(CallFlipImage);
        btnDetect.onClick.AddListener(CallDetectFace);

        inputTex = inputImg.sprite.texture;
        resultTex = resultImg.sprite.texture;

        NativeLogger.SetUpLogger();
        faces = new CvCircle[maxFaceDetectCount];

        faceSphere.position = inputImg.transform.position;
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

        NativeCodes.DetectFace(ref faces[0], cascadeXml.text, nestedCascadeXml.text, ref inputByte[0], inputTex.width, inputTex.height);

        var scaleFactor = new Vector2(resultImg.transform.localScale.x * canvas.localScale.x, resultImg.transform.localScale.x * canvas.transform.localScale.y);
        var leftTop = new Vector3(resultImg.transform.position.x + resultImg.rectTransform.rect.xMin * scaleFactor.x, resultImg.transform.position.y + resultImg.rectTransform.rect.yMax * scaleFactor.y, 0.0f);
        faceSphere.position = leftTop;
        if (faces[0].Radius != 0)
        {
            faceSphere.Translate(((float)faces[0].X / resultTex.width) * resultImg.rectTransform.rect.width * scaleFactor.x, -((float)faces[0].Y / resultTex.height) * resultImg.rectTransform.rect.height * scaleFactor.y, 0.0f);
            faceSphere.localScale = new Vector3(((float)faces[0].Radius / resultTex.width) * resultImg.rectTransform.rect.width * scaleFactor.x * 2, ((float)faces[0].Radius / resultTex.height) * resultImg.rectTransform.rect.height * scaleFactor.y * 2, 1.0f);
        }

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