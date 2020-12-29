using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaftBoard : MonoBehaviour
{

    public int width;
    public int height;
    public GameObject tilePrefab;

    private BackgroundTile[,] allTiles;



    // Start is called before the first frame update
    void Start()
    {
        allTiles = new BackgroundTile[width, height]; // tells the board how big it is going to be, not filling it in but size
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetUp()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2 (i, j);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition , Quaternion.identity) as GameObject; // populates board with the tilePrefab
                backgroundTile.transform.parent = this.transform; // setting its parent to the board
                backgroundTile.name = "( " + i + ", " + j + " )"; // naming it as its coordinates
            }
        }
    }


}
