using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    Camera cam;
    public Transform cutoutTarget;

    [SerializeField] float cutoutSize = .1f;
    [SerializeField] LayerMask cutoutLayers;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        CutoutProcess();
    }

    List<Renderer> prevRends = new();
    List<Renderer> curRends = new();

    void CutoutProcess()
    {
        if (cutoutTarget == null) return;

        Vector2 cutoutPos = cam.WorldToViewportPoint(cutoutTarget.position);
        cutoutPos.y /= (Screen.width / Screen.height);

        Vector3 offset = cutoutTarget.position - cam.transform.position;
        RaycastHit[] hits = Physics.RaycastAll(cam.transform.position, offset, offset.magnitude, cutoutLayers);

        for(int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].transform.TryGetComponent<Renderer>(out Renderer rend)) continue;

            Material[] mats = rend.materials;

            for(int j = 0; j < mats.Length; j++)
            {
                mats[j].SetVector("_CutoutPosition", cutoutPos);
                mats[j].SetFloat("_CutoutSize", cutoutSize);
            }
            curRends.Add(rend);
        }

        for(int m = 0; m < prevRends.Count; m++)
        {
            if(curRends.Contains(prevRends[m])) continue;

            Material[] mats = prevRends[m].materials;
            for (int n = 0; n < mats.Length; n++)
            {
                mats[n].SetVector("_CutoutPosition", cutoutPos);
                mats[n].SetFloat("_CutoutSize", 0f);
            }
        }
        prevRends = curRends;
        curRends = new();
    }
}
