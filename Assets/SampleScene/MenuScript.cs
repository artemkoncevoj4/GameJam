using UnityEngine;

namespace SampleScene
{
    public class PauseMenuAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private static readonly int ShowTrigger = Animator.StringToHash("Show");
        private static readonly int HideTrigger = Animator.StringToHash("Hide");
        
        void Awake()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();
            
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        public void ShowMenu()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(ShowTrigger);
            }
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }
        
        public void HideMenu()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(HideTrigger);
            }
            
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }
        
        // Вызывается в конце анимации скрытия
        public void OnHideAnimationComplete()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
            }
        }
    }
}