using FzLib.DataStorage.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEveryDay.WPF
{
    public class Config : IJsonSerializable, INotifyPropertyChanged
    {
        private static string path= Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
            FzLib.Program.App.ProgramName, "config.json");
        private static Config instance;

        public event PropertyChangedEventHandler PropertyChanged;

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Config();
                    try
                    {
                        instance.TryLoadFromJsonFile(path);
                    }
                    catch (Exception ex)
                    {
                        instance.LoadError = ex;
                    }
                }
                return instance;
            }
        }

        public Exception LoadError { get; private set; }
        public string YearTitle { get; set; }
        = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Georgia;}{\f3\fcharset0 Microsoft YaHei;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs48\f3\cf0 \cf0\qj{\f3 {\lang2052\ltrch %Year%\u24180?}\li0\ri0\sa200\sb0\fi0\ql\par}
}
}";
        public string MonthTitle { get; set; }
        = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Georgia;}{\f3\fcharset0 Microsoft YaHei;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs40\f3\cf0 \cf0\qj{\f3 {\lang2052\ltrch %Month%\u26376?}\li0\ri0\sa200\sb0\fi0\ql\par}
}
}";
        public string DayTitle { get; set; }
        = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Georgia;}{\f3\fcharset0 Microsoft YaHei;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs27\f3\cf0 \cf0\qj{\f3 {\lang2052\ltrch %Day%\u26085?}\li0\ri0\sa200\sb0\fi0\ql\par}
}
}";
    }
}
