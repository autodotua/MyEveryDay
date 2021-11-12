using System.ComponentModel.DataAnnotations;

namespace MyEveryDay.Model
{
    public abstract class ModelBase
    {
        [Key]
        public int Id { get; set; }

        public bool IsDeleted { get; set; }
    }
}