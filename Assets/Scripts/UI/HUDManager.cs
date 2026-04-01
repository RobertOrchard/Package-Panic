using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance = null;

    [SerializeField] public HUDTimer timer;
    [SerializeField] Image boxfillImage;
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

        boxfillImage.fillAmount = 0f;
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

        if(boxfillImage.fillAmount < 1f && Global.Instance != null && Global.Instance.levelData != null)
        {
            float _val = value / Global.Instance.levelData.targetVolume;
            if(_val > 1f) _val = 1f;
            boxfillImage.fillAmount = _val;
        }
    }
}
