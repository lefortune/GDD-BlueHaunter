using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SoulTextScript : MonoBehaviour
{
    TextMeshPro textMesh; 
    public float displayTime = 2f; 
    string[] deathMessages = { "Got Damage Soul!", "Got Speed Soul!", "Got Dexterity Soul!"};
    Color[] colors = {Color.red, Color.cyan, Color.green};

    void Start()
    {
        // Detach the text from its parent so it stays after the enemy is destroyed
        transform.SetParent(null); 
        // Start the countdown to destroy the text
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(1f);
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textMesh.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            yield return null;
        }
        Destroy(gameObject);
    }

    public void SetTextProperties(int seed)
    {
        if (textMesh == null) {
            textMesh = GetComponent<TextMeshPro>(); // Get the TextMesh component if not set
        }
        textMesh.text = deathMessages[seed];
        textMesh.color = colors[seed];
    }
}
