using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks gold and other currencies in memory during a play session.
/// </summary>
public class PlayerCurrencyService
{
    private readonly Dictionary<GameEnums.CurrencyType, int> _balances = new();
    
    public int GetBalance(GameEnums.CurrencyType type)
        => _balances.TryGetValue(type, out int v) ? v : 0;
    
    public bool CanAfford(GameEnums.CurrencyType type, int amount)
        => GetBalance(type) >= amount;
    
    /// <summary>Spends currency if the player has enough. Returns false otherwise.</summary>
    public bool TrySpend(GameEnums.CurrencyType type, int amount)
    {
        if (!CanAfford(type, amount)) return false;
        _balances[type] -= amount;
        GameEvents.RaiseCurrencyChanged(type, _balances[type]);
        return true;
    }
    
    public void Add(GameEnums.CurrencyType type, int amount)
    {
        if (amount <= 0) return;
        _balances[type] = GetBalance(type) + amount;
        GameEvents.RaiseCurrencyChanged(type, _balances[type]);
    }

    /// <summary>Sets balance directly (used when syncing from server).</summary>
    public void SetBalance(GameEnums.CurrencyType type, int amount)
    {
        amount = Mathf.Max(0, amount);
        _balances[type] = amount;
        GameEvents.RaiseCurrencyChanged(type, amount);
    }

    /// <summary>Converts balances to a list for saving.</summary>
    public List<CurrencyEntry> ToSaveEntries()
    {
        var entries = new List<CurrencyEntry>();
        foreach (var kvp in _balances)
        {
            entries.Add(new CurrencyEntry
            {
                type = kvp.Key,
                amount = kvp.Value
            });
        }
        return entries;
    }

    /// <summary>Restores balances from save data.</summary>
    public void ApplySaveEntries(List<CurrencyEntry> entries)
    {
        _balances.Clear();
        if (entries == null)
            return;
        foreach (var entry in entries)
            _balances[entry.type] = Mathf.Max(0, entry.amount);
    }
}
