using UnityEngine;

[CreateAssetMenu(fileName = "SubjectsData", menuName = "Scriptable Objects/SubjectsData")]
public class SubjectsData : ScriptableObject
{
    public SubjectLevelData[]  subjects;
}

[System.Serializable]
public class SubjectLevelData
{
    public SimulatorSubjects subjectID;
    public int Level;
    public int amountScoreToBeAdded;
    public int interactionToBeDeducted;
    public int amountOfStaminaReduced;
}
