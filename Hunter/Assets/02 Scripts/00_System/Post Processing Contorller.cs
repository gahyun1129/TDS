using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingContorller : MonoBehaviour
{

    #region scene singleton
    public static PostProcessingContorller Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        globalVolume.profile.TryGet(out _motionBlur);
    }

    #endregion

    [SerializeField] private Volume globalVolume;

    private MotionBlur _motionBlur;

    public void ToggleMotionBlur()
    {
        if (_motionBlur != null)
        {
            _motionBlur.active = !_motionBlur.active;
        }
    }

    public void EnableMotionBlur()
    {
        if (_motionBlur != null)
        {
            _motionBlur.active = true;
        }
    }

    public void DisableMotionBlur()
    {
        if (_motionBlur != null)
        {
            _motionBlur.active = false;
        }
    }

    public async UniTaskVoid ProcessMotionBlur(float duration)
    {
        _motionBlur.active = true;

        await UniTask.Delay((int)(duration * 1000f));

        _motionBlur.active = false;
    }
}
