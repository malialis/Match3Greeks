using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreText;
    public int score;
    public Image scoreBar;

    private TaftBoard board;
    private GameData gameData;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<TaftBoard>();
        gameData = FindObjectOfType<GameData>();
        scoreBar.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();

    }


    public void IncreaseScore(int ammountToIncrease)
    {
        score += ammountToIncrease;

        if(gameData != null)
        {
            int highScore = gameData.saveData.highScores[board.level];
            if(score > highScore)
            {
                gameData.saveData.highScores[board.level] = score;
            }
            gameData.Save();
        }
        UpdateScoreBar();        
    }

    private void UpdateScoreBar()
    {
        if (board != null && scoreBar != null)
        {
            int length = board.scoreGoals.Length;
            scoreBar.fillAmount = (float)score / (float)board.scoreGoals[length - 1];
        }
    }



}
