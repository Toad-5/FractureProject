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
   
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            Debug.Log("the canvas just fucking died");
        }
        instance = this;
        sentencesQueue = new Queue<string>();
        Debug.Log(sentencesQueue.Count);
    }

    public void InitiateDialogue(Dialogs dialogs)
    {
        Debug.Log("Starting to chat with " + dialogs.name);
        nameText.text = dialogs.name;
        phrasesText.text = dialogs.sentences[0];
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
        Debug.Log("gonna chat soon");

        while (sentencesQueue.Count > 0)
        {
            while(!(Input.GetKeyDown(KeyCode.Space)||Input.GetButtonDown("Fire1")))
            {
                animatorUrsula.SetBool("skipped", false);
                animatorUrsula.SetBool("reading", true);
 
                yield return null;
            }
            animatorUrsula.SetBool("reading", false);
            animatorUrsula.SetBool("skipped", true);
            Debug.Log(sentencesQueue.First());
            phrasesText.text = sentencesQueue.First();
            sentencesQueue.Dequeue();
            Debug.Log(sentencesQueue.Count);
            yield return new WaitForEndOfFrame();
        }
        EndDialogue();
        
    }

    public void EndDialogue()
    {
        animatorUrsula.SetBool("reading", false);
        animatorUrsula.SetBool("skipped", false);
        animatorUrsula.SetBool("finished", true);
        Debug.Log("Finished chatting.");
        StopCoroutine("DisplayNextSentence");
    }
}
