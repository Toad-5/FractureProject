using System.Collections.Generic;
using UnityEngine;

public class IntroductionManager : MonoBehaviour
{
    [System.Serializable]
    public class Step
    {
        [Header("Actions à l'entrée de l'étape")]
        public List<GameObject> objectsToActivate;
        public List<GameObject> objectsToDeactivate;
        
        [Header("Timing")]
        public float timeBeforeNextStep;
    }
    
    public List<Step> steps = new List<Step>();

    private int currentStepIndex = -1;
    private float timer = 0f;
    private bool isIntroFinished = false;

    void Start()
    {
        InitialHideAll();
        NextStep();
    }

    void Update()
    {
        if (isIntroFinished) return;
        
        timer += Time.deltaTime;
        
        if (timer >= steps[currentStepIndex].timeBeforeNextStep)
        {
            NextStep();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextStep();
        }
    }

    public void NextStep()
    {
        currentStepIndex++;
        timer = 0f;

        if (currentStepIndex < steps.Count)
        {
            ExecuteStep(steps[currentStepIndex]);
        }
        else
        {
            isIntroFinished = true;
            Debug.Log("Fin de l'introduction");
            this.gameObject.SetActive(false);
        }
    }

    public void ExecuteStep(Step step)
    {
        foreach (GameObject obj in step.objectsToActivate)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in step.objectsToDeactivate)
        {
            obj.SetActive(false);
        }
    }

    private void InitialHideAll()
    {
        foreach (Step step in steps)
        {
            foreach (GameObject obj in step.objectsToActivate) obj.SetActive(false);
            foreach (GameObject obj in step.objectsToDeactivate) obj.SetActive(false);
        }
    }
}