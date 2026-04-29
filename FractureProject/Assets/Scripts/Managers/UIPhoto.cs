using System;
using UnityEngine;

public class UIPhoto : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    
    private SceneManager sceneManager;
    
    private Action OnTransition;
    
    private void Start()
    {
        sceneManager = SceneManager.Instance;
        
        OnTransition += sceneManager.Temp;
    }
    
    public void ShowUI(bool isActive)
    {
        if (isActive)
        {
            panel.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            panel.SetActive(false);
        }
    }
    
    public void CallTransition() //Call with UI button
    {
        Time.timeScale = 1;
        OnTransition?.Invoke();
        panel.SetActive(false);
    }
}
