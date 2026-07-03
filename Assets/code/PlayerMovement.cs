using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public BoardNode currentNode; // Posisi kotak saat ini
    public float moveSpeed = 5f;
    [Header("Pengaturan Animasi Gerak")]
    public float yOffset = 0.5f;
    public float rotationSpeed = 10f; // Kecepatan putar menghadap tujuan
    [Tooltip("Isi 180 jika karakter berlari menghadap belakang (tergantung bawaan model 3D)")]
    public float rotationOffsetY = 0f;
    
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
                Debug.Log("Jalur bercabang! Menunggu input pemain...");
                isWaitingForBranch = true;
                chosenNextNode = null;

                // Tampilkan UI Pemilihan Cabang di Layar
                if (GameManager.Instance != null) GameManager.Instance.TampilkanUICabang();

                // Game akan "pause" di sini sampai pemain menekan tombol pilihan
                while (isWaitingForBranch)
                {
                    yield return null; 
                }
                
                // Sembunyikan UI setelah pemain memilih
                if (GameManager.Instance != null) GameManager.Instance.SembunyikanUICabang();

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
                Quaternion targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffsetY, 0);
                while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                    yield return null;
                }
                transform.rotation = targetRot;
            }

            // Cek pengaturan lompat yang sudah kita atur manual di Inspector node tujuan
            bool isLompatVertikal = currentNode.harusLompatKeSini;

            if (isLompatVertikal)
            {
                // Eksekusi lompatan bertenaga (ketinggian lengkungan 1.5f, durasi 0.5 detik - terasa kenceng)
                yield return StartCoroutine(LompatBertenaga(target, 1.5f, 0.5f));
            }
            else
            {
                if (visualController != null)
                {
                    visualController.AturPergerakan(true); // Mainkan lari normal
                }

                float timeElapsed = 0f;
                // Menghitung estimasi waktu perjalanan berdasarkan jarak dan moveSpeed
                float distance = Vector3.Distance(startPos, target);
                float duration = distance / moveSpeed;

                while (timeElapsed <= duration)
                {
                    timeElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(timeElapsed / duration);

                    // Hitung posisi dasar menggunakan Lerp
                    Vector3 currentPos = Vector3.Lerp(startPos, target, t);

                    // Siapkan Raycast dari posisi tinggi
                    float rayOriginY = Mathf.Max(startPos.y, target.y) + 10f;
                    Vector3 rayOrigin = new Vector3(currentPos.x, rayOriginY, currentPos.z);
                    int layerMask = ~LayerMask.GetMask("Player");

                    // Raycast ke bawah
                    if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, layerMask))
                    {
                        currentPos.y = hit.point.y + yOffset;
                    }
                    else
                    {
                        currentPos.y = Mathf.Lerp(startPos.y, target.y, t);
                    }

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
            }
            
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

    private IEnumerator LompatBertenaga(Vector3 targetPos, float height, float duration)
    {
        // 3. Integrasi: Pastikan animasi lompat vertikal dipanggil di awal
        if (visualController != null)
        {
            visualController.AturPergerakan(false);
            visualController.LompatVertikal(); // Memanggil anim.SetTrigger("TriggerLompatVertikal")
        }

        Vector3 startPos = transform.position;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            // Pastikan t tidak melebihi 1
            float t = Mathf.Clamp01(timeElapsed / duration);

            // Lerp linear untuk posisi dasar X dan Z
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // 1. Pergerakan Parabola (busur)
            // Rumus parabola dasar: 4 * height * t * (1 - t)
            // Akan bernilai 0 di t=0, bernilai 'height' di t=0.5, dan 0 di t=1.
            float parabola = 4f * height * t * (1f - t);
            
            // Posisi Y dasar adalah lerp linear antara Y asal dan Y tujuan
            float baseY = Mathf.Lerp(startPos.y, targetPos.y, t);
            
            // Tambahkan lengkungan parabola ke atas base Y
            currentPos.y = baseY + parabola;

            transform.position = currentPos;
            yield return null;
        }

        // 4. Presisi: Pastikan posisi akhir karakter persis di targetPos
        transform.position = targetPos;

        // Beri tahu visual controller bahwa lompatan fisik sudah selesai
        if (visualController != null)
        {
            visualController.AkhiriLompatVertikal();
        }
    }

    // Fungsi untuk memilih jalur saat di persimpangan
    public void SelectBranch(int branchIndex)
    {
        Debug.Log($"SelectBranch dipanggil dengan index: {branchIndex}. Total cabang tersedia: {currentNode.nextNodes.Count}");
        if (isWaitingForBranch && branchIndex < currentNode.nextNodes.Count)
        {
            chosenNextNode = currentNode.nextNodes[branchIndex];
            isWaitingForBranch = false; // Lanjutkan pergerakan
            Debug.Log($"Cabang {branchIndex} berhasil dipilih!");
        }
        else
        {
            Debug.LogWarning($"Gagal memilih cabang! isWaiting: {isWaitingForBranch}, Index valid?: {branchIndex < currentNode.nextNodes.Count}");
        }
    }

    // Fungsi untuk menyambut giliran (menghadap kamera dan memainkan animasi)
    public void SambutGiliran(Transform kamera)
    {
        StartCoroutine(ProsesMenolehKamera(kamera));
    }

    private IEnumerator ProsesMenolehKamera(Transform kamera)
    {
        float timeElapsed = 0f;
        float duration = 1.5f; // Durasi tracking, pas dengan waktu terbang Cinemachine

        // 1. Secara dinamis terus menatap kamera yang sedang melayang
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            
            if (kamera != null)
            {
                Vector3 direction = (kamera.position - transform.position).normalized;
                direction.y = 0f; 
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffsetY, 0);
                    // Putar badan secara halus secara real-time
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                }
            }
            yield return null;
        }

        // 2. Mainkan animasi selebrasi / giliran setelah posisi pas
        if (visualController != null)
        {
            visualController.MulaiGiliran();
        }
    }
}