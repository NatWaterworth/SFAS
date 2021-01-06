using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : Character
{

    [SerializeField]
    float moveSpeed;

    Vector3 lookDir;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //set default direction to look forward 
        lookDir = transform.forward;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
        Animate();
    }

    void Move()
    {
        //Move player across plane.
        Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        rb.velocity = inputVector * moveSpeed;
        if (!inputVector.Equals(Vector3.zero))
            lookDir = inputVector;
        transform.forward = lookDir;
    }

    protected override void Animate()
    {
        base.Animate();

        if (animator != null)
        {
            animator.SetFloat("moveSpeed", Mathf.Clamp01(rb.velocity.magnitude / moveSpeed)); //requires moveSpeed varirable in animator controller
        }
    }
}
