using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Debug / New Game")]
    [SerializeField] private bool _startAsNew;
    
    [SerializeField] private MainExam _mainExamData;
    [SerializeField] private GameConfigSO _gameConfig;
    [SerializeField] private QuestData _questData;
    
    /// <summary>
    /// Lets think this class contains Data Holder for Subject and its SO Data for development / Failsafe
    /// </summary>
    [Serializable]
    public class SubjectAssetEntry
    {
        public GameEnums.MainSubjects subject;
        public SubjectsDataSingle dataAsset;
    }
    
    [SerializeField] private SubjectAssetEntry[] subjectAssets;
    
    
    
    /// <summary>
    /// Putting every factors that contribute to Logic as different services for Separation of concerns 
    /// </summary>
    //-------------------------------SERVICES-----------------------
    private SubjectService _subjectService;
    private SubjectSelectionService _selectionService;
    private PlayerStateService _playerState;
    private SubjectInteractionService _interactionService;
    private DayCycleService _dayCycleService;
    private PlayerSaveService _saveService;
    private ExamService _examService;
    private PlayerCurrencyService _currencyService;
    private QuestService _questService;

    private void OnEnable()
    {
        GameEvents.OnSubjectSelected += HandleSubjectSelected;
        GameEvents.OnContinueToNextDay += HandleContinueToNextDay;
        GameEvents.OnExamAnswerSubmitted += HandleExamAnswerSubmitted;
        GameEvents.OnExamCompleted += HandleExamCompleted;
    }
    
    private void OnDisable()
    {
        GameEvents.OnSubjectSelected -= HandleSubjectSelected;
        GameEvents.OnContinueToNextDay -= HandleContinueToNextDay;
        GameEvents.OnExamAnswerSubmitted -= HandleExamAnswerSubmitted;
        GameEvents.OnExamCompleted -= HandleExamCompleted;
    }

    void Start()
    {
        InitializeDataAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }
    
    private void HandleExamAnswerSubmitted(int option)
    {
        _examService.SelectAnswer(option);
        _examService.SubmitAnswer();
    }
    
    private async void HandleExamCompleted(int correctCount)
    {
        await UniTask.Delay(500);
        Debug.Log($"Exam finished. Correct: {correctCount}/6"); // internal only, no UI
        _dayCycleService.CompleteExamDay();
        _saveService.Save();
    }
    
    private async UniTask InitializeDataAsync(CancellationToken ct)
    {
        var saveProvider = new PlayerPrefsSaveProvider();
        
        _subjectService = new SubjectService();
        _playerState = new PlayerStateService();
        _currencyService = new PlayerCurrencyService();
        _selectionService = new SubjectSelectionService(_subjectService, _playerState);
        _examService = new ExamService(_mainExamData, _playerState, _gameConfig);
        _questService = new QuestService(_questData, _playerState, _currencyService);
        _dayCycleService = new DayCycleService(_playerState, _selectionService, _examService,_questService);
        
        _saveService = new PlayerSaveService(saveProvider, _playerState, _dayCycleService,_questService,_currencyService);
        _interactionService = new SubjectInteractionService(
            _playerState,
            _dayCycleService,
            _selectionService,
            _saveService,
            _examService,
            _subjectService);
        
        foreach (var entry in subjectAssets)
        {
            var loader = new SubjectDataLoader(entry.subject, entry.dataAsset);
            _subjectService.RegisterLoader(loader);
        }
        await _subjectService.InitializeAsync().AttachExternalCancellation(ct);
        
        // _saveService.LoadOrCreateNew();
        // _questService.Initialize();
        
        if (_startAsNew)
        {
            _saveService.StartFresh();                    // delete save + apply defaults
            _dayCycleService.StartDay(resetDailyStats: true);
        }
        else
        {
            _saveService.LoadOrCreateNew();
            _dayCycleService.StartDay(resetDailyStats: false); // keep loaded mid-day state
        }
        
        RaiseAllSubjectScores();
        GameEvents.RaiseCurrencyChanged(
            GameEnums.CurrencyType.Gold,
            _currencyService.GetBalance(GameEnums.CurrencyType.Gold));
    }
    
    private void RaiseAllSubjectScores()
    {
        foreach (GameEnums.MainSubjects subject in Enum.GetValues(typeof(GameEnums.MainSubjects)))
        {
            if (subject == GameEnums.MainSubjects.None) continue;
            GameEvents.RaiseSubjectScoreChanged(subject, _playerState.GetSubjectScore(subject));
        }
    }
    
    private void HandleSubjectSelected(SubjectDisplayData selection)
    {
        _interactionService.TryPerformSubject(selection);
    }
    
    private void OnApplicationPause(bool paused)
    {
        if (paused) _saveService?.Save();
    }
    private void OnApplicationQuit()
    {
        _saveService?.Save();
    }
    
    private void HandleContinueToNextDay()
    {
        _dayCycleService.ContinueToNextDay();
        _saveService.Save(); // save here if you remove it from DayCycleService
    }
    
}
