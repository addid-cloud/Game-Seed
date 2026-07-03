using UnityEngine;
using TMPro;

public class PlayerNameplate : MonoBehaviour
{
    [Header("Pengaturan Teks")]
    public TMP_Text nameText;
    public string playerName = "Player 1";

    [Header("Efek Dinamis")]
    [Tooltip("Membuat teks mengambang naik-turun secara halus")]
    public bool gunakanFloating = true;
    public float kecepatanFloat = 2f;
    public float amplitudoFloat = 0.15f;

    [Header("Pengaturan Posisi (World Space)")]
    [Tooltip("Tinggi nama dari titik tengah karakter")]
    public float tinggiNama = 3f;

    private Camera kameraUtama;

    void Start()
    {
        kameraUtama = Camera.main;
        
        if (nameText == null)
        {
            nameText = GetComponent<TMP_Text>();
        }

        if (nameText != null)
        {
            nameText.text = playerName;
        }
    }

    void LateUpdate()
    {
        // 1. Posisi Mutlak (Abaikan rotasi tubuh karakter, pastikan selalu tepat di atas)
        if (transform.parent != null)
        {
            Vector3 posisiTarget = transform.parent.position + new Vector3(0, tinggiNama, 0);
            
            // 2. Efek Mengambang (Floating)
            if (gunakanFloating)
            {
                float ayunanY = Mathf.Sin(Time.time * kecepatanFloat) * amplitudoFloat;
                posisiTarget.y += ayunanY;
            }
            
            transform.position = posisiTarget;
        }

        // 3. Efek Billboard (Selalu menghadap kamera dari sudut manapun)
        if (kameraUtama != null)
        {
            // Memaksa objek ini menghadap searah dengan arah pandang kamera
            transform.forward = kameraUtama.transform.forward;
        }
    }
}
