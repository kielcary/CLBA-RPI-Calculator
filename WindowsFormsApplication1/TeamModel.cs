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

            public float OpponentsWinPercentage { get; set; }
            public float OpponentsOpponentWinPercentage { get; set; }

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

            public void CalcOppWinPercentage(SortableBindingList<TeamModel> teams )
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

                    opponentModel.AdjustedWins = opponentModel.AdjustedWins*timesPlayed;
                    opponentModel.AdjustedLosses = opponentModel.AdjustedLosses * timesPlayed;

                    //Add adjusted wins and losses together to get totals
                    totalAdjustedOppWins += opponentModel.AdjustedWins;
                    totalAdjustedOppLosses += opponentModel.AdjustedLosses;

                }

                OpponentsWinPercentage = totalAdjustedOppWins/(totalAdjustedOppWins + totalAdjustedOppLosses);


            }

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
	            OpponentsOpponentWinPercentage = totalOpponentWinPercentage/OpponentsList.Count;
	        }

            public void CalcRPI()
            {
                RPI = (float) ((WinningPercentage*.25) + (OpponentsWinPercentage*.5) + (OpponentsOpponentWinPercentage*.25));
                RPI = (float)Math.Round((Decimal) RPI, 3, MidpointRounding.AwayFromZero);
            }
	
        }

        
    }
        

