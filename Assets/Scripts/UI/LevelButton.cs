using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LevelButton : MonoBehaviour
{
    [Header("Active Stuff")]
    public bool isActive;
    public Sprite activeSprite;
    public Sprite lockedSprite;
    private int starsActive;

    [Header("Button Things")]
    private Image buttonImage;
    private Button myButton;

    [Header("Stars and Levels")]
    public Image[] stars;    
    public Text levelText;
    public int level;
    public GameObject confirmPanel;
    private GameData gameData;

    
    // Start is called before the first frame update
    void Start()
    {
        //find and source on load
        gameData = FindObjectOfType<GameData>();
        buttonImage = GetComponent<Image>();
        myButton = GetComponent<Button>();
        

        LoadData();        
        ActivateStars();
        ShowLevelNumber();
        DecideOnWhichSprites();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConfirmPanel(int level)
    {
        confirmPanel.GetComponent<ConfirmPanel>().level = level;
        confirmPanel.SetActive(true);

    }

    private void DecideOnWhichSprites()
    {
        if (isActive)
        {
            buttonImage.sprite = activeSprite;
            myButton.enabled = true;
            levelText.enabled = true;
        }
        else
        {
            buttonImage.sprite = lockedSprite;
            myButton.enabled = false;
            levelText.enabled = false;

        }
    }

    private void ShowLevelNumber()
    {
        levelText.text = "" + level;
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
        //is gamedata present
        if(gameData != null)
        {
            //decide if level is active
            if(gameData.saveData.isActive[level - 1])
            {
                isActive = true;
            }
            else
            {
                isActive = false;
            }
            //decide on how many stars to activate
            starsActive = gameData.saveData.stars[level - 1];
        }
    }





}
