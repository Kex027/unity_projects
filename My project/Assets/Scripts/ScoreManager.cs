using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int score = 0;
    [SerializeField] private TextMeshProUGUI scoreToDisplay;

    public void AddToScore()
    {
        score++;
    }
    private void Start()
    {
        scoreToDisplay.text = "Score: 0";
    }

    private void Update()
    {
        scoreToDisplay.text = "Score: " + score;
    }
}

