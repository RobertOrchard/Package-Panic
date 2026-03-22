using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionHelper : MonoBehaviour
{
    [SerializeField] CanvasGroup outCanvas;
    [SerializeField] Slider loadSlider;
    [SerializeField] CanvasGroup inCanvas;

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
        float remaining = inDuration;

        while(remaining > 0f)
        {
            remaining -= Time.deltaTime;
            inCanvas.alpha = remaining / inDuration;
            yield return null;
        }
        inCanvas.alpha = 0f;

        state = TransitionState.Done;
        DoneLoadingIn?.Invoke();
    }


    enum TransitionState
    {
        Out,
        In,
        Done,
    }
}
