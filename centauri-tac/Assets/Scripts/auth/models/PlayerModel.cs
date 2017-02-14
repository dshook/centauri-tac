using System;

namespace ctac
{
    public class PlayerModel
    {
        public Guid clientId { get; set; }
        public bool isLocal { get; set; }
        public bool isMe { get; set; }

        public string token { get; set; }
        public string email { get; set; }
        public int id { get; set; }
        public string registered { get; set; }
    }
}

