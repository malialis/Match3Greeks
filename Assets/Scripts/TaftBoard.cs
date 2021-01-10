using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum GameState
{
    wait,
    move
}

public enum TileKind
{
    Blank,
    Breakable,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}


public class TaftBoard : MonoBehaviour
{
    [Header("Board Variables")]
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public int offset;
    public float delayTime;
    public float refillDelay = 0.75f;
    private ScoreManager scoreManager;
    public int[] scoreGoals;
    private SoundManager soundManager;
    public int basePieceValue = 20;
    private int streakValue = 1;

    [Header("Dots Variables")]
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyFX;
    public GameObject breakablePrefab;
    [SerializeField]
    public TileType[] boardLayout;
    [SerializeField]
    private BackgroundTile[,] breakableTiles;
    private bool[,] blankSpaces;
    public GameObject[,] allDots;
    public Dots currentDot;
    public int damageToTake;

    [Header("Matches Variables")]
    private FindMatches findMatches;



    // Start is called before the first frame update
    void Start()
    {
        breakableTiles = new BackgroundTile[width, height];
        blankSpaces = new bool[width, height]; // tells the board how big it is going to be, not filling it in but size
        allDots = new GameObject[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        scoreManager = FindObjectOfType<ScoreManager>();
        soundManager = FindObjectOfType<SoundManager>();
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(!blankSpaces[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offset);
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject; // populates board with the tilePrefab
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
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (allDots[column, row -1] != null && allDots[column, row -2] != null)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }                
        }
        else if(column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
                {
                    if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
                    
            }
            if (column > 1)
            {
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
                {
                    if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
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
            //does a tile need to break or take damage
            if(breakableTiles[column, row] != null)
            {
                breakableTiles[column, row].TakeDamage(damageToTake);
                if(breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }
            
            GameObject particle = Instantiate(destroyFX, allDots[column, row].transform.position, Quaternion.identity);
            //does the sound exist
            if(soundManager != null)
            {
                soundManager.PlayRandomDestroyNoise();
            }

            Destroy(particle, delayTime);
            Destroy(allDots[column, row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
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
        StartCoroutine(DecreaseRowWithNullsCoroutine());
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
                if (allDots[i, j] == null && !blankSpaces[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offset);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;

                    while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        dotToUse = Random.Range(0, dots.Length);
                    }

                    maxIterations = 0;

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
        yield return new WaitForSeconds(refillDelay);

        while (MatchesOnBoard())
        {
            streakValue ++;
            yield return new WaitForSeconds(2 * refillDelay);
            DestroyMatches();
            yield return new WaitForSeconds(2 * refillDelay);
        }
        findMatches.currentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(refillDelay);

        if (IsBoardDeadlocked())
        {
            Debug.Log("Deadlocked yo");
            ShuffleBoard();
            Debug.Log("Board is shuffled yo");
        }

        yield return new WaitForSeconds(refillDelay);
        currentState = GameState.move;
        streakValue = 1;

    }

    private IEnumerator DecreaseRowWithNullsCoroutine()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if the current spot is not blank or empty.....
                if(!blankSpaces[i, j] && allDots[i, j] == null)
                {
                    //loop from the space above to the top of column
                    for (int k = j + 1; k < height; k++)
                    {
                        //if dot is found
                        if(allDots[i, k] != null)
                        {
                            //move that dot to the empty space
                            allDots[i, k].GetComponent<Dots>().row = j;
                            // set that spot to be null
                            allDots[i, k] = null;
                            //break loop
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCoroutine());
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if(boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        //look at all tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if tile is a breakable
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                //create the breakable
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakablePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }


    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        //take the first piece, save it in a holder
        GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
        //switching first dot to be the 2nd position
        allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
        //set the first dot to be the 2nd dot
        allDots[column, row] = holder;
    }

    private bool CheckTheBoardForMatches()
    {
        // this method is to check for deadlocks
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allDots[i, j] != null)
                {
                    //make sure 1 and 2 to the right are in the baord
                    if(i < width - 2)
                    {
                        //check if the dots and 2 to the right exist
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            // check the tags
                            if (allDots[i + 1, j].tag == allDots[i, j].tag && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                    //make sure is within the board
                    if(j < height - 2)
                    {
                        // check if they exist
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            //check if they match
                            if (allDots[i, j + 1].tag == allDots[i, j].tag && allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                   
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheckPieces(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckTheBoardForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsBoardDeadlocked()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allDots[i, j] != null)
                {
                    if(i < width - 1)
                    {
                        if(SwitchAndCheckPieces(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if(j < height - 1)
                    {
                        if(SwitchAndCheckPieces(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private void ShuffleBoard()
    {
        //create a list of dots on the board
        List<GameObject> newBoard = new List<GameObject>();
        //add all to the list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }
        //for every spot on the boar
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if this spot should not be blank 
                if (!blankSpaces[i,j])
                {
                    //pick a random number
                    int piecesToUse = Random.Range(0, newBoard.Count);
                                        
                    int maxIterations = 0;

                    while (MatchesAt(i, j, newBoard[piecesToUse]) && maxIterations < 100)
                    {
                        piecesToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                        Debug.Log(maxIterations + "Shuffles");
                    }

                    //make a container for the piece
                    Dots piece = newBoard[piecesToUse].GetComponent<Dots>();

                    maxIterations = 0;

                    piece.column = i; // assign the column to the piece
                    piece.row = j; //assign the row to the piece
                    allDots[i, j] = newBoard[piecesToUse]; // fill in the array with the new pieces
                    newBoard.Remove(newBoard[piecesToUse]); // remove it from the list
                }
            }
        }
        //check if it is deadlocked
        if (IsBoardDeadlocked())
        {
            ShuffleBoard();
        }
    }

}
