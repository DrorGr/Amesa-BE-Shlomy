

namespace AMESA_be.common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToLocalizeAttribute : Attribute
{
    private string propName;

    public ToLocalizeAttribute(string name)
    {
        propName = name;
    }

    public string Name
    {
        get { return propName; }
        set { propName = value; }
    }
}
