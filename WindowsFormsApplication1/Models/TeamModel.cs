using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class TeamModel
    {
        private string _Name;
        private int _Wins;
        private int _Losses;

        public List<OpponentModel> OpponentsList = new List<OpponentModel>();

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        public int Wins
        {
            get { return _Wins; }
            set
            {
                _Wins = value;
                if (_Wins != 0 || _Losses != 0)
                {
                    CalcWinPercentage();
                }
            }
        }
        public int Losses
        {
            get { return _Losses; }
            set
            {
                _Losses = value;
                if (_Wins != 0 || _Losses != 0)
                {
                    CalcWinPercentage();
                }
            }
        }

        public float RPI { get; set; }
        public float WinningPercentage { get; set; }
        public float SoS { get; set; }

        public int SoSRank { get; set; }
        public int RPIRank { get; set; }
        public float PreviousRPI { get; set; }
        public float RPIDiff { get; set; }

        public float OpponentsWinPercentage { get; set; }
        public float OpponentsOpponentWinPercentage { get; set; }

        /// <summary>
        /// Calculate's team's RPI
        /// </summary>
        private void CalcWinPercentage()
        {

            //Win percentage is simply wins/totalGamesPlayed
            if (_Wins == 0)
            {
                WinningPercentage = 0;
            }
            else
            {
                WinningPercentage = (float)Wins / (Wins + Losses);
            }

        }

        /// <summary>
        /// Calculates Opponent's Win Percentage, weighted by number of times played
        /// </summary>
        /// <param name="teams"></param>
        public void CalcOppWinPercentage(SortableBindingList<TeamModel> teams)
        {
            //Opponents win percentage is calculated by:
            //1.  Getting wins and losses for each team
            //2.  For the team which you are caculating opponents win percentage, you remove the wins and losses 
            //    for games played against said opponent
            //3.  Each opponent's Wins and Losses must be multiplied by the number of times the team was played.
            //4.  Tally all of these adjusted wins and losses together
            //5.  OpponentsWinPercentage = AdjustedOpponentWins/totalAdjustedOpponentGamesPlayed

            float totalAdjustedOppWins = 0;
            float totalAdjustedOppLosses = 0;
            //1.  Adjust records of opponents by subtracting wins and losses vs the current team from opposing records
            foreach (var opponentModel in OpponentsList)
            {
                //Get current wins and losses from the team standings list
                opponentModel.Wins = (teams.Where(x => x.Name.Equals(opponentModel.TeamName)).FirstOrDefault().Wins);
                opponentModel.Losses = (teams.Where(x => x.Name.Equals(opponentModel.TeamName)).FirstOrDefault().Losses);

                //Subtract current team's losses versus opponent from that opponent's wins
                opponentModel.AdjustedWins = opponentModel.Wins - opponentModel.LossesVersus;

                //Subtract current team's wins versus opponent from that opponent's losses
                opponentModel.AdjustedLosses = opponentModel.Losses - opponentModel.WinsVersus;

                //To further complicate things, we must now take into account the number of times a team was played, and 
                //multiply their win/loss record according the times they played them.
                int timesPlayed = opponentModel.LossesVersus + opponentModel.WinsVersus;

                opponentModel.AdjustedWins = opponentModel.AdjustedWins * timesPlayed;
                opponentModel.AdjustedLosses = opponentModel.AdjustedLosses * timesPlayed;

                //Add adjusted wins and losses together to get totals
                totalAdjustedOppWins += opponentModel.AdjustedWins;
                totalAdjustedOppLosses += opponentModel.AdjustedLosses;

            }

            OpponentsWinPercentage = totalAdjustedOppWins / (totalAdjustedOppWins + totalAdjustedOppLosses);


        }

        /// <summary>
        /// Calculates average of  Opponent's Opponent's Win Percentage 
        /// </summary>
        /// <param name="teams"></param>
        public void CalcOppOppWinPercentage(SortableBindingList<TeamModel> teams)
        {
            //Opponent's Opponent's Win percentage is calculated by summing all of the opponents win percentages together, and
            //dividing by total number of opponents.

            float totalOpponentWinPercentage = 0;

            foreach (var opponentModel in OpponentsList)
            {
                //Get the opponent's OpponentsWinPercentage
                float currentOppWinPercentage =
                    teams.FirstOrDefault(x => x.Name.Equals(opponentModel.TeamName)).OpponentsWinPercentage;

                //Add it to the total
                totalOpponentWinPercentage += currentOppWinPercentage;
            }

            //Divide the total by the number of opponents
            OpponentsOpponentWinPercentage = totalOpponentWinPercentage / OpponentsList.Count;
        }

        /// <summary>
        /// Calculates RPI based on WinPerc, OppWinPer, OppOppWinPer
        /// </summary>
        public void CalcRPI()
        {
            RPI = (float)((WinningPercentage * .25) + (OpponentsWinPercentage * .5) + (OpponentsOpponentWinPercentage * .25));
        }

        //Calculates team's Strenght of Schedule
        public void CalcStrengthOfSchedule()
        {
            SoS = ((2 * OpponentsWinPercentage) + OpponentsOpponentWinPercentage) / 3;
        }

        /// <summary>
        /// Determines and assigns rankings based on Strength of Schedule and RPI
        /// </summary>
        /// <param name="teams"></param>
        public void CalcRanks(SortableBindingList<TeamModel> teams)
        {
            int rpirank = 1;
            int sosrank = 1;

            foreach (var teamModel in teams)
            {
                if (teamModel.Name != Name)
                {
                    if (RPI < teamModel.RPI)
                    {
                        rpirank++;
                    }

                    if (SoS < teamModel.SoS)
                    {
                        sosrank++;
                    }
                }

            }

            RPIRank = rpirank;
            SoSRank = sosrank;

        }

        /// <summary>
        /// Rounds data to three decimals for better visual display.
        /// </summary>
        public void RoundData()
        {
            RPI = (float)Math.Round((Decimal)RPI, 3, MidpointRounding.AwayFromZero);
            SoS = (float)Math.Round((Decimal)SoS, 3, MidpointRounding.AwayFromZero);
            WinningPercentage = (float)Math.Round((Decimal)WinningPercentage, 3, MidpointRounding.AwayFromZero);
            OpponentsWinPercentage = (float)Math.Round((Decimal)OpponentsWinPercentage, 3, MidpointRounding.AwayFromZero);
            OpponentsOpponentWinPercentage = (float)Math.Round((Decimal)OpponentsOpponentWinPercentage, 3, MidpointRounding.AwayFromZero);
        }

    }


}


