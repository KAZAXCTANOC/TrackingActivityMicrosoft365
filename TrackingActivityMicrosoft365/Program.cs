using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TrackingActivityMicrosoft365.Models;
using TrackingActivityMicrosoft365.MongoDB;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace TrackingActivityMicrosoft365
{
    internal class Program
    {
        private static string[] scopes = new[] { "User.Read", "Files.Read", "Files.Read.All" };
        private static string Instance = "https://login.microsoftonline.com/";
        private static string ClientId = "f4f7d338-486e-490f-a87e-298f413f8942";
        private static string TenantId = "8a648ae3-f42e-4858-b848-ef62d3422f6d";

        private static MongoDBController _mongoDB = new MongoDBController();

        private static string access_token { get; set; }

        private static async Task Main(string[] args)
        {
            var Iam = SingAndReturnMe();

            await ScreenUserAsync(Iam);

            while (true)
            {
                if(await GetRange(Iam, "A1:C5"))
                {
                    await GetRange(Iam, "A1:C5");
                }
                else
                {
                    Iam = SingAndReturnMe();
                }
            }
        }


        /// <summary>
        /// Берет из полученого дока(онли excel) и указанной в функции Range данные
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="Range"></param>
        /// <param name="itemId">Id ексельки</param>
        /// <returns>Кароч то что было в рэндже</returns>
        private static async Task<bool> GetRange(GraphServiceClient graphClient, string Range)
        {
            try
            {
                char[] eng = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

                //Получение списка документов в общем доступе
                var driveItem = await graphClient.Me.Drive.SharedWithMe().Request().GetAsync();

                //Получние всех листов в доке
                var aw = await graphClient
                    .Drives[driveItem.ElementAt(0)
                    .RemoteItem.ParentReference.DriveId]
                    .Items[driveItem.ElementAt(0).RemoteItem.Id]
                    .Workbook.Worksheets.Request().GetAsync();

                //Перечисление всех листов
                //          id      name
                Dictionary<string, string> Lists = new Dictionary<string, string>();
                foreach (var item in aw)
                {
                    Lists.Add(item.Id, item.Name);
                }

                //Получние нужного листа из дока
                var myRange = await graphClient
                    .Drives[driveItem.ElementAt(0)
                    .RemoteItem.ParentReference.DriveId]
                    .Items[driveItem.ElementAt(0).RemoteItem.Id]
                    .Workbook.Worksheets[Lists.ElementAt(0).Key]
                    .Range(Range)
                    .Request()
                    .GetAsync();

                var jmass = JArray.Parse(myRange.Text.RootElement.ToString());
                JArray pastJMass = pastJMass = JArray.Parse(_mongoDB.GetCollection().Last().Data.ToString());

                Dictionary<string, string> realMassive = new Dictionary<string, string>();

                int x = 0;
                foreach (var v in jmass)
                {
                    x++;
                    for (int i = 0; i < v.Count(); i++)
                    {
                        realMassive.Add($"{eng[i]}{x}", v[i].ToString());
                    }
                }

                if(pastJMass.ToString() != jmass.ToString())
                {
                    Dictionary<string, string> pastMassive = new Dictionary<string, string>();
                    x = 0;
                    foreach (var v in pastJMass)
                    {
                        x++;
                        for (int i = 0; i < v.Count(); i++)
                        {
                            pastMassive.Add($"{eng[i]}{x}", v[i].ToString());
                        }
                    }

                    _mongoDB.CreateElemetInfo(new DataElementInfo 
                    {
                        Data = jmass.ToString(),
                        LastView = DateTime.Now.ToString(),
                        Changed = СomparingDictionarys(realMassive, pastMassive)
                    });
                }

                Console.ForegroundColor = ConsoleColor.Blue;

                for (int i = 0; i < realMassive.Count; i += 3)
                {
                    Console.WriteLine("|{0,12}   |{1,12}   |{2,12}   |", realMassive.ElementAt(i).Value, realMassive.ElementAt(i + 1).Value, realMassive.ElementAt(i + 2).Value);
                    Console.WriteLine("-------------------------------------------------");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"NEW ITERATION {DateTime.Now}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("-------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;

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
            string authority = string.Concat(Instance, TenantId);
            string resource = "https://graph.microsoft.com";
            try
            {
                UserPasswordCredential userPasswordCredential = new UserPasswordCredential(userName, password);
                AuthenticationContext authContext = new AuthenticationContext(authority);
                var result = authContext.AcquireTokenAsync(resource, ClientId, userPasswordCredential).Result;
                var graphserviceClient = new GraphServiceClient(
                    new DelegateAuthenticationProvider(
                        (requestMessage) =>
                        {
                            access_token = authContext.AcquireTokenSilentAsync(resource, ClientId).Result.AccessToken;
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", access_token);
                            return Task.FromResult(0);
                        }));
                var User = graphserviceClient.Me.Request().GetAsync();

                return graphserviceClient;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
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
    }
}
