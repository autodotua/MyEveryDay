using FzLib;
using MyEveryDay.Model;
using NPOI.SS.UserModel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Table = System.Windows.Documents.Table;

namespace MyEveryDay.Service
{
    public static class ExportService
    {
        private static void AddUnsupportedMessage(XWPFDocument doc, string type)
        {
            var para = doc.CreateParagraph();
            AddUnsupportedMessage(para, type);
        }
        private static void AddUnsupportedMessage(XWPFParagraph para, string type)
        {
            var run = para.CreateRun();
            run.SetColor(IndexedColors.Red.HexString);
            run.SetText(@"不支持的格式：" + type);
        }
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
                        AddUnsupportedMessage(paragraph, nameof(InlineUIContainer));
                        break;
                    case AnchoredBlock a:
                        AddUnsupportedMessage(paragraph, nameof(AnchoredBlock));
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
                        AddUnsupportedMessage(doc, nameof(Table));
                        break;
                    case BlockUIContainer c:
                        AddUnsupportedMessage(doc, nameof(BlockUIContainer));
                        break;
                    case List l:
                        AddUnsupportedMessage(doc, nameof(List));
                        //{
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
        public static async Task ExportWordAsync(string path, ExportRange range, (int? Year, int? Month, int? Day) date)
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
        public static async Task ExportRtfAsync(string path, ExportRange range, (int? Year, int? Month, int? Day) date, DayStyle dayStyle)
        {
            string dayTitle = await TemplateService.GetDayTitleAsync();
            string monthTitle = await TemplateService.GetMonthTitleAsync();
            string yearTitle = await TemplateService.GetYearTitleAsync();
            FlowDocument doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph());
            switch (range)
            {
                case ExportRange.Day://当日
                    var rtf = await RecordService.GetRichTextAsync(date.Year.Value, date.Month.Value, date.Day.Value);
                    doc.GetAllRange().LoadRtf(rtf);
                    break;
                case ExportRange.Month://当月
                    {
                        await ExportDaysAsync(doc, date.Year.Value, date.Month.Value, dayStyle, dayTitle);
                    }
                    break;
                case ExportRange.Year://当年
                    {
                        var months = await RecordService.GetMonthsAsync(date.Year.Value);
                        foreach (var month in months)
                        {
                            var rM = doc.GetTailRange().LoadRtf(monthTitle);
                            rM.Text = rM.Text.Replace("%Month%", month.ToString());

                            await ExportDaysAsync(doc, date.Year.Value, month, dayStyle, dayTitle);
                        }
                    }
                    break;
                case ExportRange.All://全部
                    {
                        var years = await RecordService.GetYears();
                        foreach (var year in years)
                        {
                            var rY = doc.GetTailRange().LoadRtf(yearTitle);
                            doc.Blocks.Add(new Paragraph());
                            rY.Text = rY.Text.Replace("%Year%", year.ToString());
                            var months = await RecordService.GetMonthsAsync(year);
                            foreach (var month in months)
                            {
                                var rM = doc.GetTailRange().LoadRtf(monthTitle);
                                rM.Text = rM.Text.Replace("%Month%", month.ToString());

                                await ExportDaysAsync(doc, year, month, dayStyle, dayTitle);
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

        private static async Task ExportDaysAsync(FlowDocument doc, int year, int month, DayStyle dayStyle, string dayTitle)
        {
            var records = await RecordService.GetRecordsAsync(year, month);
            switch (dayStyle)
            {
                case DayStyle.List:
                    {
                        List list = new List()
                        {
                            MarkerStyle = TextMarkerStyle.Decimal,//数字序号
                            Margin = new Thickness(0, 0, 0, 0),//文字部分从0缩进开始
                            //FontFamily = new System.Windows.Media.FontFamily("DengXian")
                        };
                        var items = new List<ListItem>();
                        for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++)
                        {
                            var item = new ListItem();
                            items.Add(item);
                            list.ListItems.Add(item);
                        }
                        FlowDocument tempDoc = new FlowDocument();
                        foreach (var record in records)
                        {
                            tempDoc.GetAllRange().LoadRtf(record.RichText);
                            ListItem item = items[record.Day - 1];
                            var para = new Paragraph();
                            foreach (var block in tempDoc.Blocks.ToList())
                            {
                                AddBlock(block);
                                //用软回车代替回车
                                para.Inlines.Add(new LineBreak());
                            }
                            void AddBlock(Block block)
                            {
                                switch (block)
                                {
                                    case Paragraph p:
                                        {
                                            p.Inlines.Cast<Inline>().ToList().ForEach(p => para.Inlines.Add(p));
                                        }
                                        break;
                                    case Section s:
                                        foreach (var b in s.Blocks)
                                        {
                                            AddBlock(b);
                                        }
                                        break;
                                    case BlockUIContainer ui:
                                        para.Inlines.Add(new InlineUIContainer(ui.Child));
                                        break;
                                    default:
                                        para.Inlines.Add(new Run("不支持的格式：" + block.GetType().Name) { Foreground = Brushes.Red });
                                        break;
                                }
                            }
                            item.Blocks.Add(para);
                        }
                        doc.Blocks.Add(list);
                    }
                    break;
                case DayStyle.Title:
                    {
                        foreach (var record in records)
                        {
                            var rD = doc.GetTailRange().LoadRtf(dayTitle);
                            rD.Text = rD.Text.Replace("%Day%", record.Day.ToString());
                            doc.GetTailRange().LoadRtf(record.RichText);
                        }
                    }
                    break;
                default:
                    break;
            }

        }
    }

    public enum DayStyle
    {
        List = 0,
        Title = 1
    }

    public enum ExportRange
    {
        Day = 0,
        Month = 1,
        Year = 2,
        All = 3
    }
}
