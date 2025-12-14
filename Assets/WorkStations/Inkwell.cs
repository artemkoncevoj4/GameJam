using UnityEngine;
using TaskSystem;

namespace InteractiveObjects
{
    public class SimpleInkwell : Workstation
    {
        [Header("Настройки чернильницы")]
        [SerializeField] private float _colorChangeTime = 5f; // Время смены цвета
        [SerializeField] private AudioClip signSound;
        [SerializeField] private SpriteRenderer inkVisual; 
        [SerializeField] private InkColor[] _availableInkColors = { 
            InkColor.Черные, 
            InkColor.Красные, 
            InkColor.Зеленые, 
            InkColor.Фиолетовые 
        };
        
        private int _currentColorIndex = 0;
        private float _timeSinceLastChange = 0f;
        
        void Start()
        { 
            UpdateInkVisual(); // Устанавливаем начальный цвет
        }
        
        void Update()
        {
            // Простая смена цвета по таймеру
            _timeSinceLastChange += Time.deltaTime;
            
            if (_timeSinceLastChange >= _colorChangeTime)
            {
                _timeSinceLastChange = 0f;
                _currentColorIndex = (_currentColorIndex + 1) % _availableInkColors.Length;
                
                // Меняем цвет визуально
                UpdateInkVisual();
                
                Debug.Log($"Цвет чернил: {_availableInkColors[_currentColorIndex]}");
            }
        }
        
        public string GetInteractionHint()
        {
            return $"Подписать ({_availableInkColors[_currentColorIndex]})";
        }

        public bool CanInteract()
        {
            Document doc = TaskManager.Instance?.GetCurrentDocument();
            return doc != null && !doc.IsSigned;
        }
        
        public override void UseStation()
        {
            Document doc = TaskManager.Instance?.GetCurrentDocument();
            
            if (doc == null || doc.IsSigned)
                return;
            
            // Подписываем
            doc.InkColor = _availableInkColors[_currentColorIndex];
            doc.IsSigned = true;
            
            // Звук
            //? pen done?
            AudioManager.Instance?.PlaySoundByName("pen");
            
            Debug.Log($"Подписано {_availableInkColors[_currentColorIndex]} чернилами");
        }
        
        private void UpdateInkVisual()
        {
            Color inkColor = GetInkColor(_availableInkColors[_currentColorIndex]);
            inkVisual.color = inkColor;
    
        }
        
        private Color GetInkColor(InkColor inkType)
        {
            switch (inkType)
            {
                case InkColor.Черные: return Color.black;
                case InkColor.Красные: return Color.red;
                case InkColor.Зеленые: return Color.green;
                case InkColor.Фиолетовые: return new Color(0.5f, 0f, 0.5f);
                default: return Color.black;
            }
        }
    }
}