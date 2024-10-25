using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine;

public class BucketInteract : MonoBehaviour
{
    public int bucketCounter;
    public GameManager gamemanager;
    public GameObject collectPoint;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Character")
        {
            if(gamemanager.counterFish > 0)
            {
                bucketCounter++;
            }
            gamemanager.ClearContents();
        }
    }

    void Update()
    {
        if (bucketCounter > gamemanager.sumFish)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

}
