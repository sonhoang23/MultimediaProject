using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultimediaProject.Models;
using MultimediaProject.Services.LZW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MultimediaProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILZWService _iLZWService;

        public HomeController(ILogger<HomeController> logger, ILZWService iLZWService)
        {
            _logger = logger;
            _iLZWService = iLZWService;
        }
        [HttpGet]
        public ActionResult Home()
        {
            return View();
        }
        [HttpGet]
        public IActionResult LZW()
        {
            ViewData["Title"] = "LZW";
            return View();
        }
        [HttpPost]
        public IActionResult EncodeLZW(IFormFile input)
        {
            if (input != null)
            {
                List<String> encodeLZW = _iLZWService.EncodeLZW(input);
                ViewData["outputEncodeLZW"] = encodeLZW[0];
                ViewData["inputEncodeLZW"] = encodeLZW[1];
                String path = @"C:\Users\son\Desktop\multil\FileDe.txt";


                using (StreamWriter writetext = new StreamWriter(path))
                {
                    writetext.WriteLine(encodeLZW[0]);
                }
                return View("LZW");
            }
            else
            {
                return View("LZW");
            }
        }
        [HttpPost]
        public IActionResult DecryptLZW(IFormFile input)
        {
            if (input != null)
            {
                List<String> decryptLZW = _iLZWService.DecryptLZW(input);
                ViewData["outputDecryptLZW"] = decryptLZW[0];
                ViewData["inputDecryptLZW"] = decryptLZW[1];
                return View("LZW");
            }
            else
            {
                return View("LZW");
            }
        }
    }
}
