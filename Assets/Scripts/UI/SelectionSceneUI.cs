using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionSceneUI : MonoBehaviour
{
    private const string GameSceneName = "SampleScene";
    
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _day5Button;
    [SerializeField] private Button _day30Button;
    [SerializeField] private Button _day120Button;
    [SerializeField] private Button _day360Button;
    
    private readonly PlayerPrefsSaveProvider _saveProvider = new();
    
    private void Start()
    {
        RefreshContinueButton();
        _continueButton.onClick.AddListener(OnContinueClicked);
        _day5Button.onClick.AddListener(() => OnNewGameClicked(5));
        _day30Button.onClick.AddListener(() => OnNewGameClicked(30));
        _day120Button.onClick.AddListener(() => OnNewGameClicked(120));
        _day360Button.onClick.AddListener(() => OnNewGameClicked(360));
        _day120Button.interactable = false;
        _day360Button.interactable = false;
    }
    private void RefreshContinueButton()
    {
        _continueButton.gameObject.SetActive(SaveContinueHelper.CanContinue(_saveProvider));
    }
    private void OnContinueClicked()
    {
        GameSessionContext.ContinueGame();
        SceneManager.LoadScene(GameSceneName);
    }
    private void OnNewGameClicked(int maxDays)
    {
        GameSessionContext.StartNewGame(maxDays);
        SceneManager.LoadScene(GameSceneName);
    }
}
