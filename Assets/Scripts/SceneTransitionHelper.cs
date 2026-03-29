using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionHelper : MonoBehaviour
{
    [SerializeField] CanvasGroup outCanvas;
    [SerializeField] Slider loadSlider;
    [SerializeField] CanvasGroup inCanvas;

    [SerializeField] CanvasGroup countdownCanvas;
    [SerializeField] bool runCountdown = true;
    [SerializeField] TMP_Text countdownText;
    [SerializeField] float maxCountdownSize = 48f;
    [SerializeField] float minCountdownSize = 24f;

    public static SceneTransitionHelper Instance;
    TransitionState state = TransitionState.Done;

    public Action DoneLoadingIn;
    public bool loaded = false;

    [SerializeField] float inDuration = 1.5f; // how long to transition in to scene

    private void Awake()
    {
        DoneLoadingIn += 
            () => loaded = true; // sets bool so scripts can know if the action has already been invoked

        if (Instance != null && Instance != this)
        {
            // if level transitioned out, should have the transition in effect
            if (Instance.state == TransitionState.Out) 
                StartCoroutine(InTransition());

            Destroy(Instance.gameObject);
        }

        Instance = this;
        transform.parent = null;
        DontDestroyOnLoad(gameObject);

        if (state == TransitionState.Done) DoneLoadingIn?.Invoke();
    }

    // if instance is null, just routes to SceneManager
    public static void LoadScene(string sceneName)
    {
        if(Instance == null)
        {
            Debug.LogError("Could Not Find SceneTransitionHelper Instance");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Instance.StartCoroutine(Instance.OutTransition(sceneName));
        }
    }

    IEnumerator OutTransition(string sceneName)
    {
        state = TransitionState.Out;
        outCanvas.alpha = 1f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            loadSlider.value = Mathf.Clamp01(operation.progress / .9f);
            yield return null;
        }
    }

    IEnumerator InTransition()
    {
        state = TransitionState.In;
        inCanvas.alpha = 1f;
        if(runCountdown) countdownCanvas.alpha = 1f;
        float remaining = inDuration;
        int floorTime = Mathf.FloorToInt(remaining);
        countdownText.text = floorTime.ToString();

        while (remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            inCanvas.alpha = remaining / inDuration;

            if(floorTime != Mathf.FloorToInt(remaining))
            {
                floorTime = Mathf.FloorToInt(remaining);
                countdownText.text = (floorTime + 1).ToString();
                // play some sfx or sumting
            }
            countdownText.fontSize = Mathf.Lerp(minCountdownSize, maxCountdownSize, floorTime > 0 ? remaining % floorTime : remaining);

            yield return null;
        }
        inCanvas.alpha = 0f;
        countdownCanvas.alpha = 0f;

        state = TransitionState.Done;
        DoneLoadingIn?.Invoke();
        DoneLoadingIn = null;
    }


    enum TransitionState
    {
        Out,
        In,
        Done,
    }
}
