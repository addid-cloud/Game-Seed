using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    [Header("Daftar Pemain")]
    public PlayerMovement[] players; // Diubah menjadi Array agar bisa menampung banyak pemain
    private int currentPlayerIndex = 0;

    [Header("Pengaturan Kamera")]
    public CinemachineCamera vcamOverview; 
    public CinemachineCamera vcamPlayer;   

    private bool isTurnActive = false; // Penanda agar spasi tidak bisa di-spam

    // Variabel pembantu untuk memanggil pemain yang gilirannya sedang aktif
    public PlayerMovement ActivePlayer => players[currentPlayerIndex];

    void Start()
    {
        Debug.Log(">>> Game Dimulai! Giliran Player 1");
    }

    void Update()
    {
        // 1. Logika Kamera
        if (ActivePlayer.isMoving && !ActivePlayer.isWaitingForBranch)
        {
            vcamPlayer.Priority = 20;
            vcamOverview.Priority = 10;
            
            // Kamera otomatis berpindah target ke pemain yang sedang jalan (Fitur CM3)
            vcamPlayer.Target.TrackingTarget = ActivePlayer.transform;
        }
        else
        {
            vcamPlayer.Priority = 10;
            vcamOverview.Priority = 20;
        }

        // 2. Logika Lempar Dadu
        // Hanya bisa lempar dadu jika tidak ada yang sedang jalan
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isTurnActive)
        {
            StartCoroutine(PlayTurn());
        }

        // 3. Logika Pilih Cabang (Pilihan dikirim ke pemain yang sedang aktif)
        if (Keyboard.current.digit1Key.wasPressedThisFrame) ActivePlayer.SelectBranch(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) ActivePlayer.SelectBranch(1);
    }

    // Coroutine khusus untuk mengatur alur giliran dari awal sampai akhir
    IEnumerator PlayTurn()
    {
        isTurnActive = true;

        int diceResult = Random.Range(1, 4);
        Debug.Log("=== Player " + (currentPlayerIndex + 1) + " Lempar Dadu: " + diceResult + " ===");

        // Menunggu pemain yang aktif selesai berjalan
        yield return StartCoroutine(ActivePlayer.MoveSteps(diceResult));

        // --- Proses Giliran Selesai ---
        
        currentPlayerIndex++; // Pindah ke indeks pemain berikutnya

        // Jika semua pemain sudah jalan, kembali ke Player 1 (indeks 0)
        if (currentPlayerIndex >= players.Length)
        {
            currentPlayerIndex = 0; 
        }

        Debug.Log(">>> Giliran berpindah ke Player " + (currentPlayerIndex + 1));
        isTurnActive = false; // Buka kunci agar pemain berikutnya bisa menekan Spasi
    }
}