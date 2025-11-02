namespace AMESA_be.common.Interfaces
{
    public interface IPropertiesCloneable<T>
    {
        void CloneProperties(T cloneFrom);
    }
}
