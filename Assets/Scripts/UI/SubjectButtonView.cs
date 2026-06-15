using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubjectButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI _subjectLabel;
    [SerializeField] private TextMeshProUGUI _subjectStaminaLabel;
    [SerializeField] private TextMeshProUGUI _subjectScoreLabel;
    [SerializeField] private TextMeshProUGUI _subjectLevelLabel;
    public void Bind(SubjectDisplayData data, Action<SubjectDisplayData > onClick)
    {
        _subjectLabel.text = data.DisplayName;
        _subjectStaminaLabel.text = data.staminaDescription;
        _subjectScoreLabel.text = data.scoreDescription;
        _subjectLevelLabel.text = data.levelDescription;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(data));
    }
    
    public void SetInteractable(bool value)
    {
        if (button != null)
            button.interactable = value;
    }
}
