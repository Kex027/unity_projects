using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FindObjectOfType<ScoreManager>().AddToScore();
        Destroy(gameObject);       
    }
}
