using System;
using TMPro;
using UnityEngine;

public class SubjectScoreUI : MonoBehaviour
{
    [Serializable]
    public class SubjectScoreEntry
    {
        public GameEnums.MainSubjects subject;
        public TextMeshProUGUI scoreText;
    }
    
    [SerializeField] private SubjectScoreEntry[] _scoreEntries;
    
    private void OnEnable()
    {
        GameEvents.OnSubjectScoreChanged += UpdateSubjectScore;
    }
    private void OnDisable()
    {
        GameEvents.OnSubjectScoreChanged -= UpdateSubjectScore;
    }
    
    private void UpdateSubjectScore(GameEnums.MainSubjects subject, int score)
    {
        for (int i = 0; i < _scoreEntries.Length; i++)
        {
            if (_scoreEntries[i].subject != subject)
                continue;
            if (_scoreEntries[i].scoreText != null)
                _scoreEntries[i].scoreText.text = $"{subject}: {score}";
            break;
        }
    }
}
