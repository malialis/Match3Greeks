using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FindMatches : MonoBehaviour
{
    private TaftBoard board;

    public List<GameObject> currentMatches = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<TaftBoard>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCoroutine());
    }

    private List<GameObject> IsRowBomb(Dots dot01, Dots dot02, Dots dot03)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot01.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot01.row));
        }
        if (dot02.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot02.row));
        }
        if (dot03.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot03.row));
        }
        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dots dot01, Dots dot02, Dots dot03)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot01.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot01.column, dot01.row));
        }
        if (dot02.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot02.column, dot02.row));
        }
        if (dot03.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot03.column, dot03.row));
        }
        return currentDots;
    }

    private List<GameObject> IsAdjacentBomb(Dots dot01, Dots dot02, Dots dot03)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot01.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot01.column));
        }
        if (dot02.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot02.column));
        }
        if (dot03.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot03.column));
        }
        return currentDots;
    }


    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dots>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot01, GameObject dot02, GameObject dot03)
    {
        AddToListAndMatch(dot01);
        AddToListAndMatch(dot02);
        AddToListAndMatch(dot03);
    }

    private IEnumerator FindAllMatchesCoroutine()
    {
        yield return new WaitForSeconds(.2f);
        for(int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];               

                if(currentDot != null)
                {
                    Dots currentDotDot = currentDot.GetComponent<Dots>();

                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];                        
                        GameObject rightDot = board.allDots[i + 1, j];                                               

                        if (leftDot != null && rightDot != null)
                        {
                            Dots rightDotDot = rightDot.GetComponent<Dots>();
                            Dots leftDotDot = leftDot.GetComponent<Dots>();

                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot)); // adds row bomb
                                currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot)); //checks if it is colomn bomb
                                currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot)); //checks if it is adjacent bomb

                                GetNearbyPieces(leftDot, currentDot, rightDot);                                                     
                                
                            }
                        }
                    }

                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];                        
                        GameObject downDot = board.allDots[i, j - 1];                        

                        if (upDot != null && downDot != null)
                        {
                            Dots downDotDot = downDot.GetComponent<Dots>();
                            Dots upDotDot = upDot.GetComponent<Dots>();

                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot)); // adds column bomb
                                currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot)); // checks if it is a column bomb
                                currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot)); // checks if it is a adjacent bomb

                                GetNearbyPieces(upDot, currentDot, downDot);
                            }
                        }
                    }
                }
            }
        }
    }

    public void MatchPiecesOfColor(string color)
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                //check to see if piece exists
                if (board.allDots[i, j] != null)
                {
                    // check the tag
                    if(board.allDots[i, j].tag == color)
                    {
                        //set that dot to be the match
                        board.allDots[i, j].GetComponent<Dots>().isMatched = true;
                    }
                }
            }
        }
    }

    private List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for(int i = 0; i < board.height; i++)
        {
            if(board.allDots[column, i] != null)
            {
                dots.Add(board.allDots[column, i]);
                board.allDots[column, i].GetComponent<Dots>().isMatched = true;
               // Debug.Log("Column match Found yo");
            }
        }

        return dots;
    }

    private List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                dots.Add(board.allDots[i, row]);
                board.allDots[i, row].GetComponent<Dots>().isMatched = true;
               // Debug.Log("Row match Found yo");
            }
        }

        return dots;
    }

    public void CheckBombs()
    {
        //did the player move a dot
        if(board.currentDot != null)
        {
            //is piece moved matched
            if (board.currentDot.isMatched)
            {
                //make it unmatched so it does not get destroyed
                board.currentDot.isMatched = false;
                // determin swipe angel to decide bomb type
                if((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) ||
                    board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135)
                {
                    //make row bomb
                    board.currentDot.MakeRowBomb();
                    // this is a left or right swipe
                }
                else
                {
                    //make a column bomb
                    board.currentDot.MakeColumnBomb();
                }

               /* //decide what kind of bomb to make
                int typeOfBomb = Random.Range(0, 100);
                if(typeOfBomb < 50)
                {
                    //make row bomb
                    board.currentDot.MakeRowBomb();
                }
                else if(typeOfBomb >= 50)
                {
                    //make a column bomb
                    board.currentDot.MakeColumnBomb();
                }
               */


            }
            else if (board.currentDot.otherDot != null)
            {
                //is the other piece matched
                Dots otherDot = board.currentDot.otherDot.GetComponent<Dots>();
                if (otherDot.isMatched)
                {
                    //is the other dot matched and then make it unmatched
                    otherDot.isMatched = false;
                    // decide what bomb to make

                    if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) ||
                    board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135)
                    {
                        //make row bomb
                        otherDot.MakeRowBomb();
                        // this is a left or right swipe
                    }
                    else
                    {
                        //make a column bomb
                        otherDot.MakeColumnBomb();
                    }

                    /*
                    int typeOfBomb = Random.Range(0, 100);
                    if (typeOfBomb < 50)
                    {
                        //make row bomb
                        otherDot.MakeRowBomb();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        //make a column bomb
                        otherDot.MakeColumnBomb();
                    }
                    */
                }
            }
        }
    }


    private List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = column -1; i <= column +1; i++)
        {
            for(int j = row -1; j <= row + 1; j++)
            {
                //check if inside board
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    dots.Add(board.allDots[i, j]);
                    board.allDots[i, j].GetComponent<Dots>().isMatched = true; 
                }
            }
            
        }

        return dots;
    }


}
