namespace CommonGateLib.Models.Common
{
    public class ServerInstanceModel : InstanceModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public ServerSettings ServerConnectionSettings { get; set; }
    }
}
