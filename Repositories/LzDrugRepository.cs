﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Fastdo.Repositories.Models;
using System.Threading.Tasks;
using Fastdo.backendsys.Models;
using Fastdo.backendsys.Services;

namespace Fastdo.backendsys.Repositories
{
    public class LzDrugRepository:MainRepository,ILzDrugRepository
    {
        public LzDrugRepository(SysDbContext context) : base(context)
        {
        }

        public async Task<PagedList<Show_VStock_LzDrg_ADM_Model>> GET_PageOf_VStock_LzDrgs(LzDrgResourceParameters _params)
        {
            var sourceData = _context.LzDrugs
                        .OrderBy(d => d.Name)
                        .GroupBy(d => d.Name)
                        .Select(drgGroup => new Show_VStock_LzDrg_ADM_Model { 
                         Name=drgGroup.First().Name,
                         Products=drgGroup.Select(d=>new VStock_LzDrg_For_Pharmacy_ADM_Model { 
                            DrugId=d.Id,
                            ConsumeType=d.ConsumeType,
                            Discount=d.Discount,
                            PharmacyId=d.PharmacyId,
                            PharmacyName=d.Pharmacy.Name,
                            Price=d.Price,
                            PriceType=d.PriceType,
                            Quantity=d.Quantity,
                            Type=d.Type,
                            UnitType=d.UnitType,
                            ValideDate=d.ValideDate
                         })
                        });
            return await PagedList<Show_VStock_LzDrg_ADM_Model>.CreateAsync(sourceData, _params);
        }
        public async Task<PagedList<LzDrugModel_BM>> GetAll_BM(LzDrgResourceParameters _params)
        {

            var sourceData=_context.LzDrugs
            .Where(d => d.PharmacyId == UserId)
            .OrderBy(d=>d.Name)
            .Select(d => new LzDrugModel_BM
            {
                Id = d.Id,
                Name = d.Name,
                Price = d.Price,
                PriceType = d.PriceType,
                Quantity = d.Quantity,
                Type = d.Type,
                UnitType = d.UnitType,
                ValideDate = d.ValideDate,
                ConsumeType = d.ConsumeType,
                Discount = d.Discount,
                Desc = d.Desc,
                RequestCount=d.RequestingPharms.Count
            });
            return await PagedList<LzDrugModel_BM>.CreateAsync(sourceData, _params);
        }
        public async Task<LzDrugModel_BM> Get_BM_ByIdAsync(Guid id)
        {
            return await _context.LzDrugs
                .Where(d => d.Id == id)
                .Select(d => new LzDrugModel_BM
                {
                    Id = d.Id,
                    Name = d.Name,
                    Price = d.Price,
                    PriceType = d.PriceType,
                    Quantity = d.Quantity,
                    Type = d.Type,
                    UnitType = d.UnitType,
                    ValideDate = d.ValideDate,
                    ConsumeType = d.ConsumeType,
                    Discount = d.Discount,
                    Desc = d.Desc
                }).FirstOrDefaultAsync();
        }
        public async Task<LzDrug> GetByIdAsync(Guid id)
        {
            return await _context.LzDrugs.FindAsync(id);
        }
        public void Add(LzDrug drug)
        {
            drug.PharmacyId = UserId;
            _context.LzDrugs.Add(drug);
        }
        public void Update(LzDrug drug)
        {
            drug.PharmacyId = UserId;
            _context.Entry(drug).State = EntityState.Modified;

        }
        public void Delete(LzDrug drug)
        {
            _context.LzDrugs.Remove(drug);
        }
        public async Task<bool> IsUserHas(Guid id)
        {
            return await _context.LzDrugs.AnyAsync(d => d.Id == id && d.PharmacyId==UserId);
        }
        public async Task<LzDrug> GetIfExists(Guid id)
        {
            return await _context.LzDrugs.FindAsync(id);
        }
        public async Task<bool> LzDrugExists(Guid id)
        {
            return await _context.LzDrugs.AnyAsync(d=>d.Id==id);
        }

        public async Task<Show_LzDrgReqDetails_ADM_Model> GEt_LzDrugDetails_For_ADM(Guid id)
        {
            return await _context.LzDrugs.Where(d => d.Id == id)
                .Select(d=>new Show_LzDrgReqDetails_ADM_Model
                {
                    Id=d.Id,
                    Desc=d.Desc,
                    Discount=d.Discount,
                    Name=d.Name,
                    Price=d.Price,
                    PriceType=d.PriceType,
                    Quantity=d.Quantity,
                    Type=d.Type,
                    UnitType=d.UnitType,
                    ValideDate=d.ValideDate,
                    RequestCount=d.RequestingPharms.Count
                })
                .SingleOrDefaultAsync();
        }

    }
}

