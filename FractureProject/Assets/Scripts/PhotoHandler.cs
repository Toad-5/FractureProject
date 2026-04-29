using System;
using UnityEngine;

public class PhotoHandler : MonoBehaviour
{
    [SerializeField] private UIPhoto uiPhoto;
    
    private bool playerNear;

    private void Start()
    {
        uiPhoto = GameObject.Find("UI Photo").GetComponent<UIPhoto>();
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

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNear = false;
        }
    }

    private void OpenPhoto()
    {
        uiPhoto.ShowUI(true);
    }
}
