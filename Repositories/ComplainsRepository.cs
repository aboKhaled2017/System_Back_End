﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Models;
using System.Threading.Tasks;

namespace System_Back_End.Repositories
{
    public class ComplainsRepository : MainRepository, IComplainsRepository
    {
        public ComplainsRepository(SysDbContext context) : base(context)
        {
        }

        public async Task<bool> Add(Complain complain)
        {
            _context.Complains.Add(complain);
            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<bool> ComplainExists(string id)
        {
            return await _context.Complains.AnyAsync(e => e.Id == id);
        }

        public async Task<Complain> Delete(string id)
        {
            var complain = await _context.Complains.FindAsync(id);
            if (complain!=null)
            {
                _context.Complains.Remove(complain);
                await _context.SaveChangesAsync();
            }
            return complain;
        }

        public IQueryable<Complain> GetAll()
        {
            return _context.Complains.AsQueryable();
        }

        public async Task<Complain> GetById(string id)
        {
            return await _context.Complains.FindAsync(id);
        }

        public async Task<bool> Update(Complain complain)
        {
            _context.Entry(complain).State = EntityState.Modified;
            return (await _context.SaveChangesAsync()) >0;
        }
    }
}
