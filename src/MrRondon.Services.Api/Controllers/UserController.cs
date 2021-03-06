﻿using MrRondon.Domain.Entities;
using MrRondon.Infra.Data.Context;
using MrRondon.Services.Api.Authorization;
using MrRondon.Services.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace MrRondon.Services.Api.Controllers
{
    [RoutePrefix("v1/user")]
    public class UserController : ApiController
    {
        private readonly MainContext _db;

        public UserController()
        {
            _db = new MainContext();
        }

        [Route("information")]
        [CacheOutput(ClientTimeSpan = 120, ServerTimeSpan = 120)]
        public IHttpActionResult GetInformation()
        {
            try
            {
                var user = _db.Users
                    .Include(i => i.Contacts)
                    .FirstOrDefault(f => f.UserId == Authentication.Current.UserId);

                if (user == null) return BadRequest("Usuário não existe");
                if (user.Contacts == null) return Ok(user);

                foreach (var contact in user.Contacts) contact.User = null;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public IHttpActionResult Register([FromBody]RegisterVm register)
        {
            try
            {
                var user = new User
                {
                    FirstName = register.FirstName,
                    LastName = register.LastName,
                    Cpf = register.Cpf,
                    Contacts = new List<Contact>()
                };

                if (!string.IsNullOrWhiteSpace(register.Email))
                    user.Contacts.Add(new Contact
                    {
                        ContactType = ContactType.Email,
                        Description = register.Email,
                        UserId = user.UserId
                    });

                if (!string.IsNullOrWhiteSpace(register.Telephone))
                    user.Contacts.Add(new Contact
                    {
                        ContactType = ContactType.Telephone,
                        Description = register.Telephone,
                        UserId = user.UserId
                    });

                if (!string.IsNullOrWhiteSpace(register.CellPhone))
                    user.Contacts.Add(new Contact
                    {
                        ContactType = ContactType.Cellphone,
                        Description = register.CellPhone,
                        UserId = user.UserId
                    });

                user.SetNewPassword(user.Password);
                _db.Users.Add(user);
                _db.SaveChanges();
                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}