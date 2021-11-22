using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MyEveryDay.Service
{
    public static class ExportService
    {
        public static async Task ExportRtfAsync(string path,int range, (int? Year, int? Month, int? Day) date,string yearTitle,string monthTitle,string dayTitle)
        {
            FlowDocument doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph());
            switch (range)
            {
                case 0://当日
                    var rtf = await RecordService.GetRichText(date.Year.Value, date.Month.Value, date.Day.Value);
                    doc.GetAllRange().LoadRtf(rtf);
                    break;
                case 1://当月
                    {
                        var records = await RecordService.GetRecords(date.Year, date.Month);
                        foreach (var record in records)
                        {
                            var r = doc.GetTailRange().LoadRtf(dayTitle);
                            r.Text = r.Text.Replace("%Day%", record.Day.ToString());
                            doc.GetTailRange().LoadRtf(record.RichText);
                        }
                    }
                    break;
                case 2://当年
                    {
                        var records = await RecordService.GetRecords(date.Year);
                        foreach (var month in records.Select(p => p.Month).Distinct())
                        {
                            var rM = doc.GetTailRange().LoadRtf(monthTitle);
                            rM.Text = rM.Text.Replace("%Month%", month.ToString());
                            foreach (var record in records.Where(p => p.Month == month))
                            {
                                var rD = doc.GetTailRange().LoadRtf(dayTitle);
                                rD.Text = rD.Text.Replace("%Day%", record.Day.ToString());
                                doc.GetTailRange().LoadRtf(record.RichText);
                            }
                        }
                    }
                    break;
                case 3://全部
                    {
                        var records = await RecordService.GetRecords();
                        foreach (var year in records.Select(p => p.Year).Distinct())
                        {
                            var rY = doc.GetTailRange().LoadRtf(yearTitle);
                            doc.Blocks.Add(new Paragraph());
                            rY.Text = rY.Text.Replace("%Year%", year.ToString());
                            foreach (var month in records.Where(p => p.Year == year).Select(p => p.Month).Distinct())
                            {
                                var rM = doc.GetTailRange().LoadRtf(monthTitle);
                                rM.Text = rM.Text.Replace("%Month%", month.ToString());
                                foreach (var record in records.Where(p => p.Year == year && p.Month == month))
                                {
                                    var rD = doc.GetTailRange().LoadRtf(dayTitle);
                                    rD.Text = rD.Text.Replace("%Day%", record.Day.ToString());
                                    doc.GetTailRange().LoadRtf(record.RichText);
                                }
                            }
                        }

                    }
                    break;
            }
            using FileStream fs = new FileStream(path, FileMode.Create);
            var ar = doc.GetAllRange();
            ar.ApplyPropertyValue(System.Windows.Controls.Control.ForegroundProperty, Brushes.Black);
            ar.Save(fs, DataFormats.Rtf);
        }

    }
}
