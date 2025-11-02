using AMESA_be.common.Interfaces;

namespace AMESA_be.common.Config
{
    public class HttpClientConfig : IPropertiesCloneable<HttpClientConfig>
    {
        public int? TimeoutSec { get; set; }
        public virtual void CloneProperties(HttpClientConfig cloneFrom)
        {
            TimeoutSec = cloneFrom.TimeoutSec;
        }
    }
}
