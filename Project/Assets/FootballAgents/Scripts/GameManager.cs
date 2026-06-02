using UnityEngine;
using Unity.MLAgents;
using TMPro;

public class GameManager : MonoBehaviour
{
    public AttackerAgent attacker1;
    public AttackerAgent attacker2;
    public GoalieAgent goalie1;
    public GoalieAgent goalie2;

    public Transform ball;
    public Rigidbody ballRb;

    public TextMeshPro goalCounterTeam1;
    public TextMeshPro goalCounterTeam2;

    float episodeTime;
    int episodeCounter = 0;

    public int episodeDuration = 50;
    public Vector3 fieldCenterPos;
    public int timeScale = 10;

    private void Awake()
    {
        Time.timeScale = timeScale;
    }

    void Update()
    {
        episodeTime += Time.deltaTime;

        if (episodeTime > episodeDuration)
        {
            EndEpisode();
            episodeCounter++;
            Debug.Log($"Episode: {episodeCounter}");
            goalCounterTeam1.text = "0";
            goalCounterTeam2.text = "0";
        }
    }

    public void GoalScored(bool team1)
    {
        if (team1)
        {
            attacker1.AddReward(+1f);
            goalie1.AddReward(+1f);

            attacker2.AddReward(-1f);
            goalie2.AddReward(-1f);

            goalCounterTeam1.text = (int.Parse(goalCounterTeam1.text) + 1).ToString();
        }
        else
        {
            attacker2.AddReward(+1f);
            goalie2.AddReward(+1f);

            attacker1.AddReward(-1f);
            goalie1.AddReward(-1f);

            goalCounterTeam2.text = (int.Parse(goalCounterTeam2.text) + 1).ToString();
        }

        EndEpisode();
    }

    void EndEpisode()
    {
        episodeTime = 0;

        attacker1.EndEpisode();
        attacker2.EndEpisode();
        goalie1.EndEpisode();
        goalie2.EndEpisode();

        ResetScene();
    }

    void ResetScene()
    {
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ball.localPosition = new Vector3(fieldCenterPos.x + Random.Range(-2f, 2f), fieldCenterPos.y + 1, fieldCenterPos.z + Random.Range(-2f, 2f));
        attacker1.transform.localPosition = new Vector3(fieldCenterPos.x + Random.Range(-2f, 2f), fieldCenterPos.y + 0.75f, fieldCenterPos.z - 8 + Random.Range(-2f, 2f));
        attacker2.transform.localPosition = new Vector3(fieldCenterPos.x + Random.Range(-2f, 2f), fieldCenterPos.y + 0.75f, fieldCenterPos.z + 8 + Random.Range(-2f, 2f));
        goalie1.transform.localPosition = new Vector3(fieldCenterPos.x + Random.Range(-2f, 2f), fieldCenterPos.y + 0.75f, fieldCenterPos.z - 11 + Random.Range(-2f, 2f));
        goalie2.transform.localPosition = new Vector3(fieldCenterPos.x + Random.Range(-2f, 2f), fieldCenterPos.y + 0.75f, fieldCenterPos.z + 11 + Random.Range(-2f, 2f));
        Time.timeScale = timeScale;
    }
}
