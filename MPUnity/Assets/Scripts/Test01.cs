using System;
using System.Runtime.InteropServices;
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
        var inputByte = ColorToByte(inputData);

        NativeCodes.FlipImage(inputByte, inputTex.width, inputTex.height);
        var resultPixels = ByteToColor(inputByte);
        inputTex.SetPixels32(resultPixels);
        inputTex.Apply();
    }

    private void CallDetectFace()
    {
        var inputData = inputTex.GetPixels32();
        inputData = reorderColor32(inputData);

        var inputByte = ColorToByte(inputData);

        NativeCodes.DetectFace(inputByte, inputTex.width, inputTex.height);

        var resultColors = ByteToColor(inputByte);
        resultColors = reorderColor32(resultColors);

        resultTex.SetPixels32(resultColors);
        resultTex.Apply();
    }

    private Color32[] reorderColor32(Color32[] inputData)
    {
        Color32[] temp = new Color32[inputTex.width];
        for (int i = 0; i < inputTex.height / 2; i++)
        {
            Array.ConstrainedCopy(inputData, i * inputTex.width, temp, 0, inputTex.width);
            Array.ConstrainedCopy(inputData, (inputTex.height - i - 1) * inputTex.width, inputData, i * inputTex.width, inputTex.width);
            Array.ConstrainedCopy(temp, 0, inputData, (inputTex.height - i - 1) * inputTex.width, inputTex.width);
        }

        return inputData;
    }

    // https://www.reddit.com/r/csharp/comments/94dtsv/c_intptr_to_c_uchar/
    private byte[] ColorToByte(Color32[] pixels)
    {
        if (pixels == null || pixels.Length == 0) return null;

        int colorByteLength = Marshal.SizeOf(typeof(Color32));
        int length = colorByteLength * pixels.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        return bytes;
    }

    private Color32[] ByteToColor(byte[] pixels)
    {
        Color32[] temp = new Color32[pixels.Length / 4];
        for (int i = 0; i < pixels.Length / 4; i++)
        {
            temp[i] = new Color32(pixels[i * 4], pixels[i * 4 + 1], pixels[i * 4 + 2], pixels[i * 4 + 3]);
        }

        return temp;
    }

    private void OnGUI()
    {
        // This Line should display "Foopluginmethod: 10"
        GUI.Label(new Rect(15, 125, 450, 100), "Foopluginmethod: " + NativeCodes.Foopluginmethod());
    }
}