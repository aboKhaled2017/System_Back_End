﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Models;
using System.Threading.Tasks;
using System_Back_End;
namespace Microsoft.AspNetCore.Identity
{
    public static class IdentityEF
    {
        public static async Task _addRoles(this RoleManager<IdentityRole> _roleManager, List<string> roles)
        {
            foreach (var name in roles)
            {
                if (!await _roleManager.RoleExistsAsync(name))
                {
                    var role = new IdentityRole(name);
                    await _roleManager.CreateAsync(role);
                }
            }
        }
        public async static Task<bool> UserIdentityExists(this UserManager<AppUser> userManager, AppUser user, string password)
        {
            return user != null &&
                await userManager.CheckPasswordAsync(user, password);
        }
        public async static Task<bool> UserIdentityExists(this UserManager<AppUser> userManager, AppUser user, string password,UserType userType)
        {
            string role = userType == UserType.pharmacier
                ? Variables.pharmacier
                : Variables.stocker;
            return user != null &&
                await userManager.IsInRoleAsync(user, role) &&
                await userManager.CheckPasswordAsync(user, password);
        }
       }
}
