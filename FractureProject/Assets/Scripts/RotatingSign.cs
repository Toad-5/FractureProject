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
    public SpriteRenderer lineSignRenderer;

    private RotatingPanneau visuel;
    
    [Space]


    public UnityEvent onInteraction;

    private bool isPlayerNear, cooldown;
    private void Start()
    {
        visuel = GetComponent<RotatingPanneau>();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerNear = true;
            lineSignRenderer.color = new Color(1f, 1f, 1f, 1f);
            
            StartCoroutine(Rumble());
            SoundManager.PlaySound("Interact In");
        }
    }
    

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerNear = false;
            SoundManager.PlaySound("Interact Out");

        }

    }

    private void Update()
    {
        if (isPlayerNear)
        {
            lineSignRenderer.color = new Color(1f, 1f, 1f, 1f);
            
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("Fire1"))
            {
                if (cooldown) return;
                cooldown = true;
                StartCoroutine(Cooldown());
                onInteraction.Invoke();
                visuel.Turn();
            }
        }
    }

    public IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(0.5f);
        {
            cooldown = false;
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
