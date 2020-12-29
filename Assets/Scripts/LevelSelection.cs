using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    [SerializeField]
    private bool unlocked; // default is false

    public Image unlockImage;
    public GameObject[] stars;


    private void Update()
    {
        UpdateLevelImage();
    }

    private void UpdateLevelImage()
    {
        if (!unlocked) // if unlock is false, level is still locked
        {
            unlockImage.gameObject.SetActive(true);

            for (int i = 0; i < stars.Length; i++)
            {
                stars[1].gameObject.SetActive(false);
            }
        }
        else // if unlock is true, you can play level
        {
            unlockImage.gameObject.SetActive(false);

            for (int i = 0; i < stars.Length; i++)
            {
                stars[1].gameObject.SetActive(true);
            }
        }
    }

    public void PressSelection(string _levelName)
    {
        // when clicked we can move to the level
        if (unlocked)
        {
            SceneManager.LoadScene(_levelName);
        }
    }




}
