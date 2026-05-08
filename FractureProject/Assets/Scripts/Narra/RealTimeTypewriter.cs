using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RealTimeTypewriter : MonoBehaviour
{
    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] [TextArea] private List<string> textToDisplay = new List<string>();
    [SerializeField] private float typingSpeed = 0.5f;
    private float t = 0f;
    private bool hasToWrite = true;

    private int i;
    private int j;

    private char endCharacter;

    void Start()
    {
        textDisplay.text = "";
    }

    void FixedUpdate()
    {
        if (hasToWrite)
        {
            Countdown(false);
        }
    }

    private void Countdown(bool menuAnimation)
    {
        t -= Time.fixedDeltaTime;

        if (t <= 0)
        {
            t = typingSpeed;
            if (!menuAnimation)
            {
                Write();
            }
        }
    }

    private void Write()
    {
        char character = textToDisplay[i][j];

        if (character == '¤') // Allow pauses in tht typing process
        {

        }
        else if (character == 'µ')
        {
            SelectorAnimation();
        }
        else
        {
            textDisplay.text += character;
        }

        j++;
        if (j == textToDisplay[i].Length)
        {
            j = 0;
            i++;
            if (i == textToDisplay.Count)
            {
                hasToWrite = false;
                endCharacter = '.';
                typingSpeed *= 4;
            }
            else
            {
                textDisplay.text = "█";
            }
        }
    }

    void SelectorAnimation()
    {
        string _tempTxt;
        if (textDisplay.text.Length == 0)
        {
            return;
        }

        if (textDisplay.text[^1] != '█')
        {
            textDisplay.text += '█';
        }
        else
        {
            _tempTxt = textDisplay.text.Remove(textDisplay.text.Length - 1);
            textDisplay.text = _tempTxt;
        }
    }
}