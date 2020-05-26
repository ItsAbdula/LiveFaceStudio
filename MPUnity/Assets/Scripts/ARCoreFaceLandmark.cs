using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct ARFaceLandmark
{
    enum direction
    {
        UP = 0,
        RIGHT = 1,
        DOWN = 2,
        LEFT = 3,
        MAX = 4,
    };
    List<Vector3> pointList;
    int[] pointIndex;
    Rect landmarkRect;

    public ARFaceLandmark(int up, int right, int down, int left)
    {
        pointList = new List<Vector3>();
        pointIndex = new int[4] { up, right, down, left};
        landmarkRect = new Rect();
    }

    public List<Vector3> getPointList() { return pointList; }
    public Rect getRect() { return landmarkRect; }

    public void setPoint(List<Vector3> verticeList)
    {
        pointList.Clear();
        for (int i = 0; i < (int)direction.MAX; ++i)
        {
            Vector3 point = verticeList[pointIndex[i]];
            pointList.Add(verticeList[pointIndex[i]]);
        }
        float x = pointList[(int)direction.LEFT].x;
        float y = pointList[(int)direction.UP].y;
        float width = pointList[(int)direction.RIGHT].x - x;
        float height = y - pointList[(int)direction.DOWN].y;
        if (pointList[(int)direction.UP].y < pointList[(int)direction.DOWN].y) height = 0;
        landmarkRect.Set(x, y, width, height);
    }
};

public enum FaceLandmarkPosition
{
    FACE = 0,
    RIGHT_EYE = 1,
    LEFT_EYE = 2,
    MOUSE = 3,
    MAX = 4,
};

public class ARCoreFaceLandmark : MonoBehaviour
{


    public Text logText;
    public GameObject testObj;
    ARFaceLandmark[] arFaceLandmark;

    Quaternion faceRotation;

    // Start is called before the first frame update
    void Start()
    {
        //RIGHT_EYE 159(up), 133(right), 145(down), 33(left)
        //LEFT_EYE 386(up), 263(right), 374(down), 362(left)
        //MOUSE 12(up), 292(right), 15(down), 62(left)
        //10(face up), 366(face right), 152(face down), 123(face left)
        arFaceLandmark = new ARFaceLandmark[]
        {
            new ARFaceLandmark(10, 366, 152, 123), // FACE
            new ARFaceLandmark(159, 133, 145, 33), // RIGHT_EYE
            new ARFaceLandmark(386, 263, 374, 362), // LEFT_EYE
            new ARFaceLandmark(12, 292, 15, 62) // MOUSE
        };
    }

    public void setFaceLandmark(List<Vector3> verticeList)
    {
        for (int i = 0; i < (int)FaceLandmarkPosition.MAX; ++i)
        {
            arFaceLandmark[i].setPoint(verticeList);
            Rect rect = arFaceLandmark[i].getRect();
            rect.x *= 100;
            rect.y *= 100;
            rect.height *= 100;
            rect.width *= 100;
            if (logText != null)
            {
                string str = "";
                switch (i)
                {
                    case 0:
                        str = "얼굴크기 : " + rect.height + "," + rect.width;
                        break;
                    case 1:
                        str = "오른쪽눈 : " + rect.height + "," + rect.width;
                        break;
                    case 2:
                        str = "왼쪽눈 : " + rect.height + "," + rect.width;
                        break;
                    case 3:
                        str = "입크기 : " + rect.height + "," + rect.width;
                        break;
                }
                logText.text = logText.text + str + "\n";
            }
        }
    }

    public List<Vector3> getPointListByPosition(FaceLandmarkPosition position)
    {
        return arFaceLandmark[(int)position].getPointList();
    }

    public Rect getRectByPosition(FaceLandmarkPosition position)
    {
        return arFaceLandmark[(int)position].getRect();
    }

    public void setFaceRotation(Quaternion rotation)
    {
        faceRotation = rotation;
        if(logText != null) logText.text = "rotation : " + faceRotation + "\nEuler : " + faceRotation.eulerAngles + "\n" ;
        testObj.transform.rotation = rotation;
        Debug.Log("Rotation : " + rotation.eulerAngles);
    }
    public Quaternion getFaceRotation() { return faceRotation; }
}
