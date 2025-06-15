using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ensure singleton
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // optional
    }

    public void RestartLevel(float delay = 0f)
    {
        if (delay > 0f)
            StartCoroutine(RestartAfterDelay(delay));
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}