using UnityEngine;
using UnityEngine.SceneManagement;

public class DummyMiniGameController : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Ketikkan nama Scene Map Utama di sini persis seperti yang terdaftar di Build Settings")]
    public string namaSceneMapUtama;

    [Header("Pengaturan Hadiah")]
    public int hadiahDaun = 10;

    // Fungsi ini bisa dihubungkan ke tombol UI (On Click) dan mengirimkan indeks pemain (0, 1, 2, dll)
    public void SimulasikanMenang(int pemenangIndex)
    {
        // 1. Cek apakah ada GameDataManager dari scene sebelumnya
        if (GameDataManager.Instance != null)
        {
            // Pastikan indeks pemenang valid (tidak keluar batas array pemain)
            if (pemenangIndex >= 0 && pemenangIndex < GameDataManager.Instance.savedPlayerScores.Length)
            {
                // Tambahkan poin ke pemenang
                GameDataManager.Instance.savedPlayerScores[pemenangIndex] += hadiahDaun;
                Debug.Log($"Pemain {pemenangIndex + 1} memenangkan Mini Game dan mendapatkan {hadiahDaun} poin/daun!");
            }
            else
            {
                Debug.LogWarning($"Indeks pemenang ({pemenangIndex}) di luar batas jumlah pemain yang terdaftar!");
            }
        }
        else
        {
            // Peringatan jika membuka scene ini langsung tanpa melalui map utama
            Debug.LogWarning("GameDataManager tidak ditemukan! Pastikan Anda memulai permainan dari Map Utama.");
        }

        // 2. Terlepas ada data atau tidak, pindahkan kembali ke Map Utama
        if (!string.IsNullOrEmpty(namaSceneMapUtama))
        {
            Debug.Log($"Kembali ke Map Utama: {namaSceneMapUtama}");
            SceneManager.LoadScene(namaSceneMapUtama);
        }
        else
        {
            Debug.LogError("Nama Scene Map Utama belum diisi di Inspector!");
        }
    }
}
