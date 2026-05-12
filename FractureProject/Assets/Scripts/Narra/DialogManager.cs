using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class DialogManager : MonoBehaviour
{
    private Queue<string> sentencesQueue;
    public TriggerEvent TriggerEvent;
    public static DialogManager instance;
    public TMPro.TMP_Text nameText;
    public TMPro.TMP_Text phrasesText;
    public Animator animatorUrsula;
    public Animator animatorPNJ;
    public Player controller; 
    //public GameObject trigger;
   
    void Start()
    {
        //if (instance != null)
        //{
            //Destroy(gameObject);
            
            //Debug.Log("the dialog manager isn't there");
        //}
        
        instance = this;
        
        sentencesQueue = new Queue<string>();
        
        //Debug.Log(sentencesQueue.Count);
    }

    public void InitiateDialogue(Dialogs dialogs)
    {
        Debug.Log("Starting to chat with " + dialogs.DialogueName);
        nameText.text = dialogs.DialogueName;
        phrasesText.text = "...";
        
        sentencesQueue.Clear();
        

        foreach (string sentence in dialogs.sentences.ToList())
        {
            sentencesQueue.Enqueue(sentence);
        }
    }
    
    public void StartDialogue(Dialogs dialogs)
    {
        StartCoroutine(DisplayNextSentence(dialogs));
        
    }

    IEnumerator DisplayNextSentence(Dialogs dialogs)
    {
        //Debug.Log("gonna chat soon");
        if (!dialogs) yield break;
        while (sentencesQueue.Count > 0)
        {
            if (!dialogs.ended) animatorUrsula.SetTrigger("Start");
            while(!(Input.GetKeyDown(KeyCode.Q)||Input.GetButtonDown("Fire1")))
            {
                
                yield return null;
            }

            animatorUrsula.SetTrigger("Next");

            
            //Debug.Log(sentencesQueue.First());
            //Debug.Log(sentencesQueue.Count);
            
            phrasesText.text = sentencesQueue.First();
            sentencesQueue.Dequeue();
            
            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitForSeconds(2);
        dialogs.ended = true;
        EndDialogue();

        //trigger.SetActive(false);
    }

    public void EndDialogue()
    {

        animatorUrsula.SetTrigger("End");
        
        //Debug.Log("Finished chatting.");
        
        StopCoroutine("DisplayNextSentence");
    }
}