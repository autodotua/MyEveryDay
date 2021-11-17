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
            return new TextRange(doc.ContentEnd, doc.ContentEnd);
        }

        public static void LoadRtf(this TextRange range,string rtf)
        {
            range.Load(rtf.ToStream(), DataFormats.Rtf);
        }

    }
}
