using System;
using Unity.Mathematics;
using UnityEngine;

public enum TimeState
{
    pause,
    play
}

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    public TimeState curTimeState { get; private set; }
    public float DeltaTime { get; private set; }

    [Header("타임 스케일")]
    [SerializeField] private float scale_value = 1f;
    [SerializeField] private float min_sclae = 1f;
    [SerializeField] private float max_scale = 3f;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        curTimeState = TimeState.play;
    }

    void Update()
    {
        DeltaTime = Time.deltaTime;
    }

    public void Pause()
    {
        DeltaTime = 0f;
        Time.timeScale = 0f;
        curTimeState = TimeState.pause;
    }

    public void Resume()
    {
        DeltaTime = Time.deltaTime;
        Time.timeScale = scale_value;
        curTimeState = TimeState.play;
    }

    public float ChangeTimeScale(float value)
    {
        scale_value += value;
        if (scale_value <= min_sclae)
        {
            scale_value = min_sclae;
        }
        else if (scale_value >= max_scale)
        {
            scale_value = max_scale;
        }
        Time.timeScale = scale_value;
        return math.ceil(scale_value);
    }

    public float TimeScale => scale_value;

    public void SetTimeScale(float value)
    {
        scale_value = value;
        Time.timeScale = scale_value;
    }
}
