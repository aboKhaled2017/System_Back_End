﻿using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System_Back_End.Models;
using AutoMapper;

namespace System_Back_End.Services.Auth
{
    public class AccountService
    {
        private readonly JWThandlerService _jWThandlerService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<AppUser> _userManager;
        private readonly SysDbContext _context;
        private readonly IMapper _mapper;

        private HttpContext _httpContext { get; set; }
        private IUrlHelper _Url { get; set; }
        private readonly IConfigurationSection _JWT = RequestStaticServices.GetConfiguration.GetSection("JWT");

        public AccountService(
            IEmailSender emailSender,
            JWThandlerService jWThandlerService,
            UserManager<AppUser> userManager,
            IMapper mapper,
            SysDbContext context)
        {
            _jWThandlerService = jWThandlerService;
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
            _mapper = mapper;
        }
        public void SetCurrentContext(HttpContext httpContext, IUrlHelper url)
        {
            _httpContext = httpContext;
            _Url = url;
        }
        public ISigningResponseModel GetSigningInResponseModelForPharmacy(AppUser user, Pharmacy pharmacy)
        {
                return new SigningPharmacyClientInResponseModel
                {
                    user = _mapper.MergeInto<PharmacyClientResponseModel>(user, pharmacy),
                    accessToken = _jWThandlerService.CreateAccessToken(user,Variables.pharmacier)
                };   
                      
        }
        public ISigningResponseModel GetSigningInResponseModelForStock(AppUser user, Stock stock)
        {
            return new SigningStockClientInResponseModel
            {
                user = _mapper.MergeInto<StockClientResponseModel>(user, stock),
                accessToken = _jWThandlerService.CreateAccessToken(user, Variables.stocker)
            };
        }

        public async Task<ISigningResponseModel> GetSigningInResponseModelForCurrentUser(AppUser user)
        {
            var userType = Properties.CurrentUserType;
            if (userType == UserType.pharmacier)
            {
                var pharmacy = await _context.Pharmacies.FindAsync(user.Id);
                return GetSigningInResponseModelForPharmacy(user, pharmacy);
            }
            else
            {
                var stock = await _context.Stocks.FindAsync(user.Id);
                return GetSigningInResponseModelForStock(user, stock);
            }
        }
        public async Task<ISigningResponseModel> GetSigningInResponseModelForPharmacy(AppUser user)
        {
            var pharmacy =await _context.Pharmacies.FindAsync(user.Id);
            return GetSigningInResponseModelForPharmacy(user, pharmacy);
        }

        public async Task<ISigningResponseModel> GetSigningInResponseModelForStock(AppUser user)
        {
            var stock = await _context.Stocks.FindAsync(user.Id);
            return GetSigningInResponseModelForStock(user, stock);
        }

        public async Task<ISigningResponseModel> SignUpPharmacyAsync(PharmacyClientRegisterModel model, Action<bool, IdentityResult,AppUser> ActionOnResult)
        {
            //the email is already checked at validation if it was existed before for any user
            var user = new AppUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PresPhone };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, Variables.pharmacier);
                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = _Url.EmailConfirmationLink(user.Id.ToString(), code, _httpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(model.Email,"تأكيد البريد الالكترونى لفاست دو", $"لتأكيد بريدك الالكترونى <a href='{callbackUrl}'>اضغط هنا</a>");
                    ActionOnResult(false, result,user);
                    var pharmacy = _mapper.Map<Pharmacy>(model);
                    pharmacy.Id = user.Id;
                    return GetSigningInResponseModelForPharmacy(user,pharmacy);
                }
            }
            ActionOnResult(true, result,user);
            return null;
        }
        public async Task<ISigningResponseModel> SignUpStockAsync(StockClientRegisterModel model, Action<bool, IdentityResult, AppUser> ActionOnResult)
        {
            //the email is already checked at validation if it was existed before for any user
            var user = new AppUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PresPhone };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, Variables.pharmacier);
                if (result.Succeeded)
                {
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = _Url.EmailConfirmationLink(user.Id.ToString(), code, _httpContext.Request.Scheme);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = System.Web.HttpUtility.UrlEncode(code);
                    await _emailSender.SendEmailAsync(model.Email, "get code to confirm your email", $"the code to confirm email is {code}");
                    //await RegisterCustomerUser(user, model);
                    ActionOnResult(false, result, user);
                    var stock = _mapper.Map<Stock>(model);
                    stock.Id = user.Id;
                    return GetSigningInResponseModelForStock(user,stock);
                }
            }
            ActionOnResult(true, result, user);
            return null;
        }

        public async Task SendEmailConfirmationAsync(string email, string callbackUrl)
        {
            var body = new StringBuilder();
            body.Append($"<a href='${callbackUrl}'>confirm your email</a>");
            await _emailSender.SendEmailAsync(email, "confirm your email", body.ToString());
        }
    }
}
