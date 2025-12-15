using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// ! Сгенерировано ИИ
public class ScreenFliskers : MonoBehaviour
{
    [SerializeField] private Image flicker_black;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private int num_flickers = 8;

    public void Start_flickers()
    {
        if (flicker_black == null) // ДОБАВЛЕНО
        {
            Debug.LogError("<color=red>ScreenFliskers: Start_flickers НЕ ЗАПУЩЕН! Image 'flicker_black' отсутствует. См. лог Awake().</color>");
            return;
        }
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        StartCoroutine(Flickers());
        Debug.Log($"<color=green>ScreenFliskers: Запуск эффекта мерцания. Длительность: {duration:F2} сек, Кол-во: {num_flickers}.</color>"); // ИЗМЕНЕНО
    }
    void Awake() // ДОБАВЛЕНО
    {
        if (flicker_black == null)
        {
            flicker_black = GetComponent<Image>();
            if (flicker_black == null)
            {
                Debug.LogError("<color=red>ScreenFliskers: Image component 'flicker_black' is missing! Невозможно работать без Image!</color>");
                return;
            }
            Debug.LogWarning("<color=yellow>ScreenFliskers: flicker_black не был назначен вручную. Использование найденного GetComponent<Image>().</color>");
        }
        SetAlpha(0f);
        Debug.Log("<color=green>ScreenFliskers: Компонент инициализирован. Alpha сброшена к 0.</color>");
    }
    private System.Collections.IEnumerator Flickers()
    {
        Debug.Log("<color=green>Flickers запущен</color>");
       float interval = duration / num_flickers;
        for (int i = 0; i < num_flickers; i++)
        {
            SetAlpha(1f);
            yield return new WaitForSecondsRealtime(interval * 0.8f); 
            SetAlpha(0f);
            yield return new WaitForSecondsRealtime(interval * 0.8f);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = flicker_black.color;
        c.a = alpha;
        flicker_black.color = c;
    }
}
