using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TrackingActivityMicrosoft365.Models;
using TrackingActivityMicrosoft365.MongoDB;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace TrackingActivityMicrosoft365
{
    //Y3O7Q~NgKrv5~mSyoMfIaNV1kY4dCBt1xyjFz
    //AH_7Q~iiqHTJnYKeZrE3HE06h1P7f05R4mDzR
    public partial class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string Instance = "https://login.microsoftonline.com/";
        private static string ClientIdProgress = "34f889b6-7528-4064-8840-0c4e3b355cfd";
        private static string TenantIdProgress = "8a648ae3-f42e-4858-b848-ef62d3422f6d";
        private static MongoDBController _mongoDB = new MongoDBController();
        private static string access_token { get; set; }

        private static async Task Main(string[] args) 
        {
            var Iam = SingAndReturnMe();

            await ScreenUserAsync(Iam);

            var tasks = new List<Task>();

            tasks.Add(TrakingChangeAsync("E2:E1295", 0, "E", "OfficeDataBase"));
            tasks.Add(TrakingChangeAsync("E2:E1295", 0, "F", "OfficeDataBase2"));
            tasks.Add(TrakingChangeAsync("E2:E4327", 1, "E", "OfficeDataBase3"));

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task TrakingChangeAsync(string Range, int NumberPage, string Collumn, string CollectionName)
        {
            var Iam = SingAndReturnMe();

            while (true)
            {
                if (await GetRange(Iam, Range, NumberPage, Collumn, CollectionName))
                {
                    await GetRange(Iam, Range, NumberPage, Collumn, CollectionName);
                }
                else
                {
                    Iam = SingAndReturnMe();
                }
            }
        }

        //Авторизированный клиент
        //Диапазон ячеек Range(E2:E1295)
        //Номер страницы(отсчет с нуля) 0
        //Столбец E
        private static async Task<bool> GetRange(GraphServiceClient me, string Range, int NumberPage, string Collumn, string CollectionName)
        {
            try
            {
                var excel = await me.Groups["fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5"].Drive.Items["01N2KAJ4PBJXRHT5QQ6ZCYPTTKRYQJ4BRY"]
                    .Workbook.Worksheets.Request().GetAsync();

                var SelectRange = await me.Groups["fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5"].Drive.Items["01N2KAJ4PBJXRHT5QQ6ZCYPTTKRYQJ4BRY"]
                    .Workbook.Worksheets[excel[NumberPage].Id].Range(Range).Request().GetAsync();

                JArray SelectRangeMassiveElements = JArray.Parse(SelectRange.Text.RootElement.ToString());
                JArray RangeMassiveFromDB = JArray.Parse(_mongoDB.GetCollection(CollectionName).Last().Data.ToString());

                Dictionary<string, string> SelectedRangeMassive = new Dictionary<string, string>();
                Dictionary<string, string> RangeFromDBMassive= new Dictionary<string, string>();

                if(SelectRangeMassiveElements.ToString() != RangeMassiveFromDB.ToString())
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    int x = 0;
                    foreach (var v in SelectRangeMassiveElements)
                    {
                        x++;
                        for (int i = 0; i < v.Count(); i++)
                        {
                            SelectedRangeMassive.Add($"{Collumn}{x}", v[i].ToString());
                        }
                    }

                    x = 0;
                    foreach (var v in RangeMassiveFromDB)
                    {
                        x++;
                        for (int i = 0; i < v.Count(); i++)
                        {
                            RangeFromDBMassive.Add($"{Collumn}{x}", v[i].ToString());
                        }
                    }

                    List<ChangedElement> changedElements = СomparingDictionarys(SelectedRangeMassive, RangeFromDBMassive);

                    _mongoDB.CreateElemetInfo(new DataElementInfo
                    {
                        Data = SelectRangeMassiveElements.ToString(),
                        LastView = DateTime.Now.ToString(),
                        Changed = changedElements
                    }, CollectionName);

                    foreach (var El in changedElements)
                    {
                        Console.WriteLine($"\t{excel[NumberPage].Name}: произошли изменения в ячейке:{El.Cell} {DateTime.Now.ToString("F")}");
                    }

                    var ubdateBy = await me.Groups["fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5"].Drive.Items["01N2KAJ4PBJXRHT5QQ6ZCYPTTKRYQJ4BRY"].LastModifiedByUser.Request().GetAsync();
                    await SendNote(me, changedElements, ubdateBy, excel[NumberPage].Name);

                    Console.WriteLine($"Затраченое время на сравнение, формирование массивов и сохранение в базу: {stopwatch.Elapsed}");
                    stopwatch.Stop();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }
            Thread.Sleep(5000);

            return true;
        }
        private static async Task ScreenUserAsync(GraphServiceClient graphClient)
        {
            var User = await graphClient.Me.Request().GetAsync();
            Console.WriteLine("-------- Данные пользователя для работы с Microsoft Grah --------");
            Console.Write(Environment.NewLine);
            Console.WriteLine($"Id пользователя: {User.Id}");
            Console.WriteLine($"Пользователь: {User.DisplayName}");
            Console.WriteLine($"Email\\(MbGooglePath): {User.Mail}");

            Console.WriteLine("-------------------------------------------------");
        }
        private static GraphServiceClient SingAndReturnMe(string userName = "n.ognev@bimprogress.team", string password="Gfgekz2002")
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
                Console.Error.WriteLine(e.InnerException.Message);
            }
            return null;
        }
        private static List<ChangedElement> СomparingDictionarys(Dictionary<string, string> realMassive, Dictionary<string, string> pastMassive)
        {
            List<ChangedElement> changedElements = new List<ChangedElement>();
            for (int i = 0; i < realMassive.Count; i++)
            {
                if (realMassive.ElementAt(i).Value != pastMassive.ElementAt(i).Value)
                {
                    changedElements.Add(new ChangedElement
                    {
                        Cell = realMassive.ElementAt(i).Key,
                        NowValuse = realMassive.ElementAt(i).Value,
                        PastValue = pastMassive.ElementAt(i).Value
                    });
                }
            }

            return changedElements;
        }
        public static async Task<IUserJoinedTeamsCollectionPage> GetGroups(string userName = "n.ognev@bimprogress.team", string password = "Gfgekz2002")
        {
            var clientSecret = "Y3O7Q~NgKrv5~mSyoMfIaNV1kY4dCBt1xyjFz";
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(ClientIdProgress)
                .WithTenantId(TenantIdProgress)
                .WithClientSecret(clientSecret)
                .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            var groups = await graphClient.Groups.Request().Select(x => new { x.Id, x.DisplayName }).GetAsync();
            foreach (var group in groups)
            {
                Console.WriteLine($"{group.DisplayName}, {group.Id}");
            }

            return null;
        }
        private static async Task<bool> SendNote(GraphServiceClient me, List<ChangedElement> changedElements, User ubdateBy, string name)
        {
            var chats = await me.Teams["fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5"].Channels.Request().GetAsync();
            var content2 = "";
            string content = "<ul>";
            foreach (var item in changedElements)
            {
                content2 += $"{ubdateBy.DisplayName}({ubdateBy.Mail}) изменил значение в ячейке {name}: ({item.Cell}) c {item.PastValue} на {item.NowValuse} +\n";
                content += $"<li>{ubdateBy.DisplayName}({ubdateBy.Mail}) изменил значение в ячейке {name}: ({item.Cell}) c {item.PastValue} на {item.NowValuse}</li>";
            }
            content += "</ul>";

            await client.PostAsync($"http://192.168.9.33:8005/message?messagetext={content2}", null);


            var chatMessage = new ChatMessage
            {
                Body = new ItemBody
                {
                    Content = content,
                    ContentType = BodyType.Html
                }
            };

            await me.Chats[chats[0].Id].Messages
                .Request()
                .AddAsync(chatMessage);

            return true;
        }
    }
}
