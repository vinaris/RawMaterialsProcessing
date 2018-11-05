using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RawMaterialsProcessing.Data;

namespace RawMaterialsProcessing.Models
{
    public class Work
    {
        [Display(Name = "Оборудование")]
        public MachineTool MachineTool { get; set; }
        [Display(Name = "Список партий")]
        public List<PartyProcessing> Parties { get; set; }
        [Display(Name = "Общее время")]
        public int TotalTime { get; set; }
        public Work()
        {
            Parties = new List<PartyProcessing>();
        }
    }
}
