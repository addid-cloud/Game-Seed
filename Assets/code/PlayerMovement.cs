using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public BoardNode currentNode; // Posisi kotak saat ini
    public float moveSpeed = 5f;
    
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

            // 3. Bergerak ke target
            Vector3 target = currentNode.transform.position;
            target.y += 1f; // Angkat sedikit agar kapsul berdiri di atas kotak, tidak tenggelam

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
            remainingSteps--; 
            yield return new WaitForSeconds(0.1f); 
        }

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
}