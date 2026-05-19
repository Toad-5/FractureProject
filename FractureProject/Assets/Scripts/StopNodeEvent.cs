using System;
using UnityEngine;

public class StopNodeEvent : MonoBehaviour
{
    private StopCrowdNode node;
    
    private Action onStop;

    public void Bind(StopCrowdNode node, Crowd crowd)
    {
        this.node = node;
        onStop += () => crowd.RefreshCrowdStates();
    }
    
    public void SetStop(bool stop)
    {
        if (node != null)
            node.isStopped = stop;
        
        onStop?.Invoke();
    }
    
    [ContextMenu("Trigger")]
    public void TriggerStop() => SetStop(!node.isStopped);
}