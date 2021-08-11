using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TusTest.Pages
{
    public class UploadTestModel : PageModel
    {
        public string TempPath { get; set; }

        public void OnGet()
        {
            var tempPath = Path.GetTempPath();
            TempPath = tempPath;
        }
    }
}
