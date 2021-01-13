using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public int hitPoints;
    private SpriteRenderer sprite;
    private GoalManager goalManager;


    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        goalManager = FindObjectOfType<GoalManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(hitPoints <= 0)
        {
            if(goalManager != null)
            {
                goalManager.CompareGoal(this.gameObject.tag);
                goalManager.UpdateGoalsText();
            }
            Destroy(this.gameObject);
        }
    }

   
    public void TakeDamage(int damageToTake)
    {
        hitPoints -= damageToTake;
        MakeLighter();
    }

    private void MakeLighter()
    {
        //take current color
        Color color = sprite.color;
        //take alpha value
        float newAlpha = color.a * 0.5f;

        sprite.color = new Color(color.r, color.g, color.b, newAlpha);
    }

}
