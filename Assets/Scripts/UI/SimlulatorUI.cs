using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SimlulatorUI : MonoBehaviour
{
    [Header("Subject References")]
    [SerializeField]
    private Button[] subjectButtons;
    [SerializeField]
    private TextMeshProUGUI[] subjectButtonsText;

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
    
    //------MANAGER REFERENCES--------------
    SimulatorManager simulatorManager;
    
    
    //--------------EVENTS--------------
    public static event Action<SubjectReferenceOnButton> OnAnyButtonPressed;
    
    //-----------AWAKE----------------
    private void Awake()
    {
        Debug.Log("Game Start Awake in SimulatorUI");
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
        
        simulatorManager = GetComponent<SimulatorManager>();
        UpdateAllSubjectsScore();
        
        Debug.Log("Game Start Start in SimulatorUI");
    }

    //---------------START----------------------------

    private void Start()
    {
        
    }

    private void UpdateButtonUIForSubjectAssignment(List<Subject> obj)
    {
        Debug.Log("New Subjects Received Setting them Up");
        for (int i = 0; i < subjectButtons.Length; i++)
        {
            var reference = subjectButtons[i].GetComponent<SubjectReferenceOnButton>();
            if (reference != null)
            {
                subjectButtonsText[i].text = obj[i].subjectName;
                reference.subjectID = obj[i].subjectID;
                reference.Level = 0;
                reference.amountOfStaminaReduced = 15;
                reference.amountScoreToBeAdded = 20;
                reference.interactionToBeDeducted = 1;
            }
        }
        Debug.Log("New Subjects Setting done");
    }

    void StartNewDay()
    {
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
        Debug.Log("A Subject Button Pressed in SimulatorUI");
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
        StaminaText.text = value.ToString();
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
