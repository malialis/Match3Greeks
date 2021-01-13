using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ConfirmPanel : MonoBehaviour
{
    [Header("Level Information")]
    public string levelToLoad;
    public int level;
    private int starsActive;
    private int highScore;

    [Header("UI related")]
    public Image[] stars;    
    public Text highScoreText;
    public Text starText;

    private GameData gameData;


    // Start is called before the first frame update
    void OnEnable()
    {
        gameData = FindObjectOfType<GameData>();

        LoadData();
        ActivateStars();
        SetText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Cancel()
    {
        this.gameObject.SetActive(false);
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level", level - 1);
        SceneManager.LoadScene(levelToLoad);
    }

    private void ActivateStars()
    {
        //come back to add stars with binary files
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = true;
        }
    }

    private void LoadData()
    {
        if(gameData != null)
        {
            starsActive = gameData.saveData.stars[level - 1];
            highScore = gameData.saveData.highScores[level - 1];
        }
    }

    private void SetText()
    {
        highScoreText.text = "" + highScore;
        starText.text = "" + starsActive + "/3";
    }


}
