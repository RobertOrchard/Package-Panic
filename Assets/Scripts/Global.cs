using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Global : MonoBehaviour
{
    public static Global Instance = null;

    UniversalRenderPipelineAsset urpAsset;
    InputSystem_Actions input;

    public LevelData levelData;
    [SerializeField] PackObject stretchGameObject;
    [SerializeField] GameSummary gameSummary;

    public static readonly string PackObjectTag = "Packable";
    public static float Gravity = 9.81f;

    public bool IsPaused { 
        get { return isPaused; } 
        set { PauseChange(value); } 
    }
    bool isPaused = false;
    bool triggerPauseScreen = true;

    public float PlayerScale { get { return playerScale; } 
        set { ScaleChanged(value); playerScale = value; } }
    float playerScale = 1f;

    public Vector3 GlobalForward = Vector3.forward;
    public Vector3 GlobalRight = Vector3.right;

    public Action EndLevel;
    public bool levelEnded = false;

    // Setup Instance
    private void Awake()
    {
        levelData.stretchTarget = stretchGameObject.gameObject;
        levelData.targetReached = false;
        levelData.stretchReached = false;

        if (stretchGameObject)
        {
            LevelTarget target = stretchGameObject.gameObject.AddComponent<LevelTarget>();
            target.Collected += () =>
            {
                levelData.stretchReached = true;
                if (!levelEnded) EndLevel?.Invoke();
            };
        }

        if (Instance != null)
        {
            Instance.levelData = levelData;
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        input = new();
        input.Enable();

        SceneManager.sceneLoaded += SceneChanged;
    }

    IEnumerator DelayedStart()
    {
        yield return null;

        if (HUDManager.Instance)
        {
            HUDManager.Instance.timer.TimeOut += TimeUp;
        }
    }

    void TimeUp()
    {
        if (!levelEnded) EndLevel?.Invoke();
    }

    // used for initializing values
    private void Start()
    {
        ReorientWorld();
    }

    void SceneChanged(Scene _scene, LoadSceneMode _mode)
    {
        IsPaused = false;
        //Debug.Log("Global - SceneLoaded");
        triggerPauseScreen = false;
        Cursor.lockState = CursorLockMode.Confined;

        if (SceneTransitionHelper.Instance != null && !SceneTransitionHelper.Instance.loaded && SceneTransitionHelper.Instance.runCountdown)
        {
            Cursor.lockState = CursorLockMode.Locked;

            IsPaused = true;
            SceneTransitionHelper.Instance.DoneLoadingIn += () =>
            {
                triggerPauseScreen = true;
                IsPaused = false;
            };
        }

        urpAsset.shadowDistance = 50f;

        levelEnded = false;

        EndLevel = null;
        EndLevel += EndStage;

        StartCoroutine(DelayedStart());
    }

    void EndStage()
    {
        levelEnded = true;

        Time.timeScale = 0f;

        if(!gameSummary) return;

        Cursor.lockState = CursorLockMode.Confined;

        float _time = (HUDManager.Instance.timer.RemainingTime > 0f) ? HUDManager.Instance.mapDuration - HUDManager.Instance.timer.RemainingTime : HUDManager.Instance.mapDuration;

        GameSummary.SummaryScore _score = GameSummary.SummaryScore.Failed;
        if (levelData.targetReached) _score = GameSummary.SummaryScore.Cleared;
        if(levelData.stretchReached) _score = GameSummary.SummaryScore.Perfect;

        gameSummary.RunSummary(curVolume, _time, _score);
    }

    #region Update
    private void Update()
    {
        PauseCheck();
    }

    private void PauseCheck()
    {
        if (!triggerPauseScreen) return;
        if (!input.Player.Pause.WasPressedThisFrame()) return;
        
        IsPaused = !isPaused; // toggle using the setter calls function
    }
    #endregion

    void PauseChange(bool newState)
    {
        isPaused = newState;

        if (isPaused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (PauseScreen.Instance != null && triggerPauseScreen) PauseScreen.Instance.TogglePause(newState);
    }

    private void OnDestroy()
    {
        input?.Disable();

        if (urpAsset != null) 
            urpAsset.shadowDistance = 50f;
    }

    // recalculates world directions for the player to move along
    public void ReorientWorld()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        GlobalForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        GlobalRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        //Debug.Log("New global axes:" + " Forward = " + GlobalForward + " Right = " + GlobalRight);
    }

    private void ScaleChanged(float _value)
    {
        urpAsset.shadowDistance = _value * 50f;
    }

    float curVolume = 0f;
    public void NewPlayerVolume(float _volume)
    {
        curVolume = _volume;

        if (!levelData.targetReached && _volume < levelData.targetVolume) return;

        levelData.targetReached = true;

        if(!levelData.stretchReached && _volume < levelData.stretchVolume) return;

        levelData.stretchReached = true;
        if (!levelEnded) EndLevel?.Invoke();
        float time = HUDManager.Instance.timer.RemainingTime;
    }
}
