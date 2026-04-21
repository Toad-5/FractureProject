using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    private Queue<string> sentences;
    public TriggerEvent TriggerEvent;
    public static DialogManager instance;
    public TMPro.TMP_Text nameText;
    public TMPro.TMP_Text phrasesText;
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            Debug.Log("the canvas just fucking died");
        }
        instance = this;
        
        sentences = new Queue<string>();
        Debug.Log(sentences.Count);
    }

    public void StartDialogue(Dialogs dialogs)
    {
        Debug.Log("Starting to chat with " + dialogs.name);
        nameText.text = dialogs.name;
        sentences.Clear();

        foreach (string sentence in dialogs.sentences)
        {
            sentences.Enqueue(sentence);
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
        
        Debug.Log(dialogs.sentences[0]);
        sentences.Dequeue();
        Debug.Log(sentences.Count);

        if (sentences.Count == 0)
        {
            EndDialogue();
            yield return null;
        }
    }

    public void EndDialogue()
    {
        StopCoroutine("DisplayNextSentence");
        Debug.Log("Finished chatting.");
    }
}
