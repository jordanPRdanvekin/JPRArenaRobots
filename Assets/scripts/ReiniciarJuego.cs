using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReiniciarJuego : MonoBehaviour
{
    public static ReiniciarJuego instance;

    [SerializeField]
    GameObject winCanvas;

    [SerializeField]
    GameObject loseCanvas;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }

    public void WonGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        winCanvas.SetActive(true);
    }

    public void LostGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        loseCanvas.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("laberinto");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("start");
    }
}
