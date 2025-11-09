using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // 플레이어 Transform
    public Vector3 offset = new Vector3(0f, 0f, -10f); // 카메라와 플레이어 사이의 거리
    public float smoothSpeed = 0.125f; // 카메라 이동 속도

    private Camera cam;
    private float zoomOutSize = 7f;

    [Header("배경 크기")]
    public Vector2 minBounds;
    public Vector2 maxBounds;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
    }


    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            // 카메라 경계 계산
            float camHalfHeight = cam.orthographicSize;
            float camHalfWidth = cam.orthographicSize * cam.aspect;

            // 위치 제한
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);

            transform.position = smoothedPosition;
        }
    }

    public void StartZoomOut()
    {
        StartCoroutine(ZoomOutCoroutine());
    }

    private System.Collections.IEnumerator ZoomOutCoroutine()
    {
        float elapsedTime = 0f;
        float duration = 2f;
        float startSize = cam.orthographicSize;

        while (elapsedTime < duration)
        {
            cam.orthographicSize = Mathf.Lerp(startSize, zoomOutSize, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = zoomOutSize;
    }
}
