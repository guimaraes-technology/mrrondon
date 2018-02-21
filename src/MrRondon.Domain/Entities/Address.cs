﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MrRondon.Infra.CrossCutting.Message;

namespace MrRondon.Domain.Entities
{
    public class Address
    {
        [Key]
        public Guid AddressId { get; set; } = Guid.NewGuid();

        [Display(Name = "Rua/Avenida")]
        [Required(ErrorMessageResourceType = typeof(Error), ErrorMessageResourceName = "Required")]
        [MinLength(5, ErrorMessage = "Mínimo {0} caracteres")]
        [MaxLength(60, ErrorMessage = "Máximo {0} caracteres")]
        public string Street { get; set; }

        [Display(Name = "Bairro")]
        [Required(ErrorMessageResourceType = typeof(Error), ErrorMessageResourceName = "Required")]
        [MinLength(4, ErrorMessage = "Mínimo {0} caracteres")]
        [MaxLength(30, ErrorMessage = "Máximo {0} caracteres")]
        public string Neighborhood { get; set; }

        [Display(Name = "Número")]
        [Required(ErrorMessageResourceType = typeof(Error), ErrorMessageResourceName = "Required")]
        [MinLength(2, ErrorMessage = "Mínimo {0} caracteres")]
        [MaxLength(6, ErrorMessage = "Máximo {0} caracteres")]
        public string Number { get; set; }

        [DisplayName("Complemento")]
        public string AdditionalInformation { get; set; }

        [Display(Name = "CEP")]
        [Required(ErrorMessageResourceType = typeof(Error), ErrorMessageResourceName = "Required")]
        [MaxLength(10, ErrorMessage = "Máximo {0} caracteres")]
        public string ZipCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        //[NotMapped]
        //[Display(Name = "Latitude")]
        //public string LatitudeString
        //{
        //    get => Math.Abs(Latitude) < 1 ? string.Empty : Latitude.ToString().Replace(",", ".");
        //    set => LatitudeString = value;
        //}

        //[NotMapped]
        //[Display(Name = "Longitude")]
        //public string LongitudeString
        //{
        //    get => Math.Abs(Longitude) < 1 ? string.Empty : Longitude.ToString().Replace(",", ".");
        //    set => LongitudeString = value;
        //}

        [DisplayName("Cidade")]
        public int CityId { get; set; }
        public City City { get; set; }

        public void UpdateAddress(Address newAddress)
        {
            Street = newAddress.Street;
            Neighborhood = newAddress.Neighborhood;
            Number = newAddress.Number;
            AdditionalInformation = newAddress.AdditionalInformation;
            ZipCode = newAddress.ZipCode;
            Latitude = newAddress.Latitude;
            Longitude = newAddress.Longitude;
            CityId = newAddress.CityId;
        }

        public Address SetCoordinates(string latitude, string longitude)
        {
            if (double.TryParse(latitude.Replace(".", ","), out var latitudeResult))
                Latitude = latitudeResult;
            else throw new Exception("Latitude em formato inválido");

            if (double.TryParse(longitude.Replace(".", ","), out var longitudeResult))
                Longitude = longitudeResult;
            else throw new Exception("Longitude em formato inválido");

            return this;
        }
    }
}