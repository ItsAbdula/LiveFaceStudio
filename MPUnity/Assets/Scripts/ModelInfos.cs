using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ModelInfos
{
    // 인덱스 접근용 Enum
    // 더 좋은 방법은 없나
    public enum Type
    {
        Epsilon = 0, Koharu = 1, Unitychan = 2, Hibiki = 3, Hiyori = 4
    }

    public const int MODEL_COUNT = 5;
    public const int MODEL_PARAM_COUNT = 11;

    // ModelParameters를 모델 별 CubismParameter 에 대응시키기 위한 인덱스
    public static readonly int[,] paramIndices = new int[MODEL_COUNT, MODEL_PARAM_COUNT]
    {
        {0, 1, 2, 3, 5, 7, 8, 9, 10, 18, 17},   // Epsilon
        {0, 1, 2, 3, 5, 7, 8, 14, 15, 23, 22},  // Koharu
        {0, 1, 2, 3, 5, 8, 9, 11, 12, 20, 19},  // UnityChan
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 16, 15},    // Hibiki
        {0, 1, 2, 4, 6, 8, 9, 10 ,11, 13, 12}   // Hiyori
    };

    // 모델 별 parameter 범위 변환도 필요하다. 추가 예정
    public static readonly float[,] paramRatios;
}
