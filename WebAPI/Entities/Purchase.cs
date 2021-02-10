using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Entities
{
    public class Purchase
    {
        [Key]
        public int IdPurchase { get; set; }

        public int IdUser { get; set; }

        public decimal Value { get; set; }

        public string IsoCode { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
