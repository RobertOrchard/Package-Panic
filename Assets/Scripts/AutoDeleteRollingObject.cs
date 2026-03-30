using System.Collections;
using UnityEngine;

public class AutoDeleteRollingObject : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(DeleteObj());
    }
    IEnumerator DeleteObj()
    {
        yield return new WaitForSeconds(10);

        Destroy(this.gameObject);
    }
}
