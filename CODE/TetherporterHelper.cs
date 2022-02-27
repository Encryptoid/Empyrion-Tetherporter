using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetherporter
{
    public static class TetherporterHelper
    {
        public static string FormatTetherportFileName(string steamId)
        {
            return Path.Combine("Tethers", $"{steamId}.tether");
        }
    }
}
