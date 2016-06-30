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
        private int _SeasonID;

        public CheckAndCommitForm(int seasonID)
        {
            InitializeComponent();

            TeamsGridView.AutoGenerateColumns = true;
            TeamsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            _SeasonID = seasonID;

            GetTeams();
            GetStandings();
            GetOpponents();
            RunCalculations();
            ShowImportData();

            

            dtpGameDate.Value = GetLastUploadGameDate();

            this.Shown += OnShown;

        }

        private void OnShown(object sender, EventArgs eventArgs)
        {
            CheckIfDuplicateData();
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
        ///     Runs all calculations for all teams
        /// </summary>
        public void RunCalculations()
        {
            //1.  Calc opponent win percentage
            foreach (var teamModel in Teams)
            {
                teamModel.CalcOppWinPercentage(Teams);
            }

            //2.  Using calculated opponent win percentages, calculate opponent's opponent win percentage
            foreach (var teamModel in Teams)
            {
                teamModel.CalcOppOppWinPercentage(Teams);
            }

            //3.  Using calculated data, generate metrics and round them for viewing pleasure
            foreach (var teamModel in Teams)
            {
                teamModel.CalcRPI();
                teamModel.CalcStrengthOfSchedule();
                teamModel.RoundData();
            }

            //4.  Calculate Ranks
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

        private void CheckIfDuplicateData()
        {
            using (var Content = new DataClassesDataContext())
            {
                int DatabaseSeasonWins = 0;
                int DatabaseSeasonLosses = 0;

                int ImportWins = 0;
                int ImportLosses = 0;
                var seasonRecords = Content.Records.ToList().Where(x => x.SeasonID == _SeasonID);
                foreach (var seasonRecord in seasonRecords)
                {
                    DatabaseSeasonWins += seasonRecord.Wins;
                    DatabaseSeasonLosses += seasonRecord.Losses;
                }
                foreach (var teamModel in Teams)
                {
                    ImportWins += teamModel.Wins;
                    ImportLosses += teamModel.Losses;
                }

                if (ImportWins == DatabaseSeasonWins && ImportLosses == DatabaseSeasonLosses)
                {
                    MessageBox.Show("It appears this data already exists in the database.  No import needed.",
                        "Exiting Import", MessageBoxButtons.OK);

                    this.Close();
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

        private DateTime GetLastUploadGameDate()
        {
            using (var Content = new DataClassesDataContext())
            {
                var LastUpdate = Content.Uploads.OrderByDescending(
                    d => d.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID);

                if (LastUpdate != null)
                {
                    var date = LastUpdate.GameDate.Date;
                    return date;
                }
                else
                {
                    string seasonYear = Content.Seasons.FirstOrDefault(x => x.SeasonID == _SeasonID).Year.ToString();
                    return DateTime.Parse("01/01/" + seasonYear);
                }
                
                
            }
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            using (var Content = new DataClassesDataContext())
            {
                if (dtpGameDate.Value == GetLastUploadGameDate())
                {
                    MessageBox.Show("An upload for this game date has already occured", "Cannot Proceed",
                        MessageBoxButtons.OK);

                    return;
                }
                //create a new upload
                var newUpload = new Upload();
                newUpload.GameDate = dtpGameDate.Value;
                newUpload.UploadDate = DateTime.Now.Date;
                newUpload.SeasonID = _SeasonID;
                Content.Uploads.InsertOnSubmit(newUpload);
                Content.SubmitChanges();

                foreach (var teamModel in Teams)
                {
                    
                    //Update or create record
                    var record =
                        Content.Records.FirstOrDefault(x => x.TeamID == teamModel.TeamID && x.SeasonID == _SeasonID);
                    if (record == null)
                    {
                        record = new Record();
                        record.TeamID = teamModel.TeamID;
                        record.SeasonID = _SeasonID;
                        record.Wins = teamModel.Wins;
                        record.Losses = teamModel.Losses;
                        record.DateModified = DateTime.Now.Date;
                        Content.Records.InsertOnSubmit(record);
                        Content.SubmitChanges();
                    }
                    else
                    {
                        record.Wins = teamModel.Wins;
                        record.Losses = teamModel.Losses;
                        record.DateModified = DateTime.Now.Date;
                        Content.SubmitChanges();
                    }

                    //Update or create opponentrecords
                    foreach (var opponentModel in teamModel.OpponentsList)
                    {
                        if (opponentModel.WinsVersus != 0 || opponentModel.LossesVersus != 0)
                        {
                            var opponentRecord = Content.OpponentsRecords.FirstOrDefault(
                           x => x.TeamID == teamModel.TeamID && x.OpponentTeamID == opponentModel.OpponentTeamID &&
                                x.SeasonID == _SeasonID);

                            if (opponentRecord == null)
                            {
                                opponentRecord = new OpponentsRecord();
                                opponentRecord.SeasonID = _SeasonID;
                                opponentRecord.TeamID = teamModel.TeamID;
                                opponentRecord.OpponentTeamID = opponentModel.OpponentTeamID;
                                opponentRecord.WinsAgainst = opponentModel.WinsVersus;
                                opponentRecord.LossesAgainst = opponentModel.LossesVersus;
                                opponentRecord.DateModified = DateTime.Now.Date;
                                Content.OpponentsRecords.InsertOnSubmit(opponentRecord);
                                Content.SubmitChanges();
                            }
                            else
                            {
                                opponentRecord.WinsAgainst = opponentModel.WinsVersus;
                                opponentRecord.LossesAgainst = opponentModel.LossesVersus;
                                opponentRecord.DateModified = DateTime.Now.Date;
                                Content.SubmitChanges();
                            }
                        }  
                    }

                    //Insert team calculations - new for each upload
                    var calculations = new TeamCalculation();
                    calculations.UploadID = newUpload.UploadID;
                    calculations.SeasonID = _SeasonID;
                    calculations.TeamID = teamModel.TeamID;
                    calculations.WP = teamModel.WinningPercentage;
                    calculations.OWP = teamModel.OpponentsWinPercentage;
                    calculations.OOWP = teamModel.OpponentsOpponentWinPercentage;
                    calculations.SoS = teamModel.StrengthOfSchedule;
                    calculations.RPI = teamModel.RPI;
                    calculations.DateCreated = DateTime.Now.Date;
                    Content.TeamCalculations.InsertOnSubmit(calculations);
                    Content.SubmitChanges();

                }
            }
            this.Close();
        }
        
        private void dtpGameDate_MouseDown(object sender, MouseEventArgs e)
        {
            btnCommit.Enabled = true;
        }

        private void bntClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
