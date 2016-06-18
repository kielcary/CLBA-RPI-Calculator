using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace WindowsFormsApplication1
{
    public partial class CLBRPIForm : Form
    {
        private static readonly SortableBindingList<TeamModel> teams = new SortableBindingList<TeamModel>();


        public CLBRPIForm()
        {
            InitializeComponent();

            TeamsGridView.AutoGenerateColumns = true;
            TeamsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            GetStandings();
            GetOpponents();
            RunCalculations();
            HandlePreviousRPI();

        }

        /// <summary>
        /// Stores some data in an XML file to handle the previous week's RPI.
        /// </summary>
        private void HandlePreviousRPI()
        {
            var hasNewData = false;

            var ratingsXML = XDocument.Load(Program.XMLDocPath + "\\RatingsData.xml");

            var teamsElement = ratingsXML.Element("teams");

            foreach (var teamModel in teams)
            {
                if (teamsElement.Descendants("team").Any(x => x.Element("TeamName").Value == teamModel.Name))
                {
                    var xmlTeam = teamsElement.Descendants("team").FirstOrDefault(x =>
                        x.Element("TeamName").Value == teamModel.Name);
                    var xmlName = xmlTeam.Element("TeamName").Value;
                    var xmlWins = Convert.ToInt32(xmlTeam.Element("Wins").Value);
                    var xmlLosses = Convert.ToInt32(xmlTeam.Element("Losses").Value);

                    teamModel.PreviousRPI = Convert.ToInt32(xmlTeam.Element("PreviousRPI").Value);
                    teamModel.RPIDiff = teamModel.RPI - Convert.ToInt32(xmlTeam.Element("PreviousRPI").Value);

                    //Checks to see if it's new data for this team, sets previous RPI if it is
                    if (xmlWins != teamModel.Wins || xmlLosses != teamModel.Losses)
                    {
                        hasNewData = true;
                        //set previous RPI before overwriting things...
                        xmlTeam.Element("PreviousRPI").Value = xmlTeam.Element("RPI").Value;
                        teamModel.PreviousRPI = Convert.ToInt32(xmlTeam.Element("PreviousRPI").Value);
                        teamModel.RPIDiff = teamModel.RPI - Convert.ToInt32(xmlTeam.Element("PreviousRPI").Value);

                        xmlTeam.Element("RPI").Value = teamModel.RPI.ToString();
                        xmlTeam.Element("Wins").Value = teamModel.Wins.ToString();
                        xmlTeam.Element("Losses").Value = teamModel.Losses.ToString();

                    }
                }
                else
                {
                    hasNewData = true;

                    var NewTeamElement = new XElement("team",
                        new XElement("TeamName", teamModel.Name),
                        new XElement("RPI", teamModel.RPI),
                        new XElement("Wins", teamModel.Wins),
                        new XElement("Losses", teamModel.Losses),
                        new XElement("PreviousRPI", "0"));

                    teamsElement.Add(NewTeamElement);
                }
            }

            if (hasNewData)
            {
                ratingsXML.Save(Program.XMLDocPath + "\\RatingsData.xml");
                MessageBox.Show("New data retrieved.  'PreviousRPI' has been updated.", "Attention",
                    MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("No new data was retrieved.", "Attention", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        ///     Runs all calculations for all teams
        /// </summary>
        public void RunCalculations()
        {
            foreach (var teamModel in teams)
            {
                teamModel.CalcOppWinPercentage(teams);
            }

            foreach (var teamModel in teams)
            {
                teamModel.CalcOppOppWinPercentage(teams);
            }

            foreach (var teamModel in teams)
            {
                teamModel.CalcRPI();
                teamModel.CalcStrengthOfSchedule();
                teamModel.RoundData();
            }

            foreach (var teamModel in teams)
            {
                teamModel.CalcRanks(teams);
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
        ///     Retrieves teams and team record.
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

                            if (teams.All(x => x.Name != Team) && Wins != "W")
                            {
                                teams.Add(new TeamModel
                                {
                                    Name = Team,
                                    Wins = Convert.ToInt32(Wins),
                                    Losses = Convert.ToInt32(Losses)
                                });
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

            foreach (var teamModel in teams)
            {
                //Each team is in its own table, so get the corresponding table from the htmlnodes
                var currentTeamOpponentsNode =
                    TeamVsTeamHtmlNodes.Where(x => x.ChildNodes[1].ChildNodes[1].InnerText.Equals(teamModel.Name))
                        .FirstOrDefault();

                //Cycle through the nodes and create new opponent objects based on team name
                foreach (var childNode in currentTeamOpponentsNode.ChildNodes)
                {
                    if (childNode.Name != "#text" && !childNode.InnerHtml.Contains(teamModel.Name))
                    {
                        var record = childNode.ChildNodes[3].InnerText.Split('-');

                        teamModel.OpponentsList.Add(new OpponentModel
                        {
                            TeamName = childNode.ChildNodes[1].InnerText,
                            WinsVersus = Convert.ToInt32(record[0]),
                            LossesVersus = Convert.ToInt32(record[1])
                        });
                    }
                }
            }
        }

        private void btnAll_Click(object sender, EventArgs e)
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
            TeamsGridView.Columns.Add("PrevRPI", "PrevRPI");
            TeamsGridView.Columns.Add("SoS", "SoS");
            TeamsGridView.Columns.Add("WP", "WP");
            TeamsGridView.Columns.Add("OWP", "OWP");
            TeamsGridView.Columns.Add("OOWP", "OOWP");

            
            TeamsGridView.Columns[0].DataPropertyName = "Name";
            TeamsGridView.Columns[1].DataPropertyName = "Wins";
            TeamsGridView.Columns[2].DataPropertyName = "Losses";
            TeamsGridView.Columns[3].DataPropertyName = "RPIRank";
            TeamsGridView.Columns[4].DataPropertyName = "SoSRank";
            TeamsGridView.Columns[5].DataPropertyName = "RPI";
            TeamsGridView.Columns[6].DataPropertyName = "PreviousRPI";
            TeamsGridView.Columns[7].DataPropertyName = "SoS";
            TeamsGridView.Columns[8].DataPropertyName = "WinningPercentage";
            TeamsGridView.Columns[9].DataPropertyName = "OpponentsWinPercentage";
            TeamsGridView.Columns[10].DataPropertyName = "OpponentsOpponentWinPercentage";


            TeamsGridView.DataSource = teams;
            TeamsGridView.Sort(TeamsGridView.Columns["Name"], ListSortDirection.Ascending);
        }


        private void btnRPI_Click(object sender, EventArgs e)
        {
            TeamsGridView.DataSource = null;
            TeamsGridView.Rows.Clear();
            TeamsGridView.Refresh();

            TeamsGridView.AutoGenerateColumns = false;
            TeamsGridView.Columns.Clear();

            TeamsGridView.DataSource = teams;

            TeamsGridView.Columns.Add("RPI Rank", "RPI Rank");
            TeamsGridView.Columns.Add("Name", "Team");
            TeamsGridView.Columns.Add("RPI", "RPI");
            TeamsGridView.Columns.Add("PreviousRPI", "PrevRPI");
            TeamsGridView.Columns.Add("DIFF", "RPIDIFF");

            TeamsGridView.Columns[0].DataPropertyName = "RPIRank";
            TeamsGridView.Columns[1].DataPropertyName = "Name";
            TeamsGridView.Columns[2].DataPropertyName = "RPI";
            TeamsGridView.Columns[3].DataPropertyName = "PreviousRPI";
            TeamsGridView.Columns[4].DataPropertyName = "RPIDiff";
            

            TeamsGridView.Sort(TeamsGridView.Columns["RPI"], ListSortDirection.Descending);
        }

        private void btnSoS_Click(object sender, EventArgs e)
        {
            TeamsGridView.DataSource = null;
            TeamsGridView.Rows.Clear();
            TeamsGridView.Refresh();

            TeamsGridView.AutoGenerateColumns = false;

            TeamsGridView.DataSource = teams;

            TeamsGridView.Columns.Clear();

            TeamsGridView.Columns.Add("SoSRank", "SoSRank");
            TeamsGridView.Columns.Add("Name", "Team");
            TeamsGridView.Columns.Add("SoS", "SoS");

            TeamsGridView.Columns[0].DataPropertyName = "SoSRank";
            TeamsGridView.Columns[1].DataPropertyName = "Name";
            TeamsGridView.Columns[2].DataPropertyName = "SoS";


            TeamsGridView.Sort(TeamsGridView.Columns["SoS"], ListSortDirection.Descending);
        }
    }
}