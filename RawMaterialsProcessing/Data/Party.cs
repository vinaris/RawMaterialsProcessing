using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RawMaterialsProcessing.Data
{
    public class Party
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string NomenclatureId { get; set; }
        [Display(Name = "Номенклатура")]
        public Nomenclature Nomenclature { get; set; }
    }
}
