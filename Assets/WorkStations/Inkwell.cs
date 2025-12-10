using UnityEngine;
using TaskSystem;

namespace InteractiveObjects
{
    public class SimpleInkwell : Workstation
    {
        [Header("Настройки чернильницы")]
        [SerializeField] private float _colorChangeTime = 5f; // Время смены цвета
        [SerializeField] private AudioClip _signSound;
        [SerializeField] private MeshRenderer _inkVisual; // Визуальное отображение чернил
        [SerializeField] private Material[] _inkMaterials; // Материалы для каждого цвета
        [SerializeField] private InkColor[] _availableInkColors = { 
            InkColor.Черные, 
            InkColor.Красные, 
            InkColor.Зеленые, 
            InkColor.Фиолетовые 
        };
        
        private int _currentColorIndex = 0;
        private float _timeSinceLastChange = 0f;
        private Material _currentMaterial; // Текущий материал для изменения
        
        void Start()
        {
            // Инициализация при старте
            if (_inkVisual != null)
            {
                // Создаем копию материала, чтобы не менять оригинал
                _currentMaterial = new Material(_inkVisual.material);
                _inkVisual.material = _currentMaterial;
            }
            
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
            AudioManager.Instance?.PlaySpecialSoundByIndex(2);
            
            Debug.Log($"Подписано {_availableInkColors[_currentColorIndex]} чернилами");
        }
        
        public override void ResetTable()
        {
            // Не нужно
        }
        
        private void UpdateInkVisual()
        {
            if (_inkVisual == null) return;
            
            // Вариант 1: Меняем цвет существующего материала
            if (_currentMaterial != null)
            {
                Color inkColor = GetUnityColor(_availableInkColors[_currentColorIndex]);
                _currentMaterial.color = inkColor;
            }
            
            // Вариант 2: Используем массив материалов (если он задан)
            if (_inkMaterials != null && _inkMaterials.Length > _currentColorIndex)
            {
                _inkVisual.material = _inkMaterials[_currentColorIndex];
            }
        }
        
        private Color GetUnityColor(InkColor inkType)
        {
            switch (inkType)
            {
                case InkColor.Черные: return Color.black;
                case InkColor.Красные: return Color.red;
                case InkColor.Зеленые: return Color.green;
                case InkColor.Фиолетовые: return new Color(0.5f, 0f, 0.5f); // Фиолетовый
                default: return Color.black;
            }
        }
    }
}