using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Selection scene menu: continue saved game, start new term, or view leaderboards.
/// </summary>
public class SelectionSceneUI : MonoBehaviour
{
    
    private bool _isLoadingScene;
    
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _day5Button;
    [SerializeField] private Button _day30Button;
    [SerializeField] private Button _day120Button;
    [SerializeField] private Button _day360Button;
    
    [SerializeField] private Button _viewLeaderboardsButton;
    [SerializeField] private LeaderboardPanelUI _leaderboardPanel;
    
    private readonly PlayerPrefsSaveProvider _saveProvider = new();
    
    private void Start()
    {
        RefreshContinueButton();
        _continueButton.onClick.AddListener(OnContinueClicked);
        _day5Button.onClick.AddListener(() => OnNewGameClicked(5));
        _day30Button.onClick.AddListener(() => OnNewGameClicked(30));
        _day120Button.onClick.AddListener(() => OnNewGameClicked(120));
        _day360Button.onClick.AddListener(() => OnNewGameClicked(360));
        if (_viewLeaderboardsButton != null)
            _viewLeaderboardsButton.onClick.AddListener(OnViewLeaderboardsClicked);
        _day120Button.interactable = false;
        _day360Button.interactable = false;
    }
    /// <summary>Shows or hides the Continue button based on save data.</summary>
    private void RefreshContinueButton()
    {
        _continueButton.gameObject.SetActive(SaveContinueHelper.CanContinue(_saveProvider));
    }
    private void OnContinueClicked()
    {
        GameSessionContext.ContinueGame();
        LoadGameSceneAsync().Forget();
    }
    private void OnNewGameClicked(int maxDays)
    {
        GameSessionContext.StartNewGame(maxDays);
        LoadGameSceneAsync().Forget();
    }
    
    async UniTaskVoid LoadGameSceneAsync()
    {
        if (_isLoadingScene)
            return;
        _isLoadingScene = true;
        SetButtonsInteractable(false);
        bool ok = await AddressableSceneLoader.LoadGameSceneAsync();
        if (!ok)
        {
            Debug.LogError("[SelectionScene] Addressable scene load failed.");
            SetButtonsInteractable(true);
            _isLoadingScene = false;
        }
        // On success, this scene unloads — no need to reset flag
    }
//Add
    void SetButtonsInteractable(bool value)
    {
        if (_continueButton != null) _continueButton.interactable = value;
        if (_day5Button != null) _day5Button.interactable = value;
        if (_day30Button != null) _day30Button.interactable = value;
        if (_day120Button != null) _day120Button.interactable = value;
        if (_day360Button != null) _day360Button.interactable = value;
    }
    
    private void OnViewLeaderboardsClicked()
    {
        _leaderboardPanel?.Show(5); // default 5-day tab
    }
}
