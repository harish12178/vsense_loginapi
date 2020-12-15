using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSign.Models
{
    public class UserPreference
    {
        public int ID { get; set; }
        public Guid UserID { get; set; }
        public string NavbarPrimaryBackground { get; set; }
        public string NavbarSecondaryBackground { get; set; }
        public string ToolbarBackground { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
