using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace MyEveryDay
{
    public static class RtfExtension
    {
        public static MemoryStream ToStream(this string str)
        {
            var bytes = Encoding.Default.GetBytes(str);
            return new MemoryStream(bytes);
        }
        public static TextRange GetAllRange(this FlowDocument doc)
        {
            return new TextRange(doc.ContentStart, doc.ContentEnd);
        } 
        public static TextRange GetTailRange(this FlowDocument doc)
        {
            if(doc.Blocks.LastBlock is  List)
            {
                doc.Blocks.Add(new Paragraph());
            }
            return new TextRange(doc.ContentEnd, doc.ContentEnd);
        }

        public static TextRange LoadRtf(this TextRange range,string rtf)
        {
            if (string.IsNullOrEmpty(rtf))
            {
                range.Text = "";
            }
            else
            {
                range.Load(rtf.ToStream(), DataFormats.Rtf);
            }
            return range;
        }    
        public static string GetRtf(this TextRange range)
        {
            MemoryStream ms = new MemoryStream();
            range.Save(ms, DataFormats.Rtf);
            ms.Close();
            return Encoding.Default.GetString(ms.ToArray());
        }

    }
}
