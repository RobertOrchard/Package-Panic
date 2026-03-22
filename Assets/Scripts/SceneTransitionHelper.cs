using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHelper : MonoBehaviour
{
    [SerializeField] Canvas outCanvas;
    [SerializeField] Canvas inCanvas;

    public static SceneTransitionHelper Instance;
    TransitionState state = TransitionState.Done;

    public Action DoneLoadingIn;
    public bool loaded = false;

    [SerializeField] float inDuration = 1f; // how long to transition in to scene

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
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            yield return null;
        }
    }

    IEnumerator InTransition()
    {
        state = TransitionState.In;
        float prog = 0f;

        while(prog < inDuration)
        {
            prog += Time.deltaTime;
            yield return null;
        }

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
