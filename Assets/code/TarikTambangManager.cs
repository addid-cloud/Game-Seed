using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Ditambahkan untuk TextMeshPro

public class TarikTambangManager : MonoBehaviour
{
    [Header("Komponen UI")]
    public Slider barAdu;
    public TMP_Text teksPengumuman; // Menampilkan tulisan "PEMAIN 1 MENANG!"
    public GameObject panelGameOver; // Latar belakang semi-transparan saat game over

    [Header("Objek Utama")]
    public Transform tali;
    public Animator animKiri;
    public Animator animKanan;

    [Header("Pengaturan Jarak & Kekuatan")]
    public float kekuatanTarik = 0.2f;
    public float batasKetarik = 3f;
    public float batasMenang = 5f;

    [Header("Arah Tarikan (World Space)")]
    public Vector3 arahKiri = Vector3.back;
    public Vector3 arahKanan = Vector3.forward;

    private float skorTarikan = 0f; // 0 = Tengah, Minus = Kiri Menang, Plus = Kanan Menang
    private bool gameSelesai = false;

    [Header("Transisi Scene")]
    [Tooltip("Ketikkan nama Scene papan utama Anda persis seperti di folder Scenes")]
    public string namaSceneUtama = "SampleScene"; 
    public float delayKembali = 4f; // Berapa lama tunggu setelah jatuh sebelum pindah scene

    // --- VARIABEL EFEK MELAR ---
    private Vector3 skalaAsliTali;
    private float targetMelar = 1f;
    private float currentMelar = 1f;

    void Start()
    {
        if (tali != null)
        {
            skalaAsliTali = tali.localScale;
        }

        if (barAdu != null)
        {
            barAdu.minValue = -1f;
            barAdu.maxValue = 1f;
            barAdu.value = 0f;
        }

        // Sembunyikan UI Game Over di awal permainan
        if (teksPengumuman != null) teksPengumuman.gameObject.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);

        // Set awal ke state Mulai
        PlayAnimation(animKiri, "Mulai");
        PlayAnimation(animKanan, "Mulai");
    }

    void Update()
    {
        if (gameSelesai) return;

        bool kiriInput = Input.GetKeyDown(KeyCode.A);
        bool kananInput = Input.GetKeyDown(KeyCode.L);

        // 1. LOGIKA INPUT SKOR TARIKAN (TALI DIAM DI TEMPAT)
        if (kiriInput)
        {
            skorTarikan -= kekuatanTarik; // Skor berkurang ke arah minus (Kiri)
            TriggerTarikAnimation(animKiri);
        }
        if (kananInput)
        {
            skorTarikan += kekuatanTarik; // Skor bertambah ke arah plus (Kanan)
            TriggerTarikAnimation(animKanan);
        }

        // --- EFEK MELAR (STRETCHING) ---
        if (kiriInput || kananInput)
        {
            // Setiap ditekan, tambah target melar (maksimal 1.35x dari panjang asli)
            targetMelar = Mathf.Min(targetMelar + 0.15f, 1.35f); 
        }

        // Target perlahan kembali ke 1.0 (mereda)
        targetMelar = Mathf.Lerp(targetMelar, 1f, Time.deltaTime * 3f);
        // Current bergerak mengejar target secara mulus
        currentMelar = Mathf.Lerp(currentMelar, targetMelar, Time.deltaTime * 12f);

        if (tali != null)
        {
            // Aplikasikan: Sumbu Z memanjang, Sumbu X & Y menipis agar volume masuk akal
            tali.localScale = new Vector3(
                skalaAsliTali.x / currentMelar, 
                skalaAsliTali.y / currentMelar, 
                skalaAsliTali.z * currentMelar
            );
        }

        // 2. LOGIKA UPDATE TAMPILAN BAR ADU (SLIDER)
        UpdateSlider();

        // 3. DETEKSI ZONA BAHAYA & KONDISI MENANG/KALAH
        CekKondisiGame(kiriInput, kananInput);
    }

    void TriggerTarikAnimation(Animator anim)
    {
        if (anim == null) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        // ANTI-GLITCH: Hanya mainkan Tarik jika tidak sedang di state Tarik atau Tarik 0
        // Ini menjaga transisi internal ping-pong bawaan Unity tetap berputar mulus
        if (!stateInfo.IsName("Tarik") && !stateInfo.IsName("Tarik 0"))
        {
            anim.CrossFade("Tarik", 0.1f);
        }
    }

    void UpdateSlider()
    {
        if (barAdu == null) return;

        // Masukkan nilainya ke rentang Slider -1 (Mentok Kiri) sampai 1 (Mentok Kanan)
        barAdu.value = Mathf.Clamp(skorTarikan / batasMenang, -1f, 1f);
    }

    void CekKondisiGame(bool kiriMencet, bool kananMencet)
    {
        // --- KONDISI GAME OVER (MENANG / KALAH) ---
        if (skorTarikan <= -batasMenang) // Kiri berhasil menarik sampai batas minimal
        {
            EndGame("PLAYER KIRI WIN!", animKanan, animKiri);
            return;
        }
        if (skorTarikan >= batasMenang) // Kanan berhasil menarik sampai batas maksimal
        {
            EndGame("PLAYER KANAN WIN!", animKiri, animKanan);
            return;
        }

        // --- KONDISI ZONA BAHAYA (KETARIK) ---
        // Jika skor masuk zona bahaya kiri (minus ekstrim), dan Kanan diam saja
        if (skorTarikan <= -batasKetarik && !kananMencet)
        {
            PlayAnimationIfNotActive(animKanan, "Ketarik", 0.2f);
        }
        // Jika skor masuk zona bahaya kanan (plus ekstrim), dan Kiri diam saja
        if (skorTarikan >= batasKetarik && !kiriMencet)
        {
            PlayAnimationIfNotActive(animKiri, "Ketarik", 0.2f);
        }
    }

    void EndGame(string pesanMenang, Animator animKalah, Animator animMenang)
    {
        gameSelesai = true;
        Debug.Log("GAME OVER: " + pesanMenang + " | Memaksa animasi Jatuh & Menang...");

        // Menampilkan UI Kemenangan di Layar
        if (teksPengumuman != null) 
        {
            teksPengumuman.text = pesanMenang;
            teksPengumuman.gameObject.SetActive(true);
        }
        if (panelGameOver != null) 
        {
            panelGameOver.SetActive(true);
        }

        // PAKSAAN MUTLAK: Hapus transisi yang sedang berjalan dan paksa state ke frame 0
        if (animKalah != null) 
        {
            animKalah.Play("Jatuh", -1, 0f);
            animKalah.Update(0f); // Evaluasi instan di frame ini juga
        }
        
        if (animMenang != null) 
        {
            animMenang.Play("Menang", -1, 0f); // GANTI state jadi "Menang"
            animMenang.Update(0f); // Evaluasi instan di frame ini juga
        }

        // Jalankan efek tali hilang
        StartCoroutine(HilangkanTaliKeren());
        
        StartCoroutine(VerifikasiStateAnimator(animKalah));
        
        // Mulai hitung mundur untuk kembali ke Board Game
        StartCoroutine(KembaliKeMainScene());
    }

    System.Collections.IEnumerator KembaliKeMainScene()
    {
        // Beri waktu pemain menikmati animasi selebrasi & kekalahan
        yield return new WaitForSeconds(delayKembali);
        
        Debug.Log("Transisi kembali ke scene: " + namaSceneUtama);
        SceneManager.LoadScene(namaSceneUtama);
    }

    System.Collections.IEnumerator HilangkanTaliKeren()
    {
        if (tali == null) yield break;
        
        Debug.Log("Memulai animasi tali menghilang...");
        Vector3 skalaAwal = skalaAsliTali; // Gunakan skala asli agar animasi hilangnya rapi
        
        // Fase 1: Membesar sedikit (durasi 0.15 detik)
        float timer = 0f;
        float durasiMembesar = 0.15f;
        while (timer < durasiMembesar)
        {
            timer += Time.deltaTime;
            float progress = timer / durasiMembesar;
            tali.localScale = Vector3.Lerp(skalaAwal, skalaAwal * 1.3f, progress);
            yield return null;
        }

        // Fase 2: Menyusut (durasi 0.2 detik)
        timer = 0f;
        float durasiMenyusut = 0.2f;
        Vector3 skalaPuncak = tali.localScale;
        while (timer < durasiMenyusut)
        {
            timer += Time.deltaTime;
            float progress = timer / durasiMenyusut;
            tali.localScale = Vector3.Lerp(skalaPuncak, Vector3.zero, progress);
            yield return null;
        }

        // Pastikan ukurannya benar-benar 0 lalu matikan objeknya
        tali.localScale = Vector3.zero;
        tali.gameObject.SetActive(false);
        Debug.Log("Tali berhasil dihilangkan!");
    }

    System.Collections.IEnumerator VerifikasiStateAnimator(Animator anim)
    {
        yield return new WaitForEndOfFrame(); // Tunggu hingga semua skrip selesai dieksekusi di frame ini
        if (anim != null)
        {
            var state = anim.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Jatuh"))
            {
                Debug.Log("<color=green>VERIFIKASI SUKSES:</color> Animator BERHASIL masuk ke state 'Jatuh'. Jika karakter tidak terlihat jatuh, berarti masalahnya ada di IK / Rigging / File Animasi 'rig.001_jatuh' yang kosong.");
            }
        }
    }

    void PlayAnimation(Animator anim, string stateName, float transitionDuration = 0.1f)
    {
        if (anim != null)
        {
            anim.CrossFade(stateName, transitionDuration);
        }
    }

    void PlayAnimationIfNotActive(Animator anim, string stateName, float transitionDuration = 0.1f)
    {
        if (anim == null) return;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        // Cegah animasi Ketarik menimpa state kritis (Tarik, Tarik 0, atau Jatuh)
        if (!stateInfo.IsName(stateName) && !stateInfo.IsName("Tarik") && !stateInfo.IsName("Tarik 0") && !stateInfo.IsName("Jatuh"))
        {
            anim.CrossFade(stateName, transitionDuration);
        }
    }
}