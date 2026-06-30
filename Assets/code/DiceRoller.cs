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
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public IEnumerator RollPhysicalDice()
    {
        isRolling = true;
        floatingText.text = ""; 

        // Reset posisi & rotasi dadu ke titik asal agar konsisten
        rb.isKinematic = true; 
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.isKinematic = false;

        // --- MODIFIKASI LEMPARAN HALUS ---
        // Memberi sedikit gaya dorong ke atas (Y) dan sedikit variasi menyamping (X & Z) agar menggelinding natural
        Vector3 throwForce = new Vector3(Random.Range(-1.5f, 1.5f), 5.5f, Random.Range(-1.5f, 1.5f));
        rb.AddForce(throwForce, ForceMode.Impulse);

        // Mengurangi kekuatan putaran (Torque) agar perputaran dadu lebih elegan dan jelas terlihat
        Vector3 elegantTorque = new Vector3(Random.Range(60f, 150f), Random.Range(60f, 150f), Random.Range(60f, 150f));
        rb.AddTorque(elegantTorque, ForceMode.Impulse);

        yield return new WaitForSeconds(0.4f); 

        // Tunggu sampai dadu benar-benar tenang (batas waktu 5 detik agar tidak softlock)
        float timeout = 5f;
        while ((rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f) && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        finalResult = GetTopFace();
        floatingText.text = finalResult.ToString(); 
        
        yield return new WaitForSeconds(1.2f);
        isRolling = false;
    }

    private int GetTopFace()
    {
        int topFace = 1;
        float maxDot = -Mathf.Infinity;
        for (int i = 0; i < diceFaces.Length; i++)
        {
            // Deteksi yang paling mengarah ke atas menggunakan Dot Product
            float dot = Vector3.Dot(diceFaces[i].up, Vector3.up);
            if (dot > maxDot)
            {
                maxDot = dot;
                topFace = i + 1;
            }
        }
        return topFace;
    }
}