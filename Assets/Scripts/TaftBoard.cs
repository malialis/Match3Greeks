using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum GameState
{
    wait,
    move
}



public class TaftBoard : MonoBehaviour
{
    [Header("Board Variables")]
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public int offset;
    public float delayTime;

    [Header("Dots Variables")]
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyFX;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    public Dots currentDot;

    [Header("Matches Variables")]
    private FindMatches findMatches;



    // Start is called before the first frame update
    void Start()
    {
        allTiles = new BackgroundTile[width, height]; // tells the board how big it is going to be, not filling it in but size
        allDots = new GameObject[width, height];
        findMatches = FindObjectOfType<FindMatches>();
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
                Vector2 tempPosition = new Vector2 (i, j + offset);
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
                dot.GetComponent<Dots>().row = j;
                dot.GetComponent<Dots>().column = i;

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

    private void CheckToMakeBombs()
    {
        if (findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        }
        if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            if (ColumnOrRow())
            {
                //make a color bomb
                //is current dot matched
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dots otherDot = currentDot.otherDot.GetComponent<Dots>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
                Debug.Log("Make a color bomb");
            }
            else
            {
                //make an adjacent bomb
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dots otherDot = currentDot.otherDot.GetComponent<Dots>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                    Debug.Log("Make a adjacent bomb");
                }
            }
        }
    }

    private bool ColumnOrRow()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;

        Dots firstPiece = findMatches.currentMatches[0].GetComponent<Dots>();
        if(firstPiece != null)
        {
            foreach (GameObject currentPiece in findMatches.currentMatches)
            {
                Dots dot = currentPiece.GetComponent<Dots>();
                if(dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if(dot.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }
        return (numberVertical == 5 || numberHorizontal == 5);
        
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if(allDots[column, row].GetComponent<Dots>().isMatched)
        {
            //how many elements are in the matched pieces list?
            if(findMatches.currentMatches.Count >= 4)
            {
                CheckToMakeBombs();
            }
            
            GameObject particle = Instantiate(destroyFX, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, delayTime);
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
        findMatches.currentMatches.Clear();
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

        StartCoroutine(FillBoardCoroutine());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j + offset);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);

                    allDots[i, j] = piece;
                    piece.GetComponent<Dots>().row = j;
                    piece.GetComponent<Dots>().column = i;

                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allDots[i, j] != null)
                {
                    if(allDots[i, j].GetComponent<Dots>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }


        return false;
    }

    private IEnumerator FillBoardCoroutine()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.4f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(0.4f);
            DestroyMatches();
        }
        findMatches.currentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(0.4f);
        currentState = GameState.move;

    }



}
