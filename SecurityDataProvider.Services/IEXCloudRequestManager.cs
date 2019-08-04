using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SecurityDataProvider.Entities.Configuration;
using SecurityDataProvider.Entities.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SecurityDataProvider.Batch.Services
{
    public class IEXCloudRequestManager : IIEXCloudRequestManager
    {
        private readonly IOptions<IEXCredential> credential;
        private readonly HttpClient httpClient;
        private readonly INavDateCalculator navDateCalculator;

        public IEXCloudRequestManager(IOptions<IEXCredential> credential, HttpClient httpClient, INavDateCalculator navDateCalculator)
        {
            this.credential = credential;
            this.httpClient = httpClient;
            this.navDateCalculator = navDateCalculator;

            this.httpClient.BaseAddress = new Uri(credential.Value.Url);
        }

        public IEnumerable<Symbol> GetSymbolList()
        {
            string token = credential.Value.Token;
            string requestUrl = $"ref-data/symbols?token={token}";

            return DoRequest<List<Symbol>>(requestUrl);
        }

        public Entities.Requests.SymbolPrice GetSymbolPrice(string symbol)
        {
            string sanitizedSymbol = symbol?.Trim().ToUpper() ?? string.Empty;

            string token = credential.Value.Token;
            string requestUrl = $"stock/{sanitizedSymbol}/previous?token={token}";

            var securityPriceNav = DoRequest<Entities.Requests.SymbolPrice>(requestUrl);
            securityPriceNav.date = navDateCalculator.CalculateNavDate(DateTime.Today);

            return securityPriceNav;
        }

        private T DoRequest<T>(string url) where T : new()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            using (var response = httpClient.SendAsync(request))
            {
                var content = response.Result.Content.ReadAsStringAsync();

                if (response.Result.IsSuccessStatusCode == false)
                {
                    return new T();
                }

                return JsonConvert.DeserializeObject<T>(content.Result);
            }
        }
    }
}
