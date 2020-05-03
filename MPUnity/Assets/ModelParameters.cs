using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelParameters : MonoBehaviour
{
    // 모델의 종류에 따라 CubismParameter의 이름과 범위가 다르다
    // 따라서 여기에 저장되는 값은 모델 종류와 무관하게 일정한 범위를 갖는다
    // 이후에 표정 데이터를 기반으로 모델 이미지만 변경할 수 있도록 하기 위해서

    // 얼굴인식 정보에 따라 이 값만 변경하면 되도록 할 예정

    // 머리 각도
    [Range(-30f, 30f)]
    public float FaceAngleX = 0;
    [Range(-30f, 30f)]
    public float FaceAngleY = 0;
    [Range(-30f, 30f)]
    public float FaceAngleZ = 0;
    // 눈, 눈썹
    [Range(0f, 1f)]
    public float LEyeOpen = 1;
    [Range(0f, 1f)]
    public float REyeOpen = 1;
    [Range(-1f, 1f)]
    public float EyeDirX = 0;
    [Range(-1f, 1f)]
    public float EyeDirY = 0;
    [Range(-1f, 1f)]
    public float LEyebrowHeight = 0;
    [Range(-1f, 1f)]
    public float REyebrowHeight = 0;
    // 입
    [Range(0f, 1f)]
    public float MouthOpen = 0;
}

