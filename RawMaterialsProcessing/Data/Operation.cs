using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RawMaterialsProcessing.Data
{
    public class Operation
    {
        public string MachineToolId { get; set; }
        [Display(Name = "Оборудование")]
        public MachineTool MachineTool { get; set; }
        public string NomenclatureId { get; set; }
        [Display(Name = "Номенклатура")]
        public Nomenclature Nomenclature { get; set; }
        [Display(Name = "Продолжительность")]
        public int Duration { get; set; }
    }
}
