using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform[] spaces;

    public int currentSpace = 0;
    public float moveSpeed = 5f;

    public IEnumerator MoveSteps(int steps)
    {
        if (spaces == null || spaces.Length == 0)
        {
            Debug.LogError("Spaces belum diisi!");
            yield break;
        }

        for (int i = 0; i < steps; i++)
        {
            currentSpace++;

            if (currentSpace >= spaces.Length)
                currentSpace = 0;

            Vector3 target = spaces[currentSpace].position;

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = target;

            yield return new WaitForSeconds(0.1f);
        }
    }
}