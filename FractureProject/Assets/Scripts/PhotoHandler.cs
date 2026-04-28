using System;
using UnityEngine;

public class PhotoHandler : MonoBehaviour
{
    private SceneManager sceneManager;
    private Action OnTransition;
    
    private bool playerNear;
    
    bool isSpaceUp;

    private void Start()
    {
        sceneManager = SceneManager.Instance;
        Debug.Log(sceneManager);
        OnTransition += sceneManager.Temp;
    }
    
    private void Update()
    {
        if (!playerNear) return;

        //playerNear Feedback
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
        {
            OpenPhoto();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNear = false;
        }
    }

    void OpenPhoto()
    {
        Time.timeScale = 0;
        // Call photo UI
    }
    
    void CallTransition() //Call with UI button
    {
        Time.timeScale = 1;
        OnTransition?.Invoke();
    }
}
