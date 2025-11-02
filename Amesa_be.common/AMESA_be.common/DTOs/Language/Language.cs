namespace AMESA_be.common.DTOs.Language
{
    public class Language
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsRTL { get; set; }
        public bool IsTranslated { get; set; }
        public string Flag { get; set; }
    }
}
