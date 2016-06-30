using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WindowsFormsApplication1
{
    public class TeamModel
    {
        #region private
        private int _TeamID;
        private string _Name;
        private string _Division;


        private int _Wins;
        private int _Losses;
        private float _RPI;
        private float _WinningPercentage;
        private float _StrengthOfSchedule;

        private int _StrengthOfScheduleRank;
        private int _RPIRank;
        private float _PreviousRPI;
        private float _RPIDiff;

        private float _OpponentsWinPercentage;
        private float _OpponentsOpponentWinPercentage;
        #endregion private


        #region public
        public List<OpponentModel> OpponentsList = new List<OpponentModel>();

        public int TeamID
        {
            get { return _TeamID; }
            set { _TeamID = value; }
        }
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public string Division
        {
            get { return _Division; }
            set { _Division = value; }
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

        public float RPI
        {
            get { return _RPI; }
            set { _RPI = value; }
        }
        public float WinningPercentage
        {
            get { return _WinningPercentage; }
            set { _WinningPercentage = value; }
        }
        public float StrengthOfSchedule
        {
            get { return _StrengthOfSchedule; }
            set { _StrengthOfSchedule = value; }
        }

        public int StrengthOfScheduleRank
        {
            get { return _StrengthOfScheduleRank; }
            set { _StrengthOfScheduleRank = value; }
        }
        public int RPIRank
        {
            get { return _RPIRank; }
            set { _RPIRank = value; }
        }
        public float PreviousRPI
        {
            get { return _PreviousRPI; }
            set { _PreviousRPI = value; }
        }
        public float RPIDiff
        {
            get { return _RPIDiff; }
            set { _RPIDiff = value; }
        }

        public float OpponentsWinPercentage
        {
            get { return _OpponentsWinPercentage; }
            set { _OpponentsWinPercentage = value; }
        }
        public float OpponentsOpponentWinPercentage
        {
            get { return _OpponentsOpponentWinPercentage; }
            set { _OpponentsOpponentWinPercentage = value; }
        }
        #endregion


        /// <summary>
        /// Calculate's team's Win Percentage
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
                //Make sure they've actually played the opponent...
                if (opponentModel.WinsVersus != 0 || opponentModel.LossesVersus != 0)
                {
                    //Get current wins and losses from the team standings list
                    opponentModel.Wins = (teams.Where(x => x.Name.Equals(opponentModel.OpponentTeamName)).FirstOrDefault().Wins);
                    opponentModel.Losses = (teams.Where(x => x.Name.Equals(opponentModel.OpponentTeamName)).FirstOrDefault().Losses);

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
                if (opponentModel.OpponentTeamID != TeamID)
                {
                    if (opponentModel.WinsVersus != 0 || opponentModel.LossesVersus != 0)
                    {
                        //Get the opponent's OpponentsWinPercentage
                        float currentOppWinPercentage =
                            teams.FirstOrDefault(x => x.Name.Equals(opponentModel.OpponentTeamName)).OpponentsWinPercentage;

                        //Add it to the total
                        totalOpponentWinPercentage += currentOppWinPercentage;
                    }   
                }
            }
            //Divide the total by the number of opponents against whom the team has wins or losses
            OpponentsOpponentWinPercentage = totalOpponentWinPercentage / OpponentsList.Count(x=> x.WinsVersus != 0 || x.LossesVersus != 0);
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
            StrengthOfSchedule = ((2 * OpponentsWinPercentage) + OpponentsOpponentWinPercentage) / 3;
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
                if (teamModel.TeamID != TeamID)
                {
                    if (RPI < teamModel.RPI)
                    {
                        rpirank++;
                    }

                    if (StrengthOfSchedule < teamModel.StrengthOfSchedule)
                    {
                        sosrank++;
                    }
                }

            }

            RPIRank = rpirank;
            StrengthOfScheduleRank = sosrank;

        }

        /// <summary>
        /// Rounds data to three decimals for better visual display.
        /// </summary>
        public void RoundData()
        {
            RPI = (float)Math.Round((Decimal)RPI, 3, MidpointRounding.AwayFromZero);
            StrengthOfSchedule = (float)Math.Round((Decimal)StrengthOfSchedule, 3, MidpointRounding.AwayFromZero);
            WinningPercentage = (float)Math.Round((Decimal)WinningPercentage, 3, MidpointRounding.AwayFromZero);
            OpponentsWinPercentage = (float)Math.Round((Decimal)OpponentsWinPercentage, 3, MidpointRounding.AwayFromZero);
            OpponentsOpponentWinPercentage = (float)Math.Round((Decimal)OpponentsOpponentWinPercentage, 3, MidpointRounding.AwayFromZero);
        }

    }


}


