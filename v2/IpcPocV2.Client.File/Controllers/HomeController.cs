using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IpcPocV2.Client.File.Models;

namespace IpcPocV2.Client.File.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Download(string Id)
        {

            if (Id == null)
                return Content("filename not present");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", Id);

            if (System.IO.File.Exists(path))
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory)
                                .ConfigureAwait(false);
                }
                memory.Position = 0;
                return File(memory, "application/octet-stream", Path.GetFileName(path));
            }

            return View("FileNotFound", Id);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
