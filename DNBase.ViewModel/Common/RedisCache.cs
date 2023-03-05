namespace DNBase.ViewModel
{
    public class RedisCacheSettings
    {
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; }
        public string AbortOnConnectFail { get; set; }
    }
}