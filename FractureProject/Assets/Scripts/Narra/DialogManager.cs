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
    public Animator animator;
   
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

    public void StartDialogue(Dialogs dialogs)
    {
        Debug.Log("Starting to chat with " + dialogs.name);
        nameText.text = dialogs.name;
        phrasesText.text = dialogs.sentences[0];
        sentencesQueue.Clear();

        foreach (string sentence in dialogs.sentences.ToList())
        {
            sentencesQueue.Enqueue(sentence);
        }
        
        StartCoroutine(DisplayNextSentence(dialogs));
    }

    IEnumerator DisplayNextSentence(Dialogs dialogs)
    {
        Debug.Log("gonna chat soon");
        
        while(!(Input.GetKeyDown(KeyCode.Space)||Input.GetButtonDown("Fire1")))
        {
            yield return null;
        }

        if (sentencesQueue.Count != 0)
        {
            Debug.Log(dialogs.sentences[0]);
            sentencesQueue.Dequeue();
            Debug.Log(sentencesQueue.Count);
        }
        
        if (sentencesQueue.Count == 0)
        {
            EndDialogue();
            yield return null;
        }
    }

    public void EndDialogue()
    {
        Debug.Log("Finished chatting.");
        StopCoroutine("DisplayNextSentence");
    }
}
