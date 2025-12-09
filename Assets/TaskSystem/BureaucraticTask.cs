using UnityEngine;

namespace TaskSystem
{
    public class BureaucraticTask
    {
        public int TaskID { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DocumentRequirement Requirements { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsFailed { get; private set; }
        public bool IsCorrupted { get; private set; }
        public bool IsUrgent { get; private set; }
        private bool _isSubscribed = false;
        public BureaucraticTask(string title, DocumentRequirement req, float timeLimit, bool urgent = false)
        {
            Title = title;
            Requirements = req;
            Description = GenerateDescription(Requirements);
            TimeRemaining = timeLimit;
            IsUrgent = urgent;
        }

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

        public bool Validate(Document document)
        {
            if (IsCompleted || IsFailed) return false;

            bool isValid =
                document.InkColor == Requirements.requiredInkColor &&
                document.SignaturePos == Requirements.requiredStampPos &&
                document.PaperType == Requirements.requiredPaperType &&
                document.IsSigned == Requirements.isSigned && 
                (Requirements.isStamped ? (document.IsStamped && document.StampType == Requirements.requiredStampType) : !document.IsStamped);

            return isValid;
        }

        public void Complete()
        {
            IsCompleted = true;
            IsFailed = false;
        }

        public void Fail()
        {
            IsFailed = true;
            IsCompleted = false;
        }

        private string GenerateDescription(DocumentRequirement req)
        {   
            return $"Заполнить {req.requiredPaperType} {req.requiredInkColor} чернилами. " +
                   $"Подпись: {req.requiredStampPos}. " +
                   $"{(req.isStamped ? $"Штамп: {req.requiredStampType}." : "Без штампа.")} ";
        }
    }
}