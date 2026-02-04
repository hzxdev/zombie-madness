using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDebug : MonoBehaviour
{
    public GameObject fpsBar;
    public Text fpsText;
    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.5f;
    public int fpsLimit = 60;


     void Start()
    {
        if(Application.isMobilePlatform)
        Application.targetFrameRate = fpsLimit;

        Application.targetFrameRate = 60;

       /* if(PlayerPrefs.GetInt("fpsEnabled") == 0)
        {
            fpsBar.SetActive(false);
        }
       */
    }

    void Update()
    {
        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            m_lastFramerate = m_frameCounter / m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
            fpsText.text = "FPS: " + m_lastFramerate.ToString("f0");
        }
        
    }

    public void ChangeFPSLimit()
    {
        if(fpsLimit == 60)
        {
            fpsLimit = 30;
            Application.targetFrameRate = fpsLimit;
            return;
        }

        if (fpsLimit == 30)
        {
            fpsLimit = 60;
            Application.targetFrameRate = fpsLimit;
            return;
        }


        
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("TutorialMarker"))
        {
            Tutorial.instance.MovementMarkerCollided();
        }
    }
}
