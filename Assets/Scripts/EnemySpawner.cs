using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float initialSpawnInterval;
    public float currentSpawnInterval;
    public float leftEdgeX; // Left edge x-coordinate
    public float rightEdgeX; // Right edge x-coordinate
    public float spawnY = -5f; // y-coordinate for spawning

    private void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(10f);
        while (!GameManager.Instance.isGamePaused)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(currentSpawnInterval);
            currentSpawnInterval -= 0.2f;
            if (currentSpawnInterval < 0) {
                currentSpawnInterval = 0;
            }
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
