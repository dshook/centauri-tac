using System;

namespace ctac
{

    public class SocketKey
    {
        public Guid clientId { get; private set; }
        public string componentName { get; private set; }

        public SocketKey(Guid clientId, string componentName)
        {
            this.clientId = clientId;
            this.componentName = componentName;
        }

        public override string ToString()
        {
            return string.Format("Client:{0} component:{1}", clientId, componentName);
        }

        public override int GetHashCode()
        {
            return clientId.GetHashCode() | componentName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var key = obj as SocketKey;
            if(key == null) return false;
            return this.clientId == key.clientId && this.componentName == key.componentName;
        }
    }
}
