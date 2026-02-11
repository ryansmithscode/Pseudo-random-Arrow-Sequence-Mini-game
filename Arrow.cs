using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;
using Rewired;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using TMPro;

// Ryan Smith Pseudo-random Arrow Sequence Mini-game Script

public class Arrow : MonoBehaviour
{
    [Header("Score System")]
    private static int playerScore; // Current Score
    public Text scoreText;

    // Easy To Change Point Amount
    public int pointAmount;

    // Highscore
    private static int highScore;
    public Text highScoreText;

    [Header("Rewired")]
    private int playerId = 0; // Can be public 
    private Player player;
    private string[] rewiredActions = new string[4] { "Left", "Up", "Right", "Down" };

    [Header("Player Life Variables")]
    private static int playerLives; // Life Count

    public Text playerLivesText;

    [Header("LeanTween Bar Visual")]
    public GameObject leanTweenBar;
    private int time = 10;

    // Gradually Speeding Up LeanTween
    private int lastChecked = 0;
    private int successCount = 0;

    [Header("Multiplier")]
    private bool multiplierWorking = false;
    private double multiplierValue;
    public Text multiplierText;

    [Header("Core Game Loop")]
    private string[] arrowSymbols = new string[4] { "←", "↑", "→", "↓" }; // Icons to be Displayed
    private int[] order;
    private int currentIndexNum = 0;
    public TMP_Text sequenceText;

    //-----------------------------------Start is called once upon creation-------------------------
    private void Start()
    {
        int highScore = PlayerPrefs.GetInt("highScore", 0); // Setting the score

        playerLives = 3; // If set above with playerScore game will constantly generate arrows

        playerScore = 0; // Resets score 

        player = ReInput.players.GetPlayer(playerId); // Gets the controls set in Rewired's Manager

        leanTweenTimer();
        arrowSequence();
        updateText();
    }

    //-----------------------------------Update is called once per frame----------------------------
    private void Update()
    {
        if (playerLives == 0)
        {
            SceneManager.LoadScene(0); // Simple restart, once the player has run out of lives
        }

        if (playerScore > highScore)
        {
            highScore = playerScore; // New highscore
            PlayerPrefs.GetInt("highScore");
            PlayerPrefs.GetInt("highScore", highScore); // Gets the score that should be saved
            PlayerPrefs.Save();
        }

        // Displays updated versions of counts 
        highScoreText.text = highScore.ToString();
        scoreText.text = playerScore.ToString();
        playerLivesText.text = playerLives.ToString();

        if (multiplierWorking == true)
        {
            multiplierText.text = multiplierValue.ToString() + "x";
        }

        for (int i = 0; i < rewiredActions.Length; i++)
        {
            if (player.GetButtonDown(rewiredActions[i])) // Detects inputted action
            {
                OnButtonPressed(i);
            }
        }
    }

    //-----------------------------------Create Order of Arrows--------------------------
    private void arrowSequence()
    {
        int orderNum = UnityEngine.Random.Range(1, 5); // Decides amount in sequence
        order = new int [orderNum]; // Simply creates new array that can hold said amount

        List<int> indices = new List<int> { 0, 1, 2, 3 }; 
        for (int i = 0; i < orderNum; i++) // Runs for Each Arrow
        {
            int randomIndex = UnityEngine.Random.Range(0, indices.Count); // Picks position
            order[i] = indices [randomIndex]; // 'Stores'
            indices.RemoveAt(randomIndex); // Removes position (avoids overwriting)
        }

        currentIndexNum = 0;
    }

    //-----------------------------------Change the Font and Size--------------------------
    void updateText() 
    {
        string text = ""; // Text that gets displayed

        for (int i = 0; i < order.Length; i++) // Loops through each arrow in sequence again
        {
            string symbol = arrowSymbols[order[i]]; // Getting correlating symbol

            if (i == currentIndexNum)
            {
                symbol = "<color=#FFFFFF>" + symbol + "</color>"; // Makes the 'active' button stand out better
                symbol = "<size=120>" + symbol + "</size>"; // Magic number but can easily be updated, if needs be
            }

            text += symbol; // Adds to full text

            if (i < order.Length - 1)
                text += " "; // Arrow inbetween, so not at end 
        }

        sequenceText.text = text; // Obviously takes the made sequence and overwrites so can be displayed
    }

    //-----------------------------------Input Checks--------------------------
    void OnButtonPressed(int index)
    {
        if (index == order[currentIndexNum]) // if the action correlates to that position's button
        {
            multiplierWorking = true;
            multiplierValue += 0.5; // Longer to get a larger score

            currentIndexNum++; // Moves the position to the next in the array

            updateText();

            if (currentIndexNum >= order.Length)  // This is what resets everything after all buttons inputted
            {
                successCount++; // Adds 1 to help determine later if can remove from leantween bar time

                timeSpeedUp();
                scoreSystemAdd();
                leanTweenTimerReset();
                arrowSequence();
                updateText();
            }
        }

        else // Punishment for incorrect Input
        {
            scoreSystemRemove();
            resetMultiplier();
        }
    }

    //-----------------------------------Multiplier and Point System--------------------------
    public void scoreSystemAdd()
    {
        if (multiplierWorking == true)
        {
            playerScore += (int)Math.Round(pointAmount * multiplierValue); // Adding point default amount multiplied by mutliplier count  
        }

        playerScore += pointAmount;
    }

    public void scoreSystemRemove() 
    {
        if (multiplierWorking == true)
        {
            playerScore -= (int)Math.Round(pointAmount * multiplierValue); // Works with multiplier for bigger punishment

        }

        playerScore -= pointAmount;
    }
    public void resetMultiplier()
    {
        multiplierWorking = false; // This is to let score adding/removing with multiplier know
        multiplierValue = 0; 
        multiplierText.text = multiplierValue.ToString() + "x"; // Displays the amount
    }

    public void removeLife()
    {
        playerLives--; // Removes one from count

        leanTweenTimer();
        resetMultiplier();
        scoreSystemRemove();
    }

    //-----------------------------------Countdown Bar Visual--------------------------
    public void leanTweenTimer()
    {
        LeanTween.scaleX(leanTweenBar, 1, time).setOnComplete(removeLife); // Scales x axis until max which then removes a life
        leanTweenBar.transform.localScale = new Vector3(0f, 0.7f, 0.7f); // Not necessary (for appearence)
    }

    public void leanTweenTimerReset()
    {
        LeanTween.cancel(leanTweenBar); // Uses own function from library to stop visual
        leanTweenTimer(); // Instead of repeating code
    }

    public void timeSpeedUp()
    {
        if (successCount / 3 > lastChecked / 3) // Every multiple of 3, if more than what it was last time
        {
            if (time >= 3) 
            {
                time -= 1; // Makes time, counted up to, less so it's faster
            }

            lastChecked = successCount; // Obvious but just makes the current check into the last check for next time
        }
    }
}