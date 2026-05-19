using System;
using UnityEngine;

public class UIPhoto : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    
    private SceneManager sceneManager;
    
    private Action OnTransition;
    
    private void Start()
    {
        sceneManager = SceneManager.instance;
        
        OnTransition += sceneManager.Temp;
    }
    
    public void ShowUI(bool isActive)
    {
        if (isActive)
        {
            panel.SetActive(true);
            Player.instance.locked = true;
            UIManager.instance.SetPhotoButton();
        }
        else
        {
            panel.SetActive(false);
        }
    }
    
    public void CallTransition() //Call with UI button
    {
        Time.timeScale = 1;
        Debug.Log("OnTransition Invoke");
        OnTransition?.Invoke();
        panel.SetActive(false);
    }
}
