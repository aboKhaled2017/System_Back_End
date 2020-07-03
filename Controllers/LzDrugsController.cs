﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Models;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_Back_End.Models;
using System_Back_End.Repositories;
using System_Back_End.Services;
using System_Back_End.Services.Auth;

namespace System_Back_End.Controllers
{
    [Route("api/lzdrugs")]
    [ApiController]
    [Authorize(Policy = "PharmacyPolicy")]
    public class LzDrugsController : SharedAPIController
    {
        public LzDrugRepository _lzDrugsRepository { get; }

        public LzDrugsController(
            UserManager<AppUser> userManager, IEmailSender emailSender,
            AccountService accountService, IMapper mapper,
            LzDrugRepository lzDrugsRepository,
            TransactionService transactionService) 
            : base(userManager, emailSender, accountService, mapper, transactionService)
        {
            _lzDrugsRepository = lzDrugsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDrugsOfMy()
        {
            var allDrugs = await _lzDrugsRepository.GetAllShownOfUser(GetUserId()).ToListAsync();
            return Ok(allDrugs);
        }

        // GET: api/LzDrugs/5
        [HttpGet("{id}", Name = "GetDrugById")]
        public async Task<IActionResult> Get(Guid id)
        {
            var drug = await _lzDrugsRepository.GetShownModelByIdAsync(id);
            if (drug == null)
                return NotFound();
            return Ok(drug);
        }

        // POST: api/LzDrugs
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddLzDrugModel drugModel)
        {
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            var drug = _mapper.Map<LzDrug>(drugModel);
            _lzDrugsRepository.Add(drug);
            if (!await _lzDrugsRepository.SaveAsync())
                return StatusCode(500, Functions.MakeError("حدثت مشكلة اثناء معالجة طلبك"));
            return CreatedAtRoute(
                routeName: "GetDrugById",
                routeValues: new { id = drug.Id}, 
                _mapper.Map<ShowLzDrugModel>(drug));
        }

        // PUT: api/LzDrugs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateLzDrugModel drugModel)
        {
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            if (id != drugModel.Id)
                return BadRequest();
            if (!await _lzDrugsRepository.IsUserHas(id))
                return NotFound();
            var drug = _mapper.Map<LzDrug>(drugModel);
            _lzDrugsRepository.Update(drug);
            if (!await _lzDrugsRepository.SaveAsync())
                return StatusCode(500, Functions.MakeError("حدثت مشكلة اثناء معالجة طلبك"));
            return NoContent();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _lzDrugsRepository.IsUserHas(id))
                return NotFound();
            var drugToDelete =await _lzDrugsRepository.GetByIdAsync(id);
            _lzDrugsRepository.Delete(drugToDelete);
            if (!await _lzDrugsRepository.SaveAsync())
                return StatusCode(500, Functions.MakeError("حدثت مشكلة اثناء معالجة طلبك"));
            return NoContent();
        }
    }
}