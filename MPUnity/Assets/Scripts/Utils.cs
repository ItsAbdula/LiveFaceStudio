using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Utils
{
    // https://www.reddit.com/r/csharp/comments/94dtsv/c_intptr_to_c_uchar/
    public static byte[] ColorToByte(Color32[] pixels)
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

    public static Color32[] ByteToColor(byte[] pixels)
    {
        Color32[] temp = new Color32[pixels.Length / 4];
        for (int i = 0; i < pixels.Length / 4; i++)
        {
            temp[i] = new Color32(pixels[i * 4], pixels[i * 4 + 1], pixels[i * 4 + 2], pixels[i * 4 + 3]);
        }

        return temp;
    }

    public static Color32[] reorderColor32(Color32[] inputData, int width, int height)
    {
        Color32[] temp = new Color32[width];
        for (int i = 0; i < height / 2; i++)
        {
            Array.ConstrainedCopy(inputData, i * width, temp, 0, width);
            Array.ConstrainedCopy(inputData, (height - i - 1) * width, inputData, i * width, width);
            Array.ConstrainedCopy(temp, 0, inputData, (height - i - 1) * width, width);
        }

        return inputData;
    }
}