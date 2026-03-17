using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject Main_Canvas;
    [SerializeField] GameObject Credit_Canvas;

    public void Go_To_StartMenu()
    {
        Credit_Canvas.SetActive(false);
        Main_Canvas.SetActive(true);
    }

    public void Go_To_CreditMenu()
    {
        Main_Canvas.SetActive(false);
        Credit_Canvas.SetActive(true);
    }
    public void Go_To_Game()
    {
        SceneManager.LoadScene("1 - House");
    }

    public void Quit_Game()
    {
        Application.Quit();
    }

}
