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