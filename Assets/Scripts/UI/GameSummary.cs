using System.Collections;
using TMPro;
using UnityEngine;

public class GameSummary : MonoBehaviour
{
    [SerializeField] CanvasGroup cGroup;
    [SerializeField] TMP_Text volumeText;
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text scoreText;

    [SerializeField] float typeDelay = 0.05f;

    float volume = 0f;
    float time = 0f;
    SummaryScore score;

    #region strings
    static string volume_message = "volume: ";
    static string time_message = "time: ";

    static string score_fail_message = "FIRED";
    static string score_pass_message = "CLEAR";
    static string score_perfect_message = "PROMOTION!!!";

    public static string saved_bestVolume = "savedata-bestvolume";
    public static string saved_bestTime = "savedata-besttime";
    #endregion

    private void Awake()
    {
        cGroup.alpha = 0f;
    }

    public void RunSummary(float _volume, float _time, SummaryScore _score)
    {
        volume = _volume;
        time = _time;
        score = _score;

        volumeText.text = "";
        timeText.text = "";
        scoreText.text = "";

        cGroup.alpha = 1f;
        StartCoroutine(Summary());

        if (PlayerPrefs.GetFloat(saved_bestVolume, 0f) < volume)
        {
            PlayerPrefs.SetFloat(saved_bestVolume, volume);
        }
        if (PlayerPrefs.GetFloat(saved_bestTime, 0f) > time)
        {
            PlayerPrefs.SetFloat(saved_bestTime, time);
        }
    }

    IEnumerator Summary()
    {
        string finalVolumeText = volume_message + volume.ToString() + "m<sup>3";
        yield return StartCoroutine(PrintText(finalVolumeText, volumeText));

        string finalTimeText = time_message + time.ToString() + "s";
        yield return StartCoroutine(PrintText(finalTimeText, timeText));

        string finalScoreText;
        switch (score)
        {
            case SummaryScore.Failed:
                finalScoreText = score_fail_message;
                break;
            case SummaryScore.Cleared:
                finalScoreText = score_pass_message;
                break;
            case SummaryScore.Perfect:
                finalScoreText = score_perfect_message;
                break;
            default:
                finalScoreText = "Game Broke :(";
                break;
        }
        yield return StartCoroutine(PrintText(finalScoreText, scoreText));
    }

    IEnumerator PrintText(string text, TMP_Text box)
    {
        box.text = "";
        int ind = 0;
        float elapsed = 0f;
        while(ind < text.Length)
        {
            // finds shortcuts (superscript)
            if (text[ind] == '<')
            {
                bool shortcut = false;
                int endCut = -1;
                for(int i = ind + 1; i < text.Length; i++)
                {
                    if(text[i] == '>')
                    {
                        shortcut = true;
                        endCut = i;
                        break;
                    }
                }

                if (shortcut)
                {
                    for(int i = ind; i <= endCut; i++)
                    {
                        box.text += text[i];
                    }
                    ind = endCut + 1;
                    if (ind >= text.Length) break;
                }
            }

            // adds char
            box.text += text[ind];

            // delays next printing
            while (elapsed < typeDelay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ind++;
            elapsed = 0f;
        }
    }

    public enum SummaryScore
    {
        Failed,
        Cleared,
        Perfect,
    }
}
