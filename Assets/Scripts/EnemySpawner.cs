using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval; // Time in seconds between spawns
    public float leftEdgeX = -30f; // Left edge x-coordinate
    public float rightEdgeX = 30f; // Right edge x-coordinate
    public float spawnY = -5f; // Desired y-coordinate for spawning

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (!GameManager.Instance.isGamePaused)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        // Choose a random x-coordinate between the two edges
        float spawnX = Random.value > 0.5f ? leftEdgeX : rightEdgeX; // Randomly choose left or right edge

        // Create the spawn position
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0); // z = 0 for 2D

        // Instantiate the enemy at the spawn position
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
