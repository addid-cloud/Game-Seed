using UnityEngine;
using UnityEngine.SceneManagement;

public class TarikTambangManager : MonoBehaviour
{
    [Header("Objek Terpisah")]
    public Transform tali;
    public Animator animPemainKiri;
    public Animator animPemainKanan;

    [Header("Pengaturan Permainan")]
    public float kekuatanTarik = 0.5f;
    public float batasMenang = 5f;

    [Header("Pindah Scene")]
    [Tooltip("Nama scene papan permainan monopoli Anda")]
    public string namaSceneUtama = "MapUtama";
    [Tooltip("Jeda waktu sebelum pindah (agar pemain sempat melihat animasi jatuh)")]
    public float jedaPindahScene = 1.5f;

    [Header("Arah Tarikan (World Space)")]
    public Vector3 arahKiri = Vector3.back;
    public Vector3 arahKanan = Vector3.forward;

    // Nama parameter di Animator
    private string triggerTarikKeras = "TarikKeras";
    private string stateKalah = "Kalah"; 

    private Vector3 posisiAwalTali;
    private bool gameSelesai = false;

    void Start()
    {
        if (tali != null)
        {
            posisiAwalTali = tali.position;
        }
    }

    void Update()
    {
        if (gameSelesai || tali == null) return;

        // --- PEMAIN KIRI (Tombol A) ---
        if (Input.GetKeyDown(KeyCode.A))
        {
            tali.position += arahKiri * kekuatanTarik;

            if (animPemainKiri != null)
                animPemainKiri.SetTrigger(triggerTarikKeras);

            CekMenang();
        }

        // --- PEMAIN KANAN (Tombol L) ---
        if (Input.GetKeyDown(KeyCode.L))
        {
            tali.position += arahKanan * kekuatanTarik;

            if (animPemainKanan != null)
                animPemainKanan.SetTrigger(triggerTarikKeras);

            CekMenang();
        }
    }

    void CekMenang()
    {
        float jarakDariTengah = Vector3.Distance(posisiAwalTali, tali.position);

        if (jarakDariTengah >= batasMenang)
        {
            gameSelesai = true;

            float jarakKeKiri = Vector3.Distance(tali.position, posisiAwalTali + (arahKiri * batasMenang));
            float jarakKeKanan = Vector3.Distance(tali.position, posisiAwalTali + (arahKanan * batasMenang));

            if (jarakKeKiri < jarakKeKanan)
            {
                Debug.Log("PEMAIN KIRI MENANG!");
                // Pemain Kanan jatuh
                if (animPemainKanan != null)
                    animPemainKanan.CrossFade(stateKalah, 0.2f);
            }
            else
            {
                Debug.Log("PEMAIN KANAN MENANG!");
                // Pemain Kiri jatuh
                if (animPemainKiri != null)
                    animPemainKiri.CrossFade(stateKalah, 0.2f);
            }

            // Langsung bersiap kembali ke scene utama
            StartCoroutine(ProsesKembaliKeUtama());
        }
    }

    private System.Collections.IEnumerator ProsesKembaliKeUtama()
    {
        // Beri sedikit jeda agar pemain bisa melihat siapa yang jatuh
        yield return new WaitForSeconds(jedaPindahScene);

        if (!string.IsNullOrEmpty(namaSceneUtama))
        {
            SceneManager.LoadScene(namaSceneUtama);
        }
        else
        {
            Debug.LogError("Gagal pindah scene: 'namaSceneUtama' di Inspector belum diisi!");
        }
    }
}