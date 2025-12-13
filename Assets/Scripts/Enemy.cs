using UnityEngine;
using static UnityEngine.UI.Image;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float raycastLength;
    public LayerMask playerLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool hitLeft = RaycastAndCheckPlayer(Vector2.left, raycastLength);
        bool hitRight = RaycastAndCheckPlayer(Vector2.right, raycastLength);
        Vector3 pos = Vector2.zero;
        if (hitLeft)
        {
            pos = Vector2.left;
        }
        else if (hitRight)
        {
            pos = Vector2.right;
        }
        transform.position += pos * Time.deltaTime * speed;
    }

    private bool RaycastAndCheckPlayer(Vector2 dir, float length)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, length, playerLayer);
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + Vector2.left * raycastLength);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + Vector2.right * raycastLength);
    }
}
