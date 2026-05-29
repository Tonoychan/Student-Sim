using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SimulatorManager : MonoBehaviour
{
    public int currentStamina = 100;
    public int maxNumberOfInteractions = 12;
    public int numberOfInteractionsDone = 0;
    public Subject[] subjects;
    public int currentDay;

    [SerializeField]
    private GoalData goalsData;
    [SerializeField]
    IndividualGoal goal = new IndividualGoal();
    public int numberOfGoalsCompleted = 0;

    public static event Action<Subject> UpdateScoreUI;
    public static event Action<int,int> UpdateInteractionUI;
    public static event Action<List<Subject>> SubjectsOnNewDayUI;
    public static event Action<int> UpdateStaminaUI;
    public static event Action<int> UpdateDayUI;
    public static event Action<IndividualGoal> UpdateGoalUI;
    
    
    private void OnEnable()
    {
        SimulatorUI.OnAnyButtonPressed += ButtonPressedTasks;
    }

    private void Start()
    {
        Debug.Log("Starting SimulatorManager Calling On Start");
        Debug.Log("Game Logged In Lets Set the Subjects On For 1st time");
        currentDay = 1;
        RandomizeSubjectsToShowOnButton();
        SetGoal();
    }

    private void ButtonPressedTasks(SubjectReferenceOnButton obj)
    {
        //Check The Button Component And Update the Following
        //1. Update Score
        Debug.Log("ButtonPressedTasks in SimulatorManager");
        Debug.Log("ButtonPressedTasks 1 Update Score in SimulatorManager");

        foreach (var subject in subjects)
        {
            if(subject.subjectID == SimulatorSubjects.None)
                return;

            if (subject.subjectID == obj.subjectID)
            {
                AddScoreToSubject(subject,obj.amountScoreToBeAdded);
                UpdateScoreUI?.Invoke(subject);
            }
        }
        
        //2. Update Interaction
        Debug.Log("ButtonPressedTasks 2 Update Interaction Value in SimulatorManager");
        numberOfInteractionsDone++;
        if (maxNumberOfInteractions >= numberOfInteractionsDone)
        {
            UpdateInteractionUI?.Invoke(numberOfInteractionsDone,maxNumberOfInteractions);
            if (numberOfInteractionsDone == maxNumberOfInteractions)
            {
                currentDay++;
                UpdateDayUI?.Invoke(currentDay);
                CheckGoalAchieved();
                ResetValueForNextDay();
                return;
            }
        }
        
        //3.Reduce Stamina for the Interaction
        foreach (var subject in subjects)
        {
            if(subject.subjectID == SimulatorSubjects.None)
                return;

            if (subject.subjectID == obj.subjectID)
            {
                RemoveStamina(obj.amountOfStaminaReduced);
                UpdateStaminaUI?.Invoke(currentStamina);
            }
        }
        
        Debug.Log("ButtonPressedTasks 3 Update Next Subject Value in SimulatorManager");
        //4.Next Items to show on Buttons
        RandomizeSubjectsToShowOnButton();
    }


    void RandomizeSubjectsToShowOnButton()
    {
        List<Subject> choosenSubjects = new List<Subject>(4);
        choosenSubjects= subjects.OrderBy(x => Guid.NewGuid()).Take(4).ToList();
        Debug.Log($"List Of chosen Subjects: {choosenSubjects[0].subjectName}, {choosenSubjects[1].subjectName}, {choosenSubjects[2].subjectName}, {choosenSubjects[3].subjectName}]");
        SubjectsOnNewDayUI?.Invoke(choosenSubjects);
    }

    void AddScoreToSubject(Subject subject,int score)
    {
        subject.score += score;
    }
    
    void RemoveScoreToSubject(Subject subject,int score)
    {
        subject.score -= score;
    }

    void RemoveStamina(int staminaValue)
    {
        currentStamina -= staminaValue;
        if (currentStamina < 0)
        {
            currentStamina = 0;
            StaminaDepleted();
            UpdateDayUI?.Invoke(currentDay);
        }
        if (currentStamina > 100)
        {
            currentStamina = 100;
        }
    }

    //TODO Later if Required?
    // void AddStamina(int staminaValue)
    // {
    //     currentStamina += staminaValue;
    //     if (currentStamina > 100)
    //     {
    //         currentStamina = 100;
    //     }
    // }

    void StaminaDepleted()
    {
        currentDay++;
        CheckGoalAchieved();
        ResetValueForNextDay();
        //Change to Next Day and Reset The UI and Buttons and Interactions
    }

    private void ResetValueForNextDay()
    {
        currentStamina = 100;
        numberOfInteractionsDone = 0;
        SetGoal();
        UpdateStaminaUI?.Invoke(currentStamina);
        UpdateInteractionUI?.Invoke(numberOfInteractionsDone,maxNumberOfInteractions);
    }

    private void CheckGoalAchieved()
    {
        if (currentDay > goal.subGoalDay)
        {
            foreach (var subject in subjects)
            {
                if (subject.subjectID == goal.subjectID)
                {
                    if (subject.score >= goal.subGoalScore)
                    {
                        Debug.Log("GoalAchieved");
                        numberOfGoalsCompleted++;
                    }
                    else
                    {
                        Debug.Log("Goal Not Achieved");
                    }
                }
            }
        }
    }

    private void SetGoal()
    {
        goal = goalsData.subGoals[numberOfGoalsCompleted];
        UpdateGoalUI?.Invoke(goal);
    }
}

[System.Serializable]
public class Subject
{
    public SimulatorSubjects subjectID = SimulatorSubjects.None;
    public string subjectName = nameof(SimulatorSubjects.None);
    public int score = 0;
}


