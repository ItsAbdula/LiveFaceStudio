using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeLogger : MonoBehaviour
{
    public static NativeLogger nativeLogger;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void LogFunction(string msg);

    private static readonly LogFunction logFunction = PrintLog;
    private static readonly IntPtr functionPointer = Marshal.GetFunctionPointerForDelegate(logFunction);

    public static void SetUpLogger()
    {
        LogFunction logFunct = PrintLog;

        IntPtr ptr = Marshal.GetFunctionPointerForDelegate(logFunct);
        NativeCodes.LinkLogger(ptr);
    }

    [MonoPInvokeCallback(typeof(LogFunction))]
    private static void PrintLog(string msg)
    {
        Debug.Log(msg);
    }
}