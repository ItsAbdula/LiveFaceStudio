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
            point = point * 100f;
            pointList.Add(point);
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
    ARFaceLandmark[] arFaceLandmark;

    Quaternion faceRotation;

    GoogleARCore.AugmentedFace m_AugmentedFace = null;
    private List<GoogleARCore.AugmentedFace> m_AugmentedFaceList = null;

    float leftEyeHeight=1;
    float rightEyeHeight = 1;
    float prevLeftEye;
    float prevRightEye;
    bool eyeOpen;
    Coroutine playingCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        //RIGHT_EYE 159(up), 133(right), 145(down), 33(left)
        //LEFT_EYE 386(up), 263(right), 374(down), 362(left)
        //MOUSE 12(up), 292(right), 15(down), 62(left)
        //FACE 10(face up), 366(face right), 152(face down), 123(face left)
        arFaceLandmark = new ARFaceLandmark[]
        {
            new ARFaceLandmark(10, 366, 152, 123), // FACE
            new ARFaceLandmark(159, 133, 145, 33), // RIGHT_EYE
            new ARFaceLandmark(386, 263, 374, 362), // LEFT_EYE
            new ARFaceLandmark(12, 292, 15, 62) // MOUSE
        };

        m_AugmentedFaceList = new List<GoogleARCore.AugmentedFace>();
        playingCoroutine = StartCoroutine(CorutineEyeBlink(2.0f));
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        prevLeftEye = 1;
        prevRightEye = 1;
        eyeOpen = true;
    }

    void Update()
    {
        m_AugmentedFaceList.Clear();
        GoogleARCore.Session.GetTrackables<GoogleARCore.AugmentedFace>(m_AugmentedFaceList, GoogleARCore.TrackableQueryFilter.All);
        if (m_AugmentedFaceList.Count != 0)
        {
            m_AugmentedFace = m_AugmentedFaceList[0];
        }

        if (m_AugmentedFace == null) return;

        setFaceRotation(m_AugmentedFace.CenterPose.rotation);
        List<Vector3> verticeList = new List<Vector3>();
        m_AugmentedFace.GetVertices(verticeList);
        setFaceLandmark(verticeList);
    }

    public void setFaceLandmark(List<Vector3> verticeList)
    {
        if (verticeList.Count > 0)
        {
            for (int i = 0; i < (int)FaceLandmarkPosition.MAX; ++i)
            {
                arFaceLandmark[i].setPoint(verticeList);
                Rect rect = arFaceLandmark[i].getRect();
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
    }

    public void toggleAutoEyeBlink()
    {
        if(playingCoroutine == null)
        {
            playingCoroutine = StartCoroutine(CorutineEyeBlink(2.0f));
        }
        else
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }
    }

    public void setLeftEyeHeight(Slider obj)
    {
        prevLeftEye = obj.value;
        if (eyeOpen) leftEyeHeight = prevLeftEye;
    }

    public void setRightEyeHeight(Slider obj)
    {
        prevRightEye = obj.value;
        if (eyeOpen) rightEyeHeight = prevRightEye;
    }

    public Quaternion getFaceRotation() { return faceRotation; }

    public void getEyeHeight(out float leftEye, out float rightEye)
    {
        leftEye = leftEyeHeight;
        rightEye = rightEyeHeight;
    }

    IEnumerator CorutineEyeBlink(float second)
    {
        while(true)
        {
            if (eyeOpen)
            {
                leftEyeHeight = 0;
                rightEyeHeight = 0;
                eyeOpen = false;
                yield return null;
            }
            else
            {
                leftEyeHeight = prevLeftEye;
                rightEyeHeight = prevRightEye;
                eyeOpen = true;
                yield return new WaitForSeconds(second);
            }
        }
    }
}
