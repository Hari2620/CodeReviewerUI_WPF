using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReviewerApp.Models
{
    public class ChangedFileModel
    {
        public string FileName { get; set; }
        public string Status { get; set; }
        public bool IsOverall { get; set; }
        public string RawUrl { get; set; }
    }
}
