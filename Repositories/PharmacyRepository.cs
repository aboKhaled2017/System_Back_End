﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Fastdo.Repositories.Models;
using System.Threading.Tasks;
using Fastdo.backendsys.Models;

namespace Fastdo.backendsys.Repositories
{
    public class PharmacyRepository:MainRepository,IPharmacyRepository
    {
        public PharmacyRepository(SysDbContext context) : base(context)
        {
        }
        public IQueryable<Pharmacy> GetAllAsync()
        {
            return _context.Pharmacies;
        }
        public async Task<PagedList<Get_PageOf_Pharmacies_ADMModel>> Get_PageOf_PharmacyModels_ADM(PharmaciesResourceParameters _params)
        {

            var sourceData = _context.Pharmacies
            .OrderBy(d => d.Name)
            .Select(p => new Get_PageOf_Pharmacies_ADMModel
            {
                Id = p.Id,
                MgrName = p.MgrName,
                Email=p.User.Email,
                Name = p.Name,
                OwnerName = p.OwnerName,
                PersPhone = p.PersPhone,
                LandlinePhone = p.LandlinePhone,
                LicenseImgSrc = p.LicenseImgSrc,
                CommercialRegImgSrc = p.CommercialRegImgSrc,
                Status = p.Status,
                Address = $"{p.Area.SuperArea.Name}/{p.Area.Name}",
                AreaId = p.AreaId,
                joinedStocksCount = p.GoinedStocks.Count,
                lzDrugsCount = p.LzDrugs.Count,
                requestedDrugsCount = p.RequestedLzDrugs.Count
            });
            if (!string.IsNullOrEmpty(_params.S))
            {
                var searchQueryForWhereClause = _params.S.Trim().ToLowerInvariant();
                sourceData = sourceData
                     .Where(d => d.Name.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }
            if (_params.Status != null)
            {
                sourceData = sourceData
                     .Where(p=>p.Status==_params.Status);
            }
            return await PagedList<Get_PageOf_Pharmacies_ADMModel>.CreateAsync(sourceData, _params);
        }
        public async Task<Pharmacy> GetByIdAsync(string id)
        {
            return await _context.Pharmacies.FindAsync(id);
        }
        public async Task<Get_PageOf_Pharmacies_ADMModel> Get_PharmacyModel_ADM(string id)
        {
            return await _context.Pharmacies
                .Where(p=>p.Id==id)
                .Select(p => new Get_PageOf_Pharmacies_ADMModel
            {
                Id=p.Id,
                Email=p.User.Email,
                MgrName=p.MgrName,
                Name=p.Name,
                OwnerName=p.OwnerName,
                PersPhone=p.PersPhone,
                LandlinePhone=p.LandlinePhone,
                LicenseImgSrc=p.LicenseImgSrc,
                CommercialRegImgSrc=p.CommercialRegImgSrc,
                Status=p.Status,
                Address = $"{p.Area.SuperArea.Name}/{p.Area.Name}",
                AreaId =p.AreaId,
                joinedStocksCount=p.GoinedStocks.Count,
                lzDrugsCount=p.LzDrugs.Count,
                requestedDrugsCount=p.RequestedLzDrugs.Count               
                })
               .SingleOrDefaultAsync();
        }
        //public async Task<>
        public async Task<bool> AddAsync(Pharmacy pharmacy)
        {
            _context.Pharmacies.Add(pharmacy);
            return (await _context.SaveChangesAsync()) > 0;
        }        
        public async Task<bool> UpdateAsync(Pharmacy pharmacy)
        {
            _context.Entry(pharmacy).State = EntityState.Modified;
            return (await _context.SaveChangesAsync()) > 0;
        }       
        public async Task Delete(Pharmacy pharm)
        {                   
            _context.LzDrugRequests.RemoveRange(
                _context.LzDrugRequests.Where(r => pharm.Id == r.PharmacyId || pharm.Id == r.LzDrug.PharmacyId)
            );
            await _context.SaveChangesAsync();            
            _context.Users.Remove(await _context.Users.FindAsync(pharm.Id));
        }
        public void UpdatePhone(Pharmacy pharmacy)
        {
            UpdateFields<Pharmacy>(pharmacy, prop => prop.PersPhone);
        }
        public void UpdateName(Pharmacy pharmacy)
        {
            UpdateFields<Pharmacy>(pharmacy, prop => prop.Name);
        }
        public void UpdateContacts(Pharmacy pharmacy)
        {
            UpdateFields<Pharmacy>(pharmacy,
                prop => prop.LandlinePhone,
                prop => prop.Address);
        }
        public async Task<bool> Patch_Apdate_ByAdmin(Pharmacy pharm)
        {
            return await UpdateFieldsAsync_And_Save<Pharmacy>(pharm,prop => prop.Status);
        }
        public async Task<Pharmacy> Get_IfExists(string id)
        {
            return await _context.Pharmacies.FindAsync(id);
        }
    }
}
