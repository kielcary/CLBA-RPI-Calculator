using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication1.DataBaseContext;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace WindowsFormsApplication1
{
    public partial class CheckAndCommitForm : Form
    {
        private static readonly SortableBindingList<TeamModel> Teams = new SortableBindingList<TeamModel>();

        public CheckAndCommitForm()
        {
            InitializeComponent();

            TeamsGridView.AutoGenerateColumns = true;
            TeamsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;


            GetTeams();
            GetStandings();
            GetOpponents();
            RunCalculations();
            ShowImportData();

        }


        private void GetTeams()
        {
            using (var Content = new DataClassesDataContext())
            {
                var list = Content.Teams;

                foreach (var team in list)
                {
                    Teams.Add(new TeamModel()
                    {
                        TeamID = team.TeamID,
                        Name = team.TeamName,
                        Division = team.League1.LeagueName + " " + team.Division1.DivisionName,
                        OpponentsList = (from t in Content.Teams
                                         select new OpponentModel()
                                         {
                                             OpponentTeamID = t.TeamID,
                                             OpponentTeamName = t.TeamName,
                                         }).ToList()
                    });
                }
            }
        }



        /// <summary>
        /// Handles database save.
        /// </summary>
        private void CommitNewDataToDataBase()
        {
            using (var Content = new DataClassesDataContext())
            {
                
            }
        }

        /// <summary>
        ///     Runs all calculations for all teams
        /// </summary>
        public void RunCalculations()
        {
            foreach (var teamModel in Teams)
            {
                teamModel.CalcOppWinPercentage(Teams);
            }

            foreach (var teamModel in Teams)
            {
                teamModel.CalcOppOppWinPercentage(Teams);
            }

            foreach (var teamModel in Teams)
            {
                teamModel.CalcRPI();
                teamModel.CalcStrengthOfSchedule();
                teamModel.RoundData();
            }

            foreach (var teamModel in Teams)
            {
                teamModel.CalcRanks(Teams);
            }
        }

        /// <summary>
        ///     Retrieves the HTML of a given page.
        /// </summary>
        /// <param name="url">URL of page to retrieve HTML from.</param>
        /// <returns>String containing HTML </returns>
        private string GetHTML(string url)
        {
            var urlAddress = url;

            var request = (HttpWebRequest) WebRequest.Create(urlAddress);
            var response = (HttpWebResponse) request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                var html = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                return html;
            }
            return "";
        }

        /// <summary>
        ///     Retrieves teams and team record by parsing an HTML page.
        /// </summary>
        private void GetStandings()
        {
            //Get the file text
            var source = GetHTML("http://www.thebestdamnleague.com/game/lgreports/leagues/league_100_standings.html");

            //Decode the html
            source = WebUtility.HtmlDecode(source);

            //Load the decoded string
            var resultat = new HtmlDocument();
            resultat.LoadHtml(source);

            //Retrieve the team W/L information
            var standingsHtmlNodes = resultat.DocumentNode.Descendants().Where
                (x => x.Name == "table" && x.Attributes["class"] != null &&
                      x.Attributes["class"].Value.Contains("sortable")).ToList();

            var Team = "";
            var Wins = "";
            var Losses = "";

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

                            if (Teams.Any(x => x.Name == Team) && Wins != "W")
                            {
                                Teams.FirstOrDefault(x => x.Name == Team).Wins = Convert.ToInt32(Wins);
                                Teams.FirstOrDefault(x => x.Name == Team).Losses = Convert.ToInt32(Losses);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets records versus opponents from HTML and puts them in that team's opponent list
        /// </summary>
        private void GetOpponents()
        {
            var returnList = new List<OpponentModel>();

            var TeamVsTeamHTML =
                GetHTML("http://www.thebestdamnleague.com/game/lgreports/leagues/league_100_team_vs_team.html");

            TeamVsTeamHTML = WebUtility.HtmlDecode(TeamVsTeamHTML);

            var resultat = new HtmlDocument();
            resultat.LoadHtml(TeamVsTeamHTML);

            var TeamVsTeamHtmlNodes = resultat.DocumentNode.Descendants().Where
                (x => x.Name == "table" && x.Attributes["class"] != null &&
                      x.Attributes["class"].Value.Contains("data sortable")).ToList();

            foreach (var teamModel in Teams)
            {
                //Each team is in its own table, so get the corresponding table from the htmlnodes
                var currentTeamOpponentsNode =
                    TeamVsTeamHtmlNodes
                        .FirstOrDefault(x => x.ChildNodes[1].ChildNodes[1].InnerText.Equals(teamModel.Name));

                //Cycle through the nodes and create new opponent objects based on team name
                foreach (var childNode in currentTeamOpponentsNode.ChildNodes)
                {
                    if (childNode.Name != "#text" && !childNode.InnerHtml.Contains(teamModel.Name))
                    {
                        var opponentName = childNode.ChildNodes[1].InnerText;
                        var record = childNode.ChildNodes[3].InnerText.Split('-');

                        if (teamModel.OpponentsList.Any(x => x.OpponentTeamName == opponentName))
                        {
                            teamModel.OpponentsList.FirstOrDefault(x => x.OpponentTeamName == opponentName).WinsVersus =
                            Convert.ToInt32(record[0]);
                            teamModel.OpponentsList.FirstOrDefault(x => x.OpponentTeamName == opponentName).LossesVersus =
                                Convert.ToInt32(record[1]);
                        }
                    }
                }
            }
        }

        private void ShowImportData()
        {
            TeamsGridView.DataSource = null;
            TeamsGridView.Columns.Clear();
            TeamsGridView.Rows.Clear();
            TeamsGridView.Refresh();

            TeamsGridView.AutoGenerateColumns = false;

            TeamsGridView.Columns.Add("Name", "Team");
            TeamsGridView.Columns.Add("Wins", "W");
            TeamsGridView.Columns.Add("Losses", "L");
            TeamsGridView.Columns.Add("RPIRank", "RPI Rank");
            TeamsGridView.Columns.Add("SoSRank", "SoS Rank");
            TeamsGridView.Columns.Add("RPI", "RPI");
            TeamsGridView.Columns.Add("SoS", "SoS");
            TeamsGridView.Columns.Add("WP", "WP");
            TeamsGridView.Columns.Add("OWP", "OWP");
            TeamsGridView.Columns.Add("OOWP", "OOWP");


            TeamsGridView.Columns[0].DataPropertyName = "Name";
            TeamsGridView.Columns[1].DataPropertyName = "Wins";
            TeamsGridView.Columns[2].DataPropertyName = "Losses";
            TeamsGridView.Columns[3].DataPropertyName = "RPIRank";
            TeamsGridView.Columns[4].DataPropertyName = "StrengthOfScheduleRank";
            TeamsGridView.Columns[5].DataPropertyName = "RPI";
            TeamsGridView.Columns[6].DataPropertyName = "StrengthOfSchedule";
            TeamsGridView.Columns[7].DataPropertyName = "WinningPercentage";
            TeamsGridView.Columns[8].DataPropertyName = "OpponentsWinPercentage";
            TeamsGridView.Columns[9].DataPropertyName = "OpponentsOpponentWinPercentage";


            TeamsGridView.DataSource = Teams;
            TeamsGridView.Sort(TeamsGridView.Columns["Name"], ListSortDirection.Ascending);
        }
    }
}
