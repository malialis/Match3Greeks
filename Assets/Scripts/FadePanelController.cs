using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanelController : MonoBehaviour
{
    public Animator panelAnim;
    public Animator gameInfoAnim;


    public void OK()
    {
        if(panelAnim != null && gameInfoAnim != null)
        {
            panelAnim.SetBool("Out", true);
            gameInfoAnim.SetBool("Out", true);
            StartCoroutine(GameStartCoroutine());
        }
        
    }

    private IEnumerator GameStartCoroutine()
    {
        yield return new WaitForSeconds(1f);
        TaftBoard board = FindObjectOfType<TaftBoard>();

        board.currentState = GameState.move;
    }

    public void GameOver()
    {
        panelAnim.SetBool("Out", false);
        panelAnim.SetBool("GameOver", true);
    }


}
