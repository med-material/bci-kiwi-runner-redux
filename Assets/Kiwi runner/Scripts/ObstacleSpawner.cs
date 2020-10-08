using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstacles;
    int randomNumber;
    Vector2 spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        spawnPoint = new Vector2(15 + GameController.prepPhase * 4
            + GameController.inputWindow * 2, -1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnRandomObstacle()
    {
        randomNumber = Random.Range(0, obstacles.Length);
        Instantiate(obstacles[randomNumber], spawnPoint, Quaternion.identity);
    }
}
