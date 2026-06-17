using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerMovement player;

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            int diceResult = Random.Range(1, 7);

            Debug.Log("Dadu: " + diceResult);

            StartCoroutine(player.MoveSteps(diceResult));
        }
    }
}