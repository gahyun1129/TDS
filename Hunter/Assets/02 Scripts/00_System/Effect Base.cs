using UnityEngine;
using System.Collections;

public class EffectBase : MonoBehaviour
{
    private EffectType _type;
    private EffectManager _manager;

    public void Initialize(EffectManager manager, EffectType type)
    {
        _manager = manager;
        _type = type;
    }

    public void Play(Vector3 position, Vector3 scale)
    {
        transform.position = position;
        transform.localScale = scale;

        gameObject.SetActive(true);

        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }

        float duration = GetMaxDuration();

        StopAllCoroutines();
        StartCoroutine(DisableRoutine(duration));
    }

    private float GetMaxDuration()
    {
        float maxDuration = 0f;

        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            if (!ps.main.loop)
            {
                float time = ps.main.duration + ps.main.startLifetime.constantMax;
                if (time > maxDuration) maxDuration = time;
            }
        }

        return Mathf.Max(maxDuration, 1.0f);
    }

    private IEnumerator DisableRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        _manager.ReturnEffectToPool(_type, this);
    }
}

public enum EffectType
{
    Bird_Success,
    Bird_Fail,
    Hit_Hat,
}