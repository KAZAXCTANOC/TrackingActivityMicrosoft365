using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using PulseCosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace PulseCosts.Controllers
{
    public class Microsoft365Controller
    {
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
        public async Task<PulseCostTableElement> GetDataFromPulseCostAsync(GraphServiceClient me, string IdDocument, string IdGroup, int y)
        {
            var excel = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            var SelectRange = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"A{y}:R{y}").Request().GetAsync();

            List<PulseCostTableElement> ListPulseCostTableElements = new List<PulseCostTableElement>();
            PulseCostTableElement tableElement = null;

            JArray MRange = JArray.Parse(SelectRange.Text.RootElement.ToString());
            foreach (var item in MRange)
            {
                tableElement = new PulseCostTableElement
                {
                    Work = new Work
                    {
                        ProjectWork = item[2].ToString(),
                        ActualScopeWork = item[3].ToString(),
                        ActualCostsWork = item[4].ToString(),
                        PredictedTotalCostWork = item[5].ToString()
                    },
                    Material = new Materials
                    {
                        VolumeMaterialsDesigned = item[6].ToString(),
                        ActualVolumeMaterials = item[7].ToString(),
                        ActualCostsMaterials = item[8].ToString(),
                        PredictedTotalCostMaterials = item[9].ToString(),
                    },
                    Classifier = new Classifier
                    {
                        K = item[14].ToString(),
                        M = item[15].ToString(),
                        X = item[16].ToString(),
                        P = item[17].ToString(),
                    }
                };

                if (tableElement.Work.ActualCostsWork == "")
                    tableElement.Work.ActualCostsWork = "0";

                if (tableElement.Material.ActualCostsMaterials == "")
                    tableElement.Material.ActualCostsMaterials = "0";
            }

            return tableElement;
        }
        public async Task<bool> TrakingChangeAsync(GraphServiceClient me, string IdDocument, string IdGroup, string Collumn)
        {
            int y = 5;
            MongoDBController mongo = new MongoDBController();
            var excel = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();

            while (true)
            {
                JArray MRangeLastElement = null;
                try
                {
                    MRangeLastElement = JArray.Parse(mongo.GetCollection($"Office365PulseCost{Collumn}{y}").Last().Data);
                }
                catch (Exception e)
                {
                    var SelectedRange = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"{Collumn}{y}").Request().GetAsync();
                    MongoDbElement mongoDbElement = new MongoDbElement
                    {
                        Data = JArray.Parse(SelectedRange.Text.RootElement.ToString()).ToString()
                    };

                    if (mongoDbElement.Data == "[\r\n  [\r\n    \"\"\r\n  ]\r\n]") return true;

                    mongo.CreateElemetInfo(mongoDbElement, $"Office365PulseCost{Collumn}{y}");

                    MRangeLastElement = JArray.Parse(SelectedRange.Text.RootElement.ToString());
                }

                JArray MRange = null;
                var SelectRange = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"{Collumn}{y}").Request().GetAsync();

                try
                {
                    MRange = JArray.Parse(SelectRange.Text.RootElement.ToString());
                    if (MRange.First.ToString() == "[\r\n  \"\"\r\n]") return true;
                }
                catch (Exception e)
                {
                    return true;
                }

                switch (Collumn)
                {
                    case "D":
                        {
                            if(MRange.First.Root.ToString() != MRangeLastElement.First.Root.ToString())
                            {
                                var listPrice = await GetDataBasePricesAsync(SingAndReturnMe());
                                var element = await GetDataFromPulseCostAsync(SingAndReturnMe(), IdDocument, IdGroup, y);

                                var MyNeedPrice = listPrice.Where(El => El.K == 
                                    element.Classifier.K && 
                                    El.M == element.Classifier.M && 
                                    El.X == element.Classifier.X &&
                                    El.P.Contains(element.Classifier.P)).FirstOrDefault();

                                var MRangeNum = Convert.ToDecimal(Regex.Replace(MRange.First.ToString(), @"\D", ""));
                                var MRangeLastElementNum = Convert.ToDecimal(Regex.Replace(MRangeLastElement.First.ToString(), @"\D", ""));
                                var cost = Convert.ToDecimal(MyNeedPrice.CostWork);

                                //E
                                var FinalCumDown = ((MRangeNum - MRangeLastElementNum) * cost) + Convert.ToDecimal(element.Work.ActualCostsWork);

                                #region SendMessage
                                string data = $"[[\"{FinalCumDown}\"]]";
                                JsonDocument doc = JsonDocument.Parse(data);
                                var rangeUpdate = new WorkbookRange
                                {
                                    Values = doc
                                };
                                var res = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"E{y}:E{y}")
                                    .Request()
                                    .PatchAsync(rangeUpdate);
                                #endregion

                                //F
                                decimal FinalCumDownE = 0;
                                try
                                {
                                    FinalCumDownE = ((MRangeNum - Convert.ToDecimal(element.Work.ProjectWork)) * cost) + FinalCumDown;
                                }
                                catch (Exception e)
                                {

                                }

                                #region SendMessage
                                data = $"[[\"{FinalCumDownE}\"]]";
                                doc = JsonDocument.Parse(data);
                                rangeUpdate = new WorkbookRange
                                {
                                    Values = doc
                                };
                                await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"F{y}:F{y}")
                                    .Request()
                                    .PatchAsync(rangeUpdate);
                                #endregion

                                MongoDbElement mongoDbElement = new MongoDbElement
                                {
                                    Data = JArray.Parse(MRange.ToString()).ToString()
                                };
                                mongo.CreateElemetInfo(mongoDbElement, $"Office365PulseCost{Collumn}{y}");
                            }
                        }
                        break;

                    case "H":
                        {
                            if (MRange.First.Root.ToString() != MRangeLastElement.First.Root.ToString())
                            {
                                var listPrice = await GetDataBasePricesAsync(SingAndReturnMe());
                                var element = await GetDataFromPulseCostAsync(SingAndReturnMe(), IdDocument, IdGroup, y);

                                var MyNeedPrice = listPrice.Where(El => El.K ==
                                    element.Classifier.K &&
                                    El.M == element.Classifier.M &&
                                    El.X == element.Classifier.X &&
                                    El.P.Contains(element.Classifier.P)).FirstOrDefault();

                                var MRangeNum = Convert.ToDecimal(Regex.Replace(MRange.First.ToString(), @"\D", ""));
                                var MRangeLastElementNum = Convert.ToDecimal(Regex.Replace(MRangeLastElement.First.ToString(), @"\D", ""));
                                var cost = Convert.ToDecimal(MyNeedPrice.CostMaterial);

                                //I
                                var FinalCumDown = ((MRangeNum - MRangeLastElementNum) * cost) + Convert.ToDecimal(element.Material.ActualCostsMaterials);

                                #region SendMessage
                                string data = $"[[\"{FinalCumDown}\"]]";
                                JsonDocument doc = JsonDocument.Parse(data);
                                var rangeUpdate = new WorkbookRange
                                {
                                    Values = doc
                                };
                                var res = await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"I{y}")
                                    .Request()
                                    .PatchAsync(rangeUpdate);
                                #endregion

                                //J
                                decimal FinalCumDownE = 0;
                                try
                                {
                                    FinalCumDownE = ((MRangeNum - Convert.ToDecimal(element.Material.VolumeMaterialsDesigned)) * cost) + FinalCumDown;
                                }
                                catch (Exception e) { }

                                #region SendMessage
                                data = $"[[\"{FinalCumDownE}\"]]";
                                doc = JsonDocument.Parse(data);
                                rangeUpdate = new WorkbookRange
                                {
                                    Values = doc
                                };
                                await me.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"J{y}:J{y}")
                                    .Request()
                                    .PatchAsync(rangeUpdate);
                                #endregion

                                MongoDbElement mongoDbElement = new MongoDbElement
                                {
                                    Data = JArray.Parse(MRange.ToString()).ToString()
                                };
                                mongo.CreateElemetInfo(mongoDbElement, $"Office365PulseCost{Collumn}{y}");

                            }
                        }
                        break;
                }

                y++;
            }

            return true;
        }

        // Group: fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5
        // Doc: 01N2KAJ4PBJXRHT5QQ6ZCYPTTKRYQJ4BRY
        private async Task<List<PriceDataBaseElement>> GetDataBasePricesAsync(GraphServiceClient me, string IdGroup = "fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5", string IdDocument = "01N2KAJ4PBJXRHT5QQ6ZCYPTTKRYQJ4BRY")
        {
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
    }
}
