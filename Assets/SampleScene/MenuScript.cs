using UnityEngine;
using System.Collections;

namespace SampleScene
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseMenuAnimator : MonoBehaviour
    {
        [Header("Настройки анимации")]
        [SerializeField] private float _animationSpeed = 10f;
        [SerializeField] private float _startScale = 0.9f; // Немного уменьшено при старте
        
        private CanvasGroup _canvasGroup;
        private Coroutine _currentAnimation;
        
        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // На старте сразу скрываем меню, чтобы не мелькало
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            // Сбрасываем масштаб
            transform.localScale = Vector3.one * _startScale;
        }
        
        public void ShowMenu()
        {
            // Прерываем предыдущую анимацию, если она была
            if (_currentAnimation != null) StopCoroutine(_currentAnimation);
            
            // Включаем взаимодействие
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            // Запускаем анимацию появления
            _currentAnimation = StartCoroutine(AnimateFade(1f, 1f));
        }
        
        public void HideMenu()
        {
            if (_currentAnimation != null) StopCoroutine(_currentAnimation);
            
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            // Запускаем анимацию исчезновения (альфа в 0, масштаб обратно в 0.9)
            _currentAnimation = StartCoroutine(AnimateFade(0f, _startScale));
        }
        
        // Универсальная корутина для плавного изменения
        private IEnumerator AnimateFade(float targetAlpha, float targetScale)
        {
            // Пока текущие значения не станут почти равны целевым
            while (Mathf.Abs(_canvasGroup.alpha - targetAlpha) > 0.01f)
            {
                // Используем unscaledDeltaTime, так как игра на паузе!
                float step = Time.unscaledDeltaTime * _animationSpeed;
                
                // Плавное изменение прозрачности
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, step);
                
                // Плавное изменение масштаба (эффект Pop-up)
                float newScale = Mathf.Lerp(transform.localScale.x, targetScale, step);
                transform.localScale = Vector3.one * newScale;
                
                yield return null;
            }
            
            // Жестко ставим финальные значения в конце
            _canvasGroup.alpha = targetAlpha;
            transform.localScale = Vector3.one * targetScale;
            
            // Если мы скрывали меню, сообщаем, что закончили
            if (targetAlpha == 0)
            {
                OnHideAnimationComplete();
            }
        }
        
        public void OnHideAnimationComplete()
        {
            _canvasGroup.alpha = 0;
            // Можно добавить выключение объекта, если нужно, 
            // но PauseMenu.cs сам это сделает через 0.3 сек.
        }
    }
}