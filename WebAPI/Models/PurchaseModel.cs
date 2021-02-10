using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class PurchaseModel
    {
       // [Required]
        public int IdUser { get; set; }

        //[Required]
        public decimal ValueARG { get; set; }

       // [Required]
        public string IsoCode { get; set; }
    }
}
