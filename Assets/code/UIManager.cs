using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;

    public TMP_Text tapeText;
    public TMP_Text daunText;
    public TMP_Text turnText;

    void Update()
    {
        if (gameManager != null && gameManager.ActivePlayer != null)
        {
            PlayerData player = gameManager.ActivePlayer.GetComponent<PlayerData>();
            if (player != null)
            {
                if (tapeText != null) tapeText.text = "📼 " + player.tape;
                if (daunText != null) daunText.text = "🍃 " + player.daun;
            }

            if (turnText != null)
            {
                turnText.text = "TURN\n\n\n" + gameManager.ActivePlayer.name;
            }
        }
    }
}