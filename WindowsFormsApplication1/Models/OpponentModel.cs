using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class OpponentModel
    {

        public string OpponentTeamName { get; set; }
        public int OpponentTeamID { get; set; }

        public int Wins { get; set; }
        public int Losses { get; set; }

        public int WinsVersus { get; set; }
        public int LossesVersus { get; set; }

        public int AdjustedWins { get; set; }
        public int AdjustedLosses { get; set; }

        public int PythWins { get; set; }
        public int PythLosses { get; set; }

        public int AdjustedPythWins { get; set; }
        public int AdjustedPythLosses { get; set; }
    }
}
