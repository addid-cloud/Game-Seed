using UnityEngine;

public class SceneMusicTrigger : MonoBehaviour
{
    [Header("Musik Latar untuk Scene ini")]
    public AudioClip musikSceneIni;

    private void Start()
    {
        // Segera setelah scene dimuat, suruh AudioManager ganti musik
        if (musikSceneIni != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic(musikSceneIni);
        }
    }
}
