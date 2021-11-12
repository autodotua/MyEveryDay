using System.ComponentModel.DataAnnotations;

namespace MyEveryDay.Model
{
    public class Record : ModelBase
    {
        [Range(2000, 9999)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        [Range(1, 31)]
        public int Day { get; set; }

        public string RichText { get; set; }
        public string PlainText { get; set; }
    }
}