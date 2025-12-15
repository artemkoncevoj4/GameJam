using UnityEngine;
using TaskSystem;

namespace InteractiveObjects
{
    // Муштаков А.Ю.

    /// <summary>
    /// Класс чернильницы, управляющий подписыванием документов с периодической сменой цвета чернил.
    /// Предоставляет интерактивную станцию для подписания документов с помощью различных цветов чернил.
    /// </summary>
    public class SimpleInkwell : Workstation
    {
        [Header("Настройки чернильницы")]
        [SerializeField] private float _colorChangeTime = 1.5f; // Время смены цвета чернил в секундах
        [SerializeField] private SpriteRenderer inkVisual; // Визуальный компонент отображения чернил
        [SerializeField] private InkColor[] _availableInkColors = { 
            InkColor.Черные, 
            InkColor.Красные, 
            InkColor.Зеленые, 
            InkColor.Фиолетовые 
        }; // Доступные цвета чернил для использования
        
        private int _currentColorIndex = 0; // Текущий индекс цвета в массиве доступных цветов
        private float _timeSinceLastChange = 0f; // Время, прошедшее с последней смены цвета
        
        /// <summary>
        /// Инициализирует чернильницу, устанавливая начальный визуальный цвет чернил.
        /// </summary>
        void Start()
        { 
            UpdateInkVisual(); 
        }
        
        /// <summary>
        /// Обновляет состояние чернильницы каждый кадр, автоматически меняя цвет чернил по таймеру.
        /// Смена цвета происходит каждые _colorChangeTime секунд циклически по массиву доступных цветов.
        /// </summary>
        void Update()
        {
            // Простая смена цвета по таймеру
            _timeSinceLastChange += Time.deltaTime;
            
            if (_timeSinceLastChange >= _colorChangeTime)
            {
                _timeSinceLastChange = 0f;
                _currentColorIndex = (_currentColorIndex + 1) % _availableInkColors.Length;
                
                UpdateInkVisual();
                
                Debug.Log($"Цвет чернил: {_availableInkColors[_currentColorIndex]}");
            }
        }
        
        /// <summary>
        /// Возвращает текстовую подсказку для взаимодействия с чернильницей.
        /// Подсказка включает текущий активный цвет чернил.
        /// </summary>
        /// <returns>Строка с текстом подсказки и текущим цветом чернил.</returns>
        public string GetInteractionHint()
        {
            return $"Подписать ({_availableInkColors[_currentColorIndex]})"; //TODO Должна быть как диалоговое окно снизу при приближении к станции (не реализовано)
        }

        /// <summary>
        /// Выполняет взаимодействие с чернильницей: подписывает текущий документ текущим цветом чернил.
        /// Обновляет свойства документа и воспроизводит звуковой эффект при успешном подписании.
        /// </summary>
        public override void UseStation()
        {
            Document doc = TaskManager.Instance?.GetCurrentDocument();
            
            if (doc == null || doc.IsSigned)
                return;
            

            doc.InkColor = _availableInkColors[_currentColorIndex];
            doc.IsSigned = true;
            

            AudioManager.Instance?.PlaySoundByName("pen");
            
            Debug.Log($"Подписано {_availableInkColors[_currentColorIndex]} чернилами");
        }
        
        /// <summary>
        /// Обновляет цвет спрайта в соответствии с текущим цветом чернил.
        /// </summary>
        private void UpdateInkVisual()
        {
            Color inkColor = GetInkColor(_availableInkColors[_currentColorIndex]);
            inkVisual.color = inkColor;
        }
        
        /// <summary>
        /// Преобразует перечислимый тип InkColor в соответствующий цвет Unity.
        /// </summary>
        /// <param name="inkType">Тип чернил из перечисления InkColor.</param>
        /// <returns>Соответствующий цвет Unity для визуального отображения.</returns>
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