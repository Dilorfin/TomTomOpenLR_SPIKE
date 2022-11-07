using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace TomTomOpenLR_SPIKE;

class TomTomTrafficProvider : IDisposable
{
    private readonly string[] urls = {
        <urls>
    };

    public TomTomTrafficProvider()
    {
        _handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _certificate = LoadCertificate();
        _handler.ClientCertificates.Add(_certificate);

        _httpClient = new HttpClient(_handler, disposeHandler: false);
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
    }

    public IEnumerable<HttpResponseMessage> RetrieveTrafficData(CancellationToken cancellationToken)
    {
        for (int i = 0; i < urls.Length; i++)
        {
            HttpResponseMessage response = null;
            try
            {
                var trafficDataUri = new UriBuilder(urls[i]);
                var task = _httpClient.GetAsync(trafficDataUri.Uri, cancellationToken);
                task.Wait(cancellationToken);
                response = task.Result;
                
                response.EnsureSuccessStatusCode();
            }
            catch(Exception e)
            {
                Console.WriteLine($"Can't get data from \t\t{urls[i]}");
                Console.WriteLine(e);
            }
            yield return response;
        }
        yield break;
    }

    #region Implment IDisposable
    public void Dispose()
    {
        _httpClient?.Dispose();
        _handler?.Dispose();
        _certificate?.Dispose();
    }

    #endregion

    private readonly HttpClient _httpClient;
    private readonly HttpClientHandler _handler;
    private readonly X509Certificate2 _certificate;

    private X509Certificate2 LoadCertificate()
    { 
        X509Certificate2Collection certs;
        using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
        {
            try
            {
                store.Open(OpenFlags.ReadOnly);
                certs = store.Certificates.Find(X509FindType.FindByIssuerName, "TomTom-Business-to-Business-Client-Certificate-Signing", true);
            }
            finally
            {
                store.Close();
            }
        }
        return certs[0];
    }
}