using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstacles;

    int randomNumber;
    Vector2 spawnPoint;

    void Start()
    {
        spawnPoint = new Vector2(20, 10);
    }

    public void SpawnRandomObstacle()
    {
        randomNumber = Random.Range(0, obstacles.Length);
        Instantiate(obstacles[randomNumber], spawnPoint, Quaternion.identity);
    }
}
