using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [SerializeField] private GameObject firstPauseButton;
    [SerializeField] private GameObject firstPhotoButton;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetPauseButton()
    {
        if (!firstPauseButton) return;
        
        EventSystem.current.SetSelectedGameObject(firstPauseButton);
    }

    public void SetPhotoButton()
    {
        if (!firstPhotoButton) return;
        
        EventSystem.current.SetSelectedGameObject(firstPhotoButton);
    }

    public void RemoveFirstSelectedButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
