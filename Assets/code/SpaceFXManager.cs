using UnityEngine;

public class SpaceFXManager : MonoBehaviour
{
    // 1. Membuat struktur data (kontainer) untuk menampung efek per tipe petak
    [System.Serializable]
    public struct SpaceFXData
    {
        public BoardNode.SpaceType tipePetak; // Pilihan tipe petak (Enum)
        public GameObject efekPartikelPrefab; // Aset partikel (VFX)
        public AudioClip efekSuaraSFX;        // Aset suara (SFX)
    }

    [Header("Daftar Efek Petak (Bisa Diatur di Inspector)")]
    // 2. Membuat list di Inspector agar Anda tinggal memasukkan efeknya secara visual
    public SpaceFXData[] daftarEfekPetak;

    [Header("Referensi Komponen Audio")]
    public AudioSource audioSource; // Komponen untuk menyalakan suara

    // 3. Fungsi utama yang akan dipanggil oleh GameManager untuk menyalakan efek
    public void PutarEfekPetak(BoardNode.SpaceType tipe, Vector3 posisiPetak)
    {
        // Mencari efek yang cocok dengan tipe petak yang diinjak pemain
        foreach (SpaceFXData fx in daftarEfekPetak)
        {
            if (fx.tipePetak == tipe)
            {
                // A. Nyalakan Efek Partikel (VFX) jika ada
                if (fx.efekPartikelPrefab != null)
                {
                    // Memunculkan partikel tepat di posisi petak tersebut
                    Instantiate(fx.efekPartikelPrefab, posisiPetak, Quaternion.identity);
                }

                // B. Nyalakan Efek Suara (SFX) jika ada
                if (fx.efekSuaraSFX != null && audioSource != null)
                {
                    // Memutar suara satu kali tanpa memutus suara lain
                    audioSource.PlayOneShot(fx.efekSuaraSFX);
                }

                break; // Keluar dari perulangan jika efek sudah ditemukan
            }
        }
    }
}