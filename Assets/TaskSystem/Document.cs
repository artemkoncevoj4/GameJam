namespace TaskSystem
{
    public class Document
    {
        public InkColor InkColor { get; set; }
        public SignaturePosition SignaturePos { get; set; }
        public PaperType PaperType { get; set; }
        public StampType StampType { get; set; }
        public bool IsSigned { get; set; }
        public bool IsStamped { get; set; }
    }
}