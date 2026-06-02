using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UIElements;

public class AttackerAgent : Agent
{
    public Transform ball;
    public Transform enemyGate;
    public Transform enemyAttacker;
    public Rigidbody rb;

    private float moveSpeed = 0.15f;

    public bool isTeam1;
    public Material materialTeam1;
    public Material materialTeam2;

    float prevBallDistance;
    float prevGateDistance;
    public override void OnEpisodeBegin()
    {
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (isTeam1) GetComponent<MeshRenderer>().material = materialTeam1;
        else GetComponent<MeshRenderer>().material = materialTeam2;

        prevBallDistance = Vector3.Distance(transform.position, ball.position);
        prevGateDistance = Vector3.Distance(transform.position, enemyGate.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.InverseTransformDirection(ball.position - transform.position));
        sensor.AddObservation(transform.InverseTransformDirection(enemyGate.position - transform.position));
        sensor.AddObservation(rb.linearVelocity);
        sensor.AddObservation(transform.InverseTransformDirection(enemyAttacker.position - transform.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotate = actions.ContinuousActions[2];

        transform.Rotate(0, rotate * 250f * Time.deltaTime, 0);

        Vector3 move = transform.forward * moveZ + transform.right * moveX;
        rb.AddForce(move * moveSpeed, ForceMode.VelocityChange);

        AddReward(-0.001f);

        float ball_distance = Vector3.Distance(transform.position, ball.position);

        AddReward((prevBallDistance - ball_distance) * 0.03f);
        prevBallDistance = ball_distance;

        Vector3 toBall = (ball.position - transform.position).normalized;
        float ballAimAlignment = Vector3.Dot(transform.forward, toBall);
        AddReward(ballAimAlignment * 0.001f);

        if (ball_distance < 4f)
        {
            Vector3 toEnemyGate = (enemyGate.position - transform.position).normalized;
            float gateAimAlignment = Vector3.Dot(transform.forward, toEnemyGate);
            
            if (gateAimAlignment > 0.7f && rb.linearVelocity.magnitude > 0.2f) AddReward(gateAimAlignment * 0.01f);
        }

        float distanceToEnemyAttacker = Vector3.Distance(transform.position, enemyAttacker.position);
        if (distanceToEnemyAttacker < 5f)
        {
            Vector3 toEnemyAttacker = (enemyAttacker.position - transform.position).normalized;
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                Vector3 velDir = rb.linearVelocity.normalized;
                float towardEnemyAttacker = Vector3.Dot(velDir, toEnemyAttacker);
                if (towardEnemyAttacker > 0.7f)
                {
                    AddReward(-0.02f * towardEnemyAttacker);
                }
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            Rigidbody ballRb = col.gameObject.GetComponent<Rigidbody>();
            Vector3 toBall = (col.transform.position - transform.position).normalized;
            //Vector3 force = toBall * 4f + rb.linearVelocity * 2f;
            float alignment = Vector3.Dot(transform.forward, toBall);
            Vector3 force = transform.forward * 4f;
            ballRb.AddForce(force, ForceMode.Impulse);

            AddReward(0.8f * Mathf.Max(0f, alignment));//big reward only when facing the ball

            Vector3 toGate = (enemyGate.position - ball.position).normalized;
            float hitQuality = Vector3.Dot(toGate, ballRb.linearVelocity.normalized);
            if (hitQuality > 0) AddReward(0.2f * hitQuality);
            //else AddReward(0.05f * hitQuality);//hitQuality is already negative
        }
        else if (col.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.2f);
        }
        else if (col.gameObject.CompareTag("Gate"))
        {
            AddReward(-0.4f);
        }
        else if (col.gameObject.CompareTag("Agent"))
        {
            AddReward(-0.2f);
        }
    }

    Vector3 Mirror(Vector3 v)
    {
        return isTeam1 ? v : new Vector3(v.x, v.y, -v.z);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, Mirror(ball.position - transform.position) * 0.25f);
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
