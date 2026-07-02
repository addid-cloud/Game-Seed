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

    private Vector3 posisiAwalLocal;
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

        posisiAwalLocal = transform.localPosition;
    }

    void LateUpdate()
    {
        // 1. Efek Billboard (Selalu menghadap kamera dari sudut manapun)
        if (kameraUtama != null)
        {
            // Memaksa objek ini menghadap searah dengan arah pandang kamera
            transform.forward = kameraUtama.transform.forward;
        }

        // 2. Efek Mengambang (Floating) di udara
        if (gunakanFloating)
        {
            float ayunanY = Mathf.Sin(Time.time * kecepatanFloat) * amplitudoFloat;
            transform.localPosition = posisiAwalLocal + new Vector3(0, ayunanY, 0);
        }
    }
}
