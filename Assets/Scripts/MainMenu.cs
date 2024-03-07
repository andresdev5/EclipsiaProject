using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject GameStartButton;
    [SerializeField] private GameObject ExitButton;

    // Start is called before the first frame update
    void Start()
    {
        GameStartButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnGameStartButtonClick);
        ExitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnExitButtonClick);
    }
    
    void OnGameStartButtonClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
        GameManager.Instance.GameStatus.IsPaused = false;
        GameManager.Instance.IsFinished = false;
        GameManager.Instance.PlayerStatus.Health = 100.0f;
    }

    void OnExitButtonClick()
    {
        Application.Quit();
    }
}
