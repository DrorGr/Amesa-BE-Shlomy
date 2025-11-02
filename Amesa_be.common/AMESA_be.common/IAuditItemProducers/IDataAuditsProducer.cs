
namespace AMESA_be.common.IAuditItemProducers
{
    public interface IDataAuditsProducer<T>
    {
        List<T> ToAuditProducer();
    }
}
