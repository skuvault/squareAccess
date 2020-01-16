using System.Collections.Generic;

namespace SquareAccess.Models
{
    public class SquareOrdersBatch
    {
	    public IEnumerable< SquareOrder > Orders { get; set; }
	    public string Cursor { get; set; }
    }
}
