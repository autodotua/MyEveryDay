namespace MyEveryDay.Model
{
    public class Template : ModelBase
    {
        public TemplateType Type { get; set; }
        public string Name { get; set; }
        public string RichText { get; set; }
    }
}