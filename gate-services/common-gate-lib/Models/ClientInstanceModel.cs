
namespace CommonGateLib.Models
{
    public class ClientInstanceModel : InstanceModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public ClientSettings ClientConnectionSettings { get; set; }
    }
}
