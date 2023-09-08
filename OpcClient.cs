using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcTools
{
    public class OpcClient
    {
        public Session? Client { get; set; }
        public string DefaultPrefix { get; set; }
        public OpcClient(string ipAddress, int port, string defaultPrefix, string sessionName = "", int timeout = 1000)
        {
            DefaultPrefix = defaultPrefix;
            var config = new ApplicationConfiguration()
            {
                ApplicationType = ApplicationType.Client,
                TransportQuotas = new TransportQuotas { OperationTimeout = timeout },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = timeout },
            };
            var endpointDescription = CoreClientUtils.SelectEndpoint($"opc.tcp://{ipAddress}:{port}", useSecurity: false, discoverTimeout: timeout);
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
            Client = Session.Create(config, endpoint, false, "OPC UA ParsConnectClient", (uint)timeout, new UserIdentity(new AnonymousIdentityToken()), null).GetAwaiter().GetResult();

        }

        public T? ReadTag<T>(string tagName, string prefix = "")
        {
            if (Client != null)
            {
                try
                {
                    var result = (T)Client.ReadValue((string.IsNullOrEmpty(prefix) ? DefaultPrefix : prefix) + tagName).Value;
                    return result;
                }
                catch (Exception ex)
                {
                    throw (new InvalidOperationException("Check your tag name or prefix or cast type. " + ex.ToString()));
                }

            }

            return default(T);
        }
        public void SetTag(OpcValue[] values, string prefix = "")
        {
            if (Client == null)
            {
                throw new InvalidOperationException("Client is not initialized");
            }

            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Values cannot be null or empty");
            }

            string actualPrefix = string.IsNullOrEmpty(prefix) ? DefaultPrefix : prefix;

            var valueCollection = new WriteValueCollection(
                values.Select(x => new WriteValue()
                {
                    NodeId = new NodeId(actualPrefix + x.NodeId),
                    AttributeId = 13,
                    Value = new DataValue(x.Value),
                }));

            var status = new StatusCodeCollection();
            var diag = new DiagnosticInfoCollection();

            try
            {
                Client.Write(
                    new RequestHeader(),
                    valueCollection, out status, out diag);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while writing values", ex);
            }
        }

        public IEnumerable<T>? ReadTags<T>(string[] tagNames, string prefix = "")
        {
            if (Client != null)
            {
                DataValueCollection readedTags;
                IList<ServiceResult> readedTagsResults;

                try
                {
                    Client.ReadValues(tagNames.Select(x => new NodeId((string.IsNullOrEmpty(prefix) ? DefaultPrefix : prefix) + x)).ToList(), out readedTags, out readedTagsResults);
                    return readedTags.Select(x => (T)x.Value);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;

        }
    }
}
