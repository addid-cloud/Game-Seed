using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public PlayerData player;

    public TMP_Text tapeText;
    public TMP_Text daunText;

    void Update()
    {
        tapeText.text = "📼 " + player.tape;
        daunText.text = "🍃 " + player.daun;
    }
}