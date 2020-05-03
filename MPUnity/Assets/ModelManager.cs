
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Core;

public static class ModelInfo
{
    // 인덱스 접근용 Enum
    // Gameobject 이름에서 값을 가져오는 방식으로 변경할 예정
    // 더 좋은 방법은 없나
    public enum Type
    {
        Epsilon = 0, Koharu = 1, Unitychan = 2, Hibiki = 3, Hiyori = 4
    }

    public const int MODEL_COUNT = 5;
    public const int MODEL_PARAM_COUNT = 10;

    // ModelParameters를 모델 별 CubismParameter 에 대응시키기 위한 인덱스

    public static readonly int[,] paramIndices = new int[MODEL_COUNT, MODEL_PARAM_COUNT]
    {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 1, 2, 4, 6, 8, 9, 10 ,11, 13}
    };

    // 모델 별 parameter 범위 변환도 필요하다. 추가 예정
    public static readonly float[,] paramRatios;
}

public class ModelManager : MonoBehaviour
{
    public GameObject[] modelObjects;
    public ModelInfo.Type currentModelType;

    private CubismModel cubismModel;
    private ModelParameters modelParam;
    
    private void Start()
    {
        SelectModel((int)currentModelType);

        cubismModel = UIManager.currentCharacter.GetComponent<CubismModel>();
        modelParam = GetComponent<ModelParameters>();
    }

    public void SelectModel(int modelIndex)
    {
        currentModelType = (ModelInfo.Type)modelIndex;

        foreach (GameObject obj in modelObjects)
        {
            obj.SetActive(false);
        }

        modelObjects[modelIndex].SetActive(true);
    }

    // CubismParameter는 LateUpdate()에서 업데이트해야 한다
    // https://docs.live2d.com/cubism-sdk-tutorials/about-parameterupdating-of-model/?locale=ja
    private void LateUpdate()
    {
        UpdateCubismParam(modelParam, currentModelType);
    }

    // ModelParameters의 값을 가져와 모델 종류별 index에 따라 대입
    private void UpdateCubismParam(ModelParameters model, ModelInfo.Type type)
    {
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 0]].Value = model.FaceAngleX;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 1]].Value = model.FaceAngleY;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 2]].Value = model.FaceAngleZ;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 3]].Value = model.LEyeOpen;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 4]].Value = model.REyeOpen;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 5]].Value = model.EyeDirX;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 6]].Value = model.EyeDirY;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 7]].Value = model.LEyebrowHeight;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 8]].Value = model.REyebrowHeight;
        cubismModel.Parameters[ModelInfo.paramIndices[(int)type, 9]].Value = model.MouthOpen;
    }
}