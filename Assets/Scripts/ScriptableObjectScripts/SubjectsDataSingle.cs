using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SubjectsDataSingle", menuName = "Advance Flow/SubjectsDataSingle")]
public class SubjectsDataSingle : ScriptableObject
{
    public string subjectName;
    public List<SubjectData> Data = new List<SubjectData>();
}

[System.Serializable]
public class SubjectData
{
    public GameEnums.MainSubjects subjectID;
    public string subjectClassName;
    public int subjectLevel;
    public int staminaDeducted;
    public int staminaRestored;
    public int subjectScore;
    public int subjectScoreMultiplier;
    public int interactionDeducted;
    public int interactionsToUnlockNextLevel;
}
