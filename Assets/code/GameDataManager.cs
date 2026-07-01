using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public int[] savedPlayerNodeIndices;
    public int[] savedPlayerScores; // Array untuk menyimpan skor/poin pemain jika ada
    public int savedCurrentPlayerIndex; // Mencatat giliran pemain terakhir
    public bool hasSavedData = false; // Tanda penanda apakah ada data yang tersimpan

    private void Awake()
    {
        // Sistem Singleton untuk menjaga data tetap ada meski pindah scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Hancurkan duplikat jika kembali ke scene ini
            Destroy(gameObject);
        }
    }
}
