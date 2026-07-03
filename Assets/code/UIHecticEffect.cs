using UnityEngine;
using TMPro;

public class UIHecticEffect : MonoBehaviour
{
    [Header("Efek Berdebar (Scale)")]
    public float kecepatanDebar = 15f;
    public float ukuranMin = 0.9f;
    public float ukuranMax = 1.2f;

    [Header("Efek Gemetar (Shake)")]
    public float intensitasGemetar = 3f;

    [Header("Ubah Warna Saat Ditekan (Opsional)")]
    public KeyCode tombolYangDitekan;
    public Color warnaNormal = Color.white;
    public Color warnaDitekan = Color.red;

    private Vector3 posisiAwal;
    private TMP_Text teks;

    void Start()
    {
        posisiAwal = transform.localPosition;
        teks = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // 1. Efek Berdebar (Membesar dan Mengecil dengan cepat)
        float skala = Mathf.Lerp(ukuranMin, ukuranMax, (Mathf.Sin(Time.time * kecepatanDebar) + 1f) / 2f);
        transform.localScale = new Vector3(skala, skala, 1f);

        // 2. Efek Gemetar (Kekacauan posisi secara acak)
        float shakeX = Random.Range(-intensitasGemetar, intensitasGemetar);
        float shakeY = Random.Range(-intensitasGemetar, intensitasGemetar);
        transform.localPosition = posisiAwal + new Vector3(shakeX, shakeY, 0f);

        // 3. Efek visual saat ditekan (agar makin asyik)
        if (teks != null)
        {
            if (Input.GetKey(tombolYangDitekan))
            {
                teks.color = warnaDitekan;
                transform.localScale = new Vector3(ukuranMax * 1.3f, ukuranMax * 1.3f, 1f); // Membengkak saat ditekan
            }
            else
            {
                teks.color = warnaNormal;
            }
        }
    }
}
