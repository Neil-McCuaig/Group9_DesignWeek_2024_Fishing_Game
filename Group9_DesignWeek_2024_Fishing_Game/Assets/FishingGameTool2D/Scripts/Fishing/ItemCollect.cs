using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollect : MonoBehaviour
{
    public GameManager gameManager;
    public CharacterController characterController;

    //Obj is attached to player
    public GameObject collect;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Fish")
        {
            Destroy(collider.gameObject);
        }

    }

}
