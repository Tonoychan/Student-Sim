using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private TextMeshProUGUI _staminaText;
    [SerializeField] private Image _staminaFill;
    [SerializeField] private TextMeshProUGUI _interactionText;
    [SerializeField] private Image[] _interactionDots;
    [SerializeField] private Color _unusedDotColor = new(0.24f, 1f, 0f, 1f);
    [SerializeField] private Color _usedDotColor = Color.red;
    
    [SerializeField] private Sprite _unusedImage;
    [SerializeField] private Sprite _usedImage;
    private void OnEnable()
    {
        GameEvents.OnStaminaChanged += UpdateStamina;
        GameEvents.OnInteractionsChanged += UpdateInteractions;
        GameEvents.OnDayChanged += UpdateDay;
    }
    private void OnDisable()
    {
        GameEvents.OnStaminaChanged -= UpdateStamina;
        GameEvents.OnInteractionsChanged -= UpdateInteractions;
        GameEvents.OnDayChanged -= UpdateDay;
    }
    private void UpdateStamina(int current, int max)
    {
        if (_staminaText != null)
            _staminaText.text = $"{current}/{max}";
        if (_staminaFill != null)
            _staminaFill.fillAmount = max > 0 ? (float)current / max : 0f;
    }
    private void UpdateInteractions(int used, int max)
    {
        if (_interactionText != null)
            _interactionText.text = $"{used}/{max}";
        if (_interactionDots == null)
            return;
        for (int i = 0; i < _interactionDots.Length; i++)
        {
            if (_interactionDots[i] != null)
                if (_unusedImage != null && _usedImage != null)
                {
                    _interactionDots[i].sprite = i < used ? _usedImage : _unusedImage;
                }
                else
                {
                    _interactionDots[i].color = i < used ? _usedDotColor : _unusedDotColor;
                }

        }
    }
    
    private void UpdateDay(int day)
    {
        if (_dayText != null)
            _dayText.text = $"Day {day}";
    }
}
