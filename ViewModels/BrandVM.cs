using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsvDlDbData.ViewModels
{
    public class BrandVM
    {
        public Int64 id { get; set; }
        public string brand_name { get; set; }
        public int product_count { get; set; }
        public string image_name { get; set; }
    }
}
