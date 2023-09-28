using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StudyMenu : MonoBehaviour
{
    // Public Variables //

    [Space(20)]
    [Header("Interaction Properties")]
    public CanvasGroup topCG;
    public CanvasGroup controllerInstructionsCG;
    public CanvasGroup nextButtonCG;
    public CanvasGroup skipButtonCG;
    public CanvasGroup studySelectorCG;
    public CanvasGroup startButtonCG;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.1f;

    [Space(20)]
    [Header("Tutorial Content")]
    public Sprite[] controllerTutorialSprites;
    public string[] controllerTutorialTexts;

    [Space(20)]
    [Header("Study Selection")]
    public GameObject study1GameObject;
    public GameObject study2GameObject;


    // Private Variables //

    private bool playerJoinedRoom = false;
    private int tutorialLayer = 1;
    private int selectedStudy = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (Vrsys.NetworkUser.localNetworkUser != null && !playerJoinedRoom)
        {
            playerJoinedRoom = true;

            StartCoroutine(FadeCanvasGroup(topCG, 0f, 1f, fadeInDuration, 1f));

            StartCoroutine(BeginInstructions());
        }
    }

    private IEnumerator BeginInstructions()
    {
        yield return new WaitForSeconds(2f);

        // Show controller primary button controls
        controllerInstructionsCG.GetComponentInChildren<TextMeshProUGUI>().text = controllerTutorialTexts[0];
        controllerInstructionsCG.GetComponentInChildren<Image>().sprite = controllerTutorialSprites[0];
        StartCoroutine(FadeCanvasGroup(controllerInstructionsCG, 0f, 1f, fadeInDuration));

        // Show skip controls option
        StartCoroutine(FadeCanvasGroup(skipButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));

        // Show next button
        StartCoroutine(FadeCanvasGroup(nextButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));

        yield return new WaitForSeconds(2f);
    }

    public void SkipControls()
    {
        // Disable skip controls button
        skipButtonCG.gameObject.SetActive(false);
        
        // Disable next button
        nextButtonCG.gameObject.SetActive(false);

        // Hide Controls Tutorial
        controllerInstructionsCG.alpha = 0f;
        controllerInstructionsCG.gameObject.SetActive(false);

        // Show tooltips on the player's controllers
        TooltipHandler tH = Vrsys.NetworkUser.localNetworkUser.GetComponent<TooltipHandler>();
        tH.SetShowTooltipsBool(true);
        tH.InitializeTooltips();

        // Show study selector 
        studySelectorCG.gameObject.SetActive(true);
        StartCoroutine(FadeCanvasGroup(studySelectorCG, 0f, 1f, fadeInDuration, enableInteraction: true));

        // Show start button
        StartCoroutine(FadeCanvasGroup(startButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));
    }

    public void HandleNext(Button nextButton)
    {
        nextButton.interactable = false;
        tutorialLayer += 1;

        if (tutorialLayer == 2)
        {
            // Show controller primary button controls
            controllerInstructionsCG.alpha = 0f;
            controllerInstructionsCG.GetComponentInChildren<TextMeshProUGUI>().text = controllerTutorialTexts[1];
            controllerInstructionsCG.GetComponentInChildren<Image>().sprite = controllerTutorialSprites[1];
            StartCoroutine(FadeCanvasGroup(controllerInstructionsCG, 0f, 1f, fadeInDuration));

            // Show tooltips on the player's controllers
            TooltipHandler tH = Vrsys.NetworkUser.localNetworkUser.GetComponent<TooltipHandler>();
            tH.SetShowTooltipsBool(true);
            tH.InitializeTooltips();

            // Set button interactable after delay
            StartCoroutine(SetButtonInteractableAfterDelay(nextButton, 1f));
        }
        if (tutorialLayer == 3)
        {
            // Show controller primary button controls
            controllerInstructionsCG.alpha = 0f;
            controllerInstructionsCG.GetComponentInChildren<TextMeshProUGUI>().text = controllerTutorialTexts[2];
            controllerInstructionsCG.GetComponentInChildren<Image>().sprite = controllerTutorialSprites[2];
            StartCoroutine(FadeCanvasGroup(controllerInstructionsCG, 0f, 1f, fadeInDuration));

            // Set button interactable after delay
            StartCoroutine(SetButtonInteractableAfterDelay(nextButton, 1f));
        }
        if (tutorialLayer == 4) 
        {
            // Hide Controls Tutorial
            controllerInstructionsCG.alpha = 0f;
            controllerInstructionsCG.gameObject.SetActive(false);

            // Show study selector 
            studySelectorCG.gameObject.SetActive(true);
            StartCoroutine(FadeCanvasGroup(studySelectorCG, 0f, 1f, fadeInDuration, enableInteraction: true));

            // Hide next button
            nextButton.gameObject.SetActive(false);

            // Hide skip controls button
            skipButtonCG.gameObject.SetActive(false);

            // Show start button
            StartCoroutine(FadeCanvasGroup(startButtonCG, 0f, 1f, fadeInDuration, 0.6f, true));
        }
    }

    public void SelectStudy(int studyIndex)
    {
        selectedStudy = studyIndex;
    }

    public void StartStudy()
    {
        if (selectedStudy == 0)
        {
            // Show no selection warning
            CanvasGroup noSelectionWarningCG = studySelectorCG.transform.Find("NoSelectionWarning").GetComponent<CanvasGroup>();
            StartCoroutine(FadeCanvasGroup(noSelectionWarningCG, 0f, 1f, 0.1f));
            StartCoroutine(FadeCanvasGroup(noSelectionWarningCG, 1f, 0f, 0.1f, 2f));
        }
        else if (selectedStudy == 1)
        {
            study1GameObject.SetActive(true);

            // Hide study menu
            HideStudyMenu();
        }
        else if (selectedStudy == 2)
        {
            study2GameObject.SetActive(true);

            // Hide study menu
            HideStudyMenu();
        }
    }

    private void HideStudyMenu()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator SetButtonInteractableAfterDelay(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);

        button.interactable = true;
    }


    private IEnumerator FadeCanvasGroup(CanvasGroup cG, float startAlpha, float endAlpha, float duration, float startDelay = 0f, bool enableInteraction = false)
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float t = 0f;
        while (t < duration)
        {
            cG.alpha = Mathf.Lerp(startAlpha, endAlpha, t /  duration);

            t += Time.deltaTime;
            yield return null;
        }

        cG.alpha = endAlpha;

        if (enableInteraction)
        {
            cG.interactable = true;
            cG.blocksRaycasts = true;
        }
    }
}
