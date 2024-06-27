using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fina.Core.Requests.Transactions;

public class GetTransactionByPeriodRequest : Request
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set;}
}
