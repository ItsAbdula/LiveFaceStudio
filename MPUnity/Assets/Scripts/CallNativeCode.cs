using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class CallNativeCode : MonoBehaviour
{
#if !UNITY_EDITOR_WIN

    [DllImport("OpenCV_Android")]
    private static extern float Foopluginmethod();

#endif

    private void OnGUI()
    {
        // This Line should display "Foopluginmethod: 10"
#if !UNITY_EDITOR_WIN
        GUI.Label(new Rect(15, 125, 450, 100), "Foopluginmethod: " + Foopluginmethod());
#endif
    }
}