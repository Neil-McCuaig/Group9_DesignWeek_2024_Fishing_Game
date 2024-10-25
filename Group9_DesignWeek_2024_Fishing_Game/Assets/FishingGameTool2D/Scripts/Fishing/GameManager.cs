using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{
    //UI TextMeshPros
    public TextMeshProUGUI fishCounter;

    //Collectable Counter
    public int counterFish;
    public int sumFish;


    // Start is called before the first frame update
    void Start()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");
        
        sumFish = fishes.Length;
        
    }

    // Update is called once per frame
    void Update()
    {
        fishCounter.text = "" + counterFish;
    }

    //Tracks items being carried
    public void FishCounter()
    {
        counterFish += 1;
    }

    public void ClearContents()
    {
        counterFish = 0;
    }

}
