using System;

[Serializable]
public class CloudWalletResponse
{
    public bool success;
    public int gold;
    public string[] completedQuestIds;
    public long lastUpdatedUtc;
    public string error;
}

[Serializable]
public class CloudGrantGoldResponse
{
    public bool success;
    public int gold;
    public int granted;
    public string questId;
    public string error;
}

[Serializable]
public class CloudSpendGoldResponse
{
    public bool success;
    public int gold;
    public int spent;
    public string reason;
    public string error;
}