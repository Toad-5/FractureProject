using System;
using UnityEngine;

public class GardesAnimationStateHolder : MonoBehaviour
{
    public bool blocking, moving;
    private bool savedMoving, savedBlocking;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();

    }

    private void FixedUpdate()
    {
        if (blocking != savedBlocking)
        {
            savedBlocking = blocking;
            anim.SetBool("Blocking", blocking);
        }

        if (moving != savedMoving)
        {
            savedMoving = moving;
            anim.SetBool("Moving", moving);
        }
    }
}
