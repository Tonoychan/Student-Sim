using UnityEngine;

[CreateAssetMenu(fileName = "GoalData", menuName = "Scriptable Objects/GoalData")]
public class GoalData : ScriptableObject
{
    public IndividualGoal[] subGoals;
}

[System.Serializable]
public class IndividualGoal
{
    public SimulatorSubjects subjectID;
    public int subGoalScore;
    public int subGoalDay;
}
