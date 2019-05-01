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
            Time.timeScale = 1;
        });
    }

    void Update()
    {
        TimeScaleText.text = Time.timeScale.ToString();
        bool zeroTimescale = Mathf.Approximately(0, Time.timeScale);
        PlayButton.gameObject.SetActive(zeroTimescale);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = zeroTimescale ? 1 : 0;
        }
    }
}
