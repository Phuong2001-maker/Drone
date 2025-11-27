using PT.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PT.Domain.Model
{

    public enum ContactLogType
    {
        Contact,
        Booking,
        FormTestimonials
    }

    public class ContactLog : IAggregateRoot
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string IP { get; set; }
        public int Count { get; set; }
        public ContactLogType Type { get; set; }
        public DateTime LastConnection { get; set; }
        public bool IsBanlist { get; set; }
    }
}
