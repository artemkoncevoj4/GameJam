using UnityEngine;

namespace TaskSystem
{
    [System.Serializable]
    public class DocumentRequirement
    {
        public string description;
        public InkColor requiredInkColor;
        public SignaturePosition requiredSignaturePos;
        public PaperType requiredPaperType;
        public StampType requiredStampType;
        public bool isStamped;
        public bool isSigned;
        public float timePenalty = 15f;
    }

    public enum InkColor { Черные, Красные, Зеленые, Фиолетовые }
    public enum SignaturePosition { Левый_нижний, Правый_нижний, Центр, Левая_сторона }
    public enum PaperType { Бланк_формы_7_Б, Бланк_формы_АА_Я, Пергамент, Карточка }
    public enum StampType { Одобрено, Отклонено, На_рассмотрении, Официальная_печать, Секретная_печать }
}