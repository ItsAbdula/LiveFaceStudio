using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class CaptureManager : MonoBehaviour
{
    public GetSocialSdk.Capture.Scripts.GetSocialCapture capture;
    public Text buttonText;
    public Button recordBtn;

    float recordTime;
    bool isRecord;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        isRecord = false;
    }

    private void Update()
    {
        if (isRecord)
        {
            recordTime += Time.deltaTime;
            if(recordTime>=5.0f)
            {
                isRecord = false;
                FinishRecord();
            }
        }
    }

    private void StartRecord()
    {
        Debug.Log("StartRecord");
        capture.StartCapture();
        buttonText.text = "Recording...";
    }

    private void FinishRecord()
    {
        Debug.Log("FinishRecord");
        capture.StopCapture();
        // generate gif
        Action<byte[]> result = bytes =>
        {
            successSave(capture.GetFilePath());
        };
        capture.GenerateCapture(result);

        buttonText.text = "Saving...";
        recordBtn.enabled = false;
    }

    public void onClickRecordBtn()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) == false)
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);

            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) == false)
            {
                return;
            }
        }

        if (isRecord)
        {
            isRecord = false;
            FinishRecord();
        }
        else
        {
            isRecord = true;
            StartRecord();
        }
    }

    public void successSave(string result)
    {
        buttonText.text = "Record";
        recordBtn.enabled = true;

        AndroidJavaClass classPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass classUri = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject objIntent = new AndroidJavaObject("android.content.Intent", new object[2]
        {
            "android.intent.action.MEDIA_SCANNER_SCAN_FILE",
            classUri.CallStatic<AndroidJavaObject>("parse","file://"+result)
        });
        objActivity.Call("sendBroadcast", objIntent);
        Application.OpenURL(result);
    }
}
