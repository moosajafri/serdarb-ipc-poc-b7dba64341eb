using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IPC.WebFile.Controllers
{
    [Controller]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

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
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, "application/octet-stream", Path.GetFileName(path));
            }

            return View("FileNotFound", Id);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
