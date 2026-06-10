using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DaySummaryUI : MonoBehaviour
{
    [Serializable]
    public class SubjectGainEntry
    {
        public GameEnums.MainSubjects subject;
        public TextMeshProUGUI gainText;   // shows "+8"
    }
    
    [SerializeField] private GameObject _summaryPanel;
    [SerializeField] private TextMeshProUGUI _dayCompletedText;
    [SerializeField] private SubjectGainEntry[] _subjectGainEntries;
    [SerializeField] private Button _continueButton;
    
    private void Awake()
    {
        _continueButton.onClick.AddListener(OnContinueClicked);
    }
    private void OnEnable()
    {
        GameEvents.OnDaySummaryReady += ShowSummary;
        GameEvents.OnDaySummaryClosed += HideSummary;
    }
    private void OnDisable()
    {
        GameEvents.OnDaySummaryReady -= ShowSummary;
        GameEvents.OnDaySummaryClosed -= HideSummary;
    }
    
    private void ShowSummary(DaySummaryData summary)
    {
        _summaryPanel.SetActive(true);
        _dayCompletedText.text = $"Day {summary.completedDay} Completed";
        foreach (var uiEntry in _subjectGainEntries)
        {
            int gain = 0;
            foreach (var dataEntry in summary.subjectGains)
            {
                if (dataEntry.subject != uiEntry.subject)
                    continue;
                gain = dataEntry.scoreGainedToday;
                break;
            }
            if (uiEntry.gainText != null)
                uiEntry.gainText.text = $"+{gain}";
        }
    }
    private void HideSummary()
    {
        _summaryPanel.SetActive(false);
    }
    private void OnContinueClicked()
    {
        GameEvents.RaiseContinueToNextDay();
    }
}
