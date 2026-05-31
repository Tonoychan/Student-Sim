using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectAnswer : MonoBehaviour
{
    public Image selectedImage;
    public SimulatorManager _simulatorManager;
    public static event Action<int> AnAnswerOptionSelected;
    public int answerOption = 0;
    
    public void OnBtnClick()
    {
        _simulatorManager.currentSelectedAnswer =  answerOption;
        AnAnswerOptionSelected?.Invoke(answerOption);
    }

    public void ChangeSelectionStatus(bool isSelected)
    {
        selectedImage.gameObject.SetActive(isSelected);
    }
}
