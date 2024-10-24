using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    //Does not actually effect the in Unity time. 300 for five minutes. Set it to 10 in unity for a ten second test.
    public float timeRemaining = 300f;



    public TextMeshProUGUI timerText;

    // Update is called once per frame
    void Update()
    {

        timerText.text = "Time: "; DisplayTime(timeRemaining);

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            SceneManager.LoadScene(1);
        }

    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
