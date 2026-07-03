using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Sistem Mini Game")]
    public List<string> namaMiniGames = new List<string>();

    [Header("Pengaturan Kamera")]
    public CinemachineCamera vcamOverview; 
    [Tooltip("Kamera utama yang akan bergantian menyorot pemain aktif")]
    public CinemachineCamera vcamPlayer;   
    public CinemachineCamera vcamDice; 

    [Header("Referensi Objek")]
    public DiceRoller physicalDice; 
    [Tooltip("Masukkan semua petak (Node) berurutan dari awal sampai akhir")]
    public BoardNode[] allBoardNodes; 

    public PlayerMovement ActivePlayer => players[currentPlayerIndex];

    // Status map
    private bool isOverviewActive = false;
    private int diceResult = 0;

    void Start()
    {
        // Pastikan setiap player memiliki komponen PlayerData
        foreach (var player in players)
        {
            if (player != null && player.GetComponent<PlayerData>() == null)
            {
                player.gameObject.AddComponent<PlayerData>();
            }
        }

        // Jika ada data tersimpan (kembali dari Mini-Game), muat data tersebut
        if (GameDataManager.Instance != null && GameDataManager.Instance.hasSavedData)
        {
            LoadGameData();
        }

        // Set target kamera ke pemain saat ini (bisa pemain 1, atau pemain yang tersimpan)
        if (vcamPlayer != null)
        {
            vcamPlayer.Target.TrackingTarget = ActivePlayer.transform;
            // Karakter menghadap kamera dan menyapa
            ActivePlayer.SambutGiliran(vcamPlayer.transform);
        }
        Debug.Log($">>> Giliran Player {currentPlayerIndex + 1}. Tekan SPASI untuk fokus ke dadu.");
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
            if (vcamPlayer != null) vcamPlayer.Priority = 10;
            vcamDice.Priority = 10;
        }
        else
        {
            // Jika Overview mati, jalankan logika sutradara kamera normal
            if (currentPhase == TurnPhase.ReadyToRoll || currentPhase == TurnPhase.Rolling)
            {
                // Sorot dadu
                vcamDice.Priority = 30;
                if (vcamPlayer != null) vcamPlayer.Priority = 10;
                vcamOverview.Priority = 10;
            }
            else // Fase WaitingForFocus atau Moving
            {
                // Sorot pemain
                if (vcamPlayer != null) vcamPlayer.Priority = 30;
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
        
        if (currentPlayerIndex == 0)
        {
            // Satu putaran penuh selesai, mulai mini game!
            StartCoroutine(StartMiniGameSequence());
        }
        else
        {
            // Otomatis pindah target kamera ke pemain berikutnya. 
            if (vcamPlayer != null)
            {
                vcamPlayer.Target.TrackingTarget = ActivePlayer.transform;
                // Karakter selanjutnya menghadap kamera dan menyapa
                ActivePlayer.SambutGiliran(vcamPlayer.transform);
            }

            Debug.Log($">>> Giliran Player {currentPlayerIndex + 1}. Tekan SPASI untuk fokus ke dadu.");
            physicalDice.floatingText.text = "";

            // Reset fase
            currentPhase = TurnPhase.WaitingForFocus;
        }
    }

    IEnumerator StartMiniGameSequence()
    {
        Debug.Log("SEMUA PEMAIN TELAH BERJALAN! BERSIAPLAH UNTUK MINI GAME!");
        
        // Jeda dramatis sebelum pindah scene
        yield return new WaitForSeconds(2f);
        
        if (namaMiniGames.Count > 0)
        {
            int randomIndex = Random.Range(0, namaMiniGames.Count);
            string chosenMiniGame = namaMiniGames[randomIndex];
            Debug.Log($"Memuat Mini Game: {chosenMiniGame}");
            
            // Simpan data permainan sebelum pindah scene
            SaveGameData();
            
            SceneManager.LoadScene(chosenMiniGame);
        }
        else
        {
            Debug.LogWarning("Belum ada nama Mini Game yang ditambahkan di List namaMiniGames!");
            // Fallback: Kembalikan giliran ke player 1 jika mini-game kosong
            if (vcamPlayer != null)
            {
                vcamPlayer.Target.TrackingTarget = ActivePlayer.transform;
                ActivePlayer.SambutGiliran(vcamPlayer.transform);
            }
            physicalDice.floatingText.text = "";
            currentPhase = TurnPhase.WaitingForFocus;
        }
    }

    public void SaveGameData()
    {
        if (GameDataManager.Instance == null) return;
        
        GameDataManager.Instance.savedPlayerNodeIndices = new int[players.Length];
        GameDataManager.Instance.savedPlayerScores = new int[players.Length];
        GameDataManager.Instance.savedPlayerTapes = new int[players.Length];
        GameDataManager.Instance.savedPlayerDauns = new int[players.Length];
        
        for (int i = 0; i < players.Length; i++)
        {
            // Cari indeks dari currentNode milik pemain i di allBoardNodes
            int nodeIndex = -1;
            if (allBoardNodes != null && players[i].currentNode != null)
            {
                for (int j = 0; j < allBoardNodes.Length; j++)
                {
                    if (allBoardNodes[j] == players[i].currentNode)
                    {
                        nodeIndex = j;
                        break;
                    }
                }
            }
            GameDataManager.Instance.savedPlayerNodeIndices[i] = nodeIndex;
            GameDataManager.Instance.savedPlayerScores[i] = 0; // Sementara diset 0
            
            PlayerData pd = players[i].GetComponent<PlayerData>();
            if (pd != null)
            {
                GameDataManager.Instance.savedPlayerTapes[i] = pd.tape;
                GameDataManager.Instance.savedPlayerDauns[i] = pd.daun;
            }
        }
        
        GameDataManager.Instance.savedCurrentPlayerIndex = currentPlayerIndex;
        GameDataManager.Instance.hasSavedData = true;
        Debug.Log("Game Data Tersimpan di GameDataManager!");
    }

    public void LoadGameData()
    {
        if (GameDataManager.Instance == null || !GameDataManager.Instance.hasSavedData) return;

        for (int i = 0; i < players.Length; i++)
        {
            int nodeIndex = GameDataManager.Instance.savedPlayerNodeIndices[i];
            if (allBoardNodes != null && nodeIndex >= 0 && nodeIndex < allBoardNodes.Length)
            {
                players[i].currentNode = allBoardNodes[nodeIndex];
                
                // Gunakan Raycast agar karakter tetap mematuhi Ground Hugging saat di-load
                Vector3 startPos = allBoardNodes[nodeIndex].transform.position;
                Vector3 rayOrigin = new Vector3(startPos.x, startPos.y + 10f, startPos.z);
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, ~LayerMask.GetMask("Player")))
                {
                    startPos.y = hit.point.y + players[i].yOffset;
                }
                else
                {
                    startPos.y += players[i].yOffset;
                }
                players[i].transform.position = startPos;
            }
            
            PlayerData pd = players[i].GetComponent<PlayerData>();
            if (pd != null)
            {
                if (GameDataManager.Instance.savedPlayerTapes != null && i < GameDataManager.Instance.savedPlayerTapes.Length)
                {
                    pd.tape = GameDataManager.Instance.savedPlayerTapes[i];
                }
                if (GameDataManager.Instance.savedPlayerDauns != null && i < GameDataManager.Instance.savedPlayerDauns.Length)
                {
                    pd.daun = GameDataManager.Instance.savedPlayerDauns[i];
                }
            }
        }
        
        currentPlayerIndex = GameDataManager.Instance.savedCurrentPlayerIndex;
        Debug.Log("Game Data Berhasil Dimuat dari GameDataManager!");
    }

#if UNITY_EDITOR
    [ContextMenu("Otomatis Isi Index Petak")]
    public void AutoAssignNodeIndices()
    {
        // Mencari semua petak yang ada di scene
        BoardNode[] allNodes = FindObjectsOfType<BoardNode>();
        
        // Memasukkan semuanya langsung ke dalam array allBoardNodes di GameManager
        allBoardNodes = allNodes; 
        
        for (int i = 0; i < allNodes.Length; i++)
        {
            // Memasukkan nomor indeks ke masing-masing objek petak
            allNodes[i].nodeIndex = i;
            
            // Memberitahu Unity bahwa komponen ini telah berubah agar bisa di-save
            UnityEditor.EditorUtility.SetDirty(allNodes[i]);
        }
        
        // Memberitahu Unity bahwa GameManager juga berubah (karena array allBoardNodes terisi)
        UnityEditor.EditorUtility.SetDirty(this);
        
        Debug.Log($"Berhasil mengisi indeks dan mendaftarkan {allNodes.Length} petak secara otomatis!");
    }
#endif
}