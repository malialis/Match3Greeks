using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;

    public SaveData saveData;



    private void Awake()
    {
        if (gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        Load();
    }

    private void Start()
    {
        
    }

    public void Save()
    {
        //create a binary formatter which can read binary files
        BinaryFormatter formatter = new BinaryFormatter();

        // create a route from the app to the file
        FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Create);

        //creates the  save data
        SaveData data = new SaveData();
        data = saveData;

        //save the file and close it
        formatter.Serialize(file, data);
        file.Close();
        Debug.Log("Saved it");
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
         {
            //creae a binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
            Debug.Log("loaded");
         }
    }

    private void OnDisable()
    {
        Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }



}
