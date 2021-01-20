using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used for transitioning between states. Masks any changes with complete blackness.
/// </summary>
public class Transitioner : MonoBehaviour
{
    [SerializeField] Image fadeImage;

    float transitionTime = 1f;
    float minTransitionTime = 0.1f;
    float transitionRefreshRate = 0.05f;
    float minRefreshRate = 0.01f;
    float alphaChangePerUpdate = 0.05f;
    float transitionImageOpaqueTime = 0.0f;

    bool transitioning;

    /// <summary>
    /// Fades the screen to black and back to normal. Used for transitioning between states.
    /// </summary>
    /// <param name="_transitionTime">Total duration over which the transition occurs.</param> 
    /// <param name="_transitionRefreshRate">Rate at which the transition updates.</param> 
    /// <param name="_fullyMaskedTime">Time the transition remains opaque to mask behind the scene work.</param> 
    /// <param name="_realtime">Does the transition occur in realtime (is it affected by timescale).</param>
    public void MaskTransition(float _transitionTime, float _transitionRefreshRate, float _fullyMaskedTime, bool _realtime)
    {
        if (transitioning)
            return;

        //ensure a valid values are chosen for refresh rate and transitioning times.
        _transitionRefreshRate = Mathf.Max(_transitionRefreshRate, minRefreshRate);
        _transitionTime = Mathf.Max(_transitionTime, minTransitionTime);
        _fullyMaskedTime = Mathf.Max(_fullyMaskedTime, 0);

        //Set values
        transitionTime = _transitionTime;
        transitionRefreshRate = _transitionRefreshRate;
        transitionImageOpaqueTime = _fullyMaskedTime;

        //calculate update speed with the opaque time taken into account so overall transition time is correct.
        alphaChangePerUpdate = transitionRefreshRate / (transitionTime - transitionImageOpaqueTime) ;
        alphaChangePerUpdate *= 2; //The changes have to occur fading too and from opaqueness.

        if (_realtime)
            StartCoroutine(TransitionScaledTime());
        else
            StartCoroutine(TransitionUnscaledTime());
    }

    IEnumerator TransitionUnscaledTime()
    {
        float alpha = 0;

        transitioning = true;

        while (alpha < 1)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return new WaitForSeconds(transitionRefreshRate);
            alpha += alphaChangePerUpdate;
        }

        //ensure image is fully opaque.
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
        yield return new WaitForSeconds(transitionImageOpaqueTime);

        while (alpha > 0)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return new WaitForSeconds(transitionRefreshRate);
            alpha -= alphaChangePerUpdate;
        }

        //ensure image is fully transparent.
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);

        transitioning = false;

        yield return null;
    }

    IEnumerator TransitionScaledTime()
    {
        float alpha = 0;

        transitioning = true;

        while (alpha < 1)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return new WaitForSecondsRealtime(transitionRefreshRate);
            alpha += alphaChangePerUpdate;
        }

        //ensure image is fully opaque.
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
        yield return new WaitForSecondsRealtime(transitionImageOpaqueTime);

        while (alpha > 0)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return new WaitForSecondsRealtime(transitionRefreshRate);
            alpha -= alphaChangePerUpdate;
        }

        //ensure image is fully transparent.
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);

        transitioning = false;

        yield return null;
    }
}
