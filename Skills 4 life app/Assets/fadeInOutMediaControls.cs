using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fadeInOutMediaControls : MonoBehaviour
{
    public CanvasGroup mediaControls;

    private bool fadeIn = false;
    private bool fadeOut = false;

    float timer = 0.0f;
    float seconds = 0f;

    public void ShowMedia()
    {
        fadeIn = true;
    }

    public void HideMedia()
    {
        fadeOut = true;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        seconds = timer % 60;

        if (Input.anyKeyDown && mediaControls.alpha == 0)
        {
            fadeIn = true;
        }

        if (mediaControls.alpha >= 1 && seconds >= 6)
        {
            fadeOut = true;
        }

        if (fadeIn)
        {
            if (mediaControls.alpha < 1)
            {
                mediaControls.alpha += Time.deltaTime;
                if (mediaControls.alpha >= 1)
                {
                    fadeIn = false;
                    seconds = 0;
                    timer = 0;
                }
            }
        }

        if (fadeOut)
        {
            if (mediaControls.alpha <= 1)
            {
                mediaControls.alpha -= Time.deltaTime;
                if (mediaControls.alpha == 0)
                {
                    fadeOut = false;
                }
            }
        }
    }
}
