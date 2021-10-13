using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using PulseCosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PulseCosts.Controllers
{
    public static class MicrosoftActivityHelper
    {
        #region Методы
        public static async Task<List<PriceDataBaseElement>> GetDataBasePricesAsync(string IdGroup = "fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5", string IdDocument = "01N2KAJ4PBJXRHT5QQ6ZCYPTTKRYQJ4BRY")
        {
            GraphServiceClient me = SingAndReturnMe();
            var excel = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            var SelectRange = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[0].Id].Range($"A1:F1300").Request().GetAsync();
            JArray MRange = JArray.Parse(SelectRange.Text.RootElement.ToString());

            List<PriceDataBaseElement> PriceDataBaseElements = new List<PriceDataBaseElement>();

            foreach (var el in MRange)
            {
                PriceDataBaseElements.Add(new PriceDataBaseElement
                {
                    K = el[0].ToString(),
                    M = el[1].ToString(),
                    X = el[2].ToString(),
                    P = el[3].ToString(),
                    CostMaterial = el[4].ToString(),
                    CostWork = el[5].ToString()
                });
            }
            PriceDataBaseElements.RemoveAt(0);

            return PriceDataBaseElements;
        }

        #region AutorizeInAzure
        private static string Instance = "https://login.microsoftonline.com/";
        private static string ClientIdProgress = "34f889b6-7528-4064-8840-0c4e3b355cfd";
        private static string TenantIdProgress = "8a648ae3-f42e-4858-b848-ef62d3422f6d";
        private static string access_token { get; set; }
        public static GraphServiceClient SingAndReturnMe(string userName = "n.ognev@bimprogress.team", string password = "Gfgekz2002")
        {
            string authority = string.Concat(Instance, TenantIdProgress);
            string resource = "https://graph.microsoft.com";
            try
            {
                UserPasswordCredential userPasswordCredential = new UserPasswordCredential(userName, password);
                AuthenticationContext authContext = new AuthenticationContext(authority);
                var result = authContext.AcquireTokenAsync(resource, ClientIdProgress, userPasswordCredential).Result;
                var graphserviceClient = new GraphServiceClient(
                    new DelegateAuthenticationProvider(
                        (requestMessage) =>
                        {
                            access_token = authContext.AcquireTokenSilentAsync(resource, ClientIdProgress).Result.AccessToken;
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", access_token);
                            return Task.FromResult(0);
                        }));
                return graphserviceClient;
            }
            catch (Exception e)
            {
            }
            return null;
        }
        #endregion

        #endregion
    }
}
