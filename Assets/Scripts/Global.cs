using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Global : MonoBehaviour
{
    public static Global Instance = null;

    UniversalRenderPipelineAsset urpAsset;
    InputSystem_Actions input;

    public static readonly string PackObjectTag = "Packable";
    public static float Gravity = 9.81f;

    public bool IsPaused { 
        get { return isPaused; } 
        set { PauseChange(value); } 
    }
    bool isPaused = false;

    public float PlayerScale { get { return playerScale; } 
        set { ScaleChanged(value); playerScale = value; } }
    float playerScale = 1f;

    public Vector3 GlobalForward = Vector3.forward;
    public Vector3 GlobalRight = Vector3.right;

    // Setup Instance
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        input = new();
        input.Enable();

        Cursor.lockState = CursorLockMode.Locked;

        SceneManager.sceneLoaded += SceneChanged;
    }

    // used for initializing values
    private void Start()
    {
        ReorientWorld();
    }

    void SceneChanged(Scene _scene, LoadSceneMode _mode)
    {
        IsPaused = false;
    }

    #region Update
    private void Update()
    {
        PauseCheck();
    }

    private void PauseCheck()
    {
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
        if (PauseScreen.Instance != null) PauseScreen.Instance.TogglePause(newState);
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
}
