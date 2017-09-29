using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    public class Customer
    {
        public string pnm_customer_identifier { get; set; }
        public string site_customer_identifier { get; set; }
        public string pnm_customer_email { get; set; }
        public string pnm_customer_phone { get; set; }
        public string pnm_customer_addressee { get; set; }
        public string pnm_customer_postal_code { get; set; }
    }

    public class User
    {
        public string user_type { get; set; }
        public string user_site_identifier { get; set; }
        public string pnm_user_identifier { get; set; }
        public string user_status { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string full_name { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string year_of_birth { get; set; }
        public string pre_verified { get; set; }
        public string manually_verified { get; set; }
    }

    public class Users
    {
        public List<User> user { get; set; }
    }

    public class Location
    {
        public string retailer { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string distance { get; set; }
        public string phone { get; set; }
        public string pnm_retailer_identifier { get; set; }
    }

    public class Locations
    {
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string map_url { get; set; }
        public List<Location> location { get; set; }
    }

    public class Fees
    {
        public string order_has_pos_fee { get; set; }
        public List<object> tiers { get; set; }
    }

    public class Slip
    {
        public string full_encoding { get; set; }
        public string slip_url { get; set; }
        public string slip_pdf_url { get; set; }
        public string slip_id { get; set; }
        public string retailer_name { get; set; }
    }

    public class Retailer
    {
        public string retailer_name { get; set; }
        public string retailer_identifier { get; set; }
        public string retailer_logo { get; set; }
        public string retailer_mapkey { get; set; }
        public string retailer_icon { get; set; }
        public string retailer_nickname { get; set; }
        public string retailer_mobile_barcode { get; set; }
        public Fees fees { get; set; }
        public Slip slip { get; set; }
    }

    public class Order
    {
        public string site_name { get; set; }
        public string site_logo_url { get; set; }
        public string site_order_identifier { get; set; }
        public string site_identifier { get; set; }
        public string require_auth_tracker { get; set; }
        public string pnm_order_crid { get; set; }
        public string pnm_customer_language { get; set; }
        public string pnm_order_identifier { get; set; }
        public string pnm_order_short_identifier { get; set; }
        public string site_order_key { get; set; }
        public string order_created { get; set; }
        public string order_status { get; set; }
        public string order_amount { get; set; }
        public string order_currency { get; set; }
        public string minimum_payment_amount { get; set; }
        public string minimum_payment_currency { get; set; }
        public string order_type { get; set; }
        public string order_is_standing { get; set; }
        public string pnm_balance_due_amount { get; set; }
        public string pnm_balance_due_currency { get; set; }
        public string site_order_description { get; set; }
        public string order_tracking_url { get; set; }
        public object cards { get; set; }
        public Customer customer { get; set; }
        public Users users { get; set; }
        public Locations locations { get; set; }
        public List<Retailer> retailers { get; set; }
    }

    public class CreateOrderResponse
    {
        public string status { get; set; }
        public Order order { get; set; }

        public List<Error> errors { get; set; }
    }

    public class Error
    {

        public Error(string desc) 
        {
            this.description = desc;
        }

        public Error() { }
        public string description { get; set; }
    }
}