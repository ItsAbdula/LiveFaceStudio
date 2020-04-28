using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Util;
using UnityEngine;

public class FaceLandmark : MonoBehaviour
{
    public GameObject Nose;
    public GameObject Eye1;
    public GameObject Eye2;
    public GameObject Lip1;
    public GameObject Lip2;
    public GameObject Face;

    OpenCvSharp.Rect faceRect;
    UnityEngine.Rect objectRect;

    float aspect;

    void Start()
    {
        objectRect = this.GetComponent<RectTransform>().rect;
    }

    public void setLandmark(IEnumerable<IEnumerable<Point>> pts, int type)
    {
        GameObject obj = null;
        switch(type)
        {
            case 1:
                obj = Nose;
                break;
            case 2:
                obj = Eye1;
                break;
            case 3:
                obj = Eye2;
                break;
            case 4:
                obj = Lip1;
                break;
            case 5:
                obj = Lip2;
                break;
        }
        if(obj != null)
        {
            List<Point[]> ptsList = new List<Point[]>();
            List<int> nptsList = new List<int>();
            foreach (IEnumerable<Point> pts1 in pts)
            {
                Point[] pts1Arr = EnumerableEx.ToArray(pts1);
                ptsList.Add(pts1Arr);
                nptsList.Add(pts1Arr.Length);
            }
            int maxX=0, maxY=0, minX=0, minY=0;
            for(int i=0;i<ptsList.Count; ++i)
            {
                Point[] ptsArr = ptsList[i];
                maxX = minX = ptsArr[0].X;
                maxY = minY = ptsArr[0].Y;
                for(int j=1; j<ptsArr.Length; ++j)
                {
                    maxX = ptsArr[j].X > maxX ? ptsArr[j].X : maxX;
                    minX = ptsArr[j].X < minX ? ptsArr[j].X : minX;
                    maxY = ptsArr[j].Y > maxY ? ptsArr[j].Y : maxY;
                    minY = ptsArr[j].Y < minY ? ptsArr[j].Y : minY;
                }
            }
            OpenCvSharp.Rect rect = new OpenCvSharp.Rect(minX, minY, maxX - minX, maxY - minY);

            obj.SetActive(true);
            RectTransform transform = obj.GetComponent<RectTransform>();
            transform.anchoredPosition = new Vector2(rect.X * aspect, rect.Y * aspect * -1);
            transform.sizeDelta = new Vector2(rect.Width * aspect, rect.Height * aspect);
        }
    }

    public void setFaceRect(OpenCvSharp.Rect rect)
    {
        faceRect = rect;
        Face.SetActive(true);
        RectTransform transform = Face.GetComponent<RectTransform>();
        transform.anchoredPosition = new Vector2(rect.X * aspect, rect.Y * aspect * -1);
        transform.sizeDelta = new Vector2(rect.Width * aspect, rect.Height * aspect);
    }

    public void setSize(int width, int height)
    {
        float aspectX, aspectY;
        aspectX = objectRect.width / (float)width;
        aspectY = objectRect.height / (float)height;
        aspect = aspectX>aspectY ? aspectY : aspectX;
    }
}
