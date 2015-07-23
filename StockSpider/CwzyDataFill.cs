using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using ArtOfTest.WebAii.Controls.HtmlControls;

namespace StockSpider
{
    public class CwzyDataFill : IDataFill
    {
        private string baseUrl = "http://money.finance.sina.com.cn/corp/go.php/vFD_FinanceSummary/stockid/{0}.phtml";
        private WebClient webClient = new WebClient();

        private double removeLastChar(string str)
        {
            if (str.Trim() == "&nbsp;")
            {
                return 0;
            }
            return Convert.ToDouble(str.Replace("万元", "").Replace("元", ""));
        }
        public void Fill(string area, string stockCode)
        {
            Console.WriteLine("正在处理: " + stockCode);
            var cwzyLst = new List<CWZY>();

            using (var db = new StockContext1())
            {
                var htmlContent = webClient.DownloadString(string.Format(baseUrl, stockCode));
                DomFinder _finder = new DomFinder(htmlContent);

                var table = _finder.Find.AllByTagName<HtmlTable>("table")[19];

                for (int i = 0; i < table.Rows.Count;)
                {
                    if ((i % 13) == 0)
                    {
                        CWZY cwzy = new CWZY();
                        cwzy.jzrq = Convert.ToDateTime(table.Rows[i].Cells[1].InnerText);
                        cwzy.mgjzc = Convert.ToDecimal(removeLastChar(table.Rows[i + 1].Cells[1].InnerText));
                        cwzy.mgsy = Convert.ToDecimal(removeLastChar(table.Rows[i + 2].Cells[1].InnerText));
                        cwzy.mgxjhl = Convert.ToDecimal(removeLastChar(table.Rows[i + 3].Cells[1].InnerText));
                        cwzy.mgzbgjj = Convert.ToDecimal(removeLastChar(table.Rows[i + 4].Cells[1].InnerText));
                        cwzy.mgzchj = Convert.ToDecimal(removeLastChar(table.Rows[i + 5].Cells[1].InnerText));
                        cwzy.ldzchj = Convert.ToDecimal(removeLastChar(table.Rows[i + 6].Cells[1].InnerText));
                        cwzy.zchj = Convert.ToDecimal(removeLastChar(table.Rows[i + 7].Cells[1].InnerText));
                        cwzy.cqfzhj = Convert.ToDecimal(removeLastChar(table.Rows[i + 8].Cells[1].InnerText));
                        cwzy.zyywsr = Convert.ToDecimal(removeLastChar(table.Rows[i + 9].Cells[1].InnerText));
                        cwzy.cwfy = Convert.ToDecimal(removeLastChar(table.Rows[i + 10].Cells[1].InnerText));
                        cwzy.jlr = Convert.ToDecimal(removeLastChar(table.Rows[i + 11].Cells[1].InnerText));
                        cwzy.stockCode = stockCode;

                        cwzy.Id = GuidCombGenerator.NewGuid();
                        db.T_cwzy.AddObject(cwzy);
                        i += 13;
                    }
                }

                db.SaveChanges();
            }
        }
    }
}
