using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedGameLogic : MonoBehaviour
{
    private bool isGameActive = false;
    private int score = 0;

    public bool IsGameActive => isGameActive;
    public int Score => score;

    public Boolean isFoodSuitable(Food food)
    {
        return food.IsSuitable();
    }


}