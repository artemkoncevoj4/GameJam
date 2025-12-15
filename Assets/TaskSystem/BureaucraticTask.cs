using UnityEngine;

namespace TaskSystem
{
    // Муштаков А.Ю.

    /// <summary>
    /// Представляет бюрократическое задание, которое должен выполнить игрок.
    /// Содержит требования к документу, таймер и методы для управления состоянием задания.
    /// </summary>
    public class BureaucraticTask
    {
        /// <summary>
        /// Заголовок задания.
        /// </summary>
        public string Title { get; private set; }
        
        /// <summary>
        /// Описание задания с перечислением требований.
        /// </summary>
        public string Description { get; private set; }
        
        /// <summary>
        /// Требования к документу, которые должны быть выполнены.
        /// </summary>
        public DocumentRequirement Requirements { get; private set; }
        
        /// <summary>
        /// Оставшееся время на выполнение задания.
        /// </summary>
        public float TimeRemaining { get; private set; }
        
        /// <summary>
        /// Флаг, указывающий, выполнено ли задание.
        /// </summary>
        public bool IsCompleted { get; private set; }
        
        /// <summary>
        /// Флаг, указывающий, провалено ли задание.
        /// </summary>
        public bool IsFailed { get; private set; }
        
        /// <summary>
        /// Флаг, указывающий, было ли задание искажено кроликом.
        /// </summary>
        public bool IsCorrupted { get; private set; }
        
        /// <summary>
        /// Флаг, указывающий, является ли задание срочным.
        /// </summary>
        public bool IsUrgent { get; private set; }
        
        /// <summary>
        /// Инициализирует новый экземпляр бюрократического задания.
        /// </summary>
        /// <param name="title">Заголовок задания.</param>
        /// <param name="req">Требования к документу.</param>
        /// <param name="timeLimit">Время на выполнение задания в секундах.</param>
        /// <param name="urgent">Флаг срочности задания (по умолчанию false).</param>
        public BureaucraticTask(string title, DocumentRequirement req, float timeLimit, bool urgent = false)
        {
            Title = title;
            Requirements = req;
            Description = GenerateDescription(Requirements);
            TimeRemaining = timeLimit;
            IsUrgent = urgent;
        }

        /// <summary>
        /// Обновляет таймер задания и проверяет истечение времени.
        /// </summary>
        /// <param name="deltaTime">Время, прошедшее с последнего обновления в секундах.</param>
        /// <returns>Возвращает true, если время вышло, иначе false.</returns>
        public bool UpdateTimer(float deltaTime)
        {
            if (IsCompleted || IsFailed) return false;

            TimeRemaining -= deltaTime;
            if (TimeRemaining <= 0)
            {
                IsFailed = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Искажает требования задания (вызывается при вмешательстве кролика).
        /// Изменяет случайные атрибуты требования и обновляет описание.
        /// </summary>
        public void Corrupt()
        {
            if (IsCorrupted || IsCompleted) return;

            IsCorrupted = true;

            int changes = Random.Range(1, 3);
            for (int i = 0; i < changes; i++)
            {
                switch (Random.Range(0, 4))
                {
                    case 0:
                        Requirements.requiredInkColor = (InkColor)Random.Range(0, 4);
                        break;
                    case 1:
                        Requirements.requiredStampPos = (StampPosition)Random.Range(0, 4);
                        break;
                    case 2:
                        Requirements.requiredPaperType = (PaperType)Random.Range(0, 4);
                        break;
                    case 3:
                        Requirements.requiredStampType = (StampType)Random.Range(0, 5);
                        break;
                }
            }

            Description = GenerateDescription(Requirements) + " (Требования изменены Кроликом!)";
        }

        /// <summary>
        /// Проверяет, соответствует ли документ требованиям задания.
        /// </summary>
        /// <param name="document">Документ для проверки.</param>
        /// <returns>Возвращает true, если документ соответствует требованиям, иначе false.</returns>
        public bool Validate(Document document)
        {
            if (IsCompleted || IsFailed) return false;

            Debug.LogWarning($"Документ:\n{document.InkColor}\n{document.StampPos}\n{document.PaperType}\n{document.IsStamped}\n{document.IsSigned}\nТребования:\n{Requirements.requiredInkColor}\n{Requirements.requiredStampPos}\n{Requirements.requiredPaperType}\n{Requirements.isStamped}\n{Requirements.isSigned}");

            bool isValid =
                document.PaperType == Requirements.requiredPaperType &&
                (document.IsSigned ? (document.IsSigned && document.InkColor == Requirements.requiredInkColor) : !document.IsSigned) && 
                (Requirements.isStamped ? (document.IsStamped && document.StampType == Requirements.requiredStampType && document.StampPos == Requirements.requiredStampPos) : !document.IsStamped);

            return isValid;
        }

        /// <summary>
        /// Отмечает задание как успешно выполненное.
        /// </summary>
        public void Complete()
        {
            IsCompleted = true;
            IsFailed = false;
        }

        /// <summary>
        /// Отмечает задание как проваленное.
        /// </summary>
        public void Fail()
        {
            IsFailed = true;
            IsCompleted = false;
        }

        /// <summary>
        /// Генерирует текстовое описание на основе требований к документу.
        /// </summary>
        /// <param name="req">Требования к документу.</param>
        /// <returns>Строковое описание задания.</returns>
        private string GenerateDescription(DocumentRequirement req)
        {   
            return $"Взять {req.requiredPaperType} и {(req.isSigned ? $"подписать {req.requiredInkColor} чернилами." : "Без подписи")}" +
                   $"{(req.isStamped ? $"Штамп: {req.requiredStampType}, позиция: {req.requiredStampPos}" : "Без штампа.")} ";
        }
    }
}