using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRequestDomain.Enums
{
    public enum ChecklistItemStatus
    {
        Pending,    // nothing uploaded yet
        Uploaded,   // file uploaded, awaiting officer review
        Verified,   // officer accepted this document
        Rejected    // officer rejected, applicant must re-upload
    }
}
