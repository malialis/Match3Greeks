using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    private TaftBoard board;
    public float hintDelay;
    private float hintDelaySeconds;
    public GameObject hintParticle;
    public GameObject currentHint;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<TaftBoard>();
        hintDelaySeconds = hintDelay;
    }

    // Update is called once per frame
    void Update()
    {
        hintDelaySeconds -= Time.deltaTime;
        if (hintDelaySeconds <= 0 && currentHint == null)
        {
            MarkHintOnBoard();
            hintDelaySeconds = hintDelay;
        }
    }

    // find all possible matches on the board
    private List<GameObject> FindAllMatchesForHints()
    {
        List<GameObject> possibleMoves = new List<GameObject>();

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allDots[i, j] != null)
                {
                    if (i < board.width - 1)
                    {
                        if (board.SwitchAndCheckPieces(i, j, Vector2.right))
                        {
                            possibleMoves.Add(board.allDots[i, j]);
                        }
                    }
                    if (j < board.height - 1)
                    {
                        if (board.SwitchAndCheckPieces(i, j, Vector2.up))
                        {
                            possibleMoves.Add(board.allDots[i, j]);
                        }
                    }
                }
            }
        }
        return possibleMoves;
    }
    //pick one randomly
   private GameObject PickOneRandomly()
    {
        List<GameObject> possibleMoves = new List<GameObject>();

        possibleMoves = FindAllMatchesForHints();
        if(possibleMoves.Count > 0)
        {
            int pieceToUse = Random.Range(0, possibleMoves.Count);
            return possibleMoves[pieceToUse];
        }
        return null;
    }
    //create the hint on the chosen match
    private void MarkHintOnBoard()
    {
        GameObject move = PickOneRandomly();
        if(move != null)
        {
            currentHint = Instantiate(hintParticle, move.transform.position, Quaternion.identity);
            
        }
    }
    //destroy the hint
    public void DestroyHint()
    {
        if(currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
            hintDelaySeconds = hintDelay;
        }
    }






}
