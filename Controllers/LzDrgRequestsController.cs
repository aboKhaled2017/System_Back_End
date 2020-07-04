﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Models;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_Back_End.Models;
using System_Back_End.Repositories;
using System_Back_End.Services.Auth;

namespace System_Back_End.Controllers
{
    [Route("api/phrdrgrequests")]
    [ApiController]
    [Authorize(Policy = "PharmacyPolicy")]
    public class LzDrgRequestsController : SharedAPIController
    {
        public ILzDrgRequestsRepository _lzDrgRequestsRepository { get; private set; }

        public LzDrgRequestsController(
            AccountService accountService, IMapper mapper,
            ILzDrgRequestsRepository lzDrgRequestsRepository,
            UserManager<AppUser> userManager)
            : base(accountService, mapper, userManager)
        {
            _lzDrgRequestsRepository = lzDrgRequestsRepository;
        }

        #region Get List Of LzDrug Requests
        [HttpGet("made", Name = "Get_AllRequests_I_Made")]
        public async Task<IActionResult> Get_AllRequests_I_Made([FromQuery]LzDrgReqResourceParameters _params)
        {
            var requests = await _lzDrgRequestsRepository.Get_AllRequests_I_Made(_params);
            var paginationMetaData = new PaginationMetaDataGenerator<Made_LzDrgRequest_MB, LzDrgReqResourceParameters>(
                requests, "Get_AllRequests_I_Made", _params, Create_BMs_ResourceUri
                ).Generate();
            Response.Headers.Add(Variables.X_PaginationHeader, paginationMetaData);
            return Ok(requests);
        }
        [HttpGet("received",Name = "Get_AllRequests_I_Received")]
        public async Task<IActionResult> Get_AllRequests_I_Received([FromQuery]LzDrgReqResourceParameters _params)
        {
            var requests = await _lzDrgRequestsRepository.Get_AllRequests_I_Received(_params);
            var paginationMetaData = new PaginationMetaDataGenerator<Sent_LzDrgRequest_MB, LzDrgReqResourceParameters>(
                requests, "Get_AllRequests_I_Received", _params, Create_BMs_ResourceUri
                ).Generate();
            Response.Headers.Add(Variables.X_PaginationHeader, paginationMetaData);
            return Ok(requests);
        }
        [HttpGet("received/ns", Name = "Get_AllRequests_I_Received_INS")]
        public async Task<IActionResult> Get_NotSeen_AllRequestes_I_Received([FromQuery]LzDrgReqResourceParameters _params)
        {
            var requests = await _lzDrgRequestsRepository.Get_AllRequests_I_Received_INS(_params);
            var paginationMetaData = new PaginationMetaDataGenerator<NotSeen_PhDrgRequest_MB, LzDrgReqResourceParameters>(
                requests, "Get_AllRequests_I_Received_INS", _params, Create_BMs_ResourceUri
                ).Generate();
            Response.Headers.Add(Variables.X_PaginationHeader, paginationMetaData);
            return Ok(requests);
        }
        #endregion


        #region Get Single LzDrug Request
        [HttpGet("{id}", Name = "GetRequestById")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var req = await _lzDrgRequestsRepository.GetByIdAsync(id);
            if (req == null)
                return NotFound();
            return Ok(req);
        }
        #endregion

        #region (Add/handle/cancel) Single LzDrug Request
        [HttpPost("{drugId}")]
        public async Task<IActionResult> Post_NewRequest(Guid drugId)
        {
            var req = await _lzDrgRequestsRepository.AddForUserAsync(drugId);
            if (req == null)
                return NotFound();
            if (!await _lzDrgRequestsRepository.SaveAsync())
                return StatusCode(500, Functions.MakeError("حدثت مشكلة اثناء معالجة طلبك ,من فضلك حاول مرة اخرى"));
            return CreatedAtRoute(routeName: "GetRequestById", routeValues: new { id = req.Id }, req);
        }
        [HttpPatch("received/{reqId}")]
        public async Task<IActionResult> Patch_HandleRequest_I_Received(Guid reqId,[FromBody] JsonPatchDocument<LzDrgRequest_ForUpdate_BM> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();
            var req = await _lzDrgRequestsRepository.Get_Request_I_Received_IfExistsForUser(reqId);
            if (req == null)
                return NotFound();
            var requestToPatch = _mapper.Map<LzDrgRequest_ForUpdate_BM>(req);
            patchDoc.ApplyTo(requestToPatch);
            //ad validation
            _mapper.Map(requestToPatch, req);
            var isSuccessfulluUpdated = await _lzDrgRequestsRepository.Patch_Update_Request_Sync(req);
            if (!isSuccessfulluUpdated)
                return StatusCode(500, Functions.MakeError("لقد حدثت مشكلة اثناء معالجة طلبك , من فضلك حاول مرة اخرى"));
            return NoContent();
        }

        [HttpDelete("made/{reqId}")]
        public async Task<IActionResult> Cancel_Request_I_Made([FromRoute]Guid reqId)
        {
            var req = await _lzDrgRequestsRepository.Get_Request_I_Made_IfExistsForUser(reqId);
            if (req == null)
                return NotFound();
            _lzDrgRequestsRepository.Delete(req);
            if (!await _lzDrgRequestsRepository.SaveAsync())
                return StatusCode(500, Functions.MakeError("حدثت مشكلة اثناء معالجة طلبك ,من فضلك حاول مرة اخرى"));
            return NoContent();

        }
        #endregion

        #region (handle/cancel) List Of LzDrug Requests

        [HttpDelete("Made/all")]
        public async Task<IActionResult> Delete_AllRequests_I_Made()
        {
            _lzDrgRequestsRepository.Delete_AllRequests_I_Made();
            if (!await _lzDrgRequestsRepository.SaveAsync())
                return StatusCode(500, Functions.MakeError("لقد حدثت مشكلة اثناء معالجة طلبك , من فضلك حاول مرة اخرى"));
            return NoContent();
        }

        [HttpDelete("Made")]
        public async Task<IActionResult> Delete_AllRequests_I_Made([FromBody] IEnumerable<Guid>Ids)
        {
            if (!await _lzDrgRequestsRepository.User_Made_These_Requests(Ids))
                return NotFound();
            _lzDrgRequestsRepository.Delete_SomeRequests_I_Made(Ids);
            if (!await _lzDrgRequestsRepository.SaveAsync())
                return StatusCode(500, Functions.MakeError("لقد حدثت مشكلة اثناء معالجة طلبك , من فضلك حاول مرة اخرى"));
            return NoContent();
        }

        [HttpPatch("received/({ids})")]
        public async Task<IActionResult> Patch_HandleSomeRequests_I_Received(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid>Ids, 
            [FromBody] JsonPatchDocument<LzDrgRequest_ForUpdate_BM> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();
            if (!await _lzDrgRequestsRepository.User_Received_These_Requests(Ids))
                return NotFound();
            var reqs = await _lzDrgRequestsRepository.Get_Group_Of_Requests_I_Received(Ids);
            if (reqs.Count()==0)
                return NotFound();
            reqs.ToList().ForEach(req =>
            {
                var requestToPatch = _mapper.Map<LzDrgRequest_ForUpdate_BM>(req);
                patchDoc.ApplyTo(requestToPatch);
                //ad validation
                _mapper.Map(requestToPatch, req);

            });
            _lzDrgRequestsRepository.Patch_Update_Group_Of_Requests_Sync(reqs);
            if (!await _lzDrgRequestsRepository.SaveAsync())
                return StatusCode(500, Functions.MakeError("لقد حدثت مشكلة اثناء معالجة طلبك , من فضلك حاول مرة اخرى"));
            return NoContent();
        }
        #endregion


    }
}
