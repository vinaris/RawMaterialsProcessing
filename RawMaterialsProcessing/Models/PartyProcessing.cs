using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RawMaterialsProcessing.Data;

namespace RawMaterialsProcessing.Models
{
    public class PartyProcessing
    {
        [Display(Name = "Партия")]
        public Party Party { get; set; }
        [Display(Name = "Время начала")]
        public int StartTime { get; set; }
        [Display(Name = "Время окончания")]
        public int EndTime { get; set; }
        [Display(Name = "Продолжительность")]
        public int Duration => EndTime - StartTime;
    }
}
