using NPOI.XWPF.UserModel;
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
        private static void AddInlines(XWPFParagraph paragraph, IEnumerable<Inline> inlines)
        {
            foreach (var inline in inlines)
            {
                switch (inline)
                {
                    case Run r:
                        {
                            var run = paragraph.CreateRun();
                            run.SetText(r.Text);
                            run.FontSize = r.FontSize;
                            run.FontFamily = r.FontFamily.Source;
                            if (r.TextDecorations.Equals(TextDecorations.Underline))
                            {
                                run.SetUnderline(UnderlinePatterns.Single);
                            }
                            if (r.FontWeight.Equals(FontWeights.Bold))
                            {
                                run.IsBold = true;
                            }
                            if (r.FontStyle.Equals(FontStyles.Italic))
                            {
                                run.IsItalic = true;
                            }

                        }
                        break;
                    case Span span:
                        {
                            AddInlines(paragraph, span.Inlines);
                        }
                        break;
                    case InlineUIContainer c:
                        break;
                    case AnchoredBlock a:
                        break;
                    case LineBreak n:
                        {
                            var run = paragraph.CreateRun();
                            run.AddBreak();
                        }
                        break;
                }
            }
        }
        private static void AddBlocks(XWPFDocument doc, IEnumerable<Block> blocks)
        {
            foreach (var block in blocks)
            {
                Paragraph paragraph = new Paragraph();
                switch (block)
                {
                    case Paragraph p:
                        {
                            XWPFParagraph para = doc.CreateParagraph();
                            AddInlines(para, p.Inlines);
                        }
                        break;
                    case Section s:
                        {
                            AddBlocks(doc, s.Blocks);
                        }
                        break;
                    case Table t:
                        {

                        }
                        break;
                    case BlockUIContainer c:
                        {

                        }
                        break;
                    case List l:
                        {
                        //    var numbering = doc.CreateNumbering();
                        //    foreach (var item in l.ListItems)
                        //    {

                        //        XWPFParagraph para = doc.CreateParagraph();
                        //        var id = Guid.NewGuid().ToString();
                        //        numbering.AddNum(id);
                        //        para.SetNumID(id);
                        //        if (item.Blocks.Count == 0)
                        //        {
                        //            continue;
                        //        }
                        //        else if (item.Blocks.Count == 1)
                        //        {
                        //            if (item.Blocks.FirstBlock is Paragraph p)
                        //            {
                        //                AddInlines(para, p.Inlines);
                        //            }
                        //            else
                        //            {

                        //            }
                        //        }
                        //        else
                        //        {

                        //        }
                        //    }
                        //}
                        break;

                    default:
                        break;
                }
            }

        }
        private static void Rtf2Word(XWPFDocument doc, FlowDocument rtf)
        {
            AddBlocks(doc, rtf.Blocks);
        }
        public static async Task ExportWordAsync(string path, int range, (int? Year, int? Month, int? Day) date)
        {
            string dayTitle = await TemplateService.GetDayTitleAsync();
            string monthTitle = await TemplateService.GetMonthTitleAsync();
            string yearTitle = await TemplateService.GetYearTitleAsync();

            XWPFDocument doc = new XWPFDocument();
            var rtf = new FlowDocument();
            rtf.GetAllRange().LoadRtf(await RecordService.GetRichTextAsync(date.Year.Value, date.Month.Value, date.Day.Value));

            Rtf2Word(doc, rtf);
            using var file = File.OpenWrite(path);
            doc.Write(file);
            file.Close();
        }
        public static async Task ExportRtfAsync(string path, int range, (int? Year, int? Month, int? Day) date)
        {
            string dayTitle = await TemplateService.GetDayTitleAsync();
            string monthTitle = await TemplateService.GetMonthTitleAsync();
            string yearTitle = await TemplateService.GetYearTitleAsync();
            FlowDocument doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph());
            switch (range)
            {
                case 0://当日
                    var rtf = await RecordService.GetRichTextAsync(date.Year.Value, date.Month.Value, date.Day.Value);
                    doc.GetAllRange().LoadRtf(rtf);
                    break;
                case 1://当月
                    {
                        var records = await RecordService.GetRecordsAsync(date.Year, date.Month);
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
                        var records = await RecordService.GetRecordsAsync(date.Year);
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
                        var records = await RecordService.GetRecordsAsync();
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
