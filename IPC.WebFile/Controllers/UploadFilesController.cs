using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IPC.Common.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace IPC.WebFile.Controllers
{

    [Controller]
    public class UploadFilesController : Controller
    {
        const int BusinessServerPort = 8801;
        private IHostingEnvironment _env;
        public UploadFilesController(IHostingEnvironment env)
        {
            _env = env;
        }


        public IActionResult Index()
        {
            return View();
        }

        #region snippet1
        [HttpPost("UploadFiles")]

        public async Task<IActionResult> Post(IEnumerable<IFormFile> files)
        {

            List<IFormFile> flist = new List<IFormFile>();

            List<Item> result = new List<Item>();

            if (files != null && files.Count() > 0)
            {
                foreach (var file in files)
                    flist.Add(file);

            }
            else if (HttpContext.Request.Form.Files.Count > 0)
            {
                foreach(var file in HttpContext.Request.Form.Files)
                {
                    flist.Add(file);
                }
            }


            foreach (var formFile in flist)
            {
                string filePath = _env.ContentRootPath + "\\Files\\" + formFile.FileName;

                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                        result.Add(new Item() { Id = formFile.FileName, Status = "OK" });
                    }
                }
            }

            return Json(result);
        }

    }
    #endregion

    public class Item
    {
        public string Id { get; set; }
        public string Status { get; set; }
    }
}


