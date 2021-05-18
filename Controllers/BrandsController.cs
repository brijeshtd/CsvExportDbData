using CsvDlDbData.Repositories;
using CsvDlDbData.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvDlDbData.Controllers
{
    public class BrandsController : Controller
    {
        private readonly BrandsRepository brandsRepository;
        private readonly IDataProtector _protector;

        public BrandsController(IConfiguration configuration, IDataProtectionProvider provider)
        {
            brandsRepository = new BrandsRepository(configuration);
            _protector = provider.CreateProtector("DataProtector");
        }

        [Route("Brands/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Brands/LoadData")]
        public async Task<JsonResult> LoadData()
        {
            List<BrandVM> data = new List<BrandVM>();

            string[] columns = { "id", "brand_name" };

            int limit = Convert.ToInt32(Request.Form["length"]);
            int start = Convert.ToInt32(Request.Form["start"]);
            string draw = Request.Form["draw"];
            string dir = Request.Form["order[0][dir]"];
            string search = Request.Form["search[value]"];

            // order column names
            string order = String.Empty;
            switch (Request.Form["order[0][column]"])
            {
                case "0":
                    order = "id";
                    break;
                case "1":
                    order = "brand_name";
                    break;
                default:
                    order = "id";
                    break;

            }

            int totalRecordsCount = 0;
            int totalFilteredRecordCount = 0;

            try
            {
                totalRecordsCount = await brandsRepository.CountAllBrandsAsync();

                if (totalRecordsCount >= 0)
                {
                    totalFilteredRecordCount = totalRecordsCount;
                    data = await brandsRepository.AllBrandsAsync(start, limit, order, dir, search);

                    if (data != null)
                    {
                        if (!String.IsNullOrEmpty(search))
                        {
                            totalFilteredRecordCount = await brandsRepository.AllFilteredBrandsAsync(search);
                            if (totalRecordsCount < 0)
                            {
                                totalRecordsCount = 0;
                                totalFilteredRecordCount = 0;
                                data.Clear();
                            }
                        }

                    }
                    else
                    {
                        totalRecordsCount = 0;
                        totalFilteredRecordCount = 0;
                    }

                }

                var dataObj = new
                {
                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecordsCount,
                    recordsFiltered = totalFilteredRecordCount,
                    data = data
                };

                return Json(dataObj);
            }
            catch (Exception)
            {
                var dataObj = new
                {
                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecordsCount,
                    recordsFiltered = totalFilteredRecordCount,
                    data = data
                };

                return Json(dataObj);
            }

        }

        
        [Route("Brands/Download")]
        public async Task<IActionResult> Download()
        {
            var builder = new StringBuilder();
            builder.AppendLine("id,brand_name");

            try
            {

                List<BrandVM> brands = await brandsRepository.GetAllAsync();
                foreach (var item in brands)
                {
                    builder.AppendLine($"{item.id.ToString()},{item.brand_name}");
                }

                return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "brands.csv");
            }
            catch (Exception)
            {
                return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "brands.csv");
            }

        }
    }
}
