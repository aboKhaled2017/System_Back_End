﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Fastdo.backendsys.Areas.AdminPanel.Controllers
{
    public class HomeController : MainController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}