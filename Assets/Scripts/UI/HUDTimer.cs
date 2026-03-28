using System;
using TMPro;
using UnityEngine;

public class HUDTimer : MonoBehaviour
{
    Global global = null;

    [SerializeField] TMP_Text text;
    [SerializeField] float finalStretchThreshold = 60f;
    bool triggeredFinalStretch = false;

    bool localIsPaused = true;

    public float RemainingTime { get { return this.RemainingTime; } }
    float remainingTime = 0f;

    public Action TimeOut = null; // called upon time finished
    public Action FinalStretch = null; // called upon time reaching threshold

    private void Update()
    {
        if (global == null || global.IsPaused || localIsPaused) return;

        remainingTime -= Time.deltaTime;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if(remainingTime <= 0f)
        {
            text.text = "00.00";

            TimeOut?.Invoke();
            return;
        }
        if(!triggeredFinalStretch && remainingTime <= finalStretchThreshold)
        {
            triggeredFinalStretch = true;
            FinalStretch?.Invoke();
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        if(minutes > 0)
        {
            text.text = minutes.ToString() + ":" + seconds.ToString("00");
        }
        else
        {
            float milliseconds = remainingTime - Mathf.Floor(remainingTime);

            text.text = seconds.ToString() + milliseconds.ToString(".00");
        }
    }

    public void ManualSetup(float startingTime)
    {
        global = Global.Instance;
        remainingTime = startingTime;
        UpdateDisplay();
    }

    public void ManualStart()
    {
        localIsPaused = false;
    }
}
