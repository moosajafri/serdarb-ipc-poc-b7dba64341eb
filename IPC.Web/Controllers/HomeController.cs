using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IPC.Web.Models;
using Microsoft.AspNetCore.Http;
using IPC.Common.Helpers;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using IPC.Core;

namespace IPC.Web.Controllers
{
    [Controller]
    public class HomeController : Controller
    {
        string FileDownloadURL = "http://{HostNamesAndPorts.FileClientIP}:{HostNamesAndPorts.FileClientIP}/Home/Download/";
        string FileUploadURL = $"http://{HostNamesAndPorts.FileClientIP}:{HostNamesAndPorts.FileClientIP}/UploadFiles";
        private int LoginCustomerId = 1;

        private IHostingEnvironment _env;
        public HomeController(IHostingEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetCustomer()
        {
            JsonResult result;
            using (var cacheClient = new Cache.CacheClient(HostNamesAndPorts.BusinessServerIP, HostNamesAndPorts.BusinessServerPort))
            {
                cacheClient.Connect();

                var randomCustomerId = new Random().Next(1, 30);
                var response = await cacheClient.GetCustomerById(randomCustomerId);

                cacheClient.Close();


                if (response.IsOK)
                {
                    result = new JsonResult(response.CustomerList[0].ToString());
                }
                else
                {
                    result = new JsonResult(new { CacheServerError = response.ErrorDescription });
                }
            }
            return result;
        }

        [HttpPost]
        public JsonResult InsertCustomer()
        {
            var randomCustomerId = new Random().Next(12334, 98334);

            return new JsonResult(new { Id = randomCustomerId });
        }


        [HttpGet("Files")]
        public async Task<IActionResult> GetFiles(int id)
        {
            using (IPC.Cache.BusinessServerClient client = new Cache.BusinessServerClient(HostNamesAndPorts.BusinessServerIP, HostNamesAndPorts.BusinessServerPort))
            {
                client.Connect();
                var response = await client.GetCustomerFiles(1);
                if (response.IsOK)
                {
                    foreach (var file in response.FileList)
                    {
                        file.Path = FileDownloadURL + file.Guid;
                    }


                    return View("Files", response.FileList);
                }
                else
                    return View("Files");
            }

        }

        [HttpGet("UploadFile")]
        public IActionResult UploadFile()
        {
            return View("UploadFile");
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            List<Common.CacheResponseBase> ResponseList = new List<Common.CacheResponseBase>();
            using (IPC.Cache.BusinessServerClient businessServerClient = new Cache.BusinessServerClient(HostNamesAndPorts.BusinessServerIP, HostNamesAndPorts.BusinessServerPort))
            {
                businessServerClient.Connect();

                using (HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    foreach (var formFile in files)
                    {
                        if (formFile.Length > 0)
                        {
                            var response = await SaveFile(businessServerClient, httpClient, formFile);
                            ResponseList.Add(response);
                        }
                    }
                }
            }
            return View("PostStatus", ResponseList);
        }

        private async Task<Common.CacheResponseBase> SaveFile(Cache.BusinessServerClient client, HttpClient httpClient, IFormFile formFile)
        {
            IPC.Common.File file = new Common.File();
            file.FileName = formFile.FileName;

            file.Guid = StringHelpers.GetNewUid();
            file.Path = file.Guid;
            file.CreatedAt = DateTime.Now;
            file.CustomerId = LoginCustomerId;

            string webServerResponse = "";
            Common.CacheResponseBase response = new Common.CacheResponseBase();
            response.IsOK = false;
            

            using (var form = new MultipartFormDataContent())
            {
                form.Headers.Add("UserAgent", "Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome / 70.0.3538.77 Safari / 537.36");

                var streamContent = new StreamContent(formFile.OpenReadStream());

                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = file.Guid
                };

                streamContent.Headers.Add("Content-Type", "application/octet-stream");

                form.Add(streamContent);
                using (var message = await httpClient.PostAsync(FileUploadURL, form))
                {
                    webServerResponse = await message.Content.ReadAsStringAsync();
                }
            }

            if (webServerResponse.Contains("OK"))
            {
                response = await client.Add(file);
            }
            else
            {
                response.ErrorDescription = webServerResponse;
                response.IsOK = false;
            }


            return response;
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

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
