using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Required")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Required(ErrorMessage ="Required")]        
        [Range(1,int.MaxValue,ErrorMessage ="Display order should be grater than 0")]
        public int DisplayOrder { get; set; }
    }
}
