using UnityEngine;
using UnityEngine.UI;

public class Test02 : MonoBehaviour
{
    public RawImage detectedImage;
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
        var bytes = Utils.ColorToByte(WebCam.image);

        var cascadeXml = Resources.Load<TextAsset>("data/haarcascades/haarcascade_frontalface_alt");
        var nestedCascadeXml = Resources.Load<TextAsset>("data/haarcascades/haarcascade_eye_tree_eyeglasses");

        if (cascadeXml == null)
        {
            Debug.Log("Can't load cascade");
        }
        if (nestedCascadeXml == null)
        {
            Debug.Log("Can't load nestedcascade");
        }

        NativeCodes.DetectFace(cascadeXml.text, nestedCascadeXml.text, ref bytes[0], WebCam.Width, WebCam.Height);

        Texture2D newTexture = new Texture2D(WebCam.Width, WebCam.Height, TextureFormat.RGB24, false, false);
        newTexture.SetPixels32(WebCam.image, 0);
        newTexture.Apply();
        detectedImage.texture = newTexture;

        return newTexture;
    }
}