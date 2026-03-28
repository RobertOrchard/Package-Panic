using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public float targetVolume; // required amount for win
    public float stretchVolume; // level ends upon reaching this
    [NonSerialized] public GameObject stretchTarget; // if level should be cleared upon a specific object collection
}
