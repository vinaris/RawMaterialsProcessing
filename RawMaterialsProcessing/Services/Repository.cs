using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RawMaterialsProcessing.Data;

namespace RawMaterialsProcessing.Services
{
    public class Repository : IRepository
    {
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Nomenclature>> GetAllNomenclature()
        {
            return await _context.Nomenclature.OrderBy(i => i.Id).ToListAsync();
        }

        public async Task<bool> AddNewNomenclature(Nomenclature nomenclature)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.Nomenclature.AddAsync(nomenclature);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> AddRangeNomenclature(ICollection<Nomenclature> nomenclature)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.Nomenclature.AddRangeAsync(nomenclature);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }


        public async Task<ICollection<Party>> GetAllParties()
        {
            return await _context.Parties.Include(p => p.Nomenclature).ToListAsync();
        }

        public async Task<bool> AddNewParty(Party party)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.Parties.AddAsync(party);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> AddRangeParty(ICollection<Party> parties)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.Parties.AddRangeAsync(parties);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }



        public async Task<ICollection<MachineTool>> GetAllMachineTools()
        {
            return await _context.MachineTools.OrderBy(i => i.Id).ToListAsync();
        }

        public async Task<bool> AddNewMachineTool(MachineTool machineTool)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.MachineTools.AddAsync(machineTool);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> AddRangeMachineTool(ICollection<MachineTool> machineTools)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.MachineTools.AddRangeAsync(machineTools);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }


        public async Task<ICollection<Operation>> GetAllOperations()
        {
            return await _context.Operations.Include(m => m.MachineTool).Include(n => n.Nomenclature).ToListAsync();
        }

        public async Task<bool> AddNewOperation(Operation operation)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try{

                    await _context.Operations.AddAsync(operation);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> AddRangeOperation(ICollection<Operation> operations)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.Operations.AddRangeAsync(operations);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }


        public async Task<bool> RemoveAllData()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Operations.RemoveRange(_context.Operations);
                    _context.Parties.RemoveRange(_context.Parties);
                    _context.Nomenclature.RemoveRange(_context.Nomenclature);
                    _context.MachineTools.RemoveRange(_context.MachineTools);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

        }
        public async Task<bool> RemoveAllNomenclature()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Nomenclature.RemoveRange(_context.Nomenclature);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

        }
        public async Task<bool> RemoveAllParties()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Parties.RemoveRange(_context.Parties);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

        }
        public async Task<bool> RemoveAllMachineTools()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.MachineTools.RemoveRange(_context.MachineTools);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

        }

        public async Task<bool> RemoveAllOperations()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Operations.RemoveRange(_context.Operations);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

        }

    }
}
