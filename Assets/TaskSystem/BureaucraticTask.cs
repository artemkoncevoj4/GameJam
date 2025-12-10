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

            Debug.LogWarning($"Документ:\n{document.InkColor}\n{document.StampPos}\n{document.PaperType}\n{document.IsStamped}\n{document.IsSigned}\nТребования:\n{Requirements.requiredInkColor}\n{Requirements.requiredStampPos}\n{Requirements.requiredPaperType}\n{Requirements.isStamped}\n{Requirements.isSigned}");

            bool isValid =
                document.PaperType == Requirements.requiredPaperType &&
                (document.IsSigned ? (document.IsSigned && document.InkColor == Requirements.requiredInkColor) : !document.IsSigned) && 
                (Requirements.isStamped ? (document.IsStamped && document.StampType == Requirements.requiredStampType && document.StampPos == Requirements.requiredStampPos) : !document.IsStamped);

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
            return $"Взять {req.requiredPaperType} и {(req.isSigned ? $"подписать {req.requiredInkColor} чернилами." : "Без подписи")}" +
                   $"{(req.isStamped ? $"Штамп: {req.requiredStampType}, позиция: {req.requiredStampPos}" : "Без штампа.")} ";
        }
    }
}