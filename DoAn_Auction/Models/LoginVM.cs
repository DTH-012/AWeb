﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_Auction.Models
{
    public class LoginVM
    {
        public string Username { get; set; }
        public string RawPWD { get; set; }
        public bool Remember { get; set; }
    }
}