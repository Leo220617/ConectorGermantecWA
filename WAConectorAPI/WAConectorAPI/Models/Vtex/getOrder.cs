using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Vtex
{
    public class getOrder
    {
        public string orderId { get; set; }
        public DateTime creationDate { get; set; }
        public string currencyCode { get; set; }
        public string clientName { get; set; }
        public int totalItems { get; set; }
        public string hostname { get; set; }
        
    }

    public class ListaOrdenes
    {
        public getOrder[] list { get; set; }
    }


    public class detOrder
    {
        public string orderId { get; set; }
        public double value { get; set; }
        public totals[] totals { get; set; }
        public items[] items { get; set; }
        public shippingData shippingData { get; set; }
        public clientProfileData clientProfileData { get; set; }
        public customData customData { get; set; }
        public paymentData paymentData { get; set; }
    }

    public class paymentData
    {
        public transactions[] transactions { get; set; }
    }

    public class transactions
    {
        public payments[] payments { get; set; }
    }

    public class payments
    {
        public string lastDigits { get; set; }
        public connectorResponses connectorResponses { get; set; }
    }
    public class connectorResponses
    {
        public string nsu { get; set; }
    }

    public class totals
    {
        public string id { get; set; }
        public string name { get; set; }
        public double value { get; set; }
    }
    public class items {

        public string productId { get; set; }
        public string refId { get; set; }
        public int quantity { get; set; }
        public double costPrice { get; set; }
        public double price { get; set; }
        public priceTags[] priceTags { get; set; }
        public double tax { get; set; }
      

    }

    public class priceTags
    {
        public int value { get; set; }
        public decimal rawValue { get; set; }
    }

    public class shippingData
    {
        public address address { get; set; }
    }

    public class address
    {
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string street { get; set; }
        public string complement { get; set; }
    }

    public class clientProfileData
    {
        public string email { get; set; }
        public string phone { get; set; }
        public string userProfileId { get; set; }

    }

    public class customData
    {
        public List<customApps> customApps { get; set; }
    }

    public class customApps
    {
        public fields fields { get; set; }
        public string id { get; set; }
    }

    public class fields
    {
        public string documentNew { get; set; }
    }

}