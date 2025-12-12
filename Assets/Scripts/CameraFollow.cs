using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow : MonoBehaviour
{
    public Transform targetHolder;
    public float followIntensity;

    public Tilemap tilemap;

    private Vector2 viewportHalfsize;
    private Camera followCamera;
    private float cameraZdepth;

    private float minCameraX, maxCameraX, minCameraY, maxCameraY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        followCamera = GetComponent<Camera>();
        cameraZdepth = transform.position.z;
        CalculateCameraBounds();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = targetHolder.position;
        targetPosition.z = cameraZdepth;

        if (targetPosition.x < minCameraX)
        {
            targetPosition.x = minCameraX;
        }
        else if (targetPosition.x > maxCameraX)
        {
            targetPosition.x = maxCameraX;
        }

        if (targetPosition.y < minCameraY)
        {
            targetPosition.y = minCameraY;
        }
        else if (targetPosition.y > maxCameraY)
        {
            targetPosition.y = maxCameraY;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, followIntensity * Time.deltaTime);
    }

    void CalculateCameraBounds()
    {
        tilemap.CompressBounds();
        float orthoSize = followCamera.orthographicSize;
        viewportHalfsize = new(orthoSize * followCamera.aspect, orthoSize);

        Vector3Int tilemapMin = tilemap.cellBounds.min;
        Vector3Int tilemapMax = tilemap.cellBounds.max;

        minCameraX = tilemapMin.x + viewportHalfsize.x + tilemap.transform.position.x;
        maxCameraX = tilemapMax.x - viewportHalfsize.x + tilemap.transform.position.x;
        minCameraY = tilemapMin.y + viewportHalfsize.y + tilemap.transform.position.y;
        maxCameraY = tilemapMax.y - viewportHalfsize.y + tilemap.transform.position.y;
    }
}