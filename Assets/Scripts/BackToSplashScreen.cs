using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackToSplashScreen : MonoBehaviour
{
    public string sceneToLoad;
    private GameData gameData;
    private TaftBoard board;

    // Start is called before the first frame update
    void Start()
    {
        gameData = FindObjectOfType<GameData>();
        board = GameObject.FindWithTag("Board").GetComponent<TaftBoard>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WinOK()
    {
        if(gameData != null)
        {
            gameData.saveData.isActive[board.level + 1] = true;
            gameData.Save();
        }
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoseOK()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

}
