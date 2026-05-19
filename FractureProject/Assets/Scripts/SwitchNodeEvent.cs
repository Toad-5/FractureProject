using System;
using UnityEngine;

public class SwitchNodeEvent : MonoBehaviour
{
    private SwitchCrowdNode node;

    private Action onSwitch;

    public void Bind(SwitchCrowdNode node, Crowd crowd)
    {
        this.node = node;
        onSwitch += () => crowd.RefreshCrowdStates();
    }

    public void SwitchEvent(int amount)
    {
        if (node != null)
            node.Switch(amount);
        
        onSwitch?.Invoke();
    }
    
    [ContextMenu("Trigger")]
    public void TriggerSwitch() => SwitchEvent(1);
}