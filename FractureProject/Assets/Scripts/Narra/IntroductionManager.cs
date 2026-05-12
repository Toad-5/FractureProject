using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class IntroductionManager : MonoBehaviour
{
    [System.Serializable]
    public class Step
    {
        public List<GameObject> objectsToActivate;
        public List<GameObject> objectsToDeactivate;
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

    void NextStep()
    {
        if (isIntroFinished) return;
        
        currentStepIndex++;
        timer = 0f;

        if (currentStepIndex < steps.Count)
        {
            ExecuteStep(steps[currentStepIndex]);
        }
        else
        {
            StartCoroutine(FinishIntroRoutine());
        }
    }
    
    private IEnumerator FinishIntroRoutine()
    {
        isIntroFinished = true;

        Animator canvasAnim = GetComponent<Animator>();
    
        if (canvasAnim != null)
        {
            canvasAnim.SetTrigger("FinalExit");
            yield return new WaitForSeconds(5f);
        }

        this.gameObject.SetActive(false); 
    }

    void ExecuteStep(Step step)
    {
        foreach (GameObject obj in step.objectsToActivate)
        {
            obj.SetActive(true);
            
            Animator anim = obj.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Show");
            }
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