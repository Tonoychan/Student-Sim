using TMPro;
using UnityEngine;

public class LeaderboardItemView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _positionText;
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _youBadge;
   
    public void Bind(int rank, string playerName, double score, bool isCurrentPlayer)
    {
        if (_positionText != null)
            _positionText.text = $"#{rank}";
        if (_playerNameText != null)
            _playerNameText.text = playerName ?? "—";
        if (_scoreText != null)
            _scoreText.text = score.ToString("N0");
        if (_youBadge != null)
            _youBadge.SetActive(isCurrentPlayer);
    }
}
