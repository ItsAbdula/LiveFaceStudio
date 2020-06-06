using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            successSave();
        };
        capture.GenerateCapture(result);

        buttonText.text = "Saving...";
        recordBtn.enabled = false;
    }

    public void onClickRecordBtn()
    {
        if(isRecord)
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

    public void successSave()
    {
        buttonText.text = "Record";
        recordBtn.enabled = true;
    }
}
