using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] CanvasGroup mainCanvas;
    [SerializeField] CanvasGroup creditCanvas;

    [SerializeField] CanvasGroup highScoreCanvas;
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text volumeText;

    private void Awake()
    {
        SetGroup(mainCanvas, true);
        SetGroup(creditCanvas, false);

        SetupScoreCanvas();
    }

    void SetupScoreCanvas()
    {
        if(PlayerPrefs.HasKey(GameSummary.saved_bestTime) == false || PlayerPrefs.HasKey(GameSummary.saved_bestVolume) == false)
        {
            highScoreCanvas.alpha = 0f;
            return;
        }

        highScoreCanvas.alpha = 1f;
        timeText.text = PlayerPrefs.GetFloat(GameSummary.saved_bestTime).ToString() + "s";
        volumeText.text = PlayerPrefs.GetFloat(GameSummary.saved_bestVolume).ToString() + "m<sup>3";
    }

    public void Go_To_StartMenu()
    {
        SetGroup(mainCanvas, true);
        SetGroup(creditCanvas, false);
    }

    public void Go_To_CreditMenu()
    {
        SetGroup(mainCanvas, false);
        SetGroup(creditCanvas, true);
    }
    public void Go_To_Game()
    {
        SceneTransitionHelper.LoadScene("1 - House");
    }

    public void Quit_Game()
    {
        Application.Quit();
    }

    private void SetGroup(CanvasGroup group, bool setOn)
    {
        group.alpha = setOn ? 1 : 0;
        group.blocksRaycasts = setOn;
        group.interactable = setOn;
    }
}
