using System;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Advance Flow/QuestData")]
public class QuestData : ScriptableObject
{
    public QuestEntry[] quests;
}

[Serializable]
public class QuestEntry
{
    public string questId;
    public string title;
    [TextArea] public string description;
    public GameEnums.MainSubjects subject;
    public int requiredScore;
    public int deadlineDay;
    public int goldReward; 
}