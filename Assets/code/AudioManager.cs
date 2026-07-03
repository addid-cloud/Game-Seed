using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Referensi Komponen Audio (Otomatis)")]
    public AudioSource backgroundMusicSource;

    private void Awake()
    {
        // Otomatis mengambil komponen AudioSource di GameObject yang sama
        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = GetComponent<AudioSource>();
        }

        // Memastikan hanya ada satu AudioManager di seluruh permainan (Singleton Pattern)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Jangan dihancurkan saat pindah scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (backgroundMusicSource == null) return;

        // Jika lagu yang dimainkan sama dengan yang mau diputar, jangan restart
        if (backgroundMusicSource.clip == clip && backgroundMusicSource.isPlaying) return;

        backgroundMusicSource.clip = clip;
        backgroundMusicSource.loop = true;
        backgroundMusicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.Stop();
        }
    }
}
