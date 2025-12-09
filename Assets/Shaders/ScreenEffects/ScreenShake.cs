using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private RectTransform shakeTransform;
    [SerializeField] float shiftAmount = 15f;
    [SerializeField] float shakeDuration = 0.3f;

    void Awake()
    {
        if (shakeTransform == null)
        {
            shakeTransform = GetComponent<RectTransform>();
            if (shakeTransform == null)
            {
                Debug.LogError("ScreenShake: RectTransform component is missing!");
                return;
            }
        }
    }

    /// <summary>
    /// Запускает эффект тряски.
    /// </summary>
    public void Start_shaking()
    {
        StopAllCoroutines();
        StartCoroutine(ShakingCoroutine());
    }

    private IEnumerator ShakingCoroutine()
    {
        Vector2 originalPos = shakeTransform.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shiftAmount, shiftAmount);
            float y = Random.Range(-shiftAmount, shiftAmount);
            
            shakeTransform.anchoredPosition = originalPos + new Vector2(x, y);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Возвращаем в исходную позицию
        shakeTransform.anchoredPosition = originalPos;
    }

    void Update()
    {
        // Нажать S для Shake
        if (Input.GetKeyDown(KeyCode.V))
        {
            Start_shaking();
        }
    }
}