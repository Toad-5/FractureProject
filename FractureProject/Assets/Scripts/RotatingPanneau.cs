using System;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPanneau : MonoBehaviour
{
    public float origin;
    public float target;

    public List<float> angles;
    
    public Animator anim;
    public Animator lineAnimator;
    
    private void Start()
    {
        anim = GetComponent<Animator>();

    }

    public void Turn()
    {
        anim.SetFloat("Before",origin);
        lineAnimator.SetFloat("Before",origin);
        
        SoundManager.PlaySound("Sign Flip");
        
        int index = angles.IndexOf(origin);
        if (index + 1 > angles.Count -1) index = 0;
        else index++;
        Debug.Log(index + " " + angles.Count);

        target = angles [index];
        
        anim.SetFloat("After",target);
        lineAnimator.SetFloat("After",target);

        anim.SetTrigger("Rotate");
        lineAnimator.SetTrigger("Rotate");
        
        origin = target;
    }
}
