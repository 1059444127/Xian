using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Ris.Application.Common.Admin.StaffAdmin
{
    [DataContract]
    public class ListStaffRequest : PagedDataContractBase
    {
        public ListStaffRequest()
        {
        }

        public ListStaffRequest(string surname, string givenname, bool listOnlyPractitioners)
        {
            this.LastName = surname;
            this.FirstName = givenname;
            this.ListOnlyPractitioners = listOnlyPractitioners;
        }

        public ListStaffRequest(string surname, string givenname, bool listOnlyPractitioners, PageRequestDetail page)
            : this(surname, givenname, listOnlyPractitioners)
        {
            this.PageRequest = page;
        }

        [DataMember]
        public string FirstName;

        [DataMember]
        public string LastName;

        [DataMember]
        public bool ListOnlyPractitioners;
    }
}