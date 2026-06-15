using System;
using System.Collections.Generic;

[Serializable]
public class PlayerAccountData
{
    public string playerId = "";
    public List<CurrencyEntry> currencies = new();
}