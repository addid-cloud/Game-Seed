using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [Header("Referensi Visual")]
    public Animator anim;
    
    [Header("Efek Partikel (Opsional)")]
    public ParticleSystem efekGiliranMulai; 

    // Trik Optimasi: Menggunakan Hash untuk nama parameter Animator agar lebih ringan
    private readonly int isMovingHash = Animator.StringToHash("isMoving");
    private readonly int triggerGiliranHash = Animator.StringToHash("mulaiGiliran");
    private readonly int triggerLompatVertikalHash = Animator.StringToHash("TriggerLompatVertikal");
    private readonly int triggerSelebrasiHash = Animator.StringToHash("selebrasi");

    // -------------------------------------------------------------
    // FUNGSI PUBLIK (Bisa dipanggil dari Script GameManager)
    // -------------------------------------------------------------

    // 1. Dipanggil saat dadu dilempar / giliran karakter ini dimulai
    public void MulaiGiliran()
    {
        if (anim != null) anim.SetTrigger(triggerGiliranHash);
        
        // Memutar efek visual (misal: cahaya muncul di bawah kaki)
        if (efekGiliranMulai != null) efekGiliranMulai.Play();
    }

    // Dipanggil saat karakter pindah ke ketinggian yang berbeda
    public void LompatVertikal()
    {
        if (anim != null)
        {
            // Reset trigger giliran agar tidak bertabrakan
            anim.ResetTrigger(triggerGiliranHash);
            anim.SetTrigger(triggerLompatVertikalHash);
        }
    }

    // Dipanggil saat coroutine lompat selesai untuk memaksa karakter kembali bersiap
    public void AkhiriLompatVertikal()
    {
        if (anim != null)
        {
            // Paksa transisi mulus ke state "Idle" agar animasi tidak nyangkut
            anim.CrossFade("Idle", 0.1f);
        }
    }

    // 2. Dipanggil saat karakter bergerak antar petak (True = Lari, False = Berhenti)
    public void AturPergerakan(bool jalan)
    {
        if (anim != null) anim.SetBool(isMovingHash, jalan);
    }

    // 3. Dipanggil saat efek petak dieksekusi (Sangat dinamis untuk ke depannya)
    public void MainkanAnimasiSpesifik(string namaTriggerAnimator)
    {
        if (anim != null) anim.SetTrigger(namaTriggerAnimator);
    }

    // 4. Memutar efek partikel dinamis dari luar (misal: ledakan jebakan)
    public void MunculkanEfek(GameObject prefabEfek)
    {
        if (prefabEfek != null)
        {
            // Munculkan efek tepat di posisi karakter saat ini
            Instantiate(prefabEfek, transform.position, Quaternion.identity);
        }
    }
}