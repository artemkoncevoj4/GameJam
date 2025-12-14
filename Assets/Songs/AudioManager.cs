using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // Статическая переменная для доступа к AudioManager из любого места (Singleton Pattern)
    public static AudioManager Instance;

    // Ссылки на Audio Source
    [Header("Источники звука")]
    [Tooltip("Источник для коротких звуковых эффектов (PlayOneShot)")]
    public AudioSource sfxSource; // Главный AudioSource для SFX
    
    [Tooltip("Источник для фоновой музыки")]
    public AudioSource musicSource; // Второй AudioSource для музыки (Назначается в Inspector)

    [Header("Настройки Аудио Mixer")]
    [Tooltip("Ссылка на ваш главный Audio Mixer")]
    public AudioMixer masterMixer; 
    [Tooltip("Имя группы микшера для SFX")]
    public string sfxMixerGroupName = "SFX";
    [Tooltip("Имя группы микшера для музыки")]
    public string musicMixerGroupName = "Music";

    // Основные SFX (примеры)
    [Header("1. Основные SFX (по умолчанию)")]
    public AudioClip spawnSound;
    public AudioClip clickSound;
    public AudioClip explosionSound;

    // НОВЫЕ ПОЛЯ ДЛЯ ОСОБЫХ ЗВУКОВ - РАСШИРЕНО ДО 10
    [Header("2. Особые SFX (по триггеру)")]
    [Tooltip("Специальный звук для действия зайца")]
    public AudioClip bunnySpecialSound;
    
    [Tooltip("Звук активации особого события")]
    public AudioClip eventActivationSound;
    
    [Tooltip("Звук проигрыша/критический SFX")]
    public AudioClip criticalFailureSound;
    
    [Tooltip("Звук достижения успеха")]
    public AudioClip achievementUnlockedSound;
    
    [Tooltip("Звук получения бонуса")]
    public AudioClip bonusCollectedSound;
    
    [Tooltip("Звук телепортации/перемещения")]
    public AudioClip teleportSound;
    
    [Tooltip("Звук открытия двери/сундука")]
    public AudioClip doorOpenSound;
    
    [Tooltip("Звук сбора предмета")]
    public AudioClip itemPickupSound;
    
    [Tooltip("Звук подсказки/напоминания")]
    public AudioClip hintSound;
    
    [Tooltip("Звук завершения уровня")]
    public AudioClip levelCompleteSound;

    // Словарь для удобного доступа к звукам по имени
    private Dictionary<string, AudioClip> soundClips;

    // Клипы для эффекта Хаоса
    [Header("3. Звуковые Клипы (Хаос)")]
    public AudioClip glitchSfx;
    public AudioClip horrorSting;
    public AudioClip creepyTensionRise;
    public AudioClip scaryHitsRisers;
    private AudioClip[] chaosClips;

    [Header("4. Клипы для СЛУЧАЙНЫХ СПЕЦ. SFX (Хаос)")]
    [Tooltip("Клип 1 из 2 для случайного воспроизведения")]
    public AudioClip specialRandomSFX1; 
    
    [Tooltip("Клип 2 из 2 для случайного воспроизведения")]
    public AudioClip specialRandomSFX2;
    
    private AudioClip[] specialRandomClips; // Новый массив для случайного выбора

    // Клип для фоновой музыки
    [Header("5. Музыка")]
    public AudioClip backgroundMusicClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            if (sfxSource == null)
            {
                Debug.LogError("<color=red>AudioManager: sfxSource не назначен в инспекторе!</color>");
            }
            if (musicSource == null)
            {
                Debug.LogError("<color=red>AudioManager: musicSource не назначен в инспекторе!</color>");
            }

            // Назначаем группы микшера
            if (masterMixer != null)
            {
                // Назначаем группу для SFX
                if (sfxSource != null)
                {
                    AudioMixerGroup[] sfxGroups = masterMixer.FindMatchingGroups(sfxMixerGroupName);
                    if (sfxGroups.Length > 0)
                    {
                        sfxSource.outputAudioMixerGroup = sfxGroups[0];
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>AudioManager: Mixer group с именем '{sfxMixerGroupName}' не найдена.</color>");
                    }
                }

                // Назначаем группу для Музыки
                if (musicSource != null)
                {
                    AudioMixerGroup[] musicGroups = masterMixer.FindMatchingGroups(musicMixerGroupName);
                    if (musicGroups.Length > 0)
                    {
                        musicSource.outputAudioMixerGroup = musicGroups[0];
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>AudioManager: Mixer group с именем '{musicMixerGroupName}' не найдена.</color>");
                    }
                }
            }
            else
            {
                Debug.LogWarning("<color=orange>AudioManager: masterMixer не назначен. Звуки будут воспроизводиться без микшера.</color>");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        soundClips = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase)
        {
            // Основные SFX
            { "spawn", spawnSound },
            { "click", clickSound },
            { "explosion", explosionSound },

            // Особые SFX
            { "bunny", bunnySpecialSound },
            { "bunnyspecial", bunnySpecialSound },
            { "event", eventActivationSound },
            { "eventactivation", eventActivationSound },
            { "critical", criticalFailureSound },
            { "criticalfailure", criticalFailureSound },
            { "achievement", achievementUnlockedSound },
            { "bonus", bonusCollectedSound },
            { "teleport", teleportSound },
            { "door", doorOpenSound },
            { "item", itemPickupSound },
            { "hint", hintSound },
            { "levelcomplete", levelCompleteSound }
        };
        
        // Инициализация массива случайных клипов
        specialRandomClips = new AudioClip[]
        {
            specialRandomSFX1,
            specialRandomSFX2,
        };
        
        // Инициализация массива клипов Хаоса
        chaosClips = new AudioClip[]
        {
            glitchSfx,
            horrorSting,
            creepyTensionRise,
            scaryHitsRisers
        };
    }
    
    // [НОВОЕ] Start для автоматического запуска фоновой музыки
    void Start()
    {
        PlayBackgroundMusic();
    }
    
    // =========================================================================================
    // УНИВЕРСАЛЬНЫЕ МЕТОДЫ ДЛЯ ВОСПРОИЗВЕДЕНИЯ SFX
    // =========================================================================================

    /// <summary>
    /// Воспроизводит один короткий звуковой клип
    /// </summary>
    private void PlaySpecificSFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не назначен!</color>");
            return;
        }
        
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("<color=orange>AudioManager: Попытка воспроизвести пустой AudioClip.</color>");
        }
    }
    
    /// <summary>
    /// Воспроизводит звук по имени
    /// </summary>
    public void PlaySoundByName(string soundName)
    {
        if (soundClips.TryGetValue(soundName, out AudioClip clip))
        {
            PlaySpecificSFX(clip);
        }
        else
        {
            Debug.LogWarning($"<color=orange>AudioManager: Звук с именем '{soundName}' не найден</color>");
        }
    }

    public void PlayRandomSpecialSFX()
    {
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не найден!</color>");
            return;
        }

        if (specialRandomClips.Length > 0)
        {
            AudioClip clipToPlay = null;
            int attempts = 0;
            
            while (clipToPlay == null && attempts < 100) 
            {
                clipToPlay = specialRandomClips[UnityEngine.Random.Range(0, specialRandomClips.Length)];
                attempts++;
            }

            if (clipToPlay != null)
            {
                 sfxSource.PlayOneShot(clipToPlay);
            }
            else
            {
                 Debug.LogWarning("<color=orange>AudioManager: В массиве specialRandomClips не назначены AudioClip.</color>");
            }
        }
        else
        {
            Debug.LogWarning("<color=orange>AudioManager: Массив specialRandomClips пуст.</color>");
        }
    }
    
    // =========================================================================================
    // ОСТАЛЬНЫЕ МЕТОДЫ
    // =========================================================================================
    
    public void PlayBackgroundMusic()
    {
        if (musicSource == null)
        {
            Debug.LogError("<color=red>AudioManager: Music Source не назначен!</color>");
            return;
        }
        if (backgroundMusicClip == null)
        {
            Debug.LogWarning("<color=orange>AudioManager: Клип фоновой музыки не назначен.</color>");
            return;
        }
        musicSource.clip = backgroundMusicClip;
        musicSource.loop = true; 
        musicSource.Play();
    }

    public void PlayRandomChaosSound()
    {
        if (sfxSource == null)
        {
            Debug.LogError("<color=red>AudioManager: AudioSource для SFX не найден!</color>");
            return;
        }

        if (chaosClips.Length > 0)
        {
            AudioClip clipToPlay = null;
            int attempts = 0;
            while (clipToPlay == null && attempts < 100) 
            {
                clipToPlay = chaosClips[UnityEngine.Random.Range(0, chaosClips.Length)];
                attempts++;
            }

            if (clipToPlay != null)
            {
                 sfxSource.PlayOneShot(clipToPlay);
            }
            else
            {
                 Debug.LogWarning("<color=orange>AudioManager: Не удалось найти валидный клип в массиве chaosClips.</color>");
            }
        }
        else
        {
            Debug.LogWarning("<color=orange>AudioManager: Массив chaosClips пуст.</color>");
        }
    }
}