using System;
using UnityEngine;

public class LevelTarget : MonoBehaviour
{
    public Action Collected;
    private void OnDestroy()
    {
        Collected?.Invoke();
    }
}
