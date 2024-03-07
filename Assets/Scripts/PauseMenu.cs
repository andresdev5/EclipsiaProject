using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    // ui button serialize
    [SerializeField] private GameObject uiButton;
    [SerializeField] private GameObject pauseScreen;
    private bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Resume()
    {
        isPaused = false;
        GameObject pauseScreen = GameObject.Find("PauseScreen");
        pauseScreen.GetComponent<CanvasGroup>().alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // detect if the player has pressed the "esc" key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;

            GameObject pauseScreen = GameObject.Find("PauseScreen");
            CanvasGroup canvasg = pauseScreen.GetComponent<CanvasGroup>();
            canvasg.alpha = isPaused ? 1 : 0;
        }
    }
}
