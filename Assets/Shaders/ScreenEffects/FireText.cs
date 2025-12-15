using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// ! Сгенерировано ИИ

namespace Shaders.ScreenEffects {
    public class Fire_text : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private string glitchText = "ERROR";
        [SerializeField] private float duration = 10f;
        
        private Coroutine currentGlitchCoroutine;
        private bool isEffectActive = false;

        void Start()
        {
        }
        
        void OnEnable()
        {
        }

        /// <summary>
        /// Запускает эффект глитча, используя переданный текст как исходный.
        /// </summary>
        /// <param name="textToGlitch">Исходный текст, который нужно временно заменить.</param>
        public void Fire(string textToGlitch)
        {
            if (textComponent == null)
            {
                Debug.LogError("<color=red>Fire_text: textComponent не назначен!</color>");
                return;
            }
            
            if (currentGlitchCoroutine != null)
            {
                StopCoroutine(currentGlitchCoroutine);
            }
            
            // Если объект был неактивен, активируем его для работы корутины
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            isEffectActive = true;
            currentGlitchCoroutine = StartCoroutine(GlitchCoroutine(textToGlitch));
        }

        private IEnumerator GlitchCoroutine(string originalText)
        {
            float endTime = Time.time + duration;
            // Скорость мигания (интервал между сменой текста)
            float blinkSpeed = 0.1f;
            
            while (Time.time < endTime)
            {
                textComponent.text = glitchText;
                yield return new WaitForSeconds(blinkSpeed);
                
                textComponent.text = originalText;
                yield return new WaitForSeconds(blinkSpeed);
            }
            
            textComponent.text = originalText;
            currentGlitchCoroutine = null;
            isEffectActive = false;
            
            // Деактивируем объект, если он был активен только для эффекта
            if (gameObject.activeSelf && !ShouldKeepActive())
            {
                 gameObject.SetActive(false);
            }
            
            Debug.Log($"<color=green>Fire_text завершен.</color> <color=white>Восстановлен текст: '{originalText}'</color>");
        }
        
        /// <summary>
        /// Метод-заглушка для определения, нужно ли держать объект активным после завершения эффекта. 
        /// Если компонент Fire_text используется только для glich-эффекта и должен быть выключен после, 
        /// замените 'true' на 'false'.
        /// </summary>
        private bool ShouldKeepActive()
        {
            // Если этот скрипт управляет текстом, который всегда должен быть виден, верните true.
            // Если он используется только для временного эффекта, верните false.
            return false; 
        }

        public void StopEffect()
        {
            if (currentGlitchCoroutine != null)
            {
                StopCoroutine(currentGlitchCoroutine);
                currentGlitchCoroutine = null;
            }
            
            isEffectActive = false;
            
            // Не отключаем объект здесь, так как текст еще может быть глитчевым. 
            // Восстановление текста должно произойти в вызывающем скрипте.
        }
        public bool IsEffectActive => isEffectActive;
    }
}