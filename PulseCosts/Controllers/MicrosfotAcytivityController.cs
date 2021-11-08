using log4net;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using PulseCosts.Models;
using PulseCosts.Models.SqlDbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PulseCosts.Controllers
{
    class MicrosfotAcytivityController
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Ининициализация и переменные класса
        public MicrosfotAcytivityController(string IdDocument = "01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT", string IdGroup = "892eb031-5560-4d2c-9142-0030091aabfa")
        {
            _ = Task.Run(() => InitialMicrosfotAcytivityControllerAsync()).Result;
            this.IdDocument = IdDocument;
            this.IdGroup = IdGroup;
            log4net.Config.XmlConfigurator.Configure();
        }
        private string IdDocument { get; set; }
        private string IdGroup { get; set; }

        private List<PriceDataBaseElement> database_prices_progress { get; set; }
        private DBController DBController { get; set; }
        private async Task<bool> InitialMicrosfotAcytivityControllerAsync()
        {
            DBController = new DBController();
            database_prices_progress = await MicrosoftActivityHelper.GetDataBasePricesAsync();
            return true;
        }
        #endregion

        /// <summary>
        /// Метод, отслеживающий активность в книге CalcTemplate(2)
        /// </summary>
        public async Task<bool> TrakingActivityCalcTemplateAsync(string Collumn)
        {
            try
            {
                int RowCount = await GetCountRowsAsync(5, "D");

                RowCount += 5;
                for (int i = 5; i < RowCount; i++)
                {
                    DBController = new DBController();
                    PulseCostTableElement elementFromTable = await GetDataFromPulseCostAsync(i, Collumn);
                    if (elementFromTable == null) return false;
                    //Получение данных с базы, т.е. прошлое состояние строки
                    PulseCostTableElement elementFromDB = DBController.GetDataElement($"{elementFromTable.Work.B}{elementFromTable.Classifier.K}{elementFromTable.Classifier.M}{elementFromTable.Classifier.X}{elementFromTable.Classifier.P}");

                    if (elementFromDB != null)
                    {
                        if(CompareElemetns(elementFromTable, elementFromDB, Collumn))
                        {
                            PriceDataBaseElement priceDataBase = GetNeedRowFromPriceDataBase(elementFromTable);
                            if (priceDataBase == null)
                            {
                                await SaveReusltToExcel("Данные классификатора пустые или неверные", i.ToString(), "E");
                                return true;
                            }
                            switch (Collumn)
                            {
                                case"D":
                                    {
                                        log.Info($"Обновлена строка {i}{ Collumn}");

                                        if (elementFromTable.Work.E == "")
                                        {
                                            elementFromTable.Work.E = "0";
                                        }
                                        decimal E = (((Convert.ToDecimal(elementFromTable.Work.D) - Convert.ToDecimal(elementFromDB.Work.D)) * Convert.ToDecimal(priceDataBase.CostWork)) + Convert.ToDecimal(elementFromTable.Work.E));
                                        await SaveReusltToExcel(E.ToString(), i.ToString(), "E");

                                        if (elementFromTable.Work.C == "")
                                        {
                                            await SaveReusltToExcel("Ошибка вычисления", i.ToString(), "F");
                                            elementFromTable.Work.C = "Ошибка вычисления";
                                        }
                                        else
                                        {
                                            try
                                            {
                                                decimal F = (((Convert.ToDecimal(elementFromTable.Work.C)-Convert.ToDecimal(elementFromTable.Work.D))) * Convert.ToDecimal(priceDataBase.CostWork)) + E;
                                                await SaveReusltToExcel(F.ToString(), i.ToString(), "F");
                                            }
                                            catch (Exception e)
                                            {
                                                log.Error($"{e}");
                                            }
                                        }
                                        HistoryChange change = new HistoryChange
                                        {
                                            B = elementFromTable.Work.B,
                                            C = elementFromTable.Work.C,
                                            D = elementFromTable.Work.D,
                                            E = elementFromTable.Work.E,
                                            F = elementFromTable.Work.F,
                                            G = elementFromTable.Material.G,
                                            H = elementFromTable.Material.H,
                                            I = elementFromTable.Material.I,
                                            CK = elementFromTable.Classifier.K,
                                            CM = elementFromTable.Classifier.M,
                                            CX = elementFromTable.Classifier.X,
                                            CP = elementFromTable.Classifier.P,
                                            TimeChange = DateTime.Now
                                        };
                                        DBController.CreateChange(change);
                                        log.Info($"Обновлен елемент в базе данных {elementFromDB.RowName}");
                                        DBController.UpdatePulseCostTableElement(elementFromTable, elementFromDB.RowName);
                                        log.Info($"Обновлен елемент в excel {elementFromDB.RowName}");
                                        break;
                                    }
                                case "H":
                                    {
                                        log.Info($"Обновлена строка {i}{Collumn}");

                                        decimal I = ((Convert.ToDecimal(elementFromTable.Material.H) - Convert.ToDecimal(elementFromDB.Material.H)) * Convert.ToDecimal(priceDataBase.CostMaterial)) + Convert.ToDecimal(elementFromDB.Material.H);
                                        await SaveReusltToExcel(I.ToString(), i.ToString(), "I");

                                        if (elementFromTable.Material.G == "")
                                        {
                                            await SaveReusltToExcel("Ошибка вычисления", i.ToString(), "J");
                                            elementFromTable.Material.G = "Ошибка вычисления";
                                        }
                                        if (elementFromTable.Material.J == "")
                                        {
                                            elementFromTable.Material.J = "0";
                                        }
                                        else
                                        {
                                            try
                                            {
                                                decimal J = ((Convert.ToDecimal(elementFromTable.Material.G) - (Convert.ToDecimal(elementFromTable.Material.H))) * Convert.ToDecimal(priceDataBase.CostMaterial)) + Convert.ToDecimal(elementFromTable.Material.J);
                                                await SaveReusltToExcel(J.ToString(), i.ToString(), "J");
                                            }
                                            catch (Exception e)
                                            {
                                                log.Error($"{e}");
                                            }
                                        }

                                        HistoryChange change = new HistoryChange
                                        {
                                            B = elementFromTable.Work.B,
                                            C = elementFromTable.Work.C,
                                            D = elementFromTable.Work.D,
                                            E = elementFromTable.Work.E,
                                            F = elementFromTable.Work.F,
                                            G = elementFromTable.Material.G,
                                            H = elementFromTable.Material.H,
                                            I = elementFromTable.Material.I,
                                            CK = elementFromTable.Classifier.K,
                                            CM = elementFromTable.Classifier.M,
                                            CX = elementFromTable.Classifier.X,
                                            CP = elementFromTable.Classifier.P
                                        };
                                        DBController.CreateChange(change);
                                        DBController.UpdatePulseCostTableElementH(elementFromTable, elementFromDB.RowName);
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        #region Создание сохраняемого впервые элемента базы данных

                        PulseCostTableElement row = new PulseCostTableElement()
                        {
                            Classifier = elementFromTable.Classifier,
                            Work = new Work
                            {
                                B = "0",
                                D = "0",
                                C = "0",
                                E = "0",
                                F = "0"
                            },
                            Material = new Materials
                            {
                                G = "0",
                                H = "0",
                                I = "0",
                                J = "0"
                            },
                            ChangeTime = DateTime.Now,
                            RowName = $"{elementFromTable.Work.B}{elementFromTable.Classifier.K}{elementFromTable.Classifier.M}{elementFromTable.Classifier.X}{elementFromTable.Classifier.P}"
                        };
                        log.Debug($"Обнаружена новая строка и добавлена в базу новая строка {Collumn}:{i} {elementFromTable.Work.B}{elementFromTable.Classifier.K}{elementFromTable.Classifier.M}{elementFromTable.Classifier.X}{elementFromTable.Classifier.P}");
                        #endregion

                        DBController.CreateRow(row);
                    }
                }
            return false;
            }
            catch (Exception e)
            {
                log.Error($"Ошибка {e}");
                
                return false;
            }
        }

        private async Task SaveReusltToExcel(string SaveStr, string y, string collumn)
        {
            var excel = await MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            string data = $"[[\"{SaveStr}\"]]";
            JsonDocument doc = JsonDocument.Parse(data);
            var rangeUpdate = new WorkbookRange
            {
                Values = doc
            };
            var res = await MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"{collumn}{y}")
                .Request()
                .PatchAsync(rangeUpdate);
        }

        /// <summary>
        /// Получает нужную для получения цены строчку по классификатору 
        /// </summary>
        private PriceDataBaseElement GetNeedRowFromPriceDataBase(PulseCostTableElement elementFromTable)
        {
            return database_prices_progress.Where(El => El.K ==
                                    elementFromTable.Classifier.K &&
                                    El.M == elementFromTable.Classifier.M &&
                                    El.X == elementFromTable.Classifier.X &&
                                    El.P.Contains(elementFromTable.Classifier.P)).FirstOrDefault();
        }

        /// <summary>
        /// Сравнивает два элемента и возращает false если они равны, и true если нет 
        /// </summary>
        private bool CompareElemetns(PulseCostTableElement element, PulseCostTableElement selectedElement, string Collumn)
        {
            switch (Collumn)
            {
                case "D":
                    {
                        if (element.Work.D == selectedElement.Work.D)
                            return false;
                        else return true;
                    }

                case "H":
                    {
                        if (element.Material.H == selectedElement.Material.H)
                            return false;
                        else return true;
                    }
            }

            return false;
        }

        /// <summary>
        /// Метод, получающий из документа ячейки под (Collumn:y)
        /// </summary>
        public async Task<JArray> GetRangeAsync(string Collumn, int y)
        {
            var excel = await MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            var SelectedRange = await MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"{Collumn}{y}").Request().GetAsync();
            JArray MRange;
            try
            {
                MRange = JArray.Parse(SelectedRange.Text.RootElement.ToString());
                if (MRange.ToString() == "[\r\n  [\r\n    \"\"\r\n  ]\r\n]")
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return MRange;
        }

        /// <summary>
        /// Получает строчку из листа пульса цен по ее номеру строчки
        /// </summary>
        public async Task<PulseCostTableElement> GetDataFromPulseCostAsync(int y, string collumn)
        {
            var excel = await MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            var SelectRange = await MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"A{y}:R{y}").Request().GetAsync();

            List<PulseCostTableElement> ListPulseCostTableElements = new List<PulseCostTableElement>();
            PulseCostTableElement tableElement = null;

            JArray MRange = JArray.Parse(SelectRange.Text.RootElement.ToString());
            foreach (var item in MRange)
            {
                tableElement = new PulseCostTableElement
                {
                    Work = new Work
                    {
                        B = item[1].ToString(),
                        C = item[2].ToString(),
                        D = item[3].ToString(),
                        E = item[4].ToString(),
                        F = item[5].ToString()
                    },
                    Material = new Materials
                    {
                        G = item[6].ToString(),
                        H = item[7].ToString(),
                        I = item[8].ToString(),
                        J = item[9].ToString(),
                    },
                    Classifier = new Classifier
                    {
                        K = item[14].ToString(),
                        M = item[15].ToString(),
                        X = item[16].ToString(),
                        P = item[17].ToString(),
                    }
                };
            }
            switch (collumn)
            {
                case "D":
                    {
                        if (tableElement.Work.D == "") return null;
                        break;
                    }

                case "H":
                    {
                        if (tableElement.Material.H == "") return null;
                        break;
                    }
            }

            return tableElement;
        }

        public async Task<int> GetCountRowsAsync(int y, string collumn)
        {
            var excel = await MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            int i = 0;
            while(true)
            {
                var SelectRange = await MicrosoftActivityHelper.MyClient.Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"{collumn}{y}").Request().GetAsync();
                var a = SelectRange.Values.RootElement.ToString();
                if (a != "[[\"\"]]")
                {
                    y++;
                    i++;
                }
                else return i;
            }
        }
    }
}
