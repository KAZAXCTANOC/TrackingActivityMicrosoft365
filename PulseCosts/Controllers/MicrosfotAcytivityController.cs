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
using System.Threading.Tasks;

namespace PulseCosts.Controllers
{
    class MicrosfotAcytivityController
    {
        #region Ининициализация и переменные класса
        public MicrosfotAcytivityController(string IdDocument = "01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT", string IdGroup = "892eb031-5560-4d2c-9142-0030091aabfa")
        {
            _ = Task.Run(() => InitialMicrosfotAcytivityControllerAsync()).Result;
            this.IdDocument = IdDocument;
            this.IdGroup = IdGroup;
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
            int i = 5;
            PulseCostTableElement elementFromTable = await GetDataFromPulseCostAsync(i, Collumn);

            while (elementFromTable != null)
            {
                DBController = new DBController();
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
                                    decimal E = (((Convert.ToDecimal(elementFromTable.Work.D) - Convert.ToDecimal(elementFromDB.Work.D)) * Convert.ToDecimal(priceDataBase.CostWork)) + Convert.ToDecimal(elementFromDB.Work.D));
                                    await SaveReusltToExcel(E.ToString(), i.ToString(), "E");

                                    if (elementFromTable.Work.C == "")
                                    {
                                        await SaveReusltToExcel("Ошибка вычисления", i.ToString(), "F");
                                        elementFromTable.Work.C = "Ошибка вычисления";
                                    }
                                    else
                                    {
                                        decimal F = ((Convert.ToDecimal(elementFromTable.Work.D) - Convert.ToDecimal(elementFromTable.Work.C)) * Convert.ToDecimal(priceDataBase.CostWork)) + E;
                                        await SaveReusltToExcel(F.ToString(), i.ToString(), "F");
                                    }

                                    DBController.UpdatePulseCostTableElement(elementFromTable, elementFromDB.RowName);
                                    break;
                                }
                            case "H":
                                {
                                    decimal I = ((Convert.ToDecimal(elementFromTable.Material.H) - Convert.ToDecimal(elementFromDB.Material.H)) * Convert.ToDecimal(priceDataBase.CostMaterial)) + Convert.ToDecimal(elementFromDB.Material.H);
                                    await SaveReusltToExcel(I.ToString(), i.ToString(), "I");

                                    if (elementFromTable.Material.G == "")
                                    {
                                        await SaveReusltToExcel("Ошибка вычисления", i.ToString(), "J");
                                        elementFromTable.Material.G = "Ошибка вычисления";
                                    }
                                    else
                                    {
                                        decimal J = ((Convert.ToDecimal(elementFromTable.Material.H) - Convert.ToDecimal(elementFromTable.Material.G)) * Convert.ToDecimal(priceDataBase.CostMaterial)) + I;
                                        //TODO доделать абдейт
                                        await SaveReusltToExcel(J.ToString(), i.ToString(), "J");
                                    }

                                    DBController.UpdatePulseCostTableElement(elementFromTable, elementFromDB.RowName);
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
                    #endregion

                    DBController.CreateRow(row);
                }
                i++;
                elementFromTable = await GetDataFromPulseCostAsync(i, Collumn);
            }
            return true;
        }

        private async Task SaveReusltToExcel(string SaveStr, string y, string collumn)
        {
            var excel = await MicrosoftActivityHelper.SingAndReturnMe().Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            string data = $"[[\"{SaveStr}\"]]";
            JsonDocument doc = JsonDocument.Parse(data);
            var rangeUpdate = new WorkbookRange
            {
                Values = doc
            };
            var res = await MicrosoftActivityHelper.SingAndReturnMe().Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"{collumn}{y}")
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
            var excel = await MicrosoftActivityHelper.SingAndReturnMe().Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            var SelectedRange = await MicrosoftActivityHelper.SingAndReturnMe().Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"{Collumn}{y}").Request().GetAsync();
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
            var excel = await MicrosoftActivityHelper.SingAndReturnMe().Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            var SelectRange = await MicrosoftActivityHelper.SingAndReturnMe().Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"A{y}:R{y}").Request().GetAsync();

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


    }
}
