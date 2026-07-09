using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
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
    private PlayerCloudSaveService _cloudSave;

    private void OnEnable()
    {
        GameEvents.OnSubjectSelected += HandleSubjectSelected;
        GameEvents.OnContinueToNextDay += HandleContinueToNextDay;
        GameEvents.OnExamAnswerSubmitted += HandleExamAnswerSubmitted;
        GameEvents.OnExamCompleted += HandleExamCompleted;
        GameEvents.OnCurrencyChanged += HandleCurrencyChanged;
        GameEvents.OnDayEnded += HandleDayEnded;
    }

    private void OnDisable()
    {
        GameEvents.OnSubjectSelected -= HandleSubjectSelected;
        GameEvents.OnContinueToNextDay -= HandleContinueToNextDay;
        GameEvents.OnExamAnswerSubmitted -= HandleExamAnswerSubmitted;
        GameEvents.OnExamCompleted -= HandleExamCompleted;
        GameEvents.OnCurrencyChanged -= HandleCurrencyChanged;
        GameEvents.OnDayEnded -= HandleDayEnded;
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
        _playerState.AddExamResult(correctCount);
        await UniTask.Delay(500);
        Debug.Log($"Exam finished. Correct: {correctCount}/6"); // internal only, no UI
        if (_gameConfig.IsFinalDay(_dayCycleService.CurrentDay))
        {
            _dayCycleService.CompleteTerm(_playerState);
            _saveService.Save(forceCloudFlush: true);
            return;
        }

        _dayCycleService.CompleteExamDay();
        _saveService.Save(forceCloudFlush: true);
    }

    private async UniTask InitializeDataAsync(CancellationToken ct)
    {
        // 1. Wait for Remote Config (from LoginScene) with timeout
        UnityRemoteConfigService remoteConfig = UnityRemoteConfigService.Instance;
        if (remoteConfig != null)
            await remoteConfig.WaitUntilReadyAsync(timeoutMs: 3000, ct);
        else
            Debug.LogWarning("[GameController] No Remote Config service — using defaults.");

        // 2. Build gameplay settings snapshot
        GameplaySettings gameplaySettings = GameplaySettings.FromRemoteConfig(remoteConfig);

        var saveProvider = new PlayerPrefsSaveProvider();

        bool continueGame = GameSessionContext.Mode == GameSessionContext.StartMode.Continue
                            && SaveContinueHelper.CanContinue(saveProvider);

        PlayerSaveData existingSave = continueGame ? saveProvider.Load() : null;

        int maxDays = continueGame
            ? (existingSave.maxDays > 0 ? existingSave.maxDays : 30)
            : GameSessionContext.SelectedMaxDays;

        // 3. Apply term config (exam days from RC)
        GameConfigLoader.ApplyTerm(_gameConfig, maxDays);

        // 4. Create services
        _subjectService = new SubjectService();
        _playerState = new PlayerStateService();
        _playerState.ApplyGameplaySettings(gameplaySettings); 

        _currencyService = new PlayerCurrencyService();
        _selectionService = new SubjectSelectionService(
            _subjectService,
            _playerState,
            gameplaySettings.DailySubjectCount); 
        
        QuestEntry[] quests = RemoteConfigQuestLoader.Load(_questData, remoteConfig);
        ExamDataRepository examRepository = ExamDataRepository.Load(_mainExamData, remoteConfig);
        
        // create services with RC data
        _examService = new ExamService(examRepository, _playerState, _gameConfig);
        _questService = new QuestService(quests, _playerState, _currencyService);
        _dayCycleService = new DayCycleService(
            _playerState,
            _selectionService,
            _examService,
            _questService,
            _gameConfig,
            gameplaySettings.DailySubjectCount,
            gameplaySettings.ExamScoreMultiplier);
        _cloudSave = PlayerCloudSaveService.Instance;
        _saveService = new PlayerSaveService(
            saveProvider, _playerState, _dayCycleService, _questService, _currencyService,_cloudSave);
        _saveService.SyncAccount(_currencyService);
        _interactionService = new SubjectInteractionService(
            _playerState,
            _dayCycleService,
            _selectionService,
            _saveService,
            _examService,
            _subjectService,
            gameplaySettings.DailySubjectCount);
        // register subject loaders with RC
        foreach (var entry in subjectAssets)
        {
            var loader = new SubjectDataLoader(entry.subject, entry.dataAsset, remoteConfig);
            _subjectService.RegisterLoader(loader);
        }
        await _subjectService.InitializeAsync().AttachExternalCancellation(ct);
        if (continueGame)
        {
            _saveService.LoadOrCreateNew();
            _dayCycleService.StartDay(resetDailyStats: false);
        }
        else
        {
            _saveService.StartFresh(maxDays);
            _dayCycleService.StartDay(resetDailyStats: true);
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
        if (paused)  _saveService.Save(forceCloudFlush: true);
    }

    private void OnApplicationQuit()
    {
        _saveService.Save(forceCloudFlush: true);
    }

    private void HandleContinueToNextDay()
    {
        _dayCycleService.ContinueToNextDay();
        _saveService.Save(forceCloudFlush: true);
    }

    private void HandleCurrencyChanged(GameEnums.CurrencyType type, int balance)
    {
        if (type != GameEnums.CurrencyType.Gold || _saveService == null)
            return;
        _saveService.SaveAccount(forceCloudFlush: false);
    }
    
    private void HandleDayEnded()
    {
        _saveService?.Save(forceCloudFlush: true);
    }
}