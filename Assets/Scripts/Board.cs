using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    // Board Setup on startUp
    public int width;
    public int height;
    public int borderSize;
    public int fillYOffset = 10;

    public float swapTime = 0.5f;
    public float fillMovetime = 0.5f;

    //Tile Prefabs sources
    public GameObject tileNormalPrefab;
    public GameObject tileObstaclePrefab;
    public GameObject[] gamePiecePrefabs;

    //Bomb Prefabs sources
    public GameObject adjacentBombPrefab;
    public GameObject columnBombPrefab;
    public GameObject rowBombPrefab;
    public GameObject colorBombPrefab;

    // collectable stuff
    public int maxCollectibles = 3;
    public int currentCollectibleCount = 0;
    public GameObject[] collectiblePrefabs;

    [Range(0, 1)]
    public float chanceForCollectible = 0.1f;

    //Scores
    private int m_scoreMultiplier = 0;

    private GameObject m_clickedTileBomb;
    private GameObject m_targetTileBomb;

    private Tile[,] m_allTiles;
    private Tile m_clickedTile;
    private Tile m_targetTile;

    private bool m_playerInutEnabled = true;

    private GamePiece[,] m_allGamePieces;

    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;

    private ParticleManager m_particleManager;

    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;

        public int x;
        public int y;
        public int z;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        //SetupBoard();
        m_particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();
    }

    public void SetupBoard()
    {
        SetupTiles();
        SetupGamePieces();
        SetupCamera();

        List<GamePiece> startingCollectibles = FindAllCollectibles();
        currentCollectibleCount = startingCollectibles.Count;

        FillBoard(fillYOffset, fillMovetime);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

 #region  Methods

    private void SetupTiles() 
    {
        foreach(StartingObject sTile in startingTiles)
        {
            if(sTile != null)
            {
                MakeTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
            }
        }
        for(int i = 0; i < width; i++)
            {
              for (int j = 0; j < height; j++)
                {
                  if(m_allTiles[i, j] == null)
                   {
                    // setup all the tiles that make up the background
                    MakeTile(tileNormalPrefab, i, j);
                   }
                
                }
            }
    }

    private void SetupGamePieces()
    {
        foreach(StartingObject sPiece in startingGamePieces)
        {
            if(sPiece != null)
            {
                GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity) as GameObject;
                MakeGamePiece(piece, sPiece.x, sPiece.y, fillYOffset, fillMovetime);
            }
        }
    }

    private void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;

            tile.name = "Tile (" + x + "," + y + ")";

            m_allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;
            m_allTiles[x, y].Init(x, y, this);
        }         
    }

    private void MakeGamePiece(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f )
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            prefab.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);

            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
            }

            prefab.transform.parent = transform;
        }
    }

    private GameObject MakeBomb (GameObject prefab, int x, int y)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
            bomb.GetComponent<Bomb>().Init(this);
            bomb.GetComponent<Bomb>().SetCoord(x, y);
            bomb.transform.parent = transform;
            return bomb;
        }
        return null;
    }

    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width -1 ) / 2f, (float)(height -1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)height / 2f + (float) borderSize;
        float horizontalSize = ((float) width / 2f + (float)borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize: horizontalSize;
    }

    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    private bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }
        return false;
    }

    private bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if(xStart == -1 || yStart == -1)
                {
                    xStart = piece.xIndex;
                    yStart = piece.yIndex;
                    continue;
                }

                if (piece.xIndex != xStart && piece.yIndex == yStart)
                {
                    horizontal = true;
                }

                if (piece.xIndex == xStart && piece.yIndex != yStart)
                {
                    vertical = true;
                }
            }
        }
        return (horizontal && vertical);
    }


    //general search method, specify a starting coordinate (startX, startY) and use a Vector2 for direction
    //(1,0) = right (-1,0) = left, (0,1) = up, (0, -1) = down, minLength is the minimum number to be a match

    private GameObject GetRandomObject(GameObject[] objectArray)
    {
        int randomIdx = Random.Range(0, objectArray.Length);
        if (objectArray[randomIdx] == null)
        {
            Debug.LogWarning("BOARD.GetRandomObject at indext " + randomIdx + " Does not contain a valid GameObject");
        }
        return objectArray[randomIdx];
    }

    private GameObject GetRandomGamePiece()
    {
       return GetRandomObject(gamePiecePrefabs);
    }

    private GameObject GetRandomCollectible()
    {
        return GetRandomObject(collectiblePrefabs);
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if(gamePiece == null){
            Debug.LogWarning("Board: invalid GamePiece");
            return;
        }
        gamePiece.transform.position = new Vector3(x,y,0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x,y);
    }

    private void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxIterations = 100;
        int iterations = 0;

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(m_allGamePieces[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle)
                {
                    GamePiece piece = null;

                    if(j == height - 1 && CanAddCollectable())
                    {
                        piece = FillRandomCollectibleAt(i, j, falseYOffset, moveTime);
                        currentCollectibleCount++;
                    }
                    else
                    {
                        piece = FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                        iterations = 0;

                        while (HasMatchOnFill(i, j))
                        {
                            ClearPieceAt(i, j);
                            piece = FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                            iterations++;
                            Debug.Log("Iterations Count " + iterations);

                            if (iterations >= maxIterations)
                            {
                                Debug.Log("Iterations Count " + iterations);
                                Debug.Log("Break iterations -----------");
                                break;
                            }
                        }
                    }
                        
                }
               
            }
        }
    }

    private GamePiece FillRandomGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if(IsWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);

            return randomPiece.GetComponent<GamePiece>();
        }

        return null;
    }

    private GamePiece FillRandomCollectibleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (IsWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);

            return randomPiece.GetComponent<GamePiece>();
        }

        return null;
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if(leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if(downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }


    #endregion

 #region tileClickAndReleseYo!

    public void ClickTile(Tile tile)
    {
        if(m_clickedTile == null)
        {
            m_clickedTile = tile;
            //Debug.Log("Clicked tile: " + tile.name);
        }
    }

    public void DragToTile(Tile tile)
    {
        if(m_clickedTile != null && IsNextTo(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if(m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        m_clickedTile = null;
        m_targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));     

    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_playerInutEnabled)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

            if (targetPiece != null && clickedPiece != null)
            {
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                List<GamePiece> colorMatches = new List<GamePiece>();

                if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
                {
                    clickedPiece.matchValue = targetPiece.matchValue;
                    colorMatches = FindAllMatchValue(clickedPiece.matchValue);
                }
                else if (!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                {
                    targetPiece.matchValue = clickedPiece.matchValue;
                    colorMatches = FindAllMatchValue(targetPiece.matchValue);
                }
                else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                {
                    foreach (GamePiece piece in m_allGamePieces)
                    {
                        if (!colorMatches.Contains(piece))
                        {
                            colorMatches.Add(piece);
                        }
                    }
                }

                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                    //error swap sound
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayErrorSound();
                    }
                }
                else
                {
                    if(GameManager.Instance != null)
                    {
                        GameManager.Instance.movesLeft--;
                        GameManager.Instance.UpdateMoves();
                    }

                    yield return new WaitForSeconds(swapTime);
                    Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);
                    m_clickedTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIndex, swipeDirection, clickedPieceMatches);
                    m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swipeDirection, targetPieceMatches);

                    if (m_clickedTileBomb != null && targetPiece != null)
                    {
                        GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(clickedBombPiece))
                        {
                            clickedBombPiece.ChangeColor(targetPiece);
                        }
                        
                    }
                    if (m_targetTileBomb != null && clickedPiece != null)
                    {
                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(targetBombPiece))
                        {
                            targetBombPiece.ChangeColor(clickedPiece);
                        }
                        
                    }

                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList());
                    //ClearPieceAt(clickedPieceMatches);
                    //ClearPieceAt(targetPieceMatches);
                    //CollapseColumn(clickedPieceMatches);
                    //CollapseColumn(targetPieceMatches);

                }
            }
        }
        
    }

    #endregion

 #region FindingMatchesYo!

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }
        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;
        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if(!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];

            if(nextPiece == null)
            {
                break;
            }

            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.None)
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                } 
            }

            
        }
        if(matches.Count >= minLength)
        {
            return matches;
        }
        return null;

    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if(upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;


    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }
        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;


    }

    private void HightlightMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);

            }
        }
    }

    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLength);

        if (horizontalMatches == null)
        {
            horizontalMatches = new List<GamePiece>();
        }
        if (verticalMatches == null)
        {
            verticalMatches = new List<GamePiece>();
        }
        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;

    }

    List<GamePiece> FindMatchesAt (List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }
        return matches;
    }

    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }

    private void HighlightTileOff(int x, int y)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
        
    }

    private void HightlightTileOn(int x, int y, Color color)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = color;
        }
    }

    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);

        var combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HightlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach(GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                HightlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);

            }
        }
    }

    #endregion

 #region ClearAndFillBoards

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePieces[x, y];

        if(pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
       // HighlightTileOff(x, y);
    }

    private void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    private void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);

                int bonus = 0;

                if(gamePieces.Count >= 4)
                {
                    bonus = 20;
                }
                piece.ScorePoints(m_scoreMultiplier, bonus);

                if (m_particleManager != null)
                {
                    if (bombedPieces.Contains(piece))
                    {
                        m_particleManager.BombFXAt(piece.xIndex, piece.yIndex);
                    }
                    else
                    {
                        m_particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
                    }
                    
                }
            }            
        }
    }

    private void BreakTilesAt(int x, int y)
    {
        Tile tileToBreak = m_allTiles[x, y];

        if(tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            if(m_particleManager != null)
            {
                m_particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y, 0);
            }

            tileToBreak.BreakTile();
        }
    }

    private void BreakTilesAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                BreakTilesAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height -1; i++)
        {
            if(m_allGamePieces[column, i] == null)
            {
                for(int j = i + 1; j < height; j++)
                {
                    if(m_allGamePieces[column, j] != null && m_allTiles[column, i].tileType != TileType.Obstacle)
                    {
                        //this moves the exisiting piece down to the empty spot.
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));

                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);

                        if(!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }

                        m_allGamePieces[column, j] = null;

                        break;
                    }
                }
            }
        }

        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach(int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;

    }

    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece piece in gamePieces)
        {
            if (!columns.Contains(piece.xIndex))
            {
                columns.Add(piece.xIndex);
            }
        }
        return columns;
    }

    private void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        //Clear and collapse the board temp disable input

        m_playerInutEnabled = false;
        List<GamePiece> matches = gamePieces;

        m_scoreMultiplier = 0;

        do
        {
            m_scoreMultiplier++;

            //clear and collapse
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;

            //refill the empty tiles
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();

            yield return new WaitForSeconds(0.25f);
        }
        while (matches.Count != 0);

        m_playerInutEnabled = true;

    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighlightPieces(gamePieces);

        yield return new WaitForSeconds(0.2f);

        bool isFinished = false;

        while (!isFinished)
        {
            //find pieces affected by the bombs
            List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);
            List<GamePiece> allCollectables = FindAllCollectibles();
            List<GamePiece> blockers = gamePieces.Intersect(allCollectables).ToList();
            collectedPieces = collectedPieces.Union(blockers).ToList();

            currentCollectibleCount -= collectedPieces.Count;

            gamePieces = gamePieces.Union(collectedPieces).ToList();

            ClearPieceAt(gamePieces, bombedPieces);
            BreakTilesAt(gamePieces);

            // add bombs to board here yo
            if(m_clickedTileBomb != null)
            {
                ActivateBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }
            if(m_targetTileBomb != null)
            {
                ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }

            yield return new WaitForSeconds(0.25f);

            movingPieces = CollapseColumn(gamePieces);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            matches = FindMatchesAt(movingPieces);
            collectedPieces = FindCollectiblesAt(0, true);
            matches = matches.Union(collectedPieces).ToList();

            if(matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                m_scoreMultiplier++;

                if(SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBonusSound();
                }

                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(fillYOffset, fillMovetime);
        yield return null;
    }

    private bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    #endregion

 #region BombLocatorYo

    List<GamePiece> GetRowPieces (int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for(int i = 0; i < width; i++)
        {
            if(m_allGamePieces[i, row] != null)
            {
                gamePieces.Add(m_allGamePieces[i, row]);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetColumnPieces (int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            if (m_allGamePieces[column, i] != null)
            {
                gamePieces.Add(m_allGamePieces[column, i]);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetAdjacentPieces (int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if (IsWithinBounds(i, j))
                {
                    gamePieces.Add(m_allGamePieces[i, j]);
                }
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();

                Bomb bomb = piece.GetComponent<Bomb>();

                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:
                            // add code for color soon
                            break;
                    }
                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                    allPiecesToClear = RemoveCollectibles(allPiecesToClear);
                }
            }
        }
        return allPiecesToClear;
    }

    private GameObject DropBomb (int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
    {
        GameObject bomb = null;

        if(gamePieces.Count >= 4)
        {
            if (IsCornerMatch(gamePieces))
            {
                // drop adjacent bomb
                if(adjacentBombPrefab != null)
                {
                    bomb = MakeBomb(adjacentBombPrefab, x, y);
                }
            }
            else
            {
                if (gamePieces.Count >= 5)
                {
                    if (colorBombPrefab != null)
                    {
                        bomb = MakeBomb(colorBombPrefab, x, y);
                    }
                }
                else
                {
                    // row bomb prefab
                    if (swapDirection.x != 0)
                    {
                        if (rowBombPrefab != null)
                        {
                            bomb = MakeBomb(rowBombPrefab, x, y);
                        }
                    }
                    else
                    {
                        // drop column bomb
                        if (columnBombPrefab != null)
                        {
                            bomb = MakeBomb(columnBombPrefab, x, y);
                        }
                    }
                }
                
            }
        }

        return bomb;
    }

    private void ActivateBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if(IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
        }
    }

    List<GamePiece> FindAllMatchValue (MatchValue mValue)
    {
        List<GamePiece> foundPieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] != null)
                {
                    if (m_allGamePieces[i, j].matchValue == mValue)
                    {
                        foundPieces.Add(m_allGamePieces[i, j]);
                    }
                }
            }
        }
        return foundPieces;
    }

    private bool IsColorBomb (GamePiece gamePiece)
    {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if (bomb != null)
        {
            return (bomb.bombType == BombType.Color);
        }
        return false;
    }

    #endregion

#region collectibleStuff

    List<GamePiece> FindCollectiblesAt(int row, bool clearedAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if (m_allGamePieces[i, row] != null)
            {
                Collectible collectibleComponent = m_allGamePieces[i, row].GetComponent<Collectible>();

                if (collectibleComponent != null)
                {
                    if (!clearedAtBottomOnly || (clearedAtBottomOnly && collectibleComponent.clearedAtBottom))
                    {
                        foundCollectibles.Add(m_allGamePieces[i, row]);
                    }
                    
                }
            }
        }
        return foundCollectibles;
    }

    List<GamePiece> FindAllCollectibles()
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            List<GamePiece> collectibleRow = FindCollectiblesAt(i);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }
        return foundCollectibles;
    }

    private bool CanAddCollectable()
    {
        return (Random.Range(0f, 1f) <= chanceForCollectible && collectiblePrefabs.Length > 0 && currentCollectibleCount < maxCollectibles);
    }

    List<GamePiece> RemoveCollectibles (List<GamePiece> bombedPieces)
    {
        List<GamePiece> collectiblePieces = FindAllCollectibles();
        List<GamePiece> piecesToRemove = new List<GamePiece>();

        foreach (GamePiece piece in collectiblePieces)
        {
            Collectible collectibleComponent = piece.GetComponent<Collectible>();

            if (collectiblePieces != null)
            {
                if (!collectibleComponent.clearedByBomb)
                {
                    piecesToRemove.Add(piece);
                }
            }
        }
        return bombedPieces.Except(piecesToRemove).ToList();
    }

    #endregion





}
