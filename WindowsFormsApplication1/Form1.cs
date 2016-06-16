using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        static List<TeamModel> teams = new List<TeamModel>();


        public Form1()
        {
            InitializeComponent();
            TeamsGridView.AutoGenerateColumns = true;
            GetStandings();
            GetOpponents();

            foreach (var teamModel in teams)
            {
                teamModel.CalcOppWinPercentage();
            }

            foreach (var teamModel in teams)
            {
                teamModel.CalcOppOppWinPercentage();
            }

            foreach (var teamModel in teams)
            {
                teamModel.CalcRPI();
            }

            TeamsGridView.DataSource = teams.OrderByDescending(x => x.RPI).ToList();
        }

        /// <summary>
        /// Retrieves the HTML of a given page.
        /// </summary>
        /// <param name="url">URL of page to retrieve HTML from.</param>
        /// <returns>String containing HTML </returns>
        private string GetHTML(string url)
        {
            string urlAddress = url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string html = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                return html;
            }
            return "";
        }

        /// <summary>
        /// Retrieves teams and team record.
        /// </summary>
        private void GetStandings()
        {

            //Get the file text
            string source = GetHTML("http://www.thebestdamnleague.com/game/lgreports/leagues/league_100_standings.html");

            //Decode the html
            source = WebUtility.HtmlDecode(source);

            //Load the decoded string
            HtmlDocument resultat = new HtmlDocument();
            resultat.LoadHtml(source);

            //Retrieve the team W/L information
            List<HtmlNode> standingsHtmlNodes = resultat.DocumentNode.Descendants().Where
            (x => (x.Name == "table" && x.Attributes["class"] != null &&
            x.Attributes["class"].Value.Contains("sortable"))).ToList();

            String Team = "";
            String Wins = "";
            String Losses = "";

            foreach (var htmlNode in standingsHtmlNodes)
            {
                if (htmlNode.ChildNodes.Count == 13)
                {
                    foreach (var childNode in htmlNode.ChildNodes)
                    {
                        if (childNode.Name != "#text" && childNode != htmlNode.ChildNodes[1])
                        {
                            Team = childNode.ChildNodes[1].InnerText;
                            Wins = childNode.ChildNodes[3].InnerText;
                            Losses = childNode.ChildNodes[5].InnerText;

                            if (teams.All(x => x.Name != Team) && Wins != "W")
                            {
                                teams.Add(new TeamModel() { Name = Team, Wins = Convert.ToInt32(Wins), Losses = Convert.ToInt32(Losses) });
                            }
                        }
                    }
                }
            }
        }

        private void GetOpponents()
        {
            var returnList = new List<OpponentModel>();

            string TeamVsTeamHTML = GetHTML("http://www.thebestdamnleague.com/game/lgreports/leagues/league_100_team_vs_team.html");

            TeamVsTeamHTML = WebUtility.HtmlDecode(TeamVsTeamHTML);

            HtmlDocument resultat = new HtmlDocument();
            resultat.LoadHtml(TeamVsTeamHTML);

            List<HtmlNode> TeamVsTeamHtmlNodes = resultat.DocumentNode.Descendants().Where
            (x => (x.Name == "table" && x.Attributes["class"] != null &&
            x.Attributes["class"].Value.Contains("data sortable"))).ToList();

            foreach (var teamModel in teams)
            {
                //Each team is in its own table, so get the corresponding table from the htmlnodes
                var currentTeamOpponentsNode =
                    TeamVsTeamHtmlNodes.Where((x => x.ChildNodes[1].ChildNodes[1].InnerText.Equals(teamModel.Name))).FirstOrDefault();

                //Cycle through the nodes and create new opponent objects based on team name
                foreach (var childNode in currentTeamOpponentsNode.ChildNodes)
                {
                    if (childNode.Name != "#text" && !childNode.InnerHtml.Contains(teamModel.Name))
                    {
                        string[] record = childNode.ChildNodes[3].InnerText.Split('-');

                        teamModel.OpponentsList.Add(new OpponentModel()
                        {
                            TeamName = childNode.ChildNodes[1].InnerText,
                            WinsVersus = Convert.ToInt32(record[0].ToString()),
                            LossesVersus = Convert.ToInt32(record[1].ToString())
                            
                        });
                    }
                }

            }
        }

        public class TeamModel
        {
            private string _Name;
            private int _Wins;
            private int _Losses;

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

            public List<OpponentModel> OpponentsList = new List<OpponentModel>();

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

            public void CalcOppWinPercentage()
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

	        public void CalcOppOppWinPercentage()
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

        public class OpponentModel{
	
	        public String TeamName { get; set; }
	
	        public int Wins { get; set; }
	        public int Losses { get; set; }

	        public int WinsVersus { get; set; }
	        public int LossesVersus { get; set; }

	        public int AdjustedWins { get; set;}
            public int AdjustedLosses { get; set; }
        }
    }
        

}



