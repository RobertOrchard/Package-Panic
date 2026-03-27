using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackObject : MonoBehaviour
{
    public bool beingDestroyed = false;

    [Tooltip("cm^3"), SerializeField] double volume = 1.0;
    [SerializeField] float pickupRequirementMult = 1.0f; // probably in range of 0.8 to 2.0
    [SerializeField] float IgnoreThreshold = .1f; // will need to find a good value
    [SerializeField] public List<PackObject> insides = new();

    private void Awake()
    {
        RecursiveTagSetter(transform);

        // check for infinite loop
        for (int i = 0; i < insides.Count; i++)
        {
            if (insides[i] == null) continue;

            if (insides[i].insides.Contains(this)) insides[i].insides.Remove(this);
        }
    }

    void RecursiveTagSetter(Transform t)
    {
        t.tag = Global.PackObjectTag;
        for(int i = 0; i < t.childCount; i++)
        {
            RecursiveTagSetter(t.GetChild(i));
        }
    }

    public PickupResponse PickupPermission(float packageVolume)
    {
        if(packageVolume >= (float)volume * pickupRequirementMult)
        {
            return PickupResponse.Allowed;
        }
        else if(packageVolume <= (float)volume * IgnoreThreshold)
        {
            return PickupResponse.Ignored;
        }
        else
        {
            Debug.Log("DENIED");
            return PickupResponse.Rejected;
        }
    }

    public float Pack()
    {
        if(beingDestroyed) return 0f;

        beingDestroyed = true;
        StartCoroutine(QueueDestroy());

        double vol = volume;

        // fetch insides
        for (int i = 0; i < insides.Count; i++)
        {
            if(insides[i] == null) continue;

            vol += insides[i].PackDouble();
        }

        return (float)vol;
    }

    public double PackDouble()
    {
        if (beingDestroyed) return 0;

        beingDestroyed = true;
        StartCoroutine(QueueDestroy());

        double vol = volume;

        // fetch insides
        for (int i = 0; i < insides.Count; i++)
        {
            if (insides[i] == null) continue;

            vol += insides[i].PackDouble();
        }

        return vol;
    }

    IEnumerator QueueDestroy()
    {
        yield return null;
        Destroy(gameObject);
    }
}

public enum PickupResponse
{
    Allowed,
    Rejected,
    Ignored,
}
