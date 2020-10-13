﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fastdo.Repositories.Models;
using System.Threading.Tasks;

namespace Fastdo.backendsys.Repositories
{
   public interface IAreaRepository:IRepository
    {
        IQueryable<Area> GetAll();
        Task<Area> GetById(byte id);
        Task<bool> Add(Area area);
        Task<Area> Delete(byte id);
        Task<bool> AreaExists(byte id);
    }
}
