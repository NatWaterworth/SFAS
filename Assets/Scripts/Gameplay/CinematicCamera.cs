using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera controller based on using the Unity Animator Controller to create cinematics.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CinematicCamera : CameraController
{
     Animator anim;
    [SerializeField] string cinematicStateName;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //Set animator
        if (GetComponent<Animator>() != null)
            anim = GetComponent<Animator>();
        else
            Debug.LogError("Cinematic Camera: " + this + " doesn't have an animator to use.");

        PlayCinematic();
    }

    void PlayCinematic()
    {
        if (anim != null)
        {
            anim.Play("Base Layer." + cinematicStateName, 0);
        }
        else
            Debug.LogError("Cinematic Camera: " + this + " doesn't have a cinematic to play!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetCinematicTime()
    {
        if (anim != null)
        {
            if(anim.GetCurrentAnimatorStateInfo(0).IsName(cinematicStateName))
                return anim.GetCurrentAnimatorStateInfo(0).length;
            else
                Debug.LogWarning("Cinematic Camera: " + this + " is in an unexpected state.");
        }
        else
        {
            Debug.LogError("Cinematic Camera: " + this + " doesn't have an animator to use.");
        }
    return 0;
    }
}
