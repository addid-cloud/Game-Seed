using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [Header("Resources")]
    public int tape = 0;
    public int daun = 0;

    [Header("Win Condition")]
    public int tapeToWin = 3;

    public void AddTape(int amount)
    {
        tape += amount;

        Debug.Log("Tape : " + tape);

        if (tape >= tapeToWin)
        {
            WinGame();
        }
    }

    public void AddDaun(int amount)
    {
        daun += amount;

        Debug.Log("Daun : " + daun);
    }

    public bool SpendDaun(int amount)
    {
        if (daun >= amount)
        {
            daun -= amount;
            return true;
        }

        return false;
    }

    void WinGame()
    {
        Debug.Log(gameObject.name + " MENANG!");
        
        // Panggil fungsi TriggerWin dari GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerWin(this);
        }
    }
}