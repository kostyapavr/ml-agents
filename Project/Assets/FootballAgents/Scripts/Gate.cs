using UnityEngine;

public class Gate : MonoBehaviour
{
    public GameManager gameManager;
    public bool isTeam1;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ball"))
        {
            gameManager.GoalScored(!isTeam1);
        }
    }
}
