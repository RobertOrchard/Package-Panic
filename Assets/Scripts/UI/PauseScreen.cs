using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    public static PauseScreen Instance;

    [SerializeField] CanvasGroup mainPanelGroup;
    [SerializeField] CanvasGroup confirmationPanelGroup;

    [SerializeField] string mainMenuName;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetGroupValues(mainPanelGroup, false);
        SetGroupValues(confirmationPanelGroup, false);
    }

    public void TogglePause(bool state)
    {
        SetGroupValues(mainPanelGroup, state);
        SetGroupValues(confirmationPanelGroup, false);
    }

    #region Buttons
    public void Resume()
    {
        if (Global.Instance == null) return;

        Global.Instance.IsPaused = false;
    }

    // open popup
    public void QuitRequest()
    {
        SetGroupValues(confirmationPanelGroup, true);
        SetGroupValues(mainPanelGroup, false);
    }

    // popup closed
    public void RequestDenied()
    {
        SetGroupValues(confirmationPanelGroup, false);
        SetGroupValues(mainPanelGroup, true);
    }

    public void QuitConfirm()
    {
        // quit to mainmenu
        SceneTransitionHelper.LoadScene(mainMenuName);
    }
    #endregion


    void SetGroupValues(CanvasGroup group, bool turnOn)
    {
        group.alpha = turnOn ? 1f : 0f;
        group.blocksRaycasts = turnOn;
        group.interactable = turnOn;
    }
}
