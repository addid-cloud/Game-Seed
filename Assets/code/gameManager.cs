using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    // Fase giliran sekarang lebih ringkas (tanpa DiceFinished)
    private enum TurnPhase { WaitingForFocus, ReadyToRoll, Rolling, Moving }
    private TurnPhase currentPhase = TurnPhase.WaitingForFocus;

    [Header("Pengaturan Waktu")]
    [SerializeField] private float waitBeforeMoving = 1.5f;

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

    // Status map
    private bool isOverviewActive = false;
    private int diceResult = 0;

    void Start()
    {
        // Set target kamera ke pemain pertama di awal game
        vcamPlayer.Target.TrackingTarget = ActivePlayer.transform;
        Debug.Log(">>> Giliran Player 1. Tekan SPASI untuk fokus ke dadu.");
        
        // Karakter menghadap kamera dan menyapa
        ActivePlayer.SambutGiliran(vcamPlayer.transform.position);
    }

    void Update()
    {
        // --- 1. FITUR OVERVIEW MAP (Tombol 'O') ---
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            isOverviewActive = !isOverviewActive;
        }

        // --- 2. LOGIKA PRIORITAS KAMERA ---
        if (isOverviewActive)
        {
            // Jika Overview aktif, paksa kamera atas mengambil alih
            vcamOverview.Priority = 30;
            vcamPlayer.Priority = 10;
            vcamDice.Priority = 10;
        }
        else
        {
            // Jika Overview mati, jalankan logika sutradara kamera normal
            if (currentPhase == TurnPhase.ReadyToRoll || currentPhase == TurnPhase.Rolling)
            {
                // Sorot dadu
                vcamDice.Priority = 30;
                vcamPlayer.Priority = 10;
                vcamOverview.Priority = 10;
            }
            else // Fase WaitingForFocus atau Moving
            {
                // Sorot pemain
                vcamPlayer.Priority = 30;
                vcamDice.Priority = 10;
                vcamOverview.Priority = 10;
            }
        }

        // --- 3. LOGIKA INPUT SPASI (Hanya 2 Tahap) ---
        // Spasi tidak berfungsi jika sedang melihat Overview Map atau sedang memilih cabang
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isOverviewActive && !ActivePlayer.isWaitingForBranch)
        {
            if (currentPhase == TurnPhase.WaitingForFocus)
            {
                currentPhase = TurnPhase.ReadyToRoll;
                Debug.Log("Kamera fokus ke dadu. Tekan SPASI sekali lagi untuk melempar!");
            }
            else if (currentPhase == TurnPhase.ReadyToRoll)
            {
                StartCoroutine(RollDiceSequence());
            }
        }

        // --- 4. LOGIKA CABANG JALAN ---
        if (Keyboard.current.digit1Key.wasPressedThisFrame && ActivePlayer.isWaitingForBranch) ActivePlayer.SelectBranch(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && ActivePlayer.isWaitingForBranch) ActivePlayer.SelectBranch(1);
    }

    IEnumerator RollDiceSequence()
    {
        currentPhase = TurnPhase.Rolling;
        
        // Tunggu sampai dadu fisik selesai berguling
        yield return StartCoroutine(physicalDice.RollPhysicalDice());
        diceResult = physicalDice.finalResult;
        
        Debug.Log($"Dadu berhenti di angka {diceResult}. Bersiap jalan...");

        // JEDA DRAMATIS: Biarkan pemain melihat hasil dadu sebelum bergerak
        yield return new WaitForSeconds(waitBeforeMoving);
        
        // OTOMATIS: Langsung pindah ke fase berjalan tanpa perlu Spasi lagi!
        StartCoroutine(MovePlayerSequence());
    }

    IEnumerator MovePlayerSequence()
    {
        currentPhase = TurnPhase.Moving;
        Debug.Log($"=== Player {currentPlayerIndex + 1} Mengambil Langkah: {diceResult} ===");

        // Tunggu sampai karakter selesai berjalan di papan
        yield return StartCoroutine(ActivePlayer.MoveSteps(diceResult));
        
        // --- GANTI GILIRAN & EFEK RIVAL PAN ---
        currentPlayerIndex++; 
        if (currentPlayerIndex >= players.Length) currentPlayerIndex = 0; 
        
        // Otomatis pindah target kamera ke pemain berikutnya. 
        // Cinemachine akan memicu transisi melayang (pan) secara halus!
        vcamPlayer.Target.TrackingTarget = ActivePlayer.transform;

        Debug.Log($">>> Giliran Player {currentPlayerIndex + 1}. Tekan SPASI untuk fokus ke dadu.");
        physicalDice.floatingText.text = "";
        
        // Karakter selanjutnya menghadap kamera dan menyapa
        ActivePlayer.SambutGiliran(vcamPlayer.transform.position);

        // Reset fase
        currentPhase = TurnPhase.WaitingForFocus;
    }
}