using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BlankGoal
{
    public int numberNeeded;
    public int numberCollected;

    public Sprite goalSprite;

    public string matchValue;
}


public class GoalManager : MonoBehaviour
{
    public BlankGoal[] levelGoals;

    public GameObject goalPrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;


    // Start is called before the first frame update
    void Start()
    {
        SetupIntroGoals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetupIntroGoals()
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            //create a new goal panel at the goalIntroParent position
            GameObject goal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform);
            //set the image of the goal
            GoalPanel panel = goal.GetComponent<GoalPanel>();
            panel.thisSprite = levelGoals[i].goalSprite;
            panel.thisString = "0/" + levelGoals[i].numberNeeded;

            //create a new goal panel at the goal topUI position
            GameObject gameGoal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            gameGoal.transform.SetParent(goalGameParent.transform);
        }
    }


}
