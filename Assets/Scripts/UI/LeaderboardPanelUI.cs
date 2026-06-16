using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPanelUI : MonoBehaviour
{
    [System.Serializable]
    public class TabEntry
    {
        public int maxDays;           // 5, 30, 120, 360
        public Button tabButton;
        public string leaderboardId; // empty = no board yet
    }
    
    [SerializeField] private GameObject _root;              // Leaderboard root (this panel)
    [SerializeField] private Transform _content;            // ScrollView → Viewport → Content
    [SerializeField] private LeaderboardItemView _rowPrefab; // LeaderboardItem prefab
    [SerializeField] private TabEntry[] _tabs;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _statusText; 
    
    private readonly TermLeaderboardService _service = new();
    private readonly List<LeaderboardItemView> _spawnedRows = new();
    private int _activeMaxDays = 5;
    
    private void Awake()
    {
        if (_root != null)
            _root.SetActive(false);
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Hide);
        foreach (var tab in _tabs)
        {
            if (tab.tabButton == null)
                continue;
            bool enabled = !string.IsNullOrEmpty(tab.leaderboardId);
            tab.tabButton.interactable = enabled;
            int maxDays = tab.maxDays;
            tab.tabButton.onClick.AddListener(() => ShowTab(maxDays).Forget());
        }
    }
    
    public void Show(int maxDays = 5)
    {
        if (_root != null)
            _root.SetActive(true);
        ShowTab(maxDays).Forget();
    }
    
    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);
    }
    
    private async UniTaskVoid ShowTab(int maxDays)
    {
        _activeMaxDays = maxDays;
        TabEntry tab = FindTab(maxDays);
        if (tab == null || string.IsNullOrEmpty(tab.leaderboardId))
        {
            SetStatus("Leaderboard not available for this mode yet.");
            ClearRows();
            return;
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            SetStatus("Please sign in first.");
            ClearRows();
            return;
        }
        SetStatus("Loading...");
        ClearRows();
        LeaderboardScoresPage page = await _service.GetScoresAsync(tab.leaderboardId, 20);
        if (page == null || page.Results == null || page.Results.Count == 0)
        {
            SetStatus("No scores yet.");
            return;
        }
        SetStatus("");
        string currentPlayerId = AuthenticationService.Instance.PlayerId;
        foreach (LeaderboardEntry entry in page.Results)
        {
            LeaderboardItemView row = Instantiate(_rowPrefab, _content);
            row.gameObject.SetActive(true);
            bool isYou = entry.PlayerId == currentPlayerId;
            row.Bind(entry.Rank+1, entry.PlayerName, entry.Score, isYou);
            _spawnedRows.Add(row);
        }
    }
    private TabEntry FindTab(int maxDays)
    {
        foreach (var tab in _tabs)
        {
            if (tab.maxDays == maxDays)
                return tab;
        }
        return null;
    }
    private void ClearRows()
    {
        foreach (var row in _spawnedRows)
        {
            if (row != null)
                Destroy(row.gameObject);
        }
        _spawnedRows.Clear();
    }
    private void SetStatus(string message)
    {
        if (_statusText != null)
            _statusText.text = message;
    }
}
