//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DoAn_Auction.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AuctionHistory
    {
        public int ID { get; set; }
        public int ProID { get; set; }
        public int UserID { get; set; }
        public decimal Price { get; set; }
        public System.DateTime Time { get; set; }
        public bool Status { get; set; }
        public decimal PriceHighest { get; set; }
    }
}
