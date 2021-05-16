using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hedronoid;

public class TimeController : HNDMonoBehaviour
{
    public Button PlayButton;

    public GameObject DebugPanel;

    public Text TimeScaleText;

    [SerializeField]
    int escKeyPresses = 0;

    protected override void Start()
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escKeyPresses++;
            if(!zeroTimescale)
            {
                _timeScaleBeforePause = Time.timeScale;

                Time.timeScale = 0;
            }         
            else
            {
                if (escKeyPresses == 2)
                    DebugPanel.SetActive(true);

                if (escKeyPresses == 3)
                {
                    DebugPanel.SetActive(false);
                    Time.timeScale = _timeScaleBeforePause;
                    escKeyPresses = 0;
                }                    
            }
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
