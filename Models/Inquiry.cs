﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebServer.Models
{
    public class Inquiry
    {
        [Key]
        public Guid InquiryId { get; set; }
        public string Question { get; set; }
        public string Response { get; set; }
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
