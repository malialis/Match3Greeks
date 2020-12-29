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
    private Image buttonImage;
    private Button myButton;

    public Image[] stars;
    public Text levelTexts;
    public int level;
    public GameObject confirmPanel;
    

    // Start is called before the first frame update
    void Start()
    {
        buttonImage = GetComponent<Image>();
        myButton = GetComponent<Button>();
        ShowLevelNumber();
        ActivateStars();
        DecideSprite();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConfirmPanel(int level)
    {
        confirmPanel.GetComponent<ConfirmationPanel>().level = level;
        confirmPanel.SetActive(true);
    }

    private void DecideSprite()
    {
        if (isActive)
        {
            buttonImage.sprite = activeSprite;
            myButton.enabled = true;
            levelTexts.enabled = true;
        }
        else
        {
            buttonImage.sprite = lockedSprite;
            myButton.enabled = false;
            levelTexts.enabled = false;
            
        }
    }

    private void ShowLevelNumber()
    {
        levelTexts.text = "" + level;
    }

    private void ActivateStars()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = false;
        }
    }

}
