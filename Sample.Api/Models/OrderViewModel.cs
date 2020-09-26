using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.Api.Models
{
    public class OrderViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string CustomerNumber { get; set; }

        [Required]
        public string PaymentCardNumber { get; set; }

        public string Notes { get; set; }
    }
}
