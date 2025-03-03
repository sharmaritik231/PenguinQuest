using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PenguinArea : MonoBehaviour
{
    public PenguinAgent penguinAgent;
    public GameObject audiencePerson; // Single audience person receiving fish
    public TextMeshPro cumulativeRewardText;
    public Fish fishPrefab;

    public List<GameObject> fishList = new List<GameObject>(); // Initialize the list
    public int requiredFishCount = 3;
    public int collectedFishCount = 0;

    // Property to check remaining fish
    public int FishRemaining => fishList.Count;
    public int CollectedFishCount => collectedFishCount;
    public int RequiredFishCount => requiredFishCount;

    public void ResetArea()
    {
        RemoveAllFish();
        PlacePenguin();
        int fishCount = Random.Range(3, 6); // Vary the fish count slightly for each episode
        SpawnFish(fishCount, 0.5f); 
        collectedFishCount = 0;

        // Set audience position close to the penguin with slight randomization
        Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        audiencePerson.transform.position = penguinAgent.transform.position + Vector3.back * 2f + randomOffset;
        audiencePerson.transform.LookAt(penguinAgent.transform.position);
    }

    public void RemoveSpecificFish(GameObject fishObject)
    {
        fishList.Remove(fishObject);
        Destroy(fishObject);
        collectedFishCount++;
        
        // Optional check: end episode if the required number of fish are collected
        if (collectedFishCount >= requiredFishCount && FishRemaining == 0)
        {
            penguinAgent.EndEpisode();
        }
    }

    public static Vector3 ChooseRandomPosition(Vector3 center, float minAngle, float maxAngle, float minRadius, float maxRadius)
    {
        float radius = Random.Range(minRadius, maxRadius);
        float angle = Random.Range(minAngle, maxAngle);
        return center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
    }

    private void RemoveAllFish()
    {
        foreach (var fish in fishList)
        {
            if (fish != null)
            {
                Destroy(fish);
            }
        }
        fishList.Clear(); // Clear the list instead of reinitializing to ensure it’s always available
    }

    private void PlacePenguin()
    {
        Rigidbody rigidbody = penguinAgent.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        penguinAgent.transform.position = ChooseRandomPosition(transform.position, 0f, 360f, 0f, 9f) + Vector3.up * 0.5f;
        penguinAgent.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    private void SpawnFish(int count, float fishSpeed)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject fishObject = Instantiate(fishPrefab.gameObject);
            fishObject.transform.position = ChooseRandomPosition(transform.position, 100f, 260f, 2f, 13f) + Vector3.up * 0.5f;
            fishObject.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            fishObject.transform.SetParent(transform);
            fishList.Add(fishObject);
            fishObject.GetComponent<Fish>().fishSpeed = fishSpeed;
        }
    }

    private void Start()
    {
        ResetArea();
    }

    private void Update()
    {
        // Display the cumulative reward of the penguin agent
        cumulativeRewardText.text = penguinAgent.GetCumulativeReward().ToString("0.00");
    }
}
