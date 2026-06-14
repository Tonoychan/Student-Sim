using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _goalPanel;
    
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _deadlineDayText;   // BeforeDayText
    [SerializeField] private TextMeshProUGUI _subjectNameText; // SubjectNameText
    [SerializeField] private TextMeshProUGUI _requiredScoreText;       // SubjectScoreText
    [SerializeField] private TextMeshProUGUI _rewardText;        // RewardText
    
    private QuestEntry _activeQuest;
   
    private void OnEnable()
    {
        GameEvents.OnActiveQuestChanged += ShowQuest;
    }
    
    private void OnDisable()
    {
        GameEvents.OnActiveQuestChanged -= ShowQuest;
    }
    
    private void ShowQuest(QuestEntry quest)
    {
        if (quest == null)
        {
            if (_goalPanel != null) _goalPanel.SetActive(false);
            return;
        }
        if (_goalPanel != null) _goalPanel.SetActive(true);
        if (_deadlineDayText != null)
            _deadlineDayText.text = $"Day {quest.deadlineDay}";
        if (_subjectNameText != null)
            _subjectNameText.text = $"{quest.subject} :";
        if (_requiredScoreText != null)
            _requiredScoreText.text = quest.requiredScore.ToString();  // target only, e.g. "500"
        if (_rewardText != null)
            _rewardText.text = $"Reward : {quest.goldReward}";
    }
}
