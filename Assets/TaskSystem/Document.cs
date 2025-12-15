namespace TaskSystem
{
    // Муштаков А.Ю.

    /// <summary>
    /// Представляет документ, который создается и обрабатывается в процессе выполнения задания.
    /// Содержит все атрибуты документа, необходимые для проверки соответствия требованиям задания.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Цвет чернил, использованных в документе.
        /// </summary>
        public InkColor InkColor { get; set; }
        
        /// <summary>
        /// Позиция штампа на документе.
        /// </summary>
        public StampPosition StampPos { get; set; }
        
        /// <summary>
        /// Тип бумаги, использованной для документа.
        /// </summary>
        public PaperType PaperType { get; set; }
        
        /// <summary>
        /// Тип штампа, поставленного на документе.
        /// </summary>
        public StampType StampType { get; set; }
        
        /// <summary>
        /// Флаг, указывающий, подписан ли документ.
        /// </summary>
        public bool IsSigned { get; set; }
        
        /// <summary>
        /// Флаг, указывающий, поставлен ли штамп на документе.
        /// </summary>
        public bool IsStamped { get; set; }
    }
}