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
   
    }
}
