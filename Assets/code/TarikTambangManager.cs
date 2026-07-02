using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TarikTambangManager : MonoBehaviour
{
    [Header("Objek")]
    public Transform tali;
    public Animator animKiri;
    public Animator animKanan;

    [Header("Gameplay")]
    public float kekuatanTarik = 0.5f;
    public float batasKetarik = 3f;
    public float batasMenang = 5f;
    
    [Tooltip("Waktu minimum animasi 'Tarik' dipertahankan setelah tombol ditekan")]
    public float durasiTarik = 0.3f; 

    [Header("Scene")]
    public string namaSceneUtama = "MapUtama";
    public float jedaPindahScene = 2f;

    [Header("Arah Tarik")]
    public Vector3 arahKiri = Vector3.back;
    public Vector3 arahKanan = Vector3.forward;

    private Vector3 posisiAwalTali;
    private bool gameSelesai = false;

    private float lastPullKiri = -1f;
    private float lastPullKanan = -1f;

    // Enum untuk menghindari pemanggilan CrossFade setiap frame
    public enum AnimState { Mulai, Tarik, Ketarik, Jatuh }
    private AnimState stateKiri = AnimState.Mulai;
    private AnimState stateKanan = AnimState.Mulai;

    void Start()
    {
        if (tali != null)
        {
            posisiAwalTali = tali.position;
        }

        // Set state awal
        if (animKiri != null) animKiri.Play("Mulai");
        if (animKanan != null) animKanan.Play("Mulai");
        
        stateKiri = AnimState.Mulai;
        stateKanan = AnimState.Mulai;
    }

    void Update()
    {
        if (gameSelesai) return;

        //------------------------
        // Input Pemain Kiri (A)
        //------------------------
        if (Input.GetKeyDown(KeyCode.A))
        {
            tali.position += arahKiri.normalized * kekuatanTarik;
            lastPullKiri = Time.time;
            
            if (stateKiri != AnimState.Tarik)
            {
                animKiri.CrossFade("Tarik", 0.1f);
                stateKiri = AnimState.Tarik;
            }
        }

        //------------------------
        // Input Pemain Kanan (L)
        //------------------------
        if (Input.GetKeyDown(KeyCode.L))
        {
            tali.position += arahKanan.normalized * kekuatanTarik;
            lastPullKanan = Time.time;
            
            if (stateKanan != AnimState.Tarik)
            {
                animKanan.CrossFade("Tarik", 0.1f);
                stateKanan = AnimState.Tarik;
            }
        }

        //------------------------
        // Hitung Posisi Tali
        //------------------------
        Vector3 offset = tali.position - posisiAwalTali;
        
        // Cek seberapa jauh ditarik ke masing-masing arah
        float tarikanKiri = Vector3.Dot(offset, arahKiri.normalized);
        float tarikanKanan = Vector3.Dot(offset, arahKanan.normalized);

        // Status apakah pemain sedang aktif menarik (dalam durasi tarik)
        bool isKiriTarikAktif = (Time.time - lastPullKiri) < durasiTarik;
        bool isKananTarikAktif = (Time.time - lastPullKanan) < durasiTarik;

        //------------------------
        // 1. Cek Menang / Kalah
        //------------------------
        if (tarikanKiri >= batasMenang)
        {
            SelesaikanGame(true); // Kiri menang
            return;
        }
        else if (tarikanKanan >= batasMenang)
        {
            SelesaikanGame(false); // Kanan menang
            return;
        }

        //------------------------
        // 2. Sistem Deteksi Zona Bahaya & Animasi
        //------------------------
        if (tarikanKiri >= batasKetarik)
        {
            // KIRI Mendominasi, KANAN di Zona Bahaya

            // Update Kiri (Kembali ke Mulai jika tidak menarik)
            if (!isKiriTarikAktif && stateKiri != AnimState.Mulai)
            {
                animKiri.CrossFade("Mulai", 0.2f);
                stateKiri = AnimState.Mulai;
            }

            // Update Kanan
            if (!isKananTarikAktif && stateKanan != AnimState.Ketarik)
            {
                // Kanan terseret
                animKanan.CrossFade("Ketarik", 0.2f);
                stateKanan = AnimState.Ketarik;
            }
            else if (isKananTarikAktif && stateKanan != AnimState.Tarik)
            {
                // Kanan mencoba melawan
                animKanan.CrossFade("Tarik", 0.1f);
                stateKanan = AnimState.Tarik;
            }
        }
        else if (tarikanKanan >= batasKetarik)
        {
            // KANAN Mendominasi, KIRI di Zona Bahaya

            // Update Kanan (Kembali ke Mulai jika tidak menarik)
            if (!isKananTarikAktif && stateKanan != AnimState.Mulai)
            {
                animKanan.CrossFade("Mulai", 0.2f);
                stateKanan = AnimState.Mulai;
            }

            // Update Kiri
            if (!isKiriTarikAktif && stateKiri != AnimState.Ketarik)
            {
                // Kiri terseret
                animKiri.CrossFade("Ketarik", 0.2f);
                stateKiri = AnimState.Ketarik;
            }
            else if (isKiriTarikAktif && stateKiri != AnimState.Tarik)
            {
                // Kiri mencoba melawan
                animKiri.CrossFade("Tarik", 0.1f);
                stateKiri = AnimState.Tarik;
            }
        }
        else
        {
            // ZONA AMAN (Tali di tengah, belum ada yang masuk batasKetarik)
            
            if (!isKiriTarikAktif && stateKiri != AnimState.Mulai)
            {
                animKiri.CrossFade("Mulai", 0.2f);
                stateKiri = AnimState.Mulai;
            }

            if (!isKananTarikAktif && stateKanan != AnimState.Mulai)
            {
                animKanan.CrossFade("Mulai", 0.2f);
                stateKanan = AnimState.Mulai;
            }
        }
    }

    void SelesaikanGame(bool kiriMenang)
    {
        gameSelesai = true;

        if (kiriMenang)
        {
            Debug.Log("PEMAIN KIRI MENANG!");
            if (stateKanan != AnimState.Jatuh)
            {
                animKanan.CrossFade("Jatuh", 0.1f);
                stateKanan = AnimState.Jatuh;
            }
        }
        else
        {
            Debug.Log("PEMAIN KANAN MENANG!");
            if (stateKiri != AnimState.Jatuh)
            {
                animKiri.CrossFade("Jatuh", 0.1f);
                stateKiri = AnimState.Jatuh;
            }
        }

        StartCoroutine(KembaliKeSceneUtama());
    }

    IEnumerator KembaliKeSceneUtama()
    {
        yield return new WaitForSeconds(jedaPindahScene);
        SceneManager.LoadScene(namaSceneUtama);
    }
}