﻿using System;
using System.Linq;
using System.Web.Mvc;

namespace MrRondon.Infra.CrossCutting.Helper.Buttons
{
    public class ButtonsMessage : ButtonsBase
    {
        public string ToPagination(Guid id, string[] permissions)
        {
            return permissions.Any(x => x == Constants.Roles.GeneralAdministrator) ? $"{Details(id)}" : "";
        }

        private MvcHtmlString Details(Guid id)
        {
            try
            {
                var link = new TagBuilder("a");
                var iconEdit = new TagBuilder("i");
                iconEdit.MergeAttribute("class", IconDetails);
                link.MergeAttribute("href", Url.Action("Details", "Message", new { area = "Admin", id }));
                link.MergeAttribute("class", "ui tiny icon button");
                link.MergeAttribute("data-loading", "btn");
                link.MergeAttribute("title", "Detalhes");
                link.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(null));
                link.InnerHtml = iconEdit.ToString();

                return MvcHtmlString.Create(link.ToString());
            }
            catch
            {
                return MvcHtmlString.Empty;
            }
        }

        private MvcHtmlString Edit(Guid id)
        {
            try
            {
                var link = new TagBuilder("a");
                var iconEdit = new TagBuilder("i");
                iconEdit.MergeAttribute("class", IconEdit);
                link.MergeAttribute("href", Url.Action("Edit", "Message", new { area = "Admin", id }));
                link.MergeAttribute("class", "ui tiny icon button");
                link.MergeAttribute("data-loading", "btn");
                link.MergeAttribute("title", "Editar");
                link.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(null));
                link.InnerHtml = iconEdit.ToString();

                return MvcHtmlString.Create(link.ToString());
            }
            catch
            {
                return MvcHtmlString.Empty;
            }
        }
    }
}