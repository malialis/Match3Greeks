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
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    public float swipeAngle = 0;

    

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<TaftBoard>();
        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        row = targetY;
        column = targetX;
        previousRow = row;
        previousColumn = column;
    }

    // Update is called once per frame
    void Update()
    {
        FindMatches();

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
        }
        else
        {
            //directly set the positon
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1)
        {
            //move towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.4f);
        }
        else
        {
            //directly set the positon
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }
    }


    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
       // Debug.Log(firstTouchPosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        Debug.Log(swipeAngle);
        MovePieces();
    }

    private void MovePieces()
    {
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width -1) // right swipe
        {
            otherDot = board.allDots[column + 1, row];
            otherDot.GetComponent<Dots>().column -= 1;
            column += 1;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height -1) // up swipe
        {
            otherDot = board.allDots[column, row + 1];
            otherDot.GetComponent<Dots>().row -= 1;
            row += 1;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) // left swipe
        {
            otherDot = board.allDots[column - 1, row];
            otherDot.GetComponent<Dots>().column += 1;
            column -= 1;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) // down swipe
        {
            otherDot = board.allDots[column, row - 1];
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

            if(leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
            {
                leftDot1.GetComponent<Dots>().isMatched = true;
                rightDot1.GetComponent<Dots>().isMatched = true;
                isMatched = true;
            }
        }

        if (row > 0 && row < board.height - 1)
        {
            // checking up
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];

            if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
            {
                upDot1.GetComponent<Dots>().isMatched = true;
                downDot1.GetComponent<Dots>().isMatched = true;
                isMatched = true;
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
            }
            otherDot = null;
        }
    }

}
