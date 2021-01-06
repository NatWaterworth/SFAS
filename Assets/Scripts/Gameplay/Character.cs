using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Character : MonoBehaviour
{
    protected Rigidbody rb;
    protected Animator animator;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
    

        if (GetComponent<Animator>() != null)
        {
            animator = GetComponent<Animator>();
        }
        else
            Debug.LogWarning("Animator has not been set for " + this.gameObject);
    }

    /// <summary>
    /// Sets agents animator variables so an appropraite animation can be selected.
    /// </summary>
    protected virtual void Animate()
    {

    }
}
