using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance { get; private set; }
 
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject tempText;
    
    private InputAction pauseAction;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        pauseAction = GetComponent<PlayerInput>().actions["Pause"];
        pauseAction.actionMap.Enable();
        pauseAction.performed += OnPause;
    }

    private void OnEnable()
    {
        pauseAction.performed += OnPause;
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPause;
    }
    
    private void Start()
    {
        if (!pauseMenuPanel) return;
        
        pauseMenuPanel.SetActive(false);
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (!pauseMenuPanel) return;
        
        Time.timeScale = 0;
        Player.instance.locked = true;
        UIManager.instance.SetPauseButton();
        pauseMenuPanel.SetActive(true);
        
        Debug.Log(Player.instance.locked);
    }

    public void Resume()
    {
        if (!pauseMenuPanel) return;
        
        Time.timeScale = 1;
        pauseMenuPanel.SetActive(false);
        UIManager.instance.RemoveFirstSelectedButton();
    }

    public void Settings()
    {
        //TODO: Settings Menu
        
        if (!pauseMenuPanel) return;
        
        tempText.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Application Quit");
    }
}
