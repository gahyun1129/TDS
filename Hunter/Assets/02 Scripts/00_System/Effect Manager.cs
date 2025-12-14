using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    #region Singleton
    public static EffectManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Initialize();
    }
    #endregion

    [System.Serializable]
    public class EffectPair
    {
        public EffectType type;
        public GameObject[] effectPrefabs;
    }

    [Header("Effect Settings")]
    [SerializeField] private List<EffectPair> effectDataList = new List<EffectPair>();

    private Dictionary<EffectType, Queue<EffectBase>> _effectPool = new Dictionary<EffectType, Queue<EffectBase>>();
    private Transform _poolRoot;

    private void Initialize()
    {
        _effectPool.Clear();
        GameObject rootObj = new GameObject("@Effect_Root");
        rootObj.transform.SetParent(this.transform);
        _poolRoot = rootObj.transform;
    }

    public void PlayEffect(EffectType type, Vector3 position, Vector3 scale)
    {
        EffectBase effect = GetEffect(type);
        if (effect != null)
        {
            effect.Play(position, scale);
        }
    }

    private EffectBase GetEffect(EffectType type)
    {
        if (!_effectPool.ContainsKey(type))
        {
            _effectPool.Add(type, new Queue<EffectBase>());
        }

        if (_effectPool[type].Count > 0)
        {
            EffectBase pooledEffect = _effectPool[type].Dequeue();
            if (pooledEffect == null) return GetEffect(type);
            return pooledEffect;
        }
        else
        {
            return CreateNewEffect(type);
        }
    }

    private EffectBase CreateNewEffect(EffectType type)
    {
        var data = effectDataList.Find(x => x.type == type);

        if (data == null || data.effectPrefabs == null || data.effectPrefabs.Length == 0)
        {
            return null;
        }

        GameObject container = new GameObject($"{type}_EffectGroup");
        container.transform.SetParent(_poolRoot);

        foreach (GameObject prefab in data.effectPrefabs)
        {
            if (prefab != null)
            {
                GameObject child = Instantiate(prefab, container.transform);
                child.transform.localPosition = prefab.transform.position;
                child.transform.localRotation = prefab.transform.rotation;
            }
        }

        EffectBase effectComponent = container.AddComponent<EffectBase>();
        effectComponent.Initialize(this, type);

        return effectComponent;
    }

    public void ReturnEffectToPool(EffectType type, EffectBase effect)
    {
        effect.gameObject.SetActive(false);
        if (!_effectPool.ContainsKey(type))
        {
            _effectPool.Add(type, new Queue<EffectBase>());
        }
        _effectPool[type].Enqueue(effect);
    }

    public void ShakingCam(float duration, float magnitude)
    {
        CameraController.Instance.Shake(magnitude, duration);
    }
}