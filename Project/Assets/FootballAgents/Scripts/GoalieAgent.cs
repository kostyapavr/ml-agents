using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class GoalieAgent : Agent
{
    public Transform ball;
    public Transform gate;
    public Rigidbody rb;

    private float moveSpeed = 0.14f;

    public bool isTeam1;
    public Material materialTeam1;
    public Material materialTeam2;

    Vector3 idealDefendPosition;
    float prevIdealPosDistance;

    public override void OnEpisodeBegin()
    {
        if (isTeam1) GetComponent<MeshRenderer>().material = materialTeam1;
        else GetComponent<MeshRenderer>().material = materialTeam2;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;

        idealDefendPosition = gate.position + (ball.position - gate.position).normalized * 4f;
        prevIdealPosDistance = Vector3.Distance(transform.position, idealDefendPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.InverseTransformDirection(ball.position - transform.position));
        sensor.AddObservation(transform.InverseTransformDirection(gate.position - transform.position));
        sensor.AddObservation(rb.linearVelocity);
        //sensor.AddObservation(GetRelative(ball.GetComponent<Rigidbody>().linearVelocity));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotate = actions.ContinuousActions[2];

        transform.Rotate(0, rotate * 250f * Time.deltaTime, 0);

        Vector3 move = transform.forward * moveZ + transform.right * moveX;
        rb.AddForce(move * moveSpeed, ForceMode.VelocityChange);
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, 2f);

        AddReward(-0.001f);
        if (rb.linearVelocity.magnitude > 1f)
            AddReward(rb.linearVelocity.magnitude * -0.08f);

        Vector3 pos = gate.position + (ball.position - gate.position).normalized * 4f;
        idealDefendPosition = pos;
        idealDefendPosition.x = Mathf.Clamp(idealDefendPosition.x, -2.25f, 2.25f);
        idealDefendPosition.z = gate.position.z + (isTeam1 ? 2.5f : -2.5f);

        float distToDefendPos = Vector3.Distance(transform.position, idealDefendPosition);
        AddReward(-distToDefendPos * 0.01f);
        AddReward((prevIdealPosDistance - distToDefendPos) * 0.01f);
        if (distToDefendPos < 0.75f)
        {
            AddReward(+0.03f);
        }

        if (distToDefendPos > 3f) AddReward(-0.03f);
        
        prevIdealPosDistance = distToDefendPos;

        float distToBall = Vector3.Distance(transform.position, ball.position);
        if (distToBall < 2f)
        {
            AddReward(+0.01f);
        }

        Vector3 toBall = (ball.position - transform.position).normalized;
        float ballAimAlignment = Vector3.Dot(transform.forward, toBall);
        AddReward(ballAimAlignment * 0.002f);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            Rigidbody ballRb = col.gameObject.GetComponent<Rigidbody>();
            Vector3 toBall = (col.transform.position - transform.position).normalized;
            Vector3 force = toBall * 4f + rb.linearVelocity * 2f;
            ballRb.AddForce(force, ForceMode.Impulse);

            AddReward(0.15f);
            Vector3 awayFromGate = (ball.position - gate.position).normalized;
            float clearQuality = Vector3.Dot(ballRb.linearVelocity.normalized, awayFromGate);
            AddReward(0.2f * Mathf.Max(0, clearQuality));
        }
        else if (col.gameObject.CompareTag("Wall") || col.gameObject.CompareTag("Gate"))
        {
            AddReward(-0.1f);
        }
        else if (col.gameObject.CompareTag("Agent"))
        {
            AddReward(-0.1f);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(idealDefendPosition, 0.75f);

        if (rb != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + rb.linearVelocity * 2);
        }
    }
}
