using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField]
    float UpdateInterval = 1f;


    [Header("Refs")]
    [SerializeField]
    Text Text;

    float _accumulator = 0.0f; // FPS accumulated over the interval
    int _frames = 0; // Frames drawn over the interval
    float _timeleft; // Left time for current interv

    void Awake()
    {
        _timeleft = UpdateInterval;
    }

    void Update()
    {
        _timeleft -= Time.unscaledDeltaTime;
        _accumulator += 1 / Time.unscaledDeltaTime;
        ++_frames;

        if (_timeleft <= 0.0)
        {
            Text.text = "" + (_accumulator / _frames).ToString("f2");
            _timeleft = UpdateInterval;
            _accumulator = 0.0f;
            _frames = 0;
        }
    }
}