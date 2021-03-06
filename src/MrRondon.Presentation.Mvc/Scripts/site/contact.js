﻿$(function () {
    "use strict";

    window.Contact = window.Contact || {}

    function lastContact() {
        $(".description").each(function () {
            var divAllItems = $(this).closest(".all-items");
            var select = divAllItems.find(".contactType").last().find("option:selected");
            Validation.mask($(this), select.val().toLocaleLowerCase());
        });
    }

    $(document.body).on("change", ".contactType",
        function () {
            var select = $(this).find("select");
            var divAllItems = $(this).closest(".all-items");
            var inputDescription = divAllItems.find(".description");
            Validation.mask(inputDescription, select.val().toLocaleLowerCase());
        });

    $(document.body).on("click", ".add-contact",
        function (e) {
            e.preventDefault();
            var url = $(this).attr("href");
            var divAllItems = $(this).closest(".all-items");
            var inputDescription = divAllItems.find(".description");
            if (inputDescription.val()) {
                var form = $(this).closest("form");
                $.ajax({
                    method: "POST",
                    url: url,
                    data: form.serialize(),
                    success: function (data) {
                        $(".list-contact").empty();
                        $(".list-contact").html(data);
                        GT.dropdown(".contactType");
                        lastContact();
                    },
                    error: function (x, y, z) {
                        GT.notification("error", z);
                        console.log(z);
                    }
                });
            }
        });

    $(document.body).on("click", ".remove-contact",
        function (e) {
            e.preventDefault();
            var url = $(this).attr("href");
            var form = $(this).closest("form");
            $("#index").val($(this).data("index"));
            $.ajax({
                method: "POST",
                url: url,
                data: form.serialize(),
                success: function (data) {
                    $(".list-contact").empty();
                    $(".list-contact").html(data);
                    GT.dropdown(".contactType");
                    lastContact();
                }
            });
        });

    lastContact();
});