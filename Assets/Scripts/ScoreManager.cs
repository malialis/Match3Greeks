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


    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<TaftBoard>();
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
        if(board != null && scoreBar != null)
        {
            int length = board.scoreGoals.Length;
            scoreBar.fillAmount = (float)score / (float)board.scoreGoals[length -1];
        }
    }





}
