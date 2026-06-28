using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    // Struktur fase untuk mengatur urutan Spasi
    private enum TurnPhase { WaitingForFocus, ReadyToRoll, Rolling, Moving }
    private TurnPhase currentPhase = TurnPhase.WaitingForFocus;

    [Header("Daftar Pemain")]
    public PlayerMovement[] players; 
    private int currentPlayerIndex = 0;

    [Header("Pengaturan Kamera")]
    public CinemachineCamera vcamOverview; 
    public CinemachineCamera vcamPlayer;   
    public CinemachineCamera vcamDice; 

    [Header("Referensi Objek")]
    public DiceRoller physicalDice; 

    public PlayerMovement ActivePlayer => players[currentPlayerIndex];

    void Start()
    {
        Debug.Log(">>> Giliran Player 1. Tekan SPASI untuk fokus ke dadu.");
    }

    void Update()
    {
        // 1. Logika Pengontrol Kamera Berdasarkan Fase Aktual
        if (currentPhase == TurnPhase.ReadyToRoll || currentPhase == TurnPhase.Rolling)
        {
            vcamDice.Priority = 30;
            vcamPlayer.Priority = 10;
            vcamOverview.Priority = 10;
        }
        else if (currentPhase == TurnPhase.Moving && !ActivePlayer.isWaitingForBranch)
        {
            vcamDice.Priority = 10;
            vcamPlayer.Priority = 30;
            vcamOverview.Priority = 10;
            vcamPlayer.Target.TrackingTarget = ActivePlayer.transform;
        }
        else // Ketika WaitingForFocus atau sedang memilih jalan buntu/cabang
        {
            vcamDice.Priority = 10;
            vcamPlayer.Priority = 10;
            vcamOverview.Priority = 30;
        }

        // 2. Logika Input Spasi Dua Tahap
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // TAHAP 1: Kamera pindah ke dadu, dadu belum dilempar
            if (currentPhase == TurnPhase.WaitingForFocus)
            {
                currentPhase = TurnPhase.ReadyToRoll;
                Debug.Log("Kamera fokus ke dadu. Tekan SPASI sekali lagi untuk melempar!");
            }
            // TAHAP 2: Lempar dadu fisik
            else if (currentPhase == TurnPhase.ReadyToRoll)
            {
                StartCoroutine(PlayTurnSequence());
            }
        }

        // 3. Logika Memilih Cabang Jalan
        if (Keyboard.current.digit1Key.wasPressedThisFrame) ActivePlayer.SelectBranch(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) ActivePlayer.SelectBranch(1);
    }

    IEnumerator PlayTurnSequence()
    {
        // Kunci fase agar spasi tidak bisa ditekan lagi saat dadu menggelinding
        currentPhase = TurnPhase.Rolling;

        // Jalankan lemparan dadu fisik (kamera tetap fokus di dadu)
        yield return StartCoroutine(physicalDice.RollPhysicalDice());

        int diceResult = physicalDice.finalResult;
        Debug.Log("=== Player " + (currentPlayerIndex + 1) + " Mengambil Langkah: " + diceResult + " ===");

        // Pindah ke fase bergerak (kamera otomatis terbang mengejar player)
        currentPhase = TurnPhase.Moving;
        yield return StartCoroutine(ActivePlayer.MoveSteps(diceResult));
        
        // Ganti giliran ke player berikutnya
        currentPlayerIndex++; 
        if (currentPlayerIndex >= players.Length) currentPlayerIndex = 0; 
        
        Debug.Log(">>> Giliran Player " + (currentPlayerIndex + 1) + ". Tekan SPASI untuk fokus ke dadu.");
        
        physicalDice.floatingText.text = "";
        
        // Kembalikan ke fase awal agar pemain berikutnya harus menekan spasi 2x juga
        currentPhase = TurnPhase.WaitingForFocus;
    }
}