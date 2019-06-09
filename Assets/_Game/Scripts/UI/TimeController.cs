using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    [SerializeField]
    Button PlayButton;

    [SerializeField]
    Text TimeScaleText;

    void Start()
    {
        PlayButton.onClick.AddListener(() =>
        {
            Time.timeScale = _timeScaleBeforePause;
        });
    }

    void Update()
    {
        TimeScaleText.text = Time.timeScale.ToString();
        bool zeroTimescale = Mathf.Approximately(0, Time.timeScale);
        PlayButton.gameObject.SetActive(zeroTimescale);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _timeScaleBeforePause = Time.timeScale;

            Time.timeScale = zeroTimescale ? _timeScaleBeforePause : 0;
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Time.timeScale += .25f;
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Time.timeScale -= .25f;
        }
    }

    float _timeScaleBeforePause;
}
