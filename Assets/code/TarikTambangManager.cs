using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TarikTambangManager : MonoBehaviour
{
    [Header("Setup Objek")]
    public Transform taliTengah;
    [Tooltip("Bisa berupa Prefab ledakan atau objek efek di dalam scene")]
    public GameObject efekLedakanBom;

    [Header("Pengaturan Permainan")]
    public float batasKemenangan = 5f;
    public float kekuatanTarik = 0.2f;
    public int hadiahDaun = 10;
    public string namaSceneMapUtama;

    [Header("Tim Kiri")]
    public List<Animator> animTimKiri;
    public List<KeyCode> tombolTimKiri;
    [Tooltip("Indeks pemain (0, 1, 2, dst) yang berada di Tim Kiri. Untuk pembagian hadiah.")]
    public List<int> indeksPemainTimKiri;

    [Header("Tim Kanan")]
    public List<Animator> animTimKanan;
    public List<KeyCode> tombolTimKanan;
    [Tooltip("Indeks pemain (0, 1, 2, dst) yang berada di Tim Kanan. Untuk pembagian hadiah.")]
    public List<int> indeksPemainTimKanan;

    private bool isGameOver = false;

    void Update()
    {
        if (isGameOver) return;

        // Cek Input Tim Kiri
        foreach (KeyCode key in tombolTimKiri)
        {
            if (Input.GetKeyDown(key))
            {
                // Geser tali ke arah negatif X (Kiri)
                taliTengah.position += new Vector3(-kekuatanTarik, 0, 0);
                
                // Memicu animasi "Tarik"
                TriggerAnimasi(animTimKiri, "Tarik");
            }
        }

        // Cek Input Tim Kanan
        foreach (KeyCode key in tombolTimKanan)
        {
            if (Input.GetKeyDown(key))
            {
                // Geser tali ke arah positif X (Kanan)
                taliTengah.position += new Vector3(kekuatanTarik, 0, 0);
                
                // Memicu animasi "Tarik"
                TriggerAnimasi(animTimKanan, "Tarik");
            }
        }

        PantauPosisiTali();
        CekKemenangan();
    }

    private void PantauPosisiTali()
    {
        float posX = taliTengah.position.x;
        
        // Kita menggunakan parameter boolean "KetarikMaju" agar animasinya bisa berlanjut / loop selama posisi sedang kritis
        // Jika tali terseret ke kiri (x < -0.5), tim Kanan ketarik maju
        SetBoolAnimasi(animTimKanan, "KetarikMaju", posX < -0.5f);
        
        // Jika tali terseret ke kanan (x > 0.5), tim Kiri ketarik maju
        SetBoolAnimasi(animTimKiri, "KetarikMaju", posX > 0.5f);
    }

    private void CekKemenangan()
    {
        if (taliTengah.position.x <= -batasKemenangan)
        {
            // Tim Kiri Menang
            StartCoroutine(AkhiriGame(true));
        }
        else if (taliTengah.position.x >= batasKemenangan)
        {
            // Tim Kanan Menang
            StartCoroutine(AkhiriGame(false));
        }
    }

    private IEnumerator AkhiriGame(bool timKiriMenang)
    {
        isGameOver = true;

        // Memunculkan / mengaktifkan bom
        if (efekLedakanBom != null)
        {
            // Jika ini prefab, kita Instantiate. Jika ini objek di scene, kita pindah posisinya dan aktifkan
            if (efekLedakanBom.scene.IsValid()) 
            {
                efekLedakanBom.transform.position = taliTengah.position;
                efekLedakanBom.SetActive(true);
            }
            else
            {
                Instantiate(efekLedakanBom, taliTengah.position, Quaternion.identity);
            }
        }

        // Tentukan siapa pemenang dan pecundang
        List<Animator> timMenangAnim = timKiriMenang ? animTimKiri : animTimKanan;
        List<Animator> timKalahAnim  = timKiriMenang ? animTimKanan : animTimKiri;
        List<int> indeksPemenang     = timKiriMenang ? indeksPemainTimKiri : indeksPemainTimKanan;

        // Pemicu animasi akhir
        TriggerAnimasi(timMenangAnim, "MenangJatuh");
        TriggerAnimasi(timKalahAnim, "KalahJatuh");

        // Integrasi dengan GameDataManager
        if (GameDataManager.Instance != null && GameDataManager.Instance.hasSavedData)
        {
            foreach (int indeks in indeksPemenang)
            {
                // Mencegah error Out of Bounds
                if (indeks >= 0 && indeks < GameDataManager.Instance.savedPlayerScores.Length)
                {
                    GameDataManager.Instance.savedPlayerScores[indeks] += hadiahDaun;
                    Debug.Log($"Pemain {indeks + 1} memenangkan Tambang dan mendapat {hadiahDaun} Daun!");
                }
            }
        }

        // Tunggu animasi jatuh dan ledakan selesai
        yield return new WaitForSeconds(4f);

        // Kembali ke map utama
        if (!string.IsNullOrEmpty(namaSceneMapUtama))
        {
            SceneManager.LoadScene(namaSceneMapUtama);
        }
        else
        {
            Debug.LogError("Gagal pindah scene: namaSceneMapUtama belum diisi!");
        }
    }

    // --- Helper Functions Animasi ---
    private void TriggerAnimasi(List<Animator> daftarAnim, string triggerName)
    {
        foreach (var anim in daftarAnim)
        {
            if (anim != null) anim.SetTrigger(triggerName);
        }
    }

    private void SetBoolAnimasi(List<Animator> daftarAnim, string boolName, bool state)
    {
        foreach (var anim in daftarAnim)
        {
            if (anim != null) anim.SetBool(boolName, state);
        }
    }
}
