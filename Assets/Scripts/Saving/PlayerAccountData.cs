using System;
using System.Collections.Generic;

/// <summary>Wallet data tied to a player ID (saved separately from term progress).</summary>
[Serializable]
public class PlayerAccountData
{
    public string playerId = "";
    public long lastSavedUtc;
    public List<CurrencyEntry> currencies = new();
}