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
                    if(i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        GameObject rightDot = board.allDots[i + 1, j];
                        if (leftDot != null && rightDot != null)
                        {
                            if(leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                if(currentDot.GetComponent<Dots>().isRowBomb || leftDot.GetComponent<Dots>().isRowBomb || rightDot.GetComponent<Dots>().isRowBomb)
                                {
                                    currentMatches.Union(GetRowPieces(j)); // adds row bomb
                                }
                                if (currentDot.GetComponent<Dots>().isColumnBomb)
                                {
                                    currentMatches.Union(GetColumnPieces(i));
                                }
                                if (leftDot.GetComponent<Dots>().isColumnBomb)
                                {
                                    currentMatches.Union(GetColumnPieces(i - 1));
                                }
                                if (rightDot.GetComponent<Dots>().isColumnBomb)
                                {
                                    currentMatches.Union(GetColumnPieces(i + 1));
                                }


                                if (!currentMatches.Contains(leftDot))
                                {
                                    currentMatches.Add(leftDot);
                                }
                                if (!currentMatches.Contains(rightDot))
                                {
                                    currentMatches.Add(rightDot);
                                }
                                if (!currentMatches.Contains(currentDot))
                                {
                                    currentMatches.Add(currentDot);
                                }
                                leftDot.GetComponent<Dots>().isMatched = true;
                                rightDot.GetComponent<Dots>().isMatched = true;
                                currentDot.GetComponent<Dots>().isMatched = true;
                            }
                        }
                    }

                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];
                        GameObject downDot = board.allDots[i, j - 1];
                        if (upDot != null && downDot != null)
                        {
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                if (currentDot.GetComponent<Dots>().isColumnBomb || upDot.GetComponent<Dots>().isColumnBomb || downDot.GetComponent<Dots>().isColumnBomb)
                                {
                                    currentMatches.Union(GetColumnPieces(i)); // adds column bomb
                                }
                                if (currentDot.GetComponent<Dots>().isRowBomb)
                                {
                                    currentMatches.Union(GetRowPieces(j));
                                }
                                if (upDot.GetComponent<Dots>().isRowBomb)
                                {
                                    currentMatches.Union(GetRowPieces(j + 1));
                                }
                                if (downDot.GetComponent<Dots>().isRowBomb)
                                {
                                    currentMatches.Union(GetRowPieces(j - 1));
                                }

                                if (!currentMatches.Contains(upDot))
                                {
                                    currentMatches.Add(upDot);
                                }
                                if (!currentMatches.Contains(downDot))
                                {
                                    currentMatches.Add(downDot);
                                }
                                if (!currentMatches.Contains(currentDot))
                                {
                                    currentMatches.Add(currentDot);
                                }
                                upDot.GetComponent<Dots>().isMatched = true;
                                downDot.GetComponent<Dots>().isMatched = true;
                                currentDot.GetComponent<Dots>().isMatched = true;
                            }
                        }
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
                //decide what kind of bomb to make
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
            }
            else if (board.currentDot.otherDot != null)
            {
                //is the other piece matched
            }
        }
    }




}
