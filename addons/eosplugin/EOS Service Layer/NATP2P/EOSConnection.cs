using System;
using Epic.OnlineServices;

namespace Riptide.Transports.EOS
{
    public class EOSConnection : Connection, IEquatable<EOSConnection>
    {
        public readonly ProductUserId RemoteUserId;
        public readonly ProductUserId LocalUserId;

        internal bool DidReceiveConnect;

        private readonly EOSPeer peer;

        internal EOSConnection(ProductUserId remoteUserId, ProductUserId localUserId, EOSPeer peer)
        {
            RemoteUserId = remoteUserId;
            LocalUserId = localUserId;
            this.peer = peer;
        }

        protected override void Send(byte[] dataBuffer, int amount)
        {
            peer.Send(dataBuffer, amount, RemoteUserId);
        }

        /// <inheritdoc/>
        public override string ToString() => RemoteUserId?.ToString() ?? "Invalid";

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as EOSConnection);
        
        /// <inheritdoc/>
        public bool Equals(EOSConnection other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return RemoteUserId?.ToString() == other.RemoteUserId?.ToString();
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return RemoteUserId?.GetHashCode() ?? 0;
        }

        public static bool operator ==(EOSConnection left, EOSConnection right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }

        public static bool operator !=(EOSConnection left, EOSConnection right) => !(left == right);
    }
}