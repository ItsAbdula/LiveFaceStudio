using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class CallNativeCode : MonoBehaviour
{
#if UNITY_EDITOR_WIN
    private const string dllName = "OpenCV_Windows";
#elif UNITY_ANDROID
    private const string dllName = "OpenCV_Android";
#else
    private const string dllName = "_";
#endif

    [DllImport(dllName)]
    private static extern float Foopluginmethod();

    private void OnGUI()
    {
        // This Line should display "Foopluginmethod: 10"
        GUI.Label(new Rect(15, 125, 450, 100), "Foopluginmethod: " + Foopluginmethod());
    }
}