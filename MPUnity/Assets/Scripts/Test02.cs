using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Test02 : MonoBehaviour
{
    public RawImage detectedImage;
    private float timer = 0;

    private void Start()
    {
    }

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
        NativeCodes.DetectFace(ColorToByte(WebCam.image), WebCam.Width, WebCam.Height);

        Texture2D newTexture = new Texture2D(WebCam.Width, WebCam.Height, TextureFormat.RGB24, false, false);
        newTexture.SetPixels32(WebCam.image, 0);
        newTexture.Apply();
        detectedImage.texture = newTexture;

        return newTexture;
    }

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
}