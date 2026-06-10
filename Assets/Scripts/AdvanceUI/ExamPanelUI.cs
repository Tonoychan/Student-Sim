using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExamPanelUI : MonoBehaviour
{
   [SerializeField] private GameObject _examPanel;           // MainQuestionPanel
    [SerializeField] private TextMeshProUGUI _subjectText;    // QuestionSubjectText
    [SerializeField] private TextMeshProUGUI _questionText;   // QuestionText
    [SerializeField] private ExamAnswerOptionView[] _options; // 4 rows in AnswerPanel
    [SerializeField] private Button _submitAnswerButton;      // AnswerButton
    private int _selectedOption;
    private void Awake()
    {
        if (_examPanel != null && _examPanel != gameObject)
            _examPanel.SetActive(false);
        _submitAnswerButton.onClick.AddListener(OnSubmitClicked);
        for (int i = 0; i < _options.Length; i++)
        {
            int optionIndex = i + 1;
            _options[i].Bind(optionIndex, OnOptionClicked);
        }
    }
    private void OnEnable()
    {
        GameEvents.OnExamStarted += ShowPanel;
        GameEvents.OnExamQuestionReady += ShowQuestion;
        GameEvents.OnExamClosed += HidePanel;
    }
    private void OnDisable()
    {
        GameEvents.OnExamStarted -= ShowPanel;
        GameEvents.OnExamQuestionReady -= ShowQuestion;
        GameEvents.OnExamClosed -= HidePanel;
    }
    private void ShowPanel()
    {
        if (_examPanel != null)
            _examPanel.SetActive(true);
    }
    private void HidePanel()
    {
        if (_examPanel != null)
            _examPanel.SetActive(false);
    }
    private void ShowQuestion(ExamQuestionDisplayData data)
    {
        _selectedOption = 0;
        ClearSelection();
        _subjectText.text = $"{data.Subject} - Level {data.SubjectLevel}";
        _questionText.text = data.QuestionText;
        for (int i = 0; i < _options.Length; i++)
        {
            string optionText = i < data.Options.Length ? data.Options[i] : "";
            _options[i].SetText(optionText);
        }
    }
    private void OnOptionClicked(int optionIndex)
    {
        _selectedOption = optionIndex;
        for (int i = 0; i < _options.Length; i++)
            _options[i].SetSelected(i + 1 == optionIndex);
    }
    private void OnSubmitClicked()
    {
        if (_selectedOption <= 0) return;
        GameEvents.RaiseExamAnswerSubmitted(_selectedOption);
        _selectedOption = 0;
        ClearSelection();
    }
    private void ClearSelection()
    {
        foreach (var option in _options)
            option.SetSelected(false);
    }
}
