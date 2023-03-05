using System;
using System.Collections.Generic;

namespace DNBase.ViewModel
{
    public static class ListClientsModel
    {
        private static Dictionary<string, List<ClientModel>> _clients;
        private static readonly object _lock = new object();

        public static Dictionary<string, List<ClientModel>> Instance()
        {
            lock (_lock)
            {
                if (_clients == null) _clients = new Dictionary<string, List<ClientModel>>();
            }
            return _clients;
        }
    }
    public class ClientModel
    {
        public string ConnectionId { get; set; }
        public DateTime ConnectedTime { get; set; } = DateTime.Now;
    }
}