using UnityEngine;

public class Orbiter : MonoBehaviour
{
    private Transform center;
    private float radius;
    private float angle;
    private float rotationSpeed;

    private float damage = 50;
    private float currentHP;

    private float initialZ;

    public void Initialize(Transform center, float radius, float startAngle, float rotationSpeed, float damage, float currentHP)
    {
        this.center = center;
        this.radius = radius;
        this.angle = startAngle;
        this.rotationSpeed = rotationSpeed;

        this.damage = damage;
        this.currentHP = currentHP;

        this.initialZ = transform.position.z;

    }

    void Update()
    {
        if (center == null)
        {
            Destroy(gameObject);
            return;
        }

        angle += rotationSpeed * TimeManager.Instance.DeltaTime;

        angle += rotationSpeed * Time.deltaTime;

        Vector2 offset = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * radius;

        Vector2 centerPos2D = center.position;
        Vector2 newPos2D = centerPos2D + offset;

        transform.position = new Vector3(newPos2D.x, newPos2D.y, initialZ);

        Vector2 dirToCenter = (center.position - transform.position).normalized;

        float lookAngle = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, lookAngle - 90f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            InGamePlayerStat.Instance.DealDamageTo(collision.gameObject.GetComponent<IDamageable>(), damage);
            TakeDamage(damage / 2);
        }
    }

    public void TakeDamage(float value)
    {
        currentHP -= value;
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
