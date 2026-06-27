using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    [Header("Daftar Pemain")]
    public PlayerMovement[] players; 
    private int currentPlayerIndex = 0;

    [Header("Pengaturan Kamera")]
    public CinemachineCamera vcamOverview; 
    public CinemachineCamera vcamPlayer;   
    public CinemachineCamera vcamDice; // Tempat masuknya kamera dadu

    [Header("Referensi Objek")]
    public DiceRoller physicalDice; // Tempat masuknya script dadu

    private bool isTurnActive = false; 

    public PlayerMovement ActivePlayer => players[currentPlayerIndex];

    void Start()
    {
        Debug.Log(">>> Game Dimulai! Giliran Player 1");
    }

    void Update()
    {
        // 1. Logika Kamera Sinematik Terpadu
        if (physicalDice.isRolling)
        {
            // Jika dadu sedang dilempar, kamera fokus ke dadu
            vcamDice.Priority = 30;
            vcamPlayer.Priority = 10;
            vcamOverview.Priority = 10;
        }
        else if (ActivePlayer.isMoving && !ActivePlayer.isWaitingForBranch)
        {
            // Jika pemain bergerak, kamera mengikuti pemain
            vcamDice.Priority = 10;
            vcamPlayer.Priority = 30;
            vcamOverview.Priority = 10;
            vcamPlayer.Target.TrackingTarget = ActivePlayer.transform;
        }
        else
        {
            // Jika diam menunggu giliran / di persimpangan, sorot keseluruhan papan
            vcamDice.Priority = 10;
            vcamPlayer.Priority = 10;
            vcamOverview.Priority = 30;
        }

        // 2. Logika Tekan Spasi (Lempar Dadu)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isTurnActive)
        {
            StartCoroutine(PlayTurn());
        }

        // 3. Logika Memilih Cabang Jalan
        if (Keyboard.current.digit1Key.wasPressedThisFrame) ActivePlayer.SelectBranch(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) ActivePlayer.SelectBranch(1);
    }

    IEnumerator PlayTurn()
    {
        isTurnActive = true;

        // Tahap 1: Kamera otomatis ke dadu, dan dadu dilempar
        yield return StartCoroutine(physicalDice.RollPhysicalDice());

        // Tahap 2: Ambil angka dadu fisik yang baru saja keluar
        int diceResult = physicalDice.finalResult;
        Debug.Log("=== Player " + (currentPlayerIndex + 1) + " Dapat Angka: " + diceResult + " ===");

        // Tahap 3: Pemain jalan (Kamera otomatis mengejar pemain)
        yield return StartCoroutine(ActivePlayer.MoveSteps(diceResult));
        
        // Tahap 4: Akhir giliran, ganti pemain
        currentPlayerIndex++; 
        if (currentPlayerIndex >= players.Length) currentPlayerIndex = 0; 
        
        Debug.Log(">>> Giliran berpindah ke Player " + (currentPlayerIndex + 1));
        
        // Sembunyikan teks angka di dadu untuk bersiap di giliran orang berikutnya
        physicalDice.floatingText.text = "";
        
        isTurnActive = false; 
    }
}