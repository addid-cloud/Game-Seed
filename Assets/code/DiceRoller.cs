using UnityEngine;
using TMPro;
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    public Rigidbody rb;
    public TextMeshPro floatingText;
    
    [Header("Masukkan Face_1 sampai Face_6 berurutan")]
    public Transform[] diceFaces; 

    [HideInInspector] public bool isRolling = false;
    [HideInInspector] public int finalResult = 0;

    private Vector3 startPosition;

    void Start()
    {
        // Menyimpan posisi awal dadu agar bisa dikembalikan saat dilempar lagi
        startPosition = transform.position;
    }

    public IEnumerator RollPhysicalDice()
    {
        isRolling = true;
        floatingText.text = ""; // Sembunyikan teks

        // Reset posisi dadu ke titik awal sebelum dilempar
        rb.isKinematic = true; 
        transform.position = startPosition;
        rb.isKinematic = false;

        // Lempar dadu ke atas dan putar secara acak
        rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(100, 500), Random.Range(100, 500), Random.Range(100, 500)));

        yield return new WaitForSeconds(0.5f); 

        // Tunggu sampai pergerakan dadu benar-benar berhenti
        while (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
        {
            yield return null;
        }

        // Baca hasil dan tampilkan
        finalResult = GetTopFace();
        floatingText.text = finalResult.ToString(); 
        
        // Jeda 1.5 detik agar pemain bisa melihat angka yang keluar sebelum kamera berpindah
        yield return new WaitForSeconds(1.5f);

        isRolling = false;
    }

    private int GetTopFace()
    {
        int topFace = 1;
        float highestY = -Mathf.Infinity;
        for (int i = 0; i < diceFaces.Length; i++)
        {
            if (diceFaces[i].position.y > highestY)
            {
                highestY = diceFaces[i].position.y;
                topFace = i + 1;
            }
        }
        return topFace;
    }
}