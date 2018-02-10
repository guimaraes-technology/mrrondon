﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MrRondon.Domain.Entities;
using MrRondon.Infra.CrossCutting.Helper;
using MrRondon.Infra.CrossCutting.Helper.Buttons;
using MrRondon.Infra.Data.Context;
using MrRondon.Infra.Data.Repositories;
using MrRondon.Presentation.Mvc.Extensions;
using MrRondon.Presentation.Mvc.ViewModels;

namespace MrRondon.Presentation.Mvc.Areas.Admin.Controllers
{
    public class CompanyController : Controller
    {
        private readonly MainContext _db = new MainContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(Guid id)
        {
            var repo = new RepositoryBase<Company>(_db);
            var company = repo.GetItemByExpression(x => x.CompanyId == id);
            if (company == null) return HttpNotFound();
            var model = GetCrudVm(company);

            return View(model);
        }

        public ActionResult Create()
        {
            SetBiewBags(null);
            return View();
        }

        [HttpPost]
        public ActionResult Create(CrudCompanyVm model, Address address)
        {
            try
            {
                if (model.SubCategoryId.HasValue && model.SubCategoryId != 0)
                    model.Company.SubCategoryId = model.SubCategoryId.Value;
                else model.Company.SubCategoryId = model.CategoryId;

                model.Company.Address = address;
                model.Company.Contacts = model.Contacts;
                ModelState.Remove(nameof(model.Company.Logo));
                ModelState.Remove(nameof(model.Company.Cover));

                if (!ModelState.IsValid)
                {
                    SetBiewBags(model);
                    return View(model);
                }

                _db.Companies.Add(model.Company);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                SetBiewBags(model);
                return View(model).Error(ex.Message);
            }
        }

        public ActionResult Edit(Guid id)
        {
            var repo = new RepositoryBase<Company>(_db);
            var company = repo.GetItemByExpression(x => x.CompanyId == id, "Address", "SubCategory");
            if (company == null) return HttpNotFound();

            var model = GetCrudVm(company);
            SetBiewBags(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Company company)
        {
            var model = new CrudCompanyVm { Company = company };
            try
            {
                ModelState.Remove(nameof(company.SubCategoryId));
                if (!ModelState.IsValid)
                {
                    SetBiewBags(model);
                    return View(model);
                }

                _db.Entry(company).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                SetBiewBags(model);

                return View(model).Error(ex.Message);
            }
        }

        [AllowAnonymous]
        public ActionResult Contacts(CrudCompanyVm companyContact)
        {
            UrlsContact();
            companyContact = companyContact ?? new CrudCompanyVm();
            companyContact.Contacts = companyContact.Contacts ?? new List<Contact>();
            return PartialView("_Contacts", companyContact.Contacts);
        }

        [AllowAnonymous, HttpPost]
        public ActionResult AddContact(CrudCompanyVm companyContact)
        {
            companyContact.Contacts = companyContact.Contacts ?? new List<Contact>();
            companyContact.Contacts.Add(new Contact { Description = companyContact.Description, ContactType = companyContact.ContactType });
            companyContact.Description = string.Empty;
            companyContact.ContactType = 0;
            UrlsContact();
            return PartialView("_Contacts", companyContact.Contacts);
        }

        [AllowAnonymous, HttpPost]
        public ActionResult RemoveContact(CrudCompanyVm companyContact, int index)
        {
            UrlsContact();
            companyContact.Contacts?.RemoveAt(index);
            return PartialView("_Contacts", companyContact.Contacts);
        }

        [HttpPost]
        public JsonResult GetPagination(DataTableParameters parameters)
        {
            var search = parameters.Search.Value?.ToLower() ?? string.Empty;
            var repo = new RepositoryBase<Company>(_db);
            var items = repo.GetItemsByExpression(w => w.Name.Contains(search), x => x.Name, parameters.Start, parameters.Length, out var recordsTotal).ToList();
            var dtResult = new DataTableResultSet(parameters.Draw, recordsTotal);

            var buttons = new ButtonsCompany();
            foreach (var item in items)
            {
                dtResult.data.Add(new[]
                {
                    item.CompanyId.ToString(),
                    $"{item.Name}",
                    buttons.ToPagination(item.CompanyId)
                });
            }
            return Json(dtResult, JsonRequestBehavior.AllowGet);
        }

        public void UrlsContact()
        {
            ViewBag.UrlAdd = Url.Action("AddContact", "Company", new { area = "Admin" });
            ViewBag.UrlRemove = Url.Action("RemoveContact", "Company", new { area = "Admin" });
        }

        private static CrudCompanyVm GetCrudVm(Company company)
        {
            var model = new CrudCompanyVm { Company = company };
            if (company.SubCategory?.CategoryId != null)
            {
                model.SubCategoryId = company.SubCategoryId;
                model.CategoryId = company.SubCategory.CategoryId.Value;
            }
            else model.CategoryId = company.SubCategoryId;

            return model;
        }

        private void SetBiewBags(CrudCompanyVm model)
        {
            ViewBag.Cities = new SelectList(_db.Cities, "CityId", "Name", model?.Company.Address.CityId);

            var categories = _db.SubCategories.Where(s => s.CategoryId == null).OrderBy(o => o.Name);
            ViewBag.Categories = new SelectList(categories, "SubCategoryId", "Name", model?.CategoryId);

            if (model == null || model.CategoryId == 0) ViewBag.SubCategories = new SelectList(Enumerable.Empty<SelectListItem>());
            else
            {
                var subCategories = _db.SubCategories.Where(s => s.CategoryId != null).OrderBy(o => o.Name);
                ViewBag.SubCategories = new SelectList(subCategories, "SubCategoryId", "Name", model.SubCategoryId);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}