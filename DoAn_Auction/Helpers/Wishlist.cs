using DoAn_Auction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn_Auction.Helpers
{
    public class Wishlist
    {
        public List<WishItem> Items { get; set; }
        public Wishlist()
        {
            this.Items = new List<WishItem>();
        }
        public void AddItem(WishItem item)
        {
            this.Items.Add(item);
        }
    }
    public class WishItem
    {
        public Auction Pro { get; set; }
    }
}