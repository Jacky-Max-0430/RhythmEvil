
using UnityEngine;
using System;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance;
    public float bpm = 120;
    public AudioSource musicSource;
    public event Action OnBeat; // 节拍事件（用于逻辑）
    public event Action OnBeatVisual; // 节拍事件（用于UI动画）

    private float _beatInterval;
    private double _nextBeatTime;

    void Awake() => Instance = this;

    void Start()
    {
        _beatInterval = 60f / bpm;
        _nextBeatTime = AudioSettings.dspTime + _beatInterval;
        musicSource.PlayScheduled(_nextBeatTime);
    }

    void Update()
    {
        double currentTime = AudioSettings.dspTime;
        if (currentTime >= _nextBeatTime - 0.1f) // 提前触发UI动画
            OnBeatVisual?.Invoke();

        if (currentTime >= _nextBeatTime)
        {
            OnBeat?.Invoke();
            _nextBeatTime += _beatInterval;
        }
    }

    public float GetBeatInterval()
    { 
        return _beatInterval;
    }

}