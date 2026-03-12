using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;
    Global global;
    [SerializeField] GameObject target;

    [SerializeField] Vector3 rootOffset;
    [Tooltip("x = for/back, y = up/down"), SerializeField] Vector2 armNormal;
    [SerializeField] float armMagnitude;
    [SerializeField] float lerpT = 1f;

    [SerializeField] float minPitch = -15f;
    [SerializeField] float maxPitch = 15f;

    private void Awake()
    {
        cam = Camera.main;
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        global = Global.Instance;
    }

    float rotYaw = 0f;
    float rotPitch = 0f;

    float lastYaw = 0f;
    private void LateUpdate()
    {
        Vector3 _offset = Vector3.zero;
        _offset += global.GlobalForward * rootOffset.z;
        _offset += global.GlobalRight * rootOffset.x;
        _offset += Vector3.up * rootOffset.y;
        _offset *= global.PlayerScale;

        Quaternion rotQuat = Quaternion.Euler(rotPitch, rotYaw, 0f);
        Vector3 localForward = rotQuat * Vector3.forward;

        Vector3 _arm = Vector3.zero;
        _arm += localForward * armNormal.x;
        _arm += Vector3.up * armNormal.y;
        _arm = _arm.normalized * armMagnitude * global.PlayerScale;

        Vector3 dest = target.transform.position + _offset + _arm;

        cam.transform.position = Vector3.Lerp(cam.transform.position, dest, lerpT * Time.deltaTime);

        cam.transform.LookAt(target.transform.position + _offset);

        if (lastYaw != transform.rotation.y)
        {
            global.ReorientWorld();
            lastYaw = transform.rotation.y;
        }
    }

    public void RotateYaw(float _deltaYaw)
    {
        rotYaw += _deltaYaw;
    }

    public void RotatePitch(float _deltaPitch)
    {
        rotPitch += _deltaPitch;
        if (rotPitch > maxPitch) rotPitch = maxPitch;
        if (rotPitch < minPitch) rotPitch = minPitch;
    }
}
