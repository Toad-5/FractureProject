using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RealTimeTypingTextDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] [TextArea] private List<string> textToDisplay =  new List<string>();
    [SerializeField] private float typingSpeed = 0.5f;
    [SerializeField] private bool isMainMenu;
    private float t = 0f;
    private bool hasToWrite = true;
    public bool isMainMenuEndAnimation;
    public bool isTutorial;
    public bool isTutorialOver;

    private int i;
    private int j;
    
    private char endCharacter;

    void Start()
    {
        if (!isMainMenu)
        {
            endCharacter = '█';
            textDisplay.text = endCharacter.ToString();
        }
        else
        {
            textDisplay.text = "";
        }
    }
    
    void FixedUpdate()
    {
        if (hasToWrite)
        {
            Countdown(false);
        }

        if (isMainMenuEndAnimation)
        {
            Countdown(true);
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
            else
            {
                MainMenuEndAnimation(endCharacter);
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
            if (!isMainMenu)
            {
                var tempTxt = textDisplay.text.Remove(textDisplay.text.Length-1);
                textDisplay.text = tempTxt + character + '█';
            }
            else
            {
                textDisplay.text += character;
            }
        }

        j++;
        if (j == textToDisplay[i].Length)
        {
            j = 0;
            i++;
            if (i == textToDisplay.Count)
            {
                hasToWrite = false;
                if (!isMainMenu)
                {
                    typingSpeed *= 6;
                }
                else
                {
                    endCharacter = '.';
                    typingSpeed *= 4;
                }
                
                isMainMenuEndAnimation = true;
            }
            else
            {
                textDisplay.text = "█";
            }
        }
    }

    private void SelectorAnimation()
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
            _tempTxt = textDisplay.text.Remove(textDisplay.text.Length-1);
            textDisplay.text = _tempTxt;
        }
    }

    private void MainMenuEndAnimation(char character)
    {
        string _tempTxt;
        if (textDisplay.text[^1] != character)
        {
            textDisplay.text += character;
        }
        else
        {
            _tempTxt = textDisplay.text.Remove(textDisplay.text.Length-1);
            textDisplay.text = _tempTxt;
        }
    }
}