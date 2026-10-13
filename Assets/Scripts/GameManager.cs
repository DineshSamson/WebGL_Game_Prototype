using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TMP_Text objectiveText;
    public GameObject guidAndRulePanel;
    public GameObject winPanel;
    public GameObject losePanel;

    bool hasItem;
    bool gamoeOver;

    public GuardAIController[] guards;
    public Gun gun;
    public Coroutine alerRoutin;
    private void Awake()
    {
        if(instance == null)
            instance = this;

       
        Time.timeScale = 0;
    }

    public void GameStart()
    {
        Time.timeScale = 1;

        guidAndRulePanel.SetActive(false);
        AudioController.instance.OnGameStart();
    }

    private void Start()
    {
        winPanel.gameObject.SetActive(false);
        losePanel.gameObject.SetActive(false);
        objectiveText.text = "Retrieve the evidence";
        guidAndRulePanel.SetActive(true);
    }

    public void CollectedItem()
    {
        hasItem = true;
        objectiveText.text = "Get to the exit!";
        AudioController.instance.DoAlarm();
    }

    public void ReachExit()
    {
        if (gamoeOver)
            return;

        if(hasItem) 
        {
            if (alerRoutin != null)
                StopCoroutine(alerRoutin);
            gamoeOver = true;
            winPanel.SetActive(true);
            AudioController.instance.StopAudo();
            Time.timeScale = 0;
        }
        else
        {
            objectiveText.text = "Find the evidence first!";
        }
    }

    public void Lose()
    {
        if(gamoeOver)
            return;
        
        if(alerRoutin != null)
            StopCoroutine(alerRoutin);
        gamoeOver = true;
        losePanel.SetActive(true);
        AudioController.instance.StopAudo();
        Time.timeScale = 0f;

    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CallAlerRoutin()
    {
        alerRoutin = StartCoroutine(AlertGuard());
    }

    IEnumerator AlertGuard()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i < guards.Length; i++)
        {
            guards[i].patrolSpeed = 3;
            guards[i].viewRange = 8;
        }

        gun.fireInterval = 1.5f;
    }

}
