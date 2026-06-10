using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExamAnswerOptionView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _answerText;
    [SerializeField] private GameObject _selectionMarker; // your green square
    [SerializeField] private Sprite _selectionMarkerImage;
    public void Bind(int optionIndex, Action<int> onClick)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onClick(optionIndex));
    }
    public void SetText(string text)
    {
        if (_answerText != null) _answerText.text = text;
    }
    public void SetSelected(bool isSelected)
    {
        if (_selectionMarker != null)
        {
            _selectionMarker.SetActive(isSelected);
            _selectionMarker.GetComponent<Image>().sprite = isSelected ? _selectionMarkerImage : null;
        }
            
    }
}
