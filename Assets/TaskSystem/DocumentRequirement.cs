using UnityEngine;

namespace TaskSystem
{
    // Муштаков А.Ю.
    
    /// <summary>
    /// Определяет требования к документу для выполнения задания.
    /// Содержит все необходимые атрибуты, которые должен иметь документ.
    /// </summary>
    [System.Serializable]
    public class DocumentRequirement
    {
        /// <summary>
        /// Текстовое описание требований к документу.
        /// </summary>
        public string description;
        
        /// <summary>
        /// Требуемый цвет чернил для документа.
        /// </summary>
        public InkColor requiredInkColor;
        
        /// <summary>
        /// Требуемая позиция для штампа на документе.
        /// </summary>
        public StampPosition requiredStampPos;
        
        /// <summary>
        /// Требуемый тип бумаги для документа.
        /// </summary>
        public PaperType requiredPaperType;
        
        /// <summary>
        /// Требуемый тип штампа для документа.
        /// </summary>
        public StampType requiredStampType;
        
        /// <summary>
        /// Указывает, требуется ли штамп на документе.
        /// </summary>
        public bool isStamped;
        
        /// <summary>
        /// Указывает, требуется ли подпись на документе.
        /// </summary>
        public bool isSigned;
        
        /// <summary>
        /// Штрафное время, добавляемое при провале задания.
        /// </summary>
        public float timePenalty = 15f;
    }

    /// <summary>
    /// Определяет доступные цвета чернил для документов.
    /// </summary>
    public enum InkColor { Черные, Красные, Зеленые, Фиолетовые }
    
    /// <summary>
    /// Определяет возможные позиции для размещения штампа на документе.
    /// </summary>
    public enum StampPosition { Левый_нижний, Правый_нижний, Центр, Левый_верхний }
    
    /// <summary>
    /// Определяет доступные типы бумаги для документов.
    /// </summary>
    public enum PaperType { Бланк_формы_7_Б, Бланк_формы_АА_Я, Пергамент, Карточка }
    
    /// <summary>
    /// Определяет доступные типы штампов для документов.
    /// </summary>
    public enum StampType { Одобрено, Отклонено, На_рассмотрении, Официальная_печать, Секретная_печать }
}