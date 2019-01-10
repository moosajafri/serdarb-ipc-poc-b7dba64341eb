using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

using IpcPocV2.Client.Web.Models;
using IpcPocV2.Common.Helpers;
using IpcPocV2.Common.InterProcessCommunication;
using IpcPocV2.Common.InterProcessCommunication.Clients;
using IpcPocV2.Common.Models;
using IpcPocV2.Common.Models.Command;
using ContentDispositionHeaderValue = System.Net.Http.Headers.ContentDispositionHeaderValue;

namespace IpcPocV2.Client.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _cacheHostName;
        private readonly string _businessHostName;
        string FileDownloadURL;
        string FileUploadURL;
        private int LoginCustomerId = 1;

        public HomeController(IHostNameValues values)
        {
            _cacheHostName = values.CacheHostName;
            _businessHostName = values.BusinessHostName;
            FileDownloadURL = $"http://{values.FileWebApiHostName}:{Ports.FileClientPort}/Home/Download/";
            FileUploadURL = $"http://{values.FileWebApiHostName}:{Ports.FileClientPort}/UploadFiles";
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

        [HttpPost]
        public async Task<JsonResult> GetCustomer()
        {
            JsonResult result;
            using (var cacheClient = new CacheClient(_cacheHostName, Ports.CacheServerPort))
            {
                cacheClient.Connect();

                var randomCustomerId = new Random().Next(1, 30);
                var response = await cacheClient.GetCustomerById(randomCustomerId).ConfigureAwait(false);

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
            using (var client = new BusinessServerClient(_businessHostName, Ports.BusinessServerPort))
            {
                client.Connect();
                var response = await client.GetCustomerFiles(1).ConfigureAwait(false);
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
            var ResponseList = new List<CacheResponseBase>();
            using (var businessServerClient = new BusinessServerClient(_businessHostName, Ports.BusinessServerPort))
            {
                businessServerClient.Connect();

                using (HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    foreach (var formFile in files)
                    {
                        if (formFile.Length > 0)
                        {
                            var response = await SaveFile(businessServerClient, httpClient, formFile).ConfigureAwait(false);
                            ResponseList.Add(response);
                        }
                    }
                }
            }
            return View("PostStatus", ResponseList);
        }

        private async Task<CacheResponseBase> SaveFile(BusinessServerClient client, HttpClient httpClient, IFormFile formFile)
        {
            var file = new File();
            file.FileName = formFile.FileName;

            file.Guid = StringHelpers.GetNewUid();
            file.Path = file.Guid;
            file.CreatedAt = DateTime.Now;
            file.CustomerId = LoginCustomerId;

            string webServerResponse = "";
            var response = new CacheResponseBase();
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
                using (var message = await httpClient.PostAsync(FileUploadURL, form).ConfigureAwait(false))
                {
                    webServerResponse = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }

            if (webServerResponse.Contains("OK"))
            {
                response = await client.Add(file).ConfigureAwait(false);
            }
            else
            {
                response.ErrorDescription = webServerResponse;
                response.IsOK = false;
            }


            return response;
        }
    }
}
