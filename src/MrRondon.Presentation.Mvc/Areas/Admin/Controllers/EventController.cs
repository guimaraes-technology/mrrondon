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
using MrRondon.Infra.Security.Extensions;
using MrRondon.Infra.Security.Helpers;
using MrRondon.Presentation.Mvc.Extensions;
using MrRondon.Presentation.Mvc.ViewModels;

namespace MrRondon.Presentation.Mvc.Areas.Admin.Controllers
{
    [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator, Constants.Roles.ReadOnly)]
    public class EventController : Controller
    {
        private readonly MainContext _db = new MainContext();

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator, Constants.Roles.ReadOnly)]
        public ActionResult Index()
        {
            return View();
        }

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator, Constants.Roles.ReadOnly)]
        public ActionResult Create()
        {
            SetBiewBags(null);
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator)]
        public ActionResult Create(CrudEventVm model, AddressForEventVm address)
        {
            try
            {
                model.Address = address;
                model.Event.Address = model.GetAddress(address);
                model.Event.Address.SetCoordinates(address.LatitudeString, address.LongitudeString);

                if (model.Event.Logo == null || model.LogoFile != null)
                    model.Event.Logo = FileUpload.GetBytes(model.LogoFile, "Logo");
                if (model.Event.Cover == null || model.CoverFile != null)
                    model.Event.Cover = FileUpload.GetBytes(model.CoverFile, "Capa");

                ModelState.Remove("Event.Logo");
                ModelState.Remove("Event.Cover");
                if (!ModelState.IsValid)
                {
                    SetBiewBags(model);
                    return View(model).Error(ModelState);
                }

                _db.Events.Add(model.Event);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                SetBiewBags(model);
                return View(model).Error(ex.Message);
            }
        }

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator, Constants.Roles.ReadOnly)]
        public ActionResult Details(Guid id)
        {
            var repo = new RepositoryBase<Event>(_db);
            var entity = repo.GetItemByExpression(x => x.EventId == id, x => x.Address.City, i => i.Organizer);
            if (entity == null) return HttpNotFound();
            return View(entity);
        }

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator)]
        public ActionResult Edit(Guid id)
        {
            var repo = new RepositoryBase<Event>(_db);
            var entity = repo.GetItemByExpression(x => x.EventId == id, x => x.Address, x => x.Organizer.Address);
            if (entity == null) return HttpNotFound();

            var model = GetCrudVm(entity);
            SetBiewBags(model);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator)]
        public ActionResult Edit(CrudEventVm model, AddressForEventVm address)
        {
            try
            {
                model.Address = address;
                model.Event.Address = model.GetAddress(address);
                model.Event.Address.SetCoordinates(address.LatitudeString, address.LongitudeString);

                if (model.Event.Logo == null || model.LogoFile != null)
                    model.Event.Logo = FileUpload.GetBytes(model.LogoFile, "Logo");

                if (model.Event.Cover == null || model.CoverFile != null)
                    model.Event.Cover = FileUpload.GetBytes(model.CoverFile, "Capa");

                ModelState.Remove("Event.Logo");
                ModelState.Remove("Event.Cover");
                if (!ModelState.IsValid)
                {
                    SetBiewBags(model);
                    return View(model).Error(ModelState);
                }

                var oldCompany = _db.Events
                    .Include(c => c.Address)
                    //.Include(c => c.Contacts)
                    .FirstOrDefault(x => x.EventId == model.Event.EventId);
                if (oldCompany == null) return RedirectToAction("Index").Success("Empresa atualizada com sucesso");

                // Update parent
                _db.Entry(oldCompany).CurrentValues.SetValues(model.Event);
                oldCompany.Address.UpdateAddress(model.Event.Address);
                _db.Entry(oldCompany.Address).State = EntityState.Modified;
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
        public ActionResult Contacts(CrudEventVm eventContact)
        {
            UrlsContact();
            eventContact = eventContact ?? new CrudEventVm();
            eventContact.Contacts = eventContact.Contacts ?? new List<Contact>();
            return PartialView("_Contacts", eventContact.Contacts);
        }

        [AllowAnonymous, HttpPost]
        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator)]
        public ActionResult AddContact(CrudEventVm eventContact)
        {
            eventContact.Contacts = eventContact.Contacts ?? new List<Contact>();
            eventContact.Contacts.Add(new Contact { Description = eventContact.Description, ContactType = eventContact.ContactType });
            eventContact.Description = string.Empty;
            eventContact.ContactType = 0;
            UrlsContact();
            return PartialView("_Contacts", eventContact.Contacts);
        }

        [AllowAnonymous, HttpPost]
        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator)]
        public ActionResult RemoveContact(CrudEventVm eventContact, int index)
        {
            UrlsContact();
            eventContact.Contacts?.RemoveAt(index);
            return PartialView("_Contacts", eventContact.Contacts);
        }

        public void UrlsContact()
        {
            ViewBag.UrlAdd = Url.Action("AddContact", "Event", new { area = "Admin" });
            ViewBag.UrlRemove = Url.Action("RemoveContact", "Event", new { area = "Admin" });
        }

        private static CrudEventVm GetCrudVm(Event model)
        {
            var eventVm = new CrudEventVm
            {
                Event = model,
                Address = AddressForEventVm.GetAddress(model.Address)
            };

            return eventVm;
        }

        private void SetBiewBags(CrudEventVm model)
        {
            string cityId;
            if (model?.Event == null) cityId = string.Empty;
            else cityId = model.Event.SameAddressAsOganizer ? model.Event?.Organizer?.Address?.CityId.ToString() ?? model.Event?.Address?.CityId.ToString() : model.Event?.Address?.CityId.ToString();

            var cities = _db.Cities.OrderBy(o => o.Name);
            ViewBag.Cities = new SelectList(cities, "CityId", "Name", cityId);
            ViewBag.Companies = new SelectList(_db.Companies, "CompanyId", "Name", model?.Event?.OrganizerId);
        }

        public ActionResult GetOrganizerAddress(Guid companyId)
        {
            var company = _db.Companies.Include(i => i.Address).FirstOrDefault(f => f.CompanyId == companyId);
            if (company?.Address == null) return PartialView("_FormAddress");

            var cities = _db.Cities.OrderBy(o => o.Name);

            ViewBag.Cities = new SelectList(cities, "CityId", "Name", company.Address?.CityId);
            company.Address.SetCoordinates();

            return PartialView("_Address", AddressForEventVm.GetAddress(company.Address));
        }

        [HttpPost]
        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.EventAdministrator, Constants.Roles.ReadOnly)]
        public JsonResult GetPagination(DataTableParameters parameters)
        {
            var search = parameters.Search.Value?.ToLower() ?? string.Empty;
            var repo = new RepositoryBase<Event>(_db);
            var items = repo.GetItemsByExpression(w => w.Name.Contains(search), x => x.Name, parameters.Start, parameters.Length, out var recordsTotal, i => i.Address.City)
                .Select(s =>
                    new
                    {
                        s.EventId,
                        s.Name,
                        s.StartDate,
                        s.EndDate,
                        s.Address.City
                    }).ToList();
            var dtResult = new DataTableResultSet(parameters.Draw, recordsTotal);

            var buttons = new ButtonsEvent();
            foreach (var item in items)
            {
                dtResult.data.Add(new object[]
                {
                    item.Name,
                    $"{(item.StartDate == item.EndDate ? item.StartDate.ToString("dd/MM/yyyy HH:mm") : $"{item.StartDate:dd/MM/yyyy HH:mm} - {item.EndDate:dd/MM/yyyy HH:mm}")}",
                    item.City.Name,
                    buttons.ToPagination(item.EventId, Account.Current.Roles)
                });
            }
            return Json(dtResult, JsonRequestBehavior.AllowGet);
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