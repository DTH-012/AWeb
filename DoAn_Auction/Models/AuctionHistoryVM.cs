using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn_Auction.Models
{
    public class AuctionHistoryVM
    {
        public Nullable<int> ProID { get; set; }
        public Nullable<int> UserID { get; set; }
        public string UserName { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<System.DateTime> Time { get; set; }
    }
}