using System.Collections.Generic;
using UnityEngine;

public class TruckAnimManager : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;

    [SerializeField] GameObject objectBase;
    [SerializeField] List<GameObject> objects;


    [Header("Movement")]
    [SerializeField] float minY;
    [SerializeField] float maxY;
    [SerializeField] float speedY;
    bool pulseUp = true;

    [SerializeField] float minX;
    [SerializeField] float maxX;
    [SerializeField] float speedX;
    float targetX = 0f;
    [SerializeField] float minZ;
    [SerializeField] float maxZ;
    [SerializeField] float speedZ;
    float targetZ = 0f;

    private void Awake()
    {
        Invoke(nameof(RollObject), 1f);

        transform.position = new Vector3(transform.position.x, minY, transform.position.z);
    }

    void RollObject()
    {
        GameObject _base = Instantiate(objectBase, spawnPoint.position, Quaternion.identity);
        GameObject _object = Instantiate(objects[Random.Range(0, objects.Count)], _base.transform);
        _object.transform.localPosition = Vector3.zero;

        Invoke(nameof(RollObject), Random.Range(0.5f, 2f));
    }

    private void Update()
    {
        transform.position = new Vector3(XSway(), YPulse(), ZSway());
    }

    float YPulse()
    {
        float curY = transform.position.y;

        if(curY >= maxY)
        {
            pulseUp = false;
        }
        else if (curY <= minY)
        {
            pulseUp = true;
        }

        return curY + (Time.deltaTime * speedY * (pulseUp ? 1f : -1f));
    }

    float XSway()
    {
        float curX = transform.position.x;

        if(curX > targetX - .5f && curX < targetX + .5f)
        {
            targetX = Random.Range(minX, maxX);
        }
        bool dir = curX < targetX;

        return curX + (Time.deltaTime * speedX * (dir ? 1f : -1f));
    }
    float ZSway()
    {
        float curZ = transform.position.z;

        if (curZ > targetZ - .5f && curZ < targetZ + .5f)
        {
            targetZ = Random.Range(minZ, maxZ);
        }
        bool dir = curZ < targetZ;

        return curZ + (Time.deltaTime * speedZ * (dir ? 1f : -1f));
    }
}
