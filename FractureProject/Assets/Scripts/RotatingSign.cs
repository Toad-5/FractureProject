using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RotatingSign : MonoBehaviour
{
    [Header("Controller Vibration Settings")]
    [Range(0f, 1f), Tooltip("Vibration lourde")]
    public float lowFrequency;
    [Range(0f, 1f), Tooltip("Vibration légere")]
    public float highFrequency;
    public float rumbleDuration;
    
    [Space]
    
    public UnityEvent onInteraction;
    private bool isPlayerNear;
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerNear = true;
            StartCoroutine(Rumble());
        }
        
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    private void Update()
    {
        if (isPlayerNear)
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("Fire1"))
            {
                onInteraction.Invoke();
            }
        }
    }
    
    private IEnumerator Rumble()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            yield return new WaitForSeconds(rumbleDuration);
            gamepad.PauseHaptics();
        }
    }
}
