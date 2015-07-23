using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ArtOfTest.WebAii.Controls.HtmlControls;
using MongoDB.Bson;

namespace StockSpider
{
    /// <summary>
    /// 获取现金流量表数据
    /// </summary>
    public class XianjinliuliangBiao : BaseSpider
    {
        private string baseUrl = "http://money.finance.sina.com.cn/corp/go.php/vFD_CashFlow/stockid/{0}/ctrl/{1}/displaytype/4.phtml";
        private Regex titleRowReg = new Regex("[一|二|三|四|五|六|七|八|九|十]、(.+)$");

        public override IList<BsonDocument> GetData(string area, string stockCode)
        {

            var docs = new List<BsonDocument>();

            for (int i = 2004; i <= 2015; i++)
            {
                Console.WriteLine("正在处理现金流量: " + stockCode + "," + i);

                string url = string.Format(baseUrl, stockCode, i);

                DomFinder _finder = new DomFinder(downloadHtml(url));

                var dataTables = _finder.Find.AllByTagName<HtmlTable>("table");

                HtmlTable dataTable = null;

                foreach (var tbl in dataTables)
                {
                    if (tbl.ID == "ProfitStatementNewTable0")
                    {
                        dataTable = tbl;
                        break;
                    }
                }

                if (dataTable == null)
                {
                    Console.WriteLine("没有找到现金流量表数据," +stockCode + ":" + i);
                    continue;
                }

                var strColPrefix = string.Empty;
                var reportCnt = dataTable.Rows[0].Cells.Count - 1; //第一行数据为报表日期，可以标识出有多少分报表

                for (int j = 0; j < reportCnt; j++)
                {
                    BsonDocument doc = new BsonDocument();
                    doc.Add(new BsonElement("stockCode", new BsonString(stockCode)));

                    strColPrefix = string.Empty;

                    foreach (var row in dataTable.Rows)
                    {
                        if (row.Cells.Count == 0)
                        {
                            continue;
                        }

                        if (row.Cells.Count == 1)
                        {
                            var mchRst = titleRowReg.Match(row.Cells[0].InnerText);

                            if (mchRst.Success)
                            {
                                strColPrefix = mchRst.Groups[1].Value;
                            }
                            else
                            {
                                strColPrefix = row.Cells[0].InnerText;
                            }
                            
                        }
                        else if (row.Cells.Count == (reportCnt + 1))
                        {
                            doc.Add(new BsonElement(strColPrefix + "_" + row.Cells[0].InnerText, getCellData(row.Cells[j + 1].InnerText)));
                        }
                    }

                    docs.Add(doc);
                }
            }

            return docs;
        }
    }
}
