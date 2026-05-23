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

    public static event Action<Subject> UpdateScoreUI;
    public static event Action<int,int> UpdateInteractionUI;
    public static event Action<List<Subject>> SubjectsOnNewDayUI;
    public static event Action<int> UpdateStaminaUI;
    
    private void OnEnable()
    {
        SimlulatorUI.OnAnyButtonPressed += ButtonPressedTasks;
    }

    private void Start()
    {
        Debug.Log("Starting SimulatorManager Calling On Start");
        Debug.Log("Game Logged In Lets Set the Subjects On For 1st time");
        RandomizeSubjectsToShowOnButton();
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
        }
    }

    void AddStamina(int staminaValue)
    {
        currentStamina += staminaValue;
        if (currentStamina > 100)
        {
            currentStamina = 100;
        }
    }
}

[System.Serializable]
public class Subject
{
    public SimulatorSubjects subjectID = SimulatorSubjects.None;
    public string subjectName = nameof(SimulatorSubjects.None);
    public int score = 0;
}


