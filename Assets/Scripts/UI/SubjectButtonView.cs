using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One subject button on the daily selection screen.
/// </summary>
public class SubjectButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI _subjectLabel;
    [SerializeField] private TextMeshProUGUI _subjectStaminaLabel;
    [SerializeField] private TextMeshProUGUI _subjectScoreLabel;
    [SerializeField] private TextMeshProUGUI _subjectLevelLabel;
    [SerializeField] private Image _iconImage;
    /// <summary>Sets labels/icon and wires the click handler.</summary>
    public void Bind(SubjectDisplayData data, Action<SubjectDisplayData > onClick)
    {
        _subjectLabel.text = data.DisplayName;
        _subjectStaminaLabel.text = data.staminaDescription;
        _subjectScoreLabel.text = data.scoreDescription;
        _subjectLevelLabel.text = data.levelDescription;
        if (_iconImage != null)
        {
            _iconImage.sprite = data.Icon;
            _iconImage.enabled = data.Icon != null;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(data));
    }
    
    public void SetInteractable(bool value)
    {
        if (button != null)
            button.interactable = value;
    }
}
