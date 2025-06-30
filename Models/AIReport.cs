using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReviewerApp.Models
{
    public class AIReport
    {
        public List<string> naming_convention { get; set; }
        public List<string> complexity { get; set; }
        public List<string> security { get; set; }
        public List<string> ai_comments { get; set; }
        public double ai_score { get; set; }
    }
}
