﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fastdo.Repositories.Models;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Fastdo.backendsys.Models;
using Fastdo.backendsys.Repositories;
using Fastdo.backendsys.Services;
using Fastdo.backendsys.Services.Auth;

namespace Fastdo.backendsys.Controllers.Auth
{
    [Route("api/ph/signup")]
    [ApiController]
    [AllowAnonymous]
    public class ParmacySignUpController : SharedAPIController
    {
        #region constructor and properties
        private HandlingProofImgsServices _handlingProofImgsServices { get; }
        private IPharmacyRepository _pharmacyRepository { get; }
        public IExecuterDelayer _executerDelayer { get; }

        public ParmacySignUpController(
             UserManager<AppUser> userManager,
             IEmailSender emailSender,
             AccountService accountService,
             IMapper mapper,
             HandlingProofImgsServices handlingProofImgsServices,
             IPharmacyRepository pharmacyRepository,
             IExecuterDelayer executerDelayer,
             TransactionService transactionService)
             : base(userManager, emailSender, accountService, mapper, transactionService)
        {
            _handlingProofImgsServices = handlingProofImgsServices;
            _pharmacyRepository = pharmacyRepository;
            _executerDelayer = executerDelayer;
        }

        #endregion

        #region main signup

        [HttpPost]
        public async Task<IActionResult> SignUp([FromForm]PharmacyClientRegisterModel model)
        {
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            SigningPharmacyClientInResponseModel response = null;
            try
            {
                var pharmacyModel = _mapper.Map<Pharmacy>(model);
                _transactionService.Begin();
                response = await _accountService.SignUpPharmacyAsync(model, _executerDelayer) as SigningPharmacyClientInResponseModel;
                if (response == null)
                {
                    _transactionService.RollBackChanges().End();
                    return BadRequest(Functions.MakeError("لقد فشلت عملية التسجيل,حاول مرة اخرى"));
                }
                pharmacyModel.Id = response.user.Id;
                var savingImgsResponse = _handlingProofImgsServices
                    .SavePharmacyProofImgs(model.LicenseImg, model.CommerialRegImg, pharmacyModel.Id);
                if (!savingImgsResponse.Status)
                {
                    _transactionService.RollBackChanges().End();
                    return BadRequest(Functions.MakeError($"{savingImgsResponse.errorMess}"));
                }
                pharmacyModel.LicenseImgSrc = savingImgsResponse.LicenseImgPath;
                pharmacyModel.CommercialRegImgSrc = savingImgsResponse.CommertialRegImgPath;
                var res = await _pharmacyRepository.AddAsync(pharmacyModel);
                if (!res)
                {
                    _transactionService.RollBackChanges().End();
                    return BadRequest(Functions.MakeError("لقد فشلت عملية التسجيل,حاول مرة اخرى"));
                }
                _executerDelayer.Execute();
                _transactionService.CommitChanges().End();

            }
            catch (Exception ex)
            {
                _transactionService.RollBackChanges().End();
                return BadRequest(new { err = ex.Message });
            }
            return Ok(response);
        }

        #endregion

        #region signup steps
        [HttpPost("step1")]
        public IActionResult SignUp_Step1(Phr_RegisterModel_Identity model)
        {
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            return Ok();
        }
        [HttpPost("step2")]
        //[Consumes("")]
        public IActionResult SignUp_Step2([FromForm]Phr_RegisterModel_Proof model)
        {
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            return Ok();
        }
        [HttpPost("step3")]
        public IActionResult SignUp_Step3(Phr_RegisterModel_Contacts model)
        {
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            return Ok();
        }
        [HttpPost("step4")]
        public IActionResult SignUp_Step4(Phr_RegisterModel_Account model)
        {
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            return Ok();
        }

        #endregion

    }
}