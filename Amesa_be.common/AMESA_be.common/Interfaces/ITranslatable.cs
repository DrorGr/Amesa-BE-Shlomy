using AMESA_be.common.Attributes;

namespace AMESA_be.common.Interfaces
{
    public interface ITranslatable
    {
        [ToLocalize("DisplayNameKey")]
        public string DisplayName { get; set; }
        public string DisplayNameKey { get; set; }

        [ToLocalize("DescriptionKey")]
        public string Description { get; set; }
        public string DescriptionKey { get; set; }
    }
}
