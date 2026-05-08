using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Dialogs : MonoBehaviour
{
    [FormerlySerializedAs("name")] public string DialogueName;
        
        [TextArea(3, 10)]
    public string[] sentences;
    public bool ended;

    public void PlayDialogue()
    {
        if (ended) return;
        DialogManager.instance.StartDialogue(this);
        DialogManager.instance.InitiateDialogue(this);
    }
}
