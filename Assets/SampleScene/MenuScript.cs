using UnityEngine;
using System.Collections;

namespace SampleScene
{
    // Муштаков А.Ю.

    //! Фактически сгенерировано ИИ с небольшими изменениями

    /// <summary>
    /// Управляет плавной анимацией появления и скрытия меню паузы.
    /// Использует CanvasGroup для контроля прозрачности и взаимодействия, а также масштабирование для эффекта "pop-up".
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseMenuAnimator : MonoBehaviour
    {
        [Header("Настройки анимации")]
        [SerializeField] private float _animationSpeed = 10f; // Скорость плавного перехода анимации
        [SerializeField] private float _startScale = 0.9f; // Начальный масштаб меню (немного уменьшено для эффекта появления)
        
        private CanvasGroup _canvasGroup; // Ссылка на компонент CanvasGroup для управления прозрачностью и взаимодействием
        private Coroutine _currentAnimation; // Ссылка на текущую выполняемую корутину анимации
        
        /// <summary>
        /// Инициализирует компонент CanvasGroup и устанавливает начальное скрытое состояние меню.
        /// </summary>
        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            transform.localScale = Vector3.one * _startScale;
        }
        
        /// <summary>
        /// Запускает анимацию плавного появления меню паузы.
        /// Включает возможность взаимодействия с элементами меню.
        /// </summary>
        public void ShowMenu()
        {
            if (_currentAnimation != null) StopCoroutine(_currentAnimation);

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _currentAnimation = StartCoroutine(AnimateFade(1f, 1f));
        }
        
        /// <summary>
        /// Запускает анимацию плавного скрытия меню паузы.
        /// Отключает возможность взаимодействия с элементами меню.
        /// </summary>
        public void HideMenu()
        {
            if (_currentAnimation != null) StopCoroutine(_currentAnimation);
            
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            _currentAnimation = StartCoroutine(AnimateFade(0f, _startScale));
        }
        
        /// <summary>
        /// Корутина для плавного изменения прозрачности и масштаба меню.
        /// Использует Time.unscaledDeltaTime для работы во время паузы игры.
        /// </summary>
        /// <param name="targetAlpha">Целевое значение непрозрачности (0-1).</param>
        /// <param name="targetScale">Целевое значение масштаба.</param>
        /// <returns>IEnumerator для управления корутиной.</returns>
        private IEnumerator AnimateFade(float targetAlpha, float targetScale)
        {
            while (Mathf.Abs(_canvasGroup.alpha - targetAlpha) > 0.01f)
            {
                // Используем unscaledDeltaTime, так как анимация должна работать даже когда игра на паузе (timeScale = 0)
                float step = Time.unscaledDeltaTime * _animationSpeed;
                
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, step);
                
                float newScale = Mathf.Lerp(transform.localScale.x, targetScale, step);
                transform.localScale = Vector3.one * newScale;
                
                yield return null;
            }
            
            _canvasGroup.alpha = targetAlpha;
            transform.localScale = Vector3.one * targetScale;
            
            if (targetAlpha == 0)
            {
                OnHideAnimationComplete();
            }
        }
        
        /// <summary>
        /// Вызывается по завершении анимации скрытия меню.
        /// Гарантирует полную скрытость меню и может использоваться для дополнительных действий.
        /// </summary>
        public void OnHideAnimationComplete()
        {
            _canvasGroup.alpha = 0;
        }
    }
}