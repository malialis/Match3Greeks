using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameType
{
    Moves,
    Endless,
    Time
}

[System.Serializable]
public class EndGameRequirements
{
    public GameType gameType;
    public int counterValue;
}


public class EndGameManager : MonoBehaviour
{
    public EndGameRequirements requirements;

    public GameObject movesLabel;
    public GameObject timeLabel;
    public GameObject youWinPanel;
    public GameObject tryAgainPanel;

    public Text counter;
    public int currentCounterValue;
    private float timerSeconds;

    private TaftBoard board;


    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<TaftBoard>();

        SetGameType();
        SetupGame();        
    }

    // Update is called once per frame
    void Update()
    {
        if(requirements.gameType == GameType.Time && currentCounterValue > 0)
        {
            timerSeconds -= Time.deltaTime;
            if(timerSeconds <= 0)
            {
                DecreaseCounterValue();
                timerSeconds = 1;
            }
        }
    }

    private void SetupGame()
    {
        currentCounterValue = requirements.counterValue;

        if(requirements.gameType == GameType.Moves)
        {
            movesLabel.SetActive(true);
            timeLabel.SetActive(false);
        }
        else
        {
            movesLabel.SetActive(false);
            timeLabel.SetActive(true);
            timerSeconds = 1;
        }

        counter.text = "" + currentCounterValue;
    }

    public void DecreaseCounterValue()
    {
        if(board.currentState != GameState.Pause)
        {
            currentCounterValue--;
            counter.text = "" + currentCounterValue;

            if (currentCounterValue <= 0)
            {
                LoseGame();
            }
        }        
                
    }

    public void WinGame()
    {
        youWinPanel.SetActive(true);
        board.currentState = GameState.Win;
        currentCounterValue = 0;
        counter.text = "" + currentCounterValue;
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();

    }

    public void LoseGame()
    {
        Debug.Log("The game is Neigh");
        tryAgainPanel.SetActive(true);
        board.currentState = GameState.Lose;
        currentCounterValue = 0;
        counter.text = "" + currentCounterValue;
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();

    }


    private void SetGameType()
    {
        if(board.world != null)
        {
            if(board.level < board.world.levels.Length)
            {
                if (board.world.levels[board.level] != null)
                {
                    requirements = board.world.levels[board.level].endGameRequirements;
                }
            }
            
        }
    }


}
