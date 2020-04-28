using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static int currentCharacterIndex = 4;
    public static GameObject currentCharacter;

    [SerializeField]
    private GameObject[] characters;
    [SerializeField]
    private GameObject[] leftEyes;

    private void Awake()
    {
        ShowCharacter(currentCharacterIndex);
        currentCharacter = characters[currentCharacterIndex];
    }

    public void ShowNextCharacter()
    {
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Length;
        ShowCharacter(currentCharacterIndex);
    }

    public void SetEyeColor()
    {
        // 변경 예정
    }

    private void ShowCharacter(int index)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (i == index)
            {
                characters[i].SetActive(true);
            }
            else
            {
                characters[i].SetActive(false);
            }
        }
    }
}