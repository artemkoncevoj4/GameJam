using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

// * Буйначева В.А.

public class VolumeSliderControl : MonoBehaviour
{
    public AudioMixer mixer;
    public string exposedParameterName;
    private Slider volumeSlider;

    void Awake()
    {
        volumeSlider = GetComponent<Slider>();

        if (mixer == null)
        {
            Debug.LogError("AudioMixer не назначен в VolumeSliderControl!");
            return;
        }

        float value;
        if (mixer.GetFloat(exposedParameterName, out value))
        {
            // Преобразуем значение из AudioMixer (дБ) в значение от 0 до 1
            volumeSlider.value = Mathf.Pow(10, value / 20);
        }
        else
        {
            Debug.LogError($"Параметр '{exposedParameterName}' не найден в Mixer'е!");
        }

        // Подписываемся на изменение значения ползунка
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float sliderValue)
    {
        float volume;
        
        // 1. Нормализация: Масштабируем значение от 0 до 0.5 к диапазону от 0 до 1
        float normalizedValue = sliderValue * 2f; 
        
        // 2. Защита от нуля: Используем минимальное значение, чтобы избежать Log(0)
        if (normalizedValue <= 0.0001f)
        {
            volume = -80f; // Выключено
        }
        else
        {
            // 3. Логарифмическое преобразование (из 0..1 в -80..0 дБ)
            volume = Mathf.Log10(normalizedValue) * 20;
        }

        mixer.SetFloat(exposedParameterName, volume);
    }
}