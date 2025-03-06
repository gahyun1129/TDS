using UnityEngine;

public class BackGroundManager : MonoBehaviour
{
    private static BackGroundManager instance;
    public static BackGroundManager GetInstance() => instance;

    [SerializeField] float speed = 2f;

    private float width;
    private Vector3 startPosition;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        startPosition = transform.position;
        width = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x <= startPosition.x - width)
        {
            transform.position = startPosition;
        }
    }

    public void UpdateSpeed(float _speed)
    {
        speed = _speed;
    }
}
