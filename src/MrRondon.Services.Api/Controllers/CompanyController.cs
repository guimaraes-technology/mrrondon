﻿using MrRondon.Domain.Entities;
using MrRondon.Infra.Data.Context;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace MrRondon.Services.Api.Controllers
{
    [RoutePrefix("v1/company")]
    public class CompanyController : ApiController
    {
        private readonly MainContext _db;

        public CompanyController()
        {
            _db = new MainContext();
        }

        [AllowAnonymous]
        [Route("{id:guid}")]
        [CacheOutput(ClientTimeSpan = 120, ServerTimeSpan = 120)]
        public IHttpActionResult Get(Guid id)
        {
            try
            {
                var item = _db.Companies
                    .Include(i => i.Contacts)
                    .Include(i => i.Address.City)
                    .Include(s => s.SubCategory.Category).AsNoTracking()
                    .FirstOrDefault(f => f.CompanyId == id);

                if (item == null) return NotFound();
                item.SubCategory.Companies = null;

                if (item.SubCategory.Category != null)
                {
                    item.SubCategory.Category.SubCategories = null;
                    item.SubCategory.Category.Image = null;
                }
                item.SubCategory.Image = null;

                var result = new Company
                {
                    CompanyId = item.CompanyId,
                    Name = item.Name,
                    Cover = item.Cover,
                    FancyName = item.FancyName,
                    Cnpj = item.Cnpj,
                    AddressId = item.AddressId,
                    Address = item.Address,
                    SubCategoryId = item.SubCategoryId,
                    SubCategory = item.SubCategory
                };

                if (item.Contacts == null) return Ok(result);

                foreach (var contact in item.Contacts) contact.Company = null;
                result.Contacts = item.Contacts;

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("city/{city:int}/segment/{segmentId:int}/{skip:int}/{take:int}/{name=}")]
        [CacheOutput(ClientTimeSpan = 120, ServerTimeSpan = 120, MustRevalidate = true)]
        public IHttpActionResult Get(int segmentId, int city, string name, int skip, int take)
        {
            try
            {
                name = string.IsNullOrWhiteSpace(name) ? string.Empty : name;

                var items = _db.Companies
                        .Include(i => i.Address)
                        .Where(x => x.SubCategoryId == segmentId && x.Address.CityId == city &&
                                    (x.Name.Contains(name) || x.FancyName.Contains(name)))
                    .OrderBy(x => x.FancyName)
                    .Skip(skip)
                    .Take(take)
                    .AsNoTracking();

                var result = items.Select(s => new
                {
                    s.CompanyId,
                    s.Name,
                    s.Logo,
                    s.Cnpj,
                    s.Address
                }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
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