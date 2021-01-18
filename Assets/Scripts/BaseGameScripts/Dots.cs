using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dots : MonoBehaviour
{
    [Header("Board Variables")]
    public int column; // x position
    public int row; // y position
    public bool isMatched = false;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;

    public GameObject otherDot;
    private TaftBoard board;
    private Animator anim;
    private SoundManager soundManager;
    public float shineDelay;
    private float shineDelaySeconds;
    private HintManager hintManager;
    private FindMatches findMatches;
    private EndGameManager endGameManager;
    private Vector2 firstTouchPosition = Vector2.zero;
    private Vector2 finalTouchPosition = Vector2.zero;
    private Vector2 tempPosition;

    [Header("Swipe Variables")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("PowerUp Variables")]
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isColorBomb;
    public bool isAdjacentBomb;
    public GameObject rowBomb;
    public GameObject columnBomb;
    public GameObject adjacentBomb;
    public GameObject colorBomb;


    // Start is called before the first frame update
    void Start()
    {
        //board = FindObjectOfType<TaftBoard>();
        board = GameObject.FindWithTag("Board").GetComponent<TaftBoard>();
        //possibly slightly faster searching / finding it with the tag.

        anim = GetComponent<Animator>();
        findMatches = FindObjectOfType<FindMatches>();
        hintManager = FindObjectOfType<HintManager>();
        endGameManager = FindObjectOfType<EndGameManager>();
        soundManager = FindObjectOfType<SoundManager>();

        isAdjacentBomb = false;
        isColumnBomb = false;        
        isColorBomb = false;
        isRowBomb = false;

        shineDelay = Random.Range(3f, 6f);
        shineDelaySeconds = shineDelay;
    }

    


    // Update is called once per frame
    void Update()
    {
        shineDelaySeconds -= Time.deltaTime;
        if(shineDelaySeconds <= 0)
        {
            shineDelaySeconds = shineDelay;
            StartCoroutine(StartShineCoroutine());
        }

        targetX = column;
        targetY = row;
        MoveDotOnSwipeYo();
    }


    private void OnMouseDown()
    {
        if(anim != null)
        {
            anim.SetBool("Touched", true);
        }
        if(hintManager != null)
        {
            hintManager.DestroyHint();
        }

        if(board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

    }

    private void OnMouseUp()
    {
        anim.SetBool("Touched", false);

        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
            
    }

    private void CalculateAngle()
    {
        if(Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();            
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;            
        }
        
    }

    private void MoveActualPieces(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;

        if(board.lockTiles[column, row] == null && board.lockTiles[column + (int)direction.x, row + (int)direction.y] == null)
        {
            if (otherDot != null)
            {
                otherDot.GetComponent<Dots>().column += -1 * (int)direction.x;
                otherDot.GetComponent<Dots>().row += -1 * (int)direction.y;
                column += (int)direction.x;
                row += (int)direction.y;
                StartCoroutine(CheckMoveCoroutine());
            }
            else
            {
                board.currentState = GameState.move;
            }
        }
        else
        {
            board.currentState = GameState.move;
        }

    }

    private void MovePieces()
    {
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width -1) // right swipe
        {
            MoveActualPieces(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height -1) // up swipe
        {           
            MoveActualPieces(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) // left swipe
        {
            MoveActualPieces(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) // down swipe
        {
            MoveActualPieces(Vector2.down);
        }
        //StartCoroutine(CheckMoveCoroutine());
        else
        {
            board.currentState = GameState.move;
        }

    }

    private void FindMatches()
    {
        if(column > 0 && column < board.width - 1)
        {
            // checking left
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];

            if(leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dots>().isMatched = true;
                    rightDot1.GetComponent<Dots>().isMatched = true;
                    isMatched = true;
                }
            }
            
        }

        if (row > 0 && row < board.height - 1)
        {
            // checking up
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];

            if(upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dots>().isMatched = true;
                    downDot1.GetComponent<Dots>().isMatched = true;
                    isMatched = true;
                }
            }
            
        }
    }

    public IEnumerator CheckMoveCoroutine()
    {
        if (isColorBomb)
        {
            //this piece is the color bomb, and the other piece is the color to destroy
            findMatches.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
        }
        else if(otherDot.GetComponent<Dots>().isColorBomb)
        {
            //the other piece is a color bomb, and this piece has the color to destroy
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dots>().isMatched = true;
        }

        yield return new WaitForSeconds(0.5f);

        if(otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dots>().isMatched)
            {
                otherDot.GetComponent<Dots>().row = row;
                otherDot.GetComponent<Dots>().column = column;
                row = previousRow;
                column = previousColumn;
                board.currentDot = null;
                yield return new WaitForSeconds(0.4f);
                board.currentState = GameState.move;
                Debug.Log("Wrong Peice yo");
                soundManager.PlayRandomErrorNoise();
            }
            else
            {
                if(endGameManager != null)
                {
                    if(endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
                board.DestroyMatches();                
            }
            //otherDot = null;
        }
        
    }

    #region Making making Bombs yo

    public void MakeRowBomb()
    {
        if(!isColumnBomb && !isRowBomb && !isColorBomb)
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowBomb, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }        
    }

    public void MakeColumnBomb()
    {
        if (!isColumnBomb && !isRowBomb && !isColorBomb)
        {
            isColumnBomb = true;
            GameObject arrow = Instantiate(columnBomb, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }            
    }

    public void MakeColorBomb()
    {
        if (!isColumnBomb && !isRowBomb && !isColorBomb)
        {
            isColorBomb = true;
            GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;
            this.gameObject.tag = "Color";
        }            
    }

    public void MakeAdjacentBomb()
    {
        if (!isColumnBomb && !isRowBomb && !isColorBomb)
        {
            isAdjacentBomb = true;
            GameObject adjace = Instantiate(adjacentBomb, transform.position, Quaternion.identity);
            adjace.transform.parent = this.transform;
        }            
    }

    #endregion




    private void MoveDotOnSwipeYo()
    {
        if (Mathf.Abs(targetX - transform.position.x) > 0.1)
        {
            //move towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.4f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }            
        }
        else
        {
            //directly set the positon
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1)
        {
            //move towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.4f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }           
        }
        else
        {
            //directly set the positon
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    private IEnumerator StartShineCoroutine()
    {
        anim.SetBool("Shine", true);
        yield return null;
        anim.SetBool("Shine", false);
    }

    public void PopDeathAnimation()
    {
        anim.SetBool("Popped", true);
    }


}
