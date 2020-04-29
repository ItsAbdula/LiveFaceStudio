using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Core;

public class ModelParameters : MonoBehaviour
{
    private CubismModel model;
    private CubismParameter[] parameters;

    // CubismModel 에서 수정할 parameter의 index 저장. 모델마다 다름.
    private enum ParamIdx
    {
        FaceAngleX = 0,
        FaceAngleY = 1,
        FaceAngleZ = 2,

        LEyeOpen = 4,
        REyeOpen = 6,

        EyeDirX = 8,
        EyeDirY = 9,

        LEyebrowHeight = 10,
        REyebrowHeight = 11,

        MouthOpen = 13
    }

    // 이 변수의 값을 변경할 시 LateUpdate()에서 모델에 적용
    // 전달되는 값에 따라 범위 조정 필요
    [Range(-30f, 30f)]
    public float FaceAngleX = 0;

    [Range(-30f, 30f)]
    public float FaceAngleY = 0;

    [Range(-30f, 30f)]
    public float FaceAngleZ = 0;

    [Range(0f, 1.2f)]
    public float LEyeOpen = 1;

    [Range(0f, 1.2f)]
    public float REyeOpen = 1;

    [Range(-1f, 1f)]
    public float EyeDirX = 0;

    [Range(-1f, 1f)]
    public float EyeDirY = 0;

    [Range(-1f, 1f)]
    public float LEyebrowHeight = 0;

    [Range(-1f, 1f)]
    public float REyebrowHeight = 0;

    [Range(0f, 1f)]
    public float MouthOpen = 0;

    private void Start()
    {
        model = UIManager.currentCharacter.GetComponent<CubismModel>();
        parameters = model.Parameters;
    }

    // CubismParameter는 LateUpdate()에서 업데이트해야 함
    // https://docs.live2d.com/cubism-sdk-tutorials/about-parameterupdating-of-model/?locale=ja

    private void LateUpdate()
    {
        parameters[(int)ParamIdx.FaceAngleX].Value = FaceAngleX;
        parameters[(int)ParamIdx.FaceAngleY].Value = FaceAngleY;
        parameters[(int)ParamIdx.FaceAngleZ].Value = FaceAngleZ;

        parameters[(int)ParamIdx.LEyeOpen].Value = LEyeOpen;
        parameters[(int)ParamIdx.REyeOpen].Value = REyeOpen;

        parameters[(int)ParamIdx.EyeDirX].Value = EyeDirX;
        parameters[(int)ParamIdx.EyeDirY].Value = EyeDirY;

        parameters[(int)ParamIdx.LEyebrowHeight].Value = LEyebrowHeight;
        parameters[(int)ParamIdx.REyebrowHeight].Value = REyebrowHeight;

        parameters[(int)ParamIdx.MouthOpen].Value = MouthOpen;
    }
}