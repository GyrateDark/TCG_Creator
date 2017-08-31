using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCG_Creator
{
    class GUI_Settings
    {
        public int SelectedRegionId { get; set; } = -1;
        public int SelectedCardId { get; set; } = -1;
        public bool ShowAllRegions { get; set; } = false;
        public bool ShowTemplateSettings { get; set; } = false;
    }
}
