using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum GameState
{
    wait,
    move,
    Win,
    Lose,
    Pause
}

public enum TileKind
{
    Blank,
    Breakable,
    Lock,
    Concrete,
    Slime,
    Normal
}

[System.Serializable]
public class MatchType
{
    public int type;
    public string color;
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
    public float refillDelay = 0.5f;    
    public int[] scoreGoals;     
    
    [Header("Scriptable Objects Related")]
    public int level;
    public World world;

    [Header("Dots Variables")]
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyFX;
    public GameObject breakablePrefab;
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject slimeTilePrefab;
    public int basePieceValue = 20;
    private int streakValue = 1;
    public int damageToTake;

    [SerializeField]
    public TileType[] boardLayout;
    [SerializeField]
    private BackgroundTile[,] breakableTiles;
    public BackgroundTile[,] lockTiles;
    public BackgroundTile[,] concreteTiles;
    public BackgroundTile[,] slimeTiles;
    private bool[,] blankSpaces;
    public GameObject[,] allDots;
    public Dots currentDot;
    

    [Header("Matches Variables")]
    private FindMatches findMatches;
    public MatchType matchType;
    private bool makeSlime = true;

    [Header("Managers")]
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private GoalManager goalManager;


    private void Awake()
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if(world != null)
        {
            if(level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        findMatches = FindObjectOfType<FindMatches>();
        scoreManager = FindObjectOfType<ScoreManager>();
        soundManager = FindObjectOfType<SoundManager>();
        goalManager = FindObjectOfType<GoalManager>();

        breakableTiles = new BackgroundTile[width, height];
        lockTiles = new BackgroundTile[width, height];
        concreteTiles = new BackgroundTile[width, height];
        slimeTiles = new BackgroundTile[width, height];
        blankSpaces = new bool[width, height]; // tells the board how big it is going to be, not filling it in but size

        allDots = new GameObject[width, height];
       
        SetUp();
        currentState = GameState.Pause;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateConcreteTiles();
        GenerateSlimeTiles();

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(!blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
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

    #region matching stuff yo and Column or Row

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
        //how many objects are in findMatches
        if(findMatches.currentMatches.Count > 3)
        {
            //what type of match
            MatchType typeOfMatch = ColumnOrRow();

            if(typeOfMatch.type == 1)
            {
                //make a color bomb
                //is current dot matched
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                    {
                        currentDot.isMatched = false;
                        currentDot.MakeColorBomb();
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dots otherDot = currentDot.otherDot.GetComponent<Dots>();
                            if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                            {
                                otherDot.isMatched = false;
                                otherDot.MakeColorBomb();
                            }
                        }
                    }
                
                Debug.Log("Make a color bomb");
            }
            else if (typeOfMatch.type == 2)
            {
                //make an adjacent bomb
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeAdjacentBomb();
                }
                    else if (currentDot.otherDot != null)
                        {
                            Dots otherDot = currentDot.otherDot.GetComponent<Dots>();
                            if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                            {
                                otherDot.isMatched = false;
                                otherDot.MakeAdjacentBomb();
                            }
                        }
                    
                    Debug.Log("Make a adjacent bomb");
                
            }
            else if(typeOfMatch.type == 3)
            {
                findMatches.CheckBombs(typeOfMatch);
            }
        
        
        }

    }

    private MatchType ColumnOrRow() // was Bool is now Int
    {        
        // make a copy of the current matches
        List<GameObject> matchCopy = findMatches.currentMatches as List<GameObject>;

        matchType.type = 0;
        matchType.color = "";

        //cycle through all the match copy and decide if a bomb is needed
        for (int i = 0; i < matchCopy.Count; i++)
        {
            //store this dot
            Dots thisDot = matchCopy[i].GetComponent<Dots>();
            string color = matchCopy[i].tag;
            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;
            //cycle throught he rest
            for (int j = 0; j < matchCopy.Count; j++)
            {
                //store the next dot
                Dots nextDot = matchCopy[j].GetComponent<Dots>();
                if(nextDot == thisDot)
                {
                    continue;
                }
                if(nextDot.column == thisDot.column && nextDot.tag == color)
                {
                    columnMatch++;
                }
                if (nextDot.row == thisDot.row && nextDot.tag == color)
                {
                    rowMatch++;
                }
            }
            //return 3 if row or column match
            //return 2 if adjacent
            //return 1 if color bomb
            if(columnMatch == 4 || rowMatch == 4)
            {
                matchType.type = 1;
                matchType.color = color;
                return matchType;
            }
            else if(columnMatch == 2 && rowMatch == 2)
            {
                matchType.type = 2;
                matchType.color = color;
                return matchType;
            }
            else if (columnMatch == 3 || rowMatch == 3)
            {
                matchType.type = 3;
                matchType.color = color;
                return matchType;
            }

        }

        matchType.type = 0;
        matchType.color = "";
        return matchType;

    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankSpaces[i, j] && !concreteTiles[i,j] && !slimeTiles[i, j])
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
        findMatches.FindAllMatches();

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
        
        yield return new WaitForSeconds(refillDelay);

        RefillBoard();
        yield return new WaitForSeconds(refillDelay);

        while (MatchesOnBoard())
        {
            //yield return new WaitForSeconds(refillDelay);
            streakValue++; 
            DestroyMatches();
            yield break;            
        }        
        currentDot = null;

        CheckToMakeSlime();

        if (IsBoardDeadlocked())
        {
            Debug.Log("Deadlocked yo");
            StartCoroutine(ShuffleBoard());
            Debug.Log("Board is shuffled yo");
        }

        yield return new WaitForSeconds(refillDelay);

        if(currentState != GameState.Pause)
        currentState = GameState.move;
        makeSlime = true;
        streakValue = 1;

    }

    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        if (allDots[column + (int)direction.x, row + (int)direction.y] != null)
        {
            //take the first piece, save it in a holder
            GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
            //switching first dot to be the 2nd position
            allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
            //set the first dot to be the 2nd dot
            allDots[column, row] = holder;
        }

    }

    private bool CheckTheBoardForMatches()
    {
        // this method is to check for deadlocks
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    //make sure 1 and 2 to the right are in the baord
                    if (i < width - 2)
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
                    if (j < height - 2)
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
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                        if (SwitchAndCheckPieces(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if (j < height - 1)
                    {
                        if (SwitchAndCheckPieces(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private IEnumerator ShuffleBoard()
    {
        yield return new WaitForSeconds(refillDelay);

        //create a list of dots on the board
        List<GameObject> newBoard = new List<GameObject>();
        //add all to the list
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }

        yield return new WaitForSeconds(refillDelay);
        //for every spot on the boar
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if this spot should not be blank 
                if (!blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i, j])
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
            StartCoroutine(ShuffleBoard());
        }
    }
            
    #endregion

    #region Damage and Decreasing stuff

    private void DamageConcreteTiles(int column, int row)
    {
        if (column > 0)
        {
            if (concreteTiles[column - 1, row])
            {
                concreteTiles[column - 1, row].TakeDamage(1);

                if (concreteTiles[column - 1, row].hitPoints <= 0)
                {
                    concreteTiles[column - 1, row] = null;
                }
            }
        }
        if (column < width - 1)
        {
            if (concreteTiles[column + 1, row])
            {
                concreteTiles[column + 1, row].TakeDamage(1);

                if (concreteTiles[column + 1, row].hitPoints <= 0)
                {
                    concreteTiles[column + 1, row] = null;
                }
            }
        }
        if (row > 0)
        {
            if (concreteTiles[column, row - 1])
            {
                concreteTiles[column, row - 1].TakeDamage(1);

                if (concreteTiles[column, row - 1].hitPoints <= 0)
                {
                    concreteTiles[column, row - 1] = null;
                }
            }
        }
        if (row < height - 1)
        {
            if (concreteTiles[column, row + 1])
            {
                concreteTiles[column, row + 1].TakeDamage(1);

                if (concreteTiles[column, row + 1].hitPoints <= 0)
                {
                    concreteTiles[column, row + 1] = null;
                }
            }
        }
    }

    private void DamageSlimeTiles(int column, int row)
    {
        if (column > 0)
        {
            if (slimeTiles[column - 1, row])
            {
                slimeTiles[column - 1, row].TakeDamage(1);

                if (slimeTiles[column - 1, row].hitPoints <= 0)
                {
                    slimeTiles[column - 1, row] = null;
                }
                makeSlime = false;
            }
        }
        if (column < width - 1)
        {
            if (slimeTiles[column + 1, row])
            {
                slimeTiles[column + 1, row].TakeDamage(1);

                if (slimeTiles[column + 1, row].hitPoints <= 0)
                {
                    slimeTiles[column + 1, row] = null;
                }
                makeSlime = false;
            }
        }
        if (row > 0)
        {
            if (slimeTiles[column, row - 1])
            {
                slimeTiles[column, row - 1].TakeDamage(1);

                if (slimeTiles[column, row - 1].hitPoints <= 0)
                {
                    slimeTiles[column, row - 1] = null;
                }
                makeSlime = false;
            }
        }
        if (row < height - 1)
        {
            if (slimeTiles[column, row + 1])
            {
                slimeTiles[column, row + 1].TakeDamage(1);

                if (slimeTiles[column, row + 1].hitPoints <= 0)
                {
                    slimeTiles[column, row + 1] = null;
                }
                makeSlime = false;
            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dots>().isMatched)
        {
            //does a tile need to break or take damage
            if (breakableTiles[column, row] != null)
            {
                breakableTiles[column, row].TakeDamage(damageToTake);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }

            //does a tile need to break or take damage
            if (lockTiles[column, row] != null)
            {
                lockTiles[column, row].TakeDamage(damageToTake);
                if (lockTiles[column, row].hitPoints <= 0)
                {
                    lockTiles[column, row] = null;
                }
            }

            DamageConcreteTiles(column, row);
            DamageSlimeTiles(column, row);

            if (goalManager != null)
            {
                goalManager.CompareGoal(allDots[column, row].tag.ToString());
                goalManager.UpdateGoalsText();
            }

            GameObject particle = Instantiate(destroyFX, allDots[column, row].transform.position, Quaternion.identity);
            //does the sound exist
            if (soundManager != null)
            {
                soundManager.PlayRandomDestroyNoise();
            }

            Destroy(particle, delayTime);
            allDots[column, row].GetComponent<Dots>().PopDeathAnimation();
            Destroy(allDots[column, row], 0.5f);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        //how many elements are in the matched pieces list?
        if (findMatches.currentMatches.Count >= 4)
        {
            CheckToMakeBombs();
        }

        findMatches.currentMatches.Clear();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }

        StartCoroutine(DecreaseRowWithNullsCoroutine());
    }

    private IEnumerator DecreaseRowCoroutine()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
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

    private IEnumerator DecreaseRowWithNullsCoroutine()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if the current spot is not blank or empty.....
                if (!blankSpaces[i, j] && allDots[i, j] == null && !concreteTiles[i, j] && !slimeTiles[i, j])
                {
                    //loop from the space above to the top of column
                    for (int k = j + 1; k < height; k++)
                    {
                        //if dot is found
                        if (allDots[i, k] != null)
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


    #endregion

    #region Generate Tiles yo


    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
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

    private void GenerateLockTiles()
    {
        //look at all tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if tile is a LockTile
            if (boardLayout[i].tileKind == TileKind.Lock)
            {
                //create the breakable
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateConcreteTiles()
    {
        //look at all tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if tile is a concreteTile
            if (boardLayout[i].tileKind == TileKind.Concrete)
            {
                //create the breakable
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateSlimeTiles()
    {
        //look at all tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if tile is a concreteTile
            if (boardLayout[i].tileKind == TileKind.Slime)
            {
                //create the breakable
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(slimeTilePrefab, tempPosition, Quaternion.identity);
                slimeTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }


    #endregion

    
    public void BombRow(int row)
    {
        for (int i = 0; i < width; i++)
        {            
                if(concreteTiles[i, row])
                {
                    concreteTiles[i, row].TakeDamage(1);

                if(concreteTiles[i, row].hitPoints <= 0)
                {
                    concreteTiles[i, row] = null;
                }                
            }
        }
    }

    public void BombColumn(int column)
    {
        for (int i = 0; i < width; i++)
        {
            if (concreteTiles[column, i])
                {
                    concreteTiles[column, i].TakeDamage(1);

                    if (concreteTiles[column, i].hitPoints <= 0)
                    {
                        concreteTiles[column, i] = null;
                    }
                }            
        }
    }

    private void CheckToMakeSlime()
    {
        //check the slime tiles array
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(slimeTiles[i, j] != null && makeSlime)
                {
                    // call another method to make the slime
                    MakeNewSlime();

                }
            }
        }
        return;
    }

    private Vector2 CheckForAdjacentPieces(int column, int row)
    {
        if(column < width - 1 && allDots[column + 1, row])
        {
            return Vector2.right;
        }
        if (column > 0 && allDots[column - 1, row])
        {
            return Vector2.left;
        }
        if (row < height - 1 && allDots[column, row + 1])
        {
            return Vector2.up;
        }
        if (row > 0 && allDots[column, row - 1])
        {
            return Vector2.down;
        }
        return Vector2.zero;
    }

    private void MakeNewSlime()
    {
        bool slime = false;
        int loops = 0;
        while (!slime && loops < 200)
        {
            int newX = Random.Range(0, width);
            int newY = Random.Range(0, height);
            if(slimeTiles[newX, newY])
            {
                Vector2 adjacent = CheckForAdjacentPieces(newX, newY);
                if(adjacent != Vector2.zero)
                {
                    Destroy(allDots[newX + (int)adjacent.x, newY + (int)adjacent.y]);
                    Vector2 tempPostion = new Vector2(newX + (int)adjacent.x, newY + (int)adjacent.y);
                    GameObject tile = Instantiate(slimeTilePrefab, tempPostion, Quaternion.identity);
                    slimeTiles[newX + (int)adjacent.x, newY + (int)adjacent.y] = tile.GetComponent<BackgroundTile>();
                    slime = true;
                }
            }
            loops++;
        }
    }

}
