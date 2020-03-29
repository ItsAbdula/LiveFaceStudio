using UnityEngine;
using System.Runtime.InteropServices;

public class NativeCodes : MonoBehaviour
{
    public static NativeCodes nativeCodes;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private const string dllName = "OpenCV_Windows";
#elif UNITY_ANDROID
    private const string dllName = "OpenCV_Android";
#else
    private const string dllName = "_";
#endif

    [DllImport(dllName)]
    public static extern void FlipImage(ref byte rawImage, int width, int height);

    [DllImport(dllName)]
    public static extern void DetectFace(ref byte rawImage, int width, int height);

    [DllImport(dllName)]
    public static extern float Foopluginmethod();
}