using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Menu UI")]
    public GameObject menuUI;

    [Header("Scene")]
    public string nextScene = "Board";

    void Start()
    {
        // Jangan langsung play
        videoPlayer.playOnAwake = false;

        // Persiapkan video
        videoPlayer.Prepare();

        // Setelah siap
        videoPlayer.prepareCompleted += OnVideoPrepared;

        // Setelah selesai
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        vp.Pause();
    }

    public void StartGame()
    {
        menuUI.SetActive(false);
 Debug.Log("BUTTON DIKLIK!");
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextScene);
    }
}