using System.Collections;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance = null;

    [SerializeField] public HUDTimer timer;
    public float mapDuration = 600f;

    [SerializeField] TMP_Text volumeText;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return null;

        timer.ManualSetup(mapDuration);
        timer.ManualStart();
    }

    public void UpdateVolume(float value)
    {
        volumeText.text = value.ToString() + " m<sup>3";
    }
}
