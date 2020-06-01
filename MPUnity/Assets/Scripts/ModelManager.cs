
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Core;

public static class ModelInfo
{
    
}

public class ModelManager : MonoBehaviour
{
    public GameObject[] modelObjects;
    public ModelInfos.Type currentModelType;

    public ARCoreFaceLandmark arCoreFaceLandmark;

    private CubismModel cubismModel;
    private ModelParameters modelParam;
    
    private void Start()
    {
        SelectModel((int)currentModelType);

        modelParam = GetComponent<ModelParameters>();
    }

    public void SelectModel(int modelIndex)
    {
        currentModelType = (ModelInfos.Type)modelIndex;

        foreach (GameObject obj in modelObjects)
        {
            obj.SetActive(false);
        }

        cubismModel = modelObjects[modelIndex].GetComponent<CubismModel>();
        modelObjects[modelIndex].SetActive(true);
    }

    // CubismParameter는 LateUpdate()에서 업데이트해야 한다
    // https://docs.live2d.com/cubism-sdk-tutorials/about-parameterupdating-of-model/?locale=ja
    private void LateUpdate()
    {
        if(arCoreFaceLandmark != null)
        {
            Rect mouseRect = arCoreFaceLandmark.getRectByPosition(FaceLandmarkPosition.MOUSE);
            if(mouseRect!=null)
            {
                float mouseOpen = mouseRect.height/mouseRect.width-0.1f;
                modelParam.MouthOpen = mouseOpen * 2;
                
                modelParam.MouthForm = mouseRect.width - 4.6f;

                Debug.Log(modelParam.MouthForm);
            }
            Quaternion rotation = arCoreFaceLandmark.getFaceRotation();
            Vector3 euler = rotation.eulerAngles;
            if(euler.x!=0 || euler.y!=0 || euler.z!=0)
            {
                modelParam.FaceAngleX = euler.y >= 180 ? euler.y - 360 : euler.y;
                modelParam.FaceAngleX *= 2;
                modelParam.FaceAngleY = euler.x >= 180 ? euler.x - 360 : euler.x;
                modelParam.FaceAngleY *= 2;
                modelParam.FaceAngleZ = euler.z >= 180 ? euler.z - 360 : euler.z;
            }
        }
        UpdateCubismParam(modelParam, currentModelType);
    }

    // ModelParameters의 값을 가져와 모델 종류별 index에 따라 대입
    private void UpdateCubismParam(ModelParameters model, ModelInfos.Type type)
    {
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 0]].Value = model.FaceAngleX;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 1]].Value = model.FaceAngleY;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 2]].Value = model.FaceAngleZ;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 3]].Value = model.LEyeOpen;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 4]].Value = model.REyeOpen;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 5]].Value = model.EyeDirX;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 6]].Value = model.EyeDirY;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 7]].Value = model.LEyebrowHeight;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 8]].Value = model.REyebrowHeight;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 9]].Value = model.MouthOpen;
        cubismModel.Parameters[ModelInfos.paramIndices[(int)type, 10]].Value = model.MouthForm;
    }
}