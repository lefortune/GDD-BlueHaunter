using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player; 
    public float xMin;
    public float xMax;
    public float yMin = -2.4f;
    public float yMax = -2.4f;

    void Start() {

    }
    void Update()
    {
        if (GameManager.Instance.isGamePaused) return;
        float x = Mathf.Clamp(player.transform.position.x, xMin, xMax);
//        float y = Mathf.Clamp(player.transform.position.y, yMin, yMax);
        gameObject.transform.position = new Vector3(x, gameObject.transform.position.y, gameObject.transform.position.z);
    }

    public void CameraShake(float duration, float magnitude) {
        StartCoroutine(Shake(duration, magnitude));
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = gameObject.transform.position.x + (Random.Range(-1f, 1f) * magnitude);
            float y = gameObject.transform.position.y + (Random.Range(-1f, 1f) * magnitude);

            transform.localPosition = new Vector3(x, y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}

