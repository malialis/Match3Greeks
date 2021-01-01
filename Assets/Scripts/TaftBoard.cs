﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaftBoard : MonoBehaviour
{

    public int width;
    public int height;
    public GameObject tilePrefab;
    public GameObject[] dots;

    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;



    // Start is called before the first frame update
    void Start()
    {
        allTiles = new BackgroundTile[width, height]; // tells the board how big it is going to be, not filling it in but size
        allDots = new GameObject[width, height]; 
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

                int dotToUse = Random.Range(0, dots.Length);
                int maxIterations = 0;

                while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                    Debug.Log(maxIterations);
                }

                maxIterations = 0;
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.parent = this.transform; // parent the dot to the tile
                dot.name = "( " + i + ", " + j + " )"; // naming it as its coordinates

                allDots[i, j] = dot;
            }
        }
    }


    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if(column > 1 && row > 1)
        {
            if(allDots[column -1, row].tag == piece.tag && allDots[column -2, row].tag == piece.tag)
            {
                return true;
            }
            if (allDots[column, row -1].tag == piece.tag && allDots[column, row -2].tag == piece.tag)
            {
                return true;
            }
        }
        else if(column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column -1, row].tag == piece.tag && allDots[column -2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if(allDots[column, row].GetComponent<Dots>().isMatched)
        {
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public  void DestroyMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j ++)
            {
                if(allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCoroutine());
    }

    private IEnumerator DecreaseRowCoroutine()
    {
        int nullCount = 0;
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j ++)
            {
                if(allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if(nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dots>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }

        yield return new WaitForSeconds(0.4f);
    }



}
