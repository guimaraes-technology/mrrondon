﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MrRondon.Presentation.Mvc.Extensions;

namespace MrRondon.Presentation.Mvc.Areas.Admin.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ActionResult Code(int id)
        {
            switch (id)
            {
                case 404: return View("Erro404");
                default: return View("Generic");
            }
        }

        public ICollection<string> Get(ModelStateDictionary modelState)
        {
            return modelState.Values.Where(x => x.Errors.Any()).SelectMany(s => s.Errors).Select(item => $"- {item.ErrorMessage}").ToList();
        }

        public class Error
        {
            public ICollection<string> Errors { get; set; } = new List<string>();
        }
    }
}