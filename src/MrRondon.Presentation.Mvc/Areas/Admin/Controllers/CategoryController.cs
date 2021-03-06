﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MrRondon.Domain.Entities;
using MrRondon.Infra.CrossCutting.Helper;
using MrRondon.Infra.CrossCutting.Helper.Buttons;
using MrRondon.Infra.Data.Context;
using MrRondon.Infra.Data.Repositories;
using MrRondon.Infra.Security.Extensions;
using MrRondon.Infra.Security.Helpers;
using MrRondon.Presentation.Mvc.Extensions;

namespace MrRondon.Presentation.Mvc.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly MainContext _db = new MainContext();

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.CategoryAdministrator, Constants.Roles.ReadOnly)]
        public ActionResult Index()
        {
            return View();
        }

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.CategoryAdministrator, Constants.Roles.ReadOnly)]
        public ActionResult Details(int id)
        {
            var repo = new RepositoryBase<SubCategory>(_db);
            var category = repo.GetItemByExpression(x => x.SubCategoryId == id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.CategoryAdministrator)]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.CategoryAdministrator)]
        public ActionResult Create(SubCategory model, HttpPostedFileBase imageFile)
        {
            try
            {
                if (imageFile != null) model.SetImage(FileUpload.GetBytes(imageFile, "Imagem"));

                else return View(model).Error("A imagem da categoria é obrigatória");

                if (!ModelState.IsValid) return View(model).Error(ModelState);

                _db.SubCategories.Add(model);
                _db.SaveChanges();
                return RedirectToAction("Index").Success("Operação realizada com sucesso");
            }
            catch (Exception ex)
            {
                return View(model).Error(ex.Message);
            }
        }

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.CategoryAdministrator)]
        public ActionResult Edit(int id)
        {
            var repo = new RepositoryBase<SubCategory>(_db);
            var category = repo.GetItemByExpression(x => x.SubCategoryId == id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        [HttpPost]
        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.CategoryAdministrator)]
        public ActionResult Edit(SubCategory model, HttpPostedFileBase imageFile)
        {
            try
            {
                if (imageFile == null) return View(model).Error("A imagem da categoria é obrigatória");

                if (!ModelState.IsValid) return View(model);

                model.SetImage(FileUpload.GetBytes(imageFile, "Imagem"));
                _db.Entry(model).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index").Success("Operação realizada com sucesso");
            }
            catch (Exception ex)
            {
                return View(model).Error(ex.Message);
            }
        }

        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.CategoryAdministrator)]
        public ActionResult ShowOnApp(int id)
        {
            var repo = new RepositoryBase<SubCategory>(_db);
            var category = repo.GetItemByExpression(x => x.SubCategoryId == id);
            if (category == null) return HttpNotFound();
            _db.Entry(category).Property(p => p.ShowOnApp).CurrentValue = !category.ShowOnApp;
            _db.SaveChanges();

            return RedirectToAction("Index").Success("Operação realizada com sucesso");
        }

        [HttpPost]
        [HasAny(Constants.Roles.GeneralAdministrator, Constants.Roles.CategoryAdministrator, Constants.Roles.ReadOnly)]
        public JsonResult GetPagination(DataTableParameters parameters)
        {
            var search = parameters.Search.Value?.ToLower() ?? string.Empty;
            var repo = new RepositoryBase<SubCategory>(_db);
            var items = repo.GetItemsByExpression(w => w.CategoryId == null && w.Name.Contains(search), x => x.Name, parameters.Start, parameters.Length, out var recordsTotal).ToList();
            var dtResult = new DataTableResultSet(parameters.Draw, recordsTotal);

            var buttons = new ButtonsCategory();
            foreach (var item in items)
            {
                dtResult.data.Add(new object[]
                {
                    item.Name,
                    buttons.ToPagination(item.SubCategoryId, item.ShowOnApp, Account.Current.Roles)
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