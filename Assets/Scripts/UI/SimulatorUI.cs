using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SimulatorUI : MonoBehaviour
{
    [Header("Subject References")]
    [SerializeField]
    private Button[] subjectButtons;
    [SerializeField]
    private SubjectUIReferenceOnButton[] subjectButtonUIReferences;

    [Header("Interaction References")]
    [SerializeField] 
    private Image[] DayInteractionImages;
    [SerializeField]
    private Color NotInteractedColor;
    [SerializeField]
    private Color InteractedColor;
    
    [Header("Score References")]
    [SerializeField]
    private SubjectScoreTextReferenceOnText[] subjectScoreTexts;
    
    [Header("Day and Number of Interaction References")]
    [SerializeField]
    private TextMeshProUGUI dayValueText;
    [SerializeField]
    private TextMeshProUGUI numberOfInteractionsTexts;
    
    [Header("Stamina References")]
    [SerializeField]
    private TextMeshProUGUI StaminaText;
    [SerializeField] 
    private Image StaminaFillImage;
    
    [Header("Goals References")]
    [SerializeField]
    private TextMeshProUGUI goalText;
    [SerializeField] 
    private TextMeshProUGUI goalScoreText;
    [SerializeField] 
    private TextMeshProUGUI goalDayText;
    
    //------MANAGER REFERENCES--------------
    SimulatorManager simulatorManager;
    
    //---------SO REFERENCES---------------
    [SerializeField] 
    private SubjectsData[] subjectsData;
    
    
    
    //--------------EVENTS--------------
    public static event Action<SubjectReferenceOnButton> OnAnyButtonPressed;
    
    //-----------AWAKE----------------
    private void Awake()
    {
        foreach (var button in subjectButtons)
        {
            SubjectReferenceOnButton capturedButton = button.GetComponent<SubjectReferenceOnButton>();
            button.onClick.AddListener(() => SubjectButtonPress(capturedButton));
        }

        foreach (var images in DayInteractionImages)
        {
            images.color = NotInteractedColor;
        }
    }
    
    //--------------ENABLE--------------
    private void OnEnable()
    {
        SimulatorManager.SubjectsOnNewDayUI += UpdateButtonUIForSubjectAssignment;
        SimulatorManager.UpdateScoreUI += UpdateSubjectScore;
        SimulatorManager.UpdateInteractionUI += UpdateTheNumberOfInteraction;
        SimulatorManager.UpdateStaminaUI += UpdateStamina;
        SimulatorManager.UpdateDayUI += UpdateDay;
        SimulatorManager.UpdateGoalUI += UpdateGoal;
        
        simulatorManager = GetComponent<SimulatorManager>();
        UpdateAllSubjectsScore();
        
    }

    //---------------START----------------------------

    private void Start()
    {
        
    }

    private void UpdateButtonUIForSubjectAssignment(List<Subject> obj)
    {
       
        for (int i = 0; i < subjectButtons.Length; i++)
        {
            var reference = subjectButtons[i].GetComponent<SubjectReferenceOnButton>();
            if (reference != null)
            {
                reference.subjectID = obj[i].subjectID;
                
                foreach (var data in subjectsData)
                {
                    foreach (var subData in data.subjects)
                    {
                        if (subData.subjectID != reference.subjectID) 
                            continue;
                       
                        if (subData.Level == obj[i].currentLevel)
                        {
                            reference.Level = subData.Level;
                            reference.amountOfStaminaReduced = subData.amountOfStaminaReduced;
                            reference.amountScoreToBeAdded = subData.amountScoreToBeAdded;
                            reference.interactionToBeDeducted = subData.interactionToBeDeducted;
                            SetButtonUIVisuals(subData , i);
                        }
                    }
                }
            }
        }
       
    }

    private void SetButtonUIVisuals(SubjectLevelData subData, int index)
    {
        subjectButtonUIReferences[index].subjectID = subData.subjectID;
        subjectButtonUIReferences[index].subjectNameText.text = subData.subjectID.ToString();
        subjectButtonUIReferences[index].subjectLevelText.text = subData.Level.ToString();
        subjectButtonUIReferences[index].subjectScoreText.text = subData.amountScoreToBeAdded.ToString();
        subjectButtonUIReferences[index].subjectStaminaText.text = subData.amountOfStaminaReduced.ToString();
    }

    void UpdateGoal(IndividualGoal newGoal)
    {
        goalText.text = newGoal.subjectID.ToString();
        goalScoreText.text = newGoal.subGoalScore.ToString();
        goalDayText.text = newGoal.subGoalDay.ToString();
    }

    void UpdateDay(int nextDayValue)
    {
        dayValueText.text = $"Day :{nextDayValue}";
        numberOfInteractionsTexts.text = $"0/12";
        //Start New Day 
        for (int i = 0; i < DayInteractionImages.Length; i++)
        {
            DayInteractionImages[i].color = NotInteractedColor;
        }
    }

    void UpdateAllSubjectsScore()
    {
        var allSubjects = simulatorManager.subjects;
        foreach (var sub in allSubjects)
        {
            UpdateSubjectScore(sub);
        }
    }

    void SubjectButtonPress(SubjectReferenceOnButton button)
    {
        OnAnyButtonPressed?.Invoke(button);
    }

    private void UpdateSubjectScore(Subject subject)
    {
        Debug.Log($"UpdateSubjectScore called for {nameof(subject)}: {subject.subjectID} with Score : {subject.score}");
        
        foreach (var scoreText in subjectScoreTexts)
        {
            if (scoreText.subjectID == subject.subjectID)
            {
                scoreText.text.SetText($" {subject.subjectName} : {subject.score}");
            }
        }
    }

    private void UpdateTheNumberOfInteraction(int currentInteraction, int maxNumberOfInteractions)
    {
        numberOfInteractionsTexts.text = $"{currentInteraction}/{maxNumberOfInteractions}";
        UpdateInteractionGraphics(currentInteraction);
    }
    
    private void UpdateStamina(int value)
    {
        StaminaText.text = $"{value}%";
        var staminaFillValue = (float)value / 100;
        UpdateStaminaGraphics(staminaFillValue);
    }

    private void UpdateStaminaGraphics(float staminaFillValue)
    {
       StaminaFillImage.fillAmount = staminaFillValue;
    }

    private void UpdateInteractionGraphics(int interactionValue)
    {
        for (int i=0;i<interactionValue;i++)
        {
            DayInteractionImages[i].color = InteractedColor;
        }
    }
}
