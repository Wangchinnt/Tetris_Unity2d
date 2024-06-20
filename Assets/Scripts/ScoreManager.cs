using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{   

    public int score { get; private set;} = 0;
    public int level { get; private set;} = 1; 

    public Text levelText;
    public Text linesText;
    public Text scoreText;
   
    private int linePerLevel = 10;

    private int lineCleared = 0;

    // Start is called before the first frame update
    void Start()
    {
        UpdateScoreUI();
    }

    private void LevelUp()
    {
        level++;
        linePerLevel += (10-level);
        // Update stepDelay and lockDelay in Piece.cs
        FindAnyObjectByType<Piece>().UpdateSpeed();
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateScoreUI();
    }

    public void AddScoreForLine(int linesCleared)
    {   
         switch (linesCleared)
        {
            case 1:
                score += 100;
                break;
            case 2:
                score += 300;
                break;
            case 3:
                score += 500;
                break;
            case 4:
                score += 800;
                break;
        }
        this.lineCleared += linesCleared;
        if (lineCleared >= linePerLevel * level)
        {
            LevelUp();
        }
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        // Update the UI with the new score
        scoreText.text = score.ToString();
        levelText.text = level.ToString();
        linesText.text = lineCleared.ToString();
    }
    public void ResetScore()
    {
        score = 0;
        level = 1;
        lineCleared = 0;
        linePerLevel = 10;
        UpdateScoreUI();
    }
}