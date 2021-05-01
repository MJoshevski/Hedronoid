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

    bool gamePaused = false;

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
                DebugPanel.SetActive(true);
            }
            else
            {
                if (escKeyPresses == 2)
                {
                    DebugPanel.SetActive(false);
                    Time.timeScale = _timeScaleBeforePause;
                    escKeyPresses = 0;
                }                    
            }
        }


        if (!Input.GetKeyDown(KeyCode.P) && gamePaused)
        {
            if (!zeroTimescale)
                Time.timeScale = 0.0000001f;

            if (Input.GetKey(KeyCode.RightBracket))
            {
                Time.timeScale = 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            if (!gamePaused)
            {
                D.GameError("Paused");
                Debug.LogError("Paused2");
                _timeScaleBeforePause = Time.timeScale;

                Time.timeScale = 0.0000001f;
                gamePaused = true;
            }
            else
            {
                Time.timeScale = _timeScaleBeforePause;
                gamePaused = false;

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
