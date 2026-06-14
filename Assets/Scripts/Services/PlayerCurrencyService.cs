using System.Collections.Generic;
using UnityEngine;

public class PlayerCurrencyService
{
    private readonly Dictionary<GameEnums.CurrencyType, int> _balances = new();
    
    public int GetBalance(GameEnums.CurrencyType type)
        => _balances.TryGetValue(type, out int v) ? v : 0;
    
    public bool CanAfford(GameEnums.CurrencyType type, int amount)
        => GetBalance(type) >= amount;
    
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
    
    public void SetBalance(GameEnums.CurrencyType type, int amount)
    {
        amount = Mathf.Max(0, amount);
        _balances[type] = amount;
        GameEvents.RaiseCurrencyChanged(type, amount);
    }

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

    public void ApplySaveEntries(List<CurrencyEntry> entries)
    {
        _balances.Clear();
        if (entries == null)
            return;
        foreach (var entry in entries)
            _balances[entry.type] = Mathf.Max(0, entry.amount);
    }
}
