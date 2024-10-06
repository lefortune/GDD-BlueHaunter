using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player; 
    public float xMin = 0f; // Fixed vertical position
    public float xMax = 1000f;
    public float yMin = -2.4f;
    public float yMax = -2.4f;

    void Start() {

    }
    void Update()
    {
        if (GameManager.Instance.isGamePaused) return;
        float x = Mathf.Clamp(player.transform.position.x, xMin, xMax);
        float y = Mathf.Clamp(player.transform.position.y, yMin, yMax);
        gameObject.transform.position = new Vector3(x, y, gameObject.transform.position.z);
    }
}

