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

    private GameObject otherDot;
    private TaftBoard board;
    private FindMatches findMatches;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [Header("Swipe Variables")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("PowerUp Variables")]
    public bool isColumnBomb;
    public bool isRowBomb;
    public GameObject rowBomb;
    public GameObject colomnBomb;


    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<TaftBoard>();
        findMatches = FindObjectOfType<FindMatches>();

        isColumnBomb = false;
        isRowBomb = false;

        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row = targetY;
        //column = targetX;
        //previousRow = row;
        //previousColumn = column;
    }

    //this is for testing and debug yo
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowBomb, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }



    // Update is called once per frame
    void Update()
    {
       
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, 0.2f);
        }
        targetX = column;
        targetY = row;
        if(Mathf.Abs(targetX - transform.position.x) > 0.1)
        {
            //move towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.4f);
            if(board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            //directly set the positon
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            //board.allDots[column, row] = this.gameObject;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1)
        {
            //move towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.4f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();

        }
        else
        {
            //directly set the positon
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
           // board.allDots[column, row] = this.gameObject;
        }
    }


    private void OnMouseDown()
    {
        if(board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Debug.Log(firstTouchPosition);
        }

    }

    private void OnMouseUp()
    {
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
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            Debug.Log(swipeAngle);
            MovePieces();
            board.currentState = GameState.wait;
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
            otherDot = board.allDots[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dots>().column -= 1;
            column += 1;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height -1) // up swipe
        {
            otherDot = board.allDots[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dots>().row -= 1;
            row += 1;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) // left swipe
        {
            otherDot = board.allDots[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dots>().column += 1;
            column -= 1;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) // down swipe
        {
            otherDot = board.allDots[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dots>().row += 1;
            row -= 1;
        }
        StartCoroutine(CheckMoveCoroutine());

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
        yield return new WaitForSeconds(0.25f);
        if(otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dots>().isMatched)
            {
                otherDot.GetComponent<Dots>().row = row;
                otherDot.GetComponent<Dots>().column = column;
                row = previousRow;
                column = previousColumn;

                yield return new WaitForSeconds(0.4f);
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();                
            }
            otherDot = null;
        }
        
    }



}
