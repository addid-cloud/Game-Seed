using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public BoardNode currentNode; // Posisi kotak saat ini
    public float moveSpeed = 5f;
    [Header("Pengaturan Animasi Gerak")]
    public float yOffset = 0.5f;
    public float rotationSpeed = 10f; // Kecepatan putar menghadap tujuan
    
    [Header("Referensi Visual")]
    public PlayerVisualController visualController; // Referensi untuk memicu animasi
    
    [HideInInspector] public bool isMoving = false;
    public bool isWaitingForBranch = false;
    private BoardNode chosenNextNode = null;

    public IEnumerator MoveSteps(int steps)
    {
        if (currentNode == null) yield break;
        isMoving = true;
        int remainingSteps = steps;

        while (remainingSteps > 0)
        {
            // 1. Cek apakah jalan buntu
            if (currentNode.nextNodes.Count == 0)
            {
                Debug.Log("Jalan buntu!");
                break;
            }

            // 2. Cek apakah ini jalan bercabang
            if (currentNode.nextNodes.Count > 1)
            {
                Debug.Log("Jalur bercabang! Tekan '1' untuk jalur pertama, '2' untuk jalur kedua.");
                isWaitingForBranch = true;
                chosenNextNode = null;

                // Game akan "pause" di sini sampai pemain menekan tombol pilihan
                while (isWaitingForBranch)
                {
                    yield return null; 
                }
                
                currentNode = chosenNextNode;
            }
            else
            {
                // Kalau cuma ada 1 jalan, otomatis maju ke sana
                currentNode = currentNode.nextNodes[0];
            }

            // 3. Bergerak lurus mendatar tanpa fisika
            Vector3 startPos = transform.position;
            Vector3 target = currentNode.transform.position + new Vector3(0, yOffset, 0);

            // Hadap ke arah target secara perlahan
            Vector3 direction = (target - startPos).normalized;
            direction.y = 0; // Kunci sumbu Y agar tidak mendongak/menunduk
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                    yield return null;
                }
                transform.rotation = targetRot;
            }

            // Memicu animasi jalan
            if (visualController != null) visualController.AturPergerakan(true);

            float timeElapsed = 0f;
            // Menghitung estimasi waktu perjalanan berdasarkan jarak dan moveSpeed
            float distance = Vector3.Distance(startPos, target);
            float duration = distance / moveSpeed;

            while (timeElapsed <= duration)
            {
                timeElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(timeElapsed / duration);

                // 1. Hitung posisi dasar X dan Z menggunakan Lerp
                Vector3 currentPos = Vector3.Lerp(startPos, target, t);

                // 2. Siapkan Raycast dari posisi tinggi untuk mendeteksi tanah di bawahnya
                float rayOriginY = Mathf.Max(startPos.y, target.y) + 10f;
                Vector3 rayOrigin = new Vector3(currentPos.x, rayOriginY, currentPos.z);

                // 3. Gunakan LayerMask untuk MENGABAIKAN layer "Player"
                int layerMask = ~LayerMask.GetMask("Player");

                // 4. Tembakkan Raycast ke bawah (Vector3.down)
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, layerMask))
                {
                    // Jika kena tanah, posisikan Y karakter di titik sentuh tanah tersebut + yOffset
                    currentPos.y = hit.point.y + yOffset;
                }
                else
                {
                    // Fallback jika tidak mendeteksi tanah
                    currentPos.y = Mathf.Lerp(startPos.y, target.y, t);
                }

                // 6. Terakhir, terapkan ke posisi
                transform.position = currentPos;
                yield return null;
            }

            // Memastikan posisi akhir di target juga presisi di atas tanah
            Vector3 finalPos = target;
            float finalRayY = finalPos.y + 10f;
            if (Physics.Raycast(new Vector3(finalPos.x, finalRayY, finalPos.z), Vector3.down, out RaycastHit finalHit, 20f, ~LayerMask.GetMask("Player")))
            {
                finalPos.y = finalHit.point.y + yOffset;
            }
            transform.position = finalPos;
            
            // Menghentikan animasi jalan sesaat jika ada jeda antar petak
            if (visualController != null) visualController.AturPergerakan(false);
            remainingSteps--; 
            yield return new WaitForSeconds(0.1f); 
        }

        // Pastikan animasi benar-benar mati setelah selesai giliran
        if (visualController != null) visualController.AturPergerakan(false);
        isMoving = false;
        Debug.Log("Giliran Selesai!");
    }

    // Fungsi untuk memilih jalur saat di persimpangan
    public void SelectBranch(int branchIndex)
    {
        if (isWaitingForBranch && branchIndex < currentNode.nextNodes.Count)
        {
            chosenNextNode = currentNode.nextNodes[branchIndex];
            isWaitingForBranch = false; // Lanjutkan pergerakan
        }
    }

    // Fungsi untuk menyambut giliran (menghadap kamera dan memainkan animasi)
    public void SambutGiliran(Vector3 posisiKamera)
    {
        StartCoroutine(ProsesMenolehKamera(posisiKamera));
    }

    private IEnumerator ProsesMenolehKamera(Vector3 posisiKamera)
    {
        // 1. Hitung arah menghadap kamera dengan mengabaikan sumbu Y (ketinggian)
        Vector3 direction = (posisiKamera - transform.position).normalized;
        direction.y = 0f; 
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            
            // Putar badan secara halus sampai menghadap kamera
            while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
            {
                // Menggunakan kecepatan rotasi yang lebih lambat agar terlihat elegan
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * (rotationSpeed * 0.5f));
                yield return null;
            }
            transform.rotation = targetRot;
        }

        // 2. Mainkan animasi selebrasi / giliran setelah posisi pas
        if (visualController != null)
        {
            visualController.MulaiGiliran();
        }
    }
}