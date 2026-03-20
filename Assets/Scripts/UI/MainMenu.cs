using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] CanvasGroup mainCanvas;
    [SerializeField] CanvasGroup creditCanvas;

    private void Awake()
    {
        SetGroup(mainCanvas, true);
        SetGroup(creditCanvas, false);
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
        SceneManager.LoadScene("1 - House");
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
