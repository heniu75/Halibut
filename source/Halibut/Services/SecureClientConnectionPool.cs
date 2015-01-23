using System;
using System.Collections.Concurrent;
using Halibut.Client;
using Halibut.Protocol;

namespace Halibut.Services
{
    public class SecureClientConnectionPool
    {
        readonly ConcurrentDictionary<ServiceEndPoint, ConcurrentBag<SecureConnection>> pool = new ConcurrentDictionary<ServiceEndPoint, ConcurrentBag<SecureConnection>>();

        public SecureConnection Take(ServiceEndPoint endPoint, IConnectionTransactionLog log)
        {
            var connections = pool.GetOrAdd(endPoint, i => new ConcurrentBag<SecureConnection>());
            SecureConnection connection;
            connections.TryTake(out connection);
            return connection;
        }

        public void Return(ServiceEndPoint endPoint, SecureConnection connection, IConnectionTransactionLog log)
        {
            var connections = pool.GetOrAdd(endPoint, i => new ConcurrentBag<SecureConnection>());
            connections.Add(connection);

            log.AppendLine("");

            while (connections.Count > 5)
            {
                SecureConnection dispose;
                if (connections.TryTake(out dispose))
                {
                    dispose.Dispose();
                }
            }
        }
    }
}