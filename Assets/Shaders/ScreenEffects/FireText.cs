using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Shaders.ScreenEffects {
    public class Fire_text : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private string glitchText = "ERROR";
        [SerializeField] private float duration = 10f; // Ограничено 10 секундами

        private Coroutine currentGlitchCoroutine;
        private string targetOriginalText; // Текст, который нужно вернуть после эффекта

        void Start()
        {
            if (textComponent != null)
            {
                targetOriginalText = textComponent.text;
            }
        }

        public void Fire()
        {
            if (currentGlitchCoroutine != null)
            {
                StopCoroutine(currentGlitchCoroutine);
            }
            
            // Сохраняем текущий текст как целевой для возврата
            if (textComponent != null)
            {
                targetOriginalText = textComponent.text;
            }
            
            currentGlitchCoroutine = StartCoroutine(GlitchRoutine());
        }

        // Новый метод для установки целевого текста
        public void SetTargetText(string text)
        {
            targetOriginalText = text;
        }

        private IEnumerator GlitchRoutine()
        {
            if (textComponent == null) yield break;
            
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            // Используем сохраненный текст как целевой для возврата
            string currentTarget = targetOriginalText;
            
            while (Time.time < endTime)
            {
                // Меняем на glitchText
                textComponent.text = glitchText;
                yield return new WaitForSeconds(0.1f);
                
                // Возвращаем к целевому тексту
                textComponent.text = currentTarget;
                yield return new WaitForSeconds(0.1f);
            }
            
            // Гарантированно возвращаем целевой текст
            textComponent.text = currentTarget;
            currentGlitchCoroutine = null;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Fire();
            }
        }
    }
}