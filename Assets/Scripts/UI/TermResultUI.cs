using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TermResultUI : MonoBehaviour
{
    [SerializeField] private LeaderboardPanelUI _leaderboardPanel;
    
    private TermResultData _lastResult;
    
    [Serializable]
    public class SubjectRow
    {
        public GameEnums.MainSubjects subject;
        public TextMeshProUGUI subjectNameText;
        public TextMeshProUGUI valueText; 
    }
    
    [SerializeField] private GameObject _panel;
    [SerializeField] private SubjectRow[] _subjectRows;      // size 6
    [SerializeField] private TextMeshProUGUI _examCorrectText;   // ExamAnswers/SubjectScore_Text
    [SerializeField] private TextMeshProUGUI _multiplierText;    // AnswersMultiplier/SubjectScore_Text
    [SerializeField] private TextMeshProUGUI _finalScoreText;    // Score/SubjectScore_Text
    [SerializeField] private Button _leaderboardButton;          // optional, disable for now
    [SerializeField] private Button _playAgainButton;  
    
    private void Awake()
    {
        if (_panel != null)
            _panel.SetActive(false);
        if (_leaderboardButton != null)
            _leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        if (_playAgainButton != null)
            _playAgainButton.onClick.AddListener(OnPlayAgainClicked);
    }
    
    private void OnEnable()  => GameEvents.OnTermResultsReady += Show;
    private void OnDisable() => GameEvents.OnTermResultsReady -= Show;
    
    private void Show(TermResultData result)
    {
        _lastResult = result;
        _panel.SetActive(true);
        foreach (var row in _subjectRows)
        {
            int score = 0;
            foreach (var entry in result.subjectScores)
            {
                if (entry.subject != row.subject) continue;
                score = entry.score;
                break;
            }
            if(row.subjectNameText != null)
                row.subjectNameText.text = $"{row.subject} :";;
            if (row.valueText != null)
                row.valueText.text = score.ToString("N0");
        }
        _examCorrectText.text = $"{result.totalExamCorrect}/{result.maxExamCorrect}";
        _multiplierText.text = result.examMultiplier.ToString("F2");
        _finalScoreText.text = result.finalScore.ToString("N0");
    }
    
    private void OnPlayAgainClicked()
    {
        GameSessionContext.Reset();
        SceneManager.LoadScene("SelectionScene");
    }
    
    private void OnLeaderboardClicked()
    {
        if (_leaderboardPanel == null || _lastResult == null)
            return;
        
        int maxDays = _lastResult != null ? _lastResult.maxDays : 5;
        
        _leaderboardPanel.Show(maxDays);
    }
}
