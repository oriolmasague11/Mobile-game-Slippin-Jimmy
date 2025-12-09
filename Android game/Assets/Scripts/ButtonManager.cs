using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void onResume()
    {
        GameManager.Instance.player.GetComponent<PlayerControler>().Pause();
    }

    public void onExit()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }

    public void onPlay()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(1);
    }

    public void onReplay()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
