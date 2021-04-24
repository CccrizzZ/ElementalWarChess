using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ButtonScript : MonoBehaviour
{

    public GameObject CreditPanel;
    public GameObject HowToPlayPanel;
    

    public GameObject PausePanel;




    private void Start()
    {
        if (HowToPlayPanel)
        {
            HowToPlayPanel.SetActive(false);
            
        }
        if (CreditPanel)
        {
            CreditPanel.SetActive(false);
        }
        
        if (PausePanel)
        {
            PausePanel.SetActive(false);
        }
    }

    public void Resume()
    {
        PausePanel.SetActive(false);
        // set time scale
        Time.timeScale = 1;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().isPaused = false;
    
    }


    public void NewGameButton()
    {
        SceneManager.LoadScene("GameScene");
    }   


    public void CreditButton()
    {
        CreditPanel.SetActive(true);

    }


    public void QuitGameButton()
    {
        Application.Quit();
    }

    public void HowtoPlayPanel()
    {
        HowToPlayPanel.SetActive(true);
    }


    public void CloseButton()
    {
        if (HowToPlayPanel)
        {
            HowToPlayPanel.SetActive(false);
            
        }
        if (CreditPanel)
        {
            CreditPanel.SetActive(false);
        }
    }


    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
        // set time scale
        Time.timeScale = 1;
    }
}
