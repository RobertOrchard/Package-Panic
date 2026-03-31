using UnityEngine;

public class RoadPieceLogic : MonoBehaviour
{
    public float speed;
    public float size = 100f;

    public void Tick(float delta)
    {
        transform.position += new Vector3(speed * delta, 0f, 0f);

        if(transform.position.x > size)
        {
            transform.position -= new Vector3(size * 2f, 0f, 0f);
        }
    }
}
