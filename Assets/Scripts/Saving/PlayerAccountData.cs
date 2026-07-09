using System;
using System.Collections.Generic;

[Serializable]
public class PlayerAccountData
{
    public string playerId = "";
    public long lastSavedUtc;
    public List<CurrencyEntry> currencies = new();
}