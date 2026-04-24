using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SucessionOfEvents : MonoBehaviour
{
    [SerializeField] private string tagToTrigger;
    [SerializeField] private bool isTriggeredOnEnter,isTriggeredOnExit,isTriggeredOnAwake;
    [SerializeField] private List<TimedEvent> events;
    private Coroutine eventsCoroutine;
    

    public void PlayEvents()
    {
        if(eventsCoroutine != null) StopCoroutine(eventsCoroutine);
        eventsCoroutine= StartCoroutine(EventPlay(events));
    }

    private IEnumerator EventPlay(List<TimedEvent> timedEvents)
    {
        foreach (TimedEvent e in events)
        { 
            float timer = e.timeBeforeNextEvent;
            e.ExecuteEvent(e.type);
            while (timer > 0)
            {
                timer-=Time.deltaTime;
                yield return null;
            }
        }
        yield return null;
    }
    #region triggers
    private void Awake()
    {
        if(isTriggeredOnAwake) PlayEvents();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggeredOnEnter && other.CompareTag(tagToTrigger)) PlayEvents();
    }

    private void OnTriggerExit(Collider other)
    {
        if (isTriggeredOnExit && other.CompareTag(tagToTrigger)) PlayEvents();
    }
    #endregion
}

[System.Serializable]
public class TimedEvent
{
    public EventType type;
    [Header ("Parameters")]
    public UnityEvent unityEvent;
    public Transform cameraTargetPoint;
    
    [Header ("Time")]
    public float timeBeforeNextEvent;
    
    private delegate void EventDelegate();
    private EventDelegate eventDelegate;
    
    public enum EventType
    {
        UnityEvent,
        CameraMovement,
        ObjectMovement,
        Dialogue
    }
    
    public void ExecuteEvent(EventType eventType)
    {
        switch (eventType)
        {
            case EventType.UnityEvent:
                eventDelegate += EventUnity;
                break;
            case EventType.CameraMovement:
                eventDelegate += CameraMovement;
                break;
            case EventType.ObjectMovement:
                eventDelegate += ObjectMovement;
                break;
            case EventType.Dialogue:
                eventDelegate += Dialogue;
                break;
            default:
                Debug.LogWarning("Unknown event type: " + eventType);
                break;
        }
        eventDelegate();
        eventDelegate = null;
    }

    public void EventUnity()
    {
        unityEvent.Invoke();   
    }

    public void CameraMovement()
    {
        IsometricCameraFollow.instance.ChangeTarget(cameraTargetPoint);
    }

    public void ObjectMovement()
    {
        
    }

    public void Dialogue()
    {
        
    }
    
}
