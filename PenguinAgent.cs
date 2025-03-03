using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PenguinAgent : Agent
{
    public float moveSpeed = 5f;
    public float turnSpeed = 180f;
    public GameObject heartPrefab;
    public GameObject regurgitatedFishPrefab;

    private PenguinArea penguinArea;
    private Rigidbody penguinRigidbody;
    private GameObject audiencePerson;
    private int fishCollected; // Track the number of fish collected
    private float previousDistanceToFish;

    public override void Initialize()
    {
        base.Initialize();
        penguinArea = GetComponentInParent<PenguinArea>();
        audiencePerson = penguinArea.audiencePerson;
        penguinRigidbody = GetComponent<Rigidbody>();
        MaxStep = 1000; // Set a maximum step limit to prevent excessive accumulation
        fishCollected = 0; // Initialize fish counter
    }

    public override void OnEpisodeBegin()
    {
        fishCollected = 0; // Reset fish counter at the beginning of each episode
        penguinArea.ResetArea();
        previousDistanceToFish = Vector3.Distance(transform.position, GetNearestFishPosition());
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int forwardAction = actionBuffers.DiscreteActions[0];
        int turnAction = actionBuffers.DiscreteActions[1];
        
        // Move forward if action is 1
        if (forwardAction == 1)
        {
            penguinRigidbody.MovePosition(transform.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
        }

        // Turn left or right based on action
        float turnAmount = 0f;
        if (turnAction == 1) turnAmount = -1f;
        else if (turnAction == 2) turnAmount = 1f;
        transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        // Reward for reducing distance to nearest fish
        float currentDistanceToFish = Vector3.Distance(transform.position, GetNearestFishPosition());
        if (currentDistanceToFish < previousDistanceToFish)
        {
            AddReward(0.02f); // Reward for moving closer to fish
        }
        else
        {
            AddReward(-0.01f); // Penalty for moving away from the fish
        }
        previousDistanceToFish = currentDistanceToFish;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        int turnAction = 0;
        if (Input.GetKey(KeyCode.W)) forwardAction = 1;
        if (Input.GetKey(KeyCode.A)) turnAction = 1;
        else if (Input.GetKey(KeyCode.D)) turnAction = 2;

        actionsOut.DiscreteActions.Array[0] = forwardAction;
        actionsOut.DiscreteActions.Array[1] = turnAction;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(fishCollected); // Observing number of fish collected
        sensor.AddObservation(Vector3.Distance(audiencePerson.transform.position, transform.position));
        sensor.AddObservation((audiencePerson.transform.position - transform.position).normalized);
        sensor.AddObservation((GetNearestFishPosition() - transform.position).normalized);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("fish"))
        {
            EatFish(collision.gameObject);
        }
        else if (collision.transform.CompareTag("audience"))
        {
            RegurgitateFish();
        }
    }

    private void EatFish(GameObject fishObject)
    {
        penguinArea.RemoveSpecificFish(fishObject);
        fishCollected++; // Increment fish counter
        AddReward(0.5f); // Reward for collecting fish
    }

    private void RegurgitateFish()
    {
        if (fishCollected > 0) // Only regurgitate if fish have been collected
        {
            fishCollected--; // Decrement fish counter

            GameObject regurgitatedFish = Instantiate(regurgitatedFishPrefab);
            regurgitatedFish.transform.parent = transform.parent;
            regurgitatedFish.transform.position = audiencePerson.transform.position;
            Destroy(regurgitatedFish, 4f);

            GameObject heart = Instantiate(heartPrefab);
            heart.transform.parent = transform.parent;
            heart.transform.position = audiencePerson.transform.position + Vector3.up;
            Destroy(heart, 4f);

            AddReward(0.75f); // Reward for successfully feeding the audience

            // End episode if all fish are collected and given to the audience
            if (penguinArea.FishRemaining <= 0 && fishCollected == 0)
            {
                AddReward(0.2f); // Completion reward
                EndEpisode();
            }
        }
    }

    private Vector3 GetNearestFishPosition()
    {
        float minDistance = float.MaxValue;
        Vector3 nearestFishPosition = Vector3.zero;
        foreach (var fish in penguinArea.fishList)
        {
            float distance = Vector3.Distance(fish.transform.position, transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestFishPosition = fish.transform.position;
            }
        }
        return nearestFishPosition;
    }
}
