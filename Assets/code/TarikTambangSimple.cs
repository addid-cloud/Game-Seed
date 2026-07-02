using UnityEngine;

public class TarikTambangSimple : MonoBehaviour
{
    [Header("Masukkan Objek TALI di sini")]
    public Transform tali; 
    
    public float kekuatanTarik = 0.5f;
    public float batasMenang = 5f;

    [Header("Arah Geser (Ganti kalau salah arah)")]
    public Vector3 arahKiri = Vector3.back;
    public Vector3 arahKanan = Vector3.forward;

    private Vector3 posisiAwalTali;
    private bool gameSelesai = false;

    void Start()
    {
        // Ingat posisi tengah saat game mulai
        posisiAwalTali = tali.position;
    }

    void Update()
    {
        if (gameSelesai) return;

        // KIRI NARIK
        if (Input.GetKeyDown(KeyCode.A))
        {
            tali.position += arahKiri * kekuatanTarik;
            CekMenang();
        }

        // KANAN NARIK
        if (Input.GetKeyDown(KeyCode.L))
        {
            tali.position += arahKanan * kekuatanTarik;
            CekMenang();
        }
    }

    void CekMenang()
    {
        // Cek jarak tali dari posisi awal
        float jarak = Vector3.Distance(posisiAwalTali, tali.position);
        
        if (jarak >= batasMenang)
        {
            gameSelesai = true;
            
            // Cek tali lebih dekat ke arah mana
            float jarakKiri = Vector3.Distance(tali.position, posisiAwalTali + (arahKiri * batasMenang));
            float jarakKanan = Vector3.Distance(tali.position, posisiAwalTali + (arahKanan * batasMenang));

            if (jarakKiri < jarakKanan)
            {
                Debug.Log("=== PEMAIN KIRI MENANG! ===");
            }
            else
            {
                Debug.Log("=== PEMAIN KANAN MENANG! ===");
            }
        }
    }
}