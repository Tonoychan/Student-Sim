using System;
using System.Collections.Generic;
using UnityEngine;

public class SubjectSelectionUI : MonoBehaviour
{
    [SerializeField] private SubjectButtonView[] _subjectButtons;

    private void OnEnable()
    {
        GameEvents.OnDailySubjectsReady += Show;
        GameEvents.OnDaySummaryReady += _ => SetButtonsInteractable(false);
        GameEvents.OnDaySummaryClosed += () => SetButtonsInteractable(true);
    }
    private void OnDisable()
    {
        GameEvents.OnDailySubjectsReady -= Show;
        GameEvents.OnDaySummaryReady -= _ => SetButtonsInteractable(false);
        GameEvents.OnDaySummaryClosed -= () => SetButtonsInteractable(true);
    }

    public void Show(IReadOnlyList<SubjectDisplayData> subjects)
    {
        for (int i = 0; i < _subjectButtons.Length; i++)
        {
            if (_subjectButtons[i] == null)
                continue;
            if (i < subjects.Count)
            {
                _subjectButtons[i].gameObject.SetActive(true);
                _subjectButtons[i].Bind(subjects[i], HandleClick);
            }
            else
            {
                _subjectButtons[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void HandleClick(SubjectDisplayData data)
    {
        GameEvents.RaiseSubjectSelected(data);
    }
    
    private void SetButtonsInteractable(bool value)
    {
        foreach (var btn in _subjectButtons)
        {
            if (btn != null)
                btn.SetInteractable(value);
        }
    }
}

public class SubjectDisplayData
{
    public GameEnums.MainSubjects Subject;
    public string DisplayName;      // "Math", "Science"
    public string staminaDescription;
    public string scoreDescription;
    public string levelDescription;
    public string classNameDescription;
    public int CurrentLevel;        // if you track progress later
    public Sprite Icon;             // optional, from config later
    
    public int StaminaCost;
    public int ScoreGain;
    public int InteractionCost;
    public int StaminaRestore;
}
