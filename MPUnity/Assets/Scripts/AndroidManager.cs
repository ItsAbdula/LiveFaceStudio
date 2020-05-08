using UnityEngine;
using UnityEngine.UI;

public struct MIME_TYPE
{
    public static string TEXT { get { return "text/plain"; } }

    public static string IMAGE { get { return "image/*"; } }
    public static string JPG { get { return "image/jpg"; } }
    public static string PNG { get { return "image/png"; } }
    public static string GIF { get { return "image/gif"; } }

    public static string VIDEO { get { return "video/*"; } }
    public static string MP4 { get { return "video/mp4"; } }
}

public class AnroidWrapper
{
    private const string UNITY_PLAYER_CLASS = "com.unity3d.player.UnityPlayer";
    private const string UNITY_ACTIVITY_FIELD = "currentActivity";
    private const string WRAPPER_CLASS = "com.example.androidplugins.ShareBridge";

    private AndroidJavaClass ajc;
    private AndroidJavaObject ajo;
    private AndroidJavaObject wrapper;

    public AnroidWrapper()
    {
        ajc = new AndroidJavaClass(UNITY_PLAYER_CLASS);
        if (ajc == null)
        {
            Debug.LogError(string.Format("Can't Make AndroidJavaClass : {0}", UNITY_PLAYER_CLASS));
        }

        ajo = ajc.GetStatic<AndroidJavaObject>(UNITY_ACTIVITY_FIELD);
        if (ajo == null)
        {
            Debug.LogError(string.Format("Can't Make AndroidJavaObject : {0}", UNITY_ACTIVITY_FIELD));
        }

        wrapper = new AndroidJavaObject(WRAPPER_CLASS, ajo);
        if (wrapper == null)
        {
            Debug.LogError(string.Format("Can't Make AndroidJavaObject : {0}", WRAPPER_CLASS));
        }
    }

    public void SendToAndroid(string function)
    {
        if (wrapper == null) return;

        wrapper.Call(function);
    }

    public void SendToAndroid(string function, string message)
    {
        if (wrapper == null) return;

        wrapper.Call(function, message);
    }
}

public class AndroidManager : MonoBehaviour
{
    private AnroidWrapper androidWrapper;
    private string LatestLog;

    public Text logText;

    private void Awake()
    {
        androidWrapper = new AnroidWrapper();
    }

    public void RequestMakeToast(string message)
    {
        androidWrapper.SendToAndroid("makeToast", message);
    }

    public void RequestShareText(string message)
    {
        androidWrapper.SendToAndroid("shareText", MIME_TYPE.TEXT);
    }

    //TODO: public void RequestShareImage(string mimeType); TODO

    public void RequsetSayHelloToUnity()
    {
        androidWrapper.SendToAndroid("sayHelloToUnity");
    }

    public void ReceiveFromAndroid(string message)
    {
        LatestLog = message;

        UpdateLog();
    }

    private void UpdateLog()
    {
        var log = string.Format("{0}[{1}] : {2}", System.Environment.NewLine, System.DateTime.Now.ToShortTimeString(), LatestLog);

        logText.text += log;
        Debug.Log(log);
    }
}