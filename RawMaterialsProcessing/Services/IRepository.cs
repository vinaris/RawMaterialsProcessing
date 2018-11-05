using RawMaterialsProcessing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RawMaterialsProcessing.Models;

namespace RawMaterialsProcessing.Services
{
    public interface IRepository
    {
        Task<ICollection<Nomenclature>> GetAllNomenclature();
        Task<ICollection<Party>> GetAllParties();
        Task<ICollection<MachineTool>> GetAllMachineTools();
        Task<ICollection<Operation>> GetAllOperations();

        Task<bool> AddNewNomenclature(Nomenclature nomenclature);
        Task<bool> AddNewParty(Party party);
        Task<bool> AddNewOperation(Operation operation);
        Task<bool> AddNewMachineTool(MachineTool machineTool);


        Task<bool> AddRangeNomenclature(ICollection<Nomenclature> nomenclature);
        Task<bool> AddRangeParty(ICollection<Party> party);
        Task<bool> AddRangeOperation(ICollection<Operation> operation);
        Task<bool> AddRangeMachineTool(ICollection<MachineTool> machineTool);

        Task<bool> RemoveAllData();
        Task<bool> RemoveAllNomenclature();
        Task<bool> RemoveAllParties();
        Task<bool> RemoveAllMachineTools();
        Task<bool> RemoveAllOperations();



    }
}
