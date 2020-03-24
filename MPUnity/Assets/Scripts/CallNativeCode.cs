using UnityEngine;
using System.Runtime.InteropServices;

public class CallNativeCode : MonoBehaviour
{
    public static CallNativeCode callNativeCode;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private const string dllName = "OpenCV_Windows";
#elif UNITY_ANDROID
    private const string dllName = "OpenCV_Android";
#else
    private const string dllName = "_";
#endif

    [DllImport(dllName)]
    public static extern void FlipImage(ref Color32[] rawImage, int width, int height);

    [DllImport(dllName)]
    public static extern void DetectFace(ref Color32[] rawImage, int width, int height);

    [DllImport(dllName)]
    public static extern float Foopluginmethod();
}