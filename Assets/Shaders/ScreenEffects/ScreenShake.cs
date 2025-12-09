using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class Screen_Shake : MonoBehaviour
{
    [SerializeField] private RectTransform shake_shape;
    [SerializeField] float shift = 15f;
    [SerializeField] float duration = 0.3f;

    public void Start_shaking()
    {
        StartCoroutine(shaking());
    }

    private System.Collections.IEnumerator shaking()
    {
        Vector2 originalPos = shake_shape.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-shift, shift);
            float y = Random.Range(-shift, shift);
            shake_shape.anchoredPosition = originalPos + new Vector2(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        shake_shape.anchoredPosition = originalPos;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Start_shaking();
        }

    }
}
