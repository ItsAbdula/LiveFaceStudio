using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    private int _currentCharacterIndex = 0;

    [SerializeField]
    private Color _currentColor;

    [SerializeField]
    private GameObject[] characters;
    [SerializeField]
    private GameObject[] leftEyes;

    void Start()
    {
        ShowCharacter(_currentCharacterIndex);
    }

    public void ShowNextCharacter()
    {
        _currentCharacterIndex = (_currentCharacterIndex + 1) % characters.Length;
        ShowCharacter(_currentCharacterIndex);
    }

    public void SetEyeColor()
    {
        if(leftEyes[_currentCharacterIndex] == null)
        {
            return;
        }

        leftEyes[_currentCharacterIndex].GetComponent<Live2D.Cubism.Rendering.CubismRenderer>().Color = _currentColor;
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
