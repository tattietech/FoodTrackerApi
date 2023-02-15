using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foodTrackerApi.Interfaces
{
    public interface IDynamoModel
    {
        public static string Identifier { get; }
    }
}
