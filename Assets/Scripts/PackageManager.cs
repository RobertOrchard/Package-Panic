using System.Collections;
using UnityEngine;

public class PackageManager : MonoBehaviour
{
    [SerializeField] PlayerControl control;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip PickupAudioClip;

    [Tooltip("X-Scale"), SerializeField] float shapeLength = 1f; // m
    [Tooltip("Y-Scale"), SerializeField] float shapeHeight = 1f; // m
    [Tooltip("Z-Scale"), SerializeField] float shapeWidth = 1f; // m
    [SerializeField] float startingVolume = 1f;
    float totalVolume; // m^3

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        totalVolume = startingVolume;

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        CalculateSurfaceArea();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Global.PackObjectTag))
        {

            collision.gameObject.TryGetComponent<PackObject>(out PackObject objData); // check in this obj
            // check in parent if failed
            if (objData == null && collision.transform.parent != null) 
            {
                collision.transform.parent.TryGetComponent<PackObject>(out objData);
                // check in parent2
                if (objData == null && collision.transform.parent.parent != null) 
                    collision.transform.parent.parent.TryGetComponent<PackObject>(out objData);
            }

            if (objData == null) return;
            switch (objData.PickupPermission(totalVolume))
            {
                case PickupResponse.Allowed:
                    Pickup(objData);
                    break;
                case PickupResponse.Rejected:
                    control.DeniedPickup(collision.transform.position);
                    break;
            }
        }
    }

    void Pickup(PackObject data)
    {
        if (data.beingDestroyed) return;

        float objVolume = data.Pack();
        totalVolume += objVolume;

        CalculateSurfaceArea();
        Global.Instance.NewPlayerVolume(totalVolume);

        audioSource.PlayOneShot(PickupAudioClip);
    }

    // Volume = length * width * height
    void CalculateSurfaceArea()
    {
        if(shapeLength == shapeHeight && shapeHeight == shapeWidth) // is cube shape
        {
            Debug.Log("Shape is Cube");
            float l = Mathf.Pow(totalVolume, 1f / 3f); // length of every side
            transform.localScale = new Vector3(l, l, l);
        }
        else
        {
            Debug.Log("Shape is Rectangular Prism");            // using deconstruction of a rectangular prism
            float a = shapeLength * shapeHeight * shapeWidth;   // V = L*H*W = lx*hx*wx = a*x^3
            float unit = totalVolume / a;                       // V/a = x^3
            unit = Mathf.Pow(unit, 1f / 3f);                    // (V/a)^(1/3) = x
            transform.localScale = new Vector3(unit * shapeLength, unit * shapeHeight, unit * shapeWidth); // L = l*x // H = h*x // W = w*x
        }
        rb.mass = totalVolume;

        control.RecalculateEdge();
        control.CalculateDigits(totalVolume);
        if (HUDManager.Instance) HUDManager.Instance.UpdateVolume(totalVolume);

        Global.Instance.PlayerScale = (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;
        //Debug.Log("New playerscale" + Global.Instance.PlayerScale);
    }
}
