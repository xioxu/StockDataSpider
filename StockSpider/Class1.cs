using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.Core;
using System.IO;

namespace StockSpider
{
    class Class1
    {
        public void AddAStock()
        {
            Manager myMgr = new Manager(new Settings());
            myMgr.Start();
            myMgr.LaunchNewBrowser();


            var ash = File.ReadAllText(@"c:\a.txt");

            Regex regex = new Regex("\\<li.*href=\"(.*)\">(.*)\\</.*/li");

            try
            {
                bool start = false;

                using (var db = new StockContext1())
                {
                    foreach (Match match in regex.Matches(ash))
                    {
                        var href = match.Groups[1].Value;
                        var codeStr = match.Groups[2].Value;
                        var idx1 = codeStr.IndexOf("(");
                        var name = codeStr.Substring(0, idx1);
                        var code = codeStr.Substring(idx1 + 1, codeStr.Length - idx1 - 2);
                        var categories = "";

                        if (!start && code != "161714")
                        {
                            continue;
                        }
                        else
                        {
                            start = true;
                        }

                        myMgr.ActiveBrowser.NavigateTo(href);
                        myMgr.ActiveBrowser.WaitUntilReady();

                        var table = myMgr.ActiveBrowser.Find.ByExpression<HtmlTable>("id=wahaha", "|", "tagIndex=table:0");

                        if (table != null)
                        {
                            foreach (var row in table.BodyRows)
                            {
                                categories += row.Cells[0].InnerText + ",";
                            }

                            if (categories.Length > 1)
                            {
                                categories = categories.Substring(0, categories.Length - 1);
                            }
                        }

                        db.StockItem.AddObject(new StockItem()
                        {
                            Name = name,
                            Code = code,
                            Area = "sz",
                            Categories = categories
                        });

                        Console.WriteLine(code);
                        db.SaveChanges();
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
