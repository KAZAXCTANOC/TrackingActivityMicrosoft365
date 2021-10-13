using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using PulseCosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PulseCosts.Controllers
{
    class MicrosfotAcytivityController
    {
        #region Ининициализация и переменные класса
        public MicrosfotAcytivityController()
        {
            _ = Task.Run(() => InitialMicrosfotAcytivityControllerAsync()).Result;
        }
        private List<PriceDataBaseElement> database_prices_progress { get; set; }
        private DBController DBController { get; set; }
        private async Task<bool> InitialMicrosfotAcytivityControllerAsync()
        {
            DBController = new DBController();
            database_prices_progress = await MicrosoftActivityHelper.GetDataBasePricesAsync();
            return true;
        }
        #endregion
        public void TrakingActivityDataBase()
        {

        }

        /// <summary>
        /// Метод, отслеживающий активность в книге CalcTemplate(2)
        /// </summary>
        public async Task TrakingActivityCalcTemplateAsync(string Collumn)
        {
            int i = 5;
            JArray Range = await GetRangeAsync(Collumn, i);
            while(Range!= null)
            {
                Range = await GetRangeAsync(Collumn, i);
                i++;
                if(DBController.GetDataElement(Regex.Replace(Range.First.ToString(), @"\D", ""))!=null)
                {
                    //TODO тут будем сравнивать
                }
                else
                {
                    //TODO тут сначала сохранем элемент в базу и сравниваем как впервые вставленный, возможно нужно вставить нулевой элемент для сравнения
                }
            }
        }

        /// <summary>
        /// Метод, получающий из документа ячейки под (Collumn:y)
        /// </summary>
        public async Task<JArray> GetRangeAsync(string Collumn, int y, string IdGroup = "892eb031-5560-4d2c-9142-0030091aabfa", string IdDocument = "01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT")
        {
            var excel = await MicrosoftActivityHelper.SingAndReturnMe().Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets.Request().GetAsync();
            var SelectedRange = await MicrosoftActivityHelper.SingAndReturnMe().Groups[IdGroup].Drive.Items[IdDocument].Workbook.Worksheets[excel[5].Id].Range($"{Collumn}{y}").Request().GetAsync();
            JArray MRange;
            try
            {
                MRange = JArray.Parse(SelectedRange.Text.RootElement.ToString());
            }
            catch (Exception e)
            {
                return null;
            }
            return MRange;
        }
    }
}
