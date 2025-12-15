using UnityEngine;
using UnityEngine.UI;
using TMPro;

// * Жаркова Т.В.

/// <summary>
/// Отображает индикатор стресс-уровня в виде полосы заполнения и текста.
/// Также управляет анимациями в зависимости от уровня стресса.
/// </summary>
namespace UI {
    public class SimpleStressIndicator : MonoBehaviour
    {

        [SerializeField] private Image _fillBar;
        [SerializeField] private TextMeshProUGUI _percentageText;
        [SerializeField] private Animator _animator;

        private float _currentStress = 0f;
        private static readonly int StressHigh = Animator.StringToHash("StressHigh");
        private static readonly int StressCritical = Animator.StringToHash("StressCritical");

        void Start()
        {
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnStressLevelChanged += OnStressChanged;
            }
        }

        void OnDestroy()
        {
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnStressLevelChanged -= OnStressChanged;
            }
        }

        private void OnStressChanged(float stressLevel)
        {
            _currentStress = stressLevel;

            if (_fillBar != null)
            {
                _fillBar.fillAmount = stressLevel / 100;
                Color stressColor = GetStressColor(stressLevel / 100);
                _fillBar.color = stressColor;
            }

            if (_percentageText != null)
            {
                _percentageText.text = $"СТРЕСС: {Mathf.RoundToInt(stressLevel)}%";
                _percentageText.color = GetStressColor(stressLevel / 100);
            }
            UpdateAnimations(stressLevel);
        }

        private Color GetStressColor(float stress)
        {
            if (stress < 0.3f) return Color.green;
            if (stress < 0.7f) return Color.yellow;
            return Color.red;
        }

        private void UpdateAnimations(float stress)
        {
            if (_animator == null) return;

            _animator.SetBool(StressHigh, stress > 0.5f);
            _animator.SetBool(StressCritical, stress > 0.8f);

            // Устанавливаем уровень стресс-уровня в аниматор для плавной анимации
            _animator.SetFloat("StressLevel", stress);
        }
    }
}