using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSliderControl : MonoBehaviour
{
    // [Назначается в Inspector] Ссылка на Audio Mixer
    public AudioMixer mixer;
    
    // [Назначается в Inspector] Имя открытого параметра громкости (например, "MusicVolume" или "SFXVolume")
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

        // 1. Получаем текущее значение из Mixer'а
        float value;
        if (mixer.GetFloat(exposedParameterName, out value))
        {
            // Unity Mixer использует логарифмическую шкалу (от -80 до 0), 
            // поэтому нам нужно преобразовать это для ползунка (от 0.0001 до 1)
            volumeSlider.value = Mathf.Pow(10, value / 20);
        }
        else
        {
            Debug.LogError($"Параметр '{exposedParameterName}' не найден в Mixer'е!");
        }

        // 2. Подписываемся на изменение значения ползунка
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float sliderValue)
    {
        // Преобразуем значение ползунка (0 до 1) обратно в логарифмическую шкалу (-80 до 0)
        // Чтобы избежать Log(0), используем Mathf.Log10(sliderValue)
        float volume = Mathf.Log10(sliderValue) * 20;

        // Устанавливаем громкость в Mixer'е
        mixer.SetFloat(exposedParameterName, volume);
    }
}