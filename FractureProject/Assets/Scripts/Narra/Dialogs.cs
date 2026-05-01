using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogs : MonoBehaviour
    {
        public string name;
        
        [TextArea(3, 10)]
        public string[] sentences;

        public void PlayDialogue()
        {
            DialogManager.instance.StartDialogue(this);
            DialogManager.instance.InitiateDialogue(this);
        }
    }
