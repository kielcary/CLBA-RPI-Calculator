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
    public partial class CLBRPIForm : Form
    {
        static SortableBindingList<TeamModel> teams = new SortableBindingList<TeamModel>();


        public CLBRPIForm()
        {
            InitializeComponent();

            TeamsGridView.AutoGenerateColumns = true;

            GetStandings();
            GetOpponents();
            RunCalculations();

            TeamsGridView.DataSource = teams;

            foreach (DataGridViewColumn column in TeamsGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }

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
            }
        }

        /// <summary>
        /// Retrieves the HTML of a given page.
        /// </summary>
        /// <param name="url">URL of page to retrieve HTML from.</param>
        /// <returns>String containing HTML </returns>
        private string GetHTML(string url)
        {
            string urlAddress = url;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();

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
                                teams.Add(new TeamModel()
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

        private void GetOpponents()
        {
            var returnList = new List<OpponentModel>();

            string TeamVsTeamHTML =
                GetHTML("http://www.thebestdamnleague.com/game/lgreports/leagues/league_100_team_vs_team.html");

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
                    TeamVsTeamHtmlNodes.Where((x => x.ChildNodes[1].ChildNodes[1].InnerText.Equals(teamModel.Name)))
                        .FirstOrDefault();

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


    }
}



