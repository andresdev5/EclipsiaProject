using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus
{
    public float Health { get; set; } = 100.0f;
    public int Souls { get; set; } = 0;
}

public class GameStatus
{
    public int Level { get; set; } = 1;
    public bool IsPaused { get; set; } = false;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerStatus PlayerStatus { get; set; } = new PlayerStatus();
    public GameStatus GameStatus { get; set; } = new GameStatus();

    [SerializeField] public GameObject LoadingScreen;

    public bool IsFinished { get; set; } = false;

    private void Awake()
    {
        // start of new code
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // end of new code

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Pause()
    {
        this.GameStatus.IsPaused = true;
    }

    public void PlaySFX(string name, bool wait = false, float pitch = 1f)
    {
        AudioSource SFXSource = GameObject.Find("SFXSource").GetComponent<AudioSource>();

        if (SFXSource != null && wait && SFXSource.isPlaying)
        {
            return;
        }

        SFXSource.clip = Resources.Load<AudioClip>($"SFX/{name}");
        SFXSource.pitch = pitch;
        SFXSource.Play();
    }

    public void PlaySFX(string name, AudioSource source, bool wait = false)
    {
        if (wait && source.isPlaying)
        {
            return;
        }

        source.clip = Resources.Load<AudioClip>($"SFX/{name}");
        source.Play();
    }

    public void FinishGame()
    {
        IsFinished = true;
    }

    public void NextLevel()
    {
        Animator loadingScreenAnimator = LoadingScreen.GetComponent<Animator>();
        loadingScreenAnimator.SetTrigger("FadeIn");

        StartCoroutine(LoadNextLevel());
    }

    private IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(3.0f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(string.Format("Level{0}", GameStatus.Level + 1));
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(string.Format("Level{0}", GameStatus.Level));
               
        GameStatus.Level++;
    }
}
