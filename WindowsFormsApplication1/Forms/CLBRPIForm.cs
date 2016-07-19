using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using WindowsFormsApplication1.DataBaseContext;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace WindowsFormsApplication1
{
    public partial class CLBRPIForm : Form
    {
        private static readonly SortableBindingList<TeamModel> Teams = new SortableBindingList<TeamModel>();
        private int _SeasonID;

        public CLBRPIForm()
        {
            InitializeComponent();

            using (var seasonForm = new SeasonCheckForm())
            {
                seasonForm.StartPosition = FormStartPosition.CenterScreen;
                var result = seasonForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _SeasonID = seasonForm.SeasonID;
                }
            }

            using (var newDataForm = new CheckAndCommitForm(_SeasonID))
            {
                newDataForm.StartPosition = FormStartPosition.CenterScreen;
                newDataForm.ShowDialog();
            }

            TeamsGridView.AutoGenerateColumns = true;
            TeamsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;


            GetData();


        }

        private void GetData()
        {
            using (var Content = new DataClassesDataContext())
            {
                if (!Content.Uploads.Any())
                {
                    MessageBox.Show("There are no uploads, so there is no data.", "Upload Something!",
                        MessageBoxButtons.OK);
                    return;
                }
                var list = Content.Teams;

                foreach (var team in list)
                {
                    Teams.Add(new TeamModel()
                    {
                        TeamID = team.TeamID,
                        Name = team.TeamName,
                        Division = team.League1.LeagueName + " " + team.Division1.DivisionName,
                        OpponentsList = (from t in Content.OpponentsRecords
                                         where t.TeamID.Equals(team.TeamID)
                                         && t.SeasonID.Equals(_SeasonID)
                                         select new OpponentModel()
                                         {
                                             OpponentTeamName = t.Team.TeamName,
                                             OpponentTeamID = t.OpponentTeamID,
                                             WinsVersus = t.WinsAgainst,
                                             LossesVersus = t.LossesAgainst,
                                             
                                         }).ToList(),
                        Wins = team.Records.FirstOrDefault(x => x.SeasonID == _SeasonID).Wins,
                        Losses = team.Records.FirstOrDefault(x => x.SeasonID == _SeasonID).Losses,
                        PythWins = team.Records.FirstOrDefault(x => x.SeasonID == _SeasonID).PythWins,
                        PythLosses = team.Records.FirstOrDefault(x => x.SeasonID == _SeasonID).PythLosses,
                        RPI = (float)team.TeamCalculations.OrderByDescending(x => x.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID).RPI,
                        StrengthOfSchedule = (float)team.TeamCalculations.OrderByDescending(x => x.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID).SoS,
                        OpponentsWinPercentage = (float)team.TeamCalculations.OrderByDescending(x => x.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID).OWP,
                        OpponentsOpponentWinPercentage =
                            (float)team.TeamCalculations.OrderByDescending(x => x.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID).OOWP,
                        PythOpponentsWinPercentage = (float)team.TeamCalculations.OrderByDescending(x => x.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID).PythOWP,
                        PythOpponentsOpponentWinPercentage =
                            (float)team.TeamCalculations.OrderByDescending(x => x.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID).PythOOWP,
                        PythRPI = (float)team.TeamCalculations.OrderByDescending(x => x.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID).PythRPI,
                        
                    });
                }

                foreach (var teamModel in Teams)
                {
                    var currentUpload = Content.Uploads.OrderByDescending(
                        x => x.UploadID).FirstOrDefault(x => x.SeasonID == _SeasonID);

                    if (currentUpload != null)
                    {
                        var previousUpload =
                            Content.Uploads.FirstOrDefault(x => x.UploadID == (currentUpload.UploadID - 1));

                        if (previousUpload != null)
                        {
                            teamModel.PreviousRPI = (float)Content.TeamCalculations.FirstOrDefault(
                                x => x.UploadID == previousUpload.UploadID && x.TeamID == teamModel.TeamID).RPI;

                            teamModel.RPIDiff = teamModel.RPI - teamModel.PreviousRPI;
                        }   
                    }
                }

                foreach (var teamModel in Teams)
                {
                    teamModel.RoundData();
                }

                foreach (var teamModel in Teams)
                {
                    teamModel.CalcRanks(Teams);
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
            TeamsGridView.Columns.Add("Division", "Division");
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
            TeamsGridView.Columns[1].DataPropertyName = "Division";
            TeamsGridView.Columns[2].DataPropertyName = "Wins";
            TeamsGridView.Columns[3].DataPropertyName = "Losses";
            TeamsGridView.Columns[4].DataPropertyName = "RPIRank";
            TeamsGridView.Columns[5].DataPropertyName = "StrengthOfScheduleRank";
            TeamsGridView.Columns[6].DataPropertyName = "RPI";
            TeamsGridView.Columns[7].DataPropertyName = "PreviousRPI";
            TeamsGridView.Columns[8].DataPropertyName = "StrengthOfSchedule";
            TeamsGridView.Columns[9].DataPropertyName = "WinningPercentage";
            TeamsGridView.Columns[10].DataPropertyName = "OpponentsWinPercentage";
            TeamsGridView.Columns[11].DataPropertyName = "OpponentsOpponentWinPercentage";


            TeamsGridView.DataSource = Teams;
            TeamsGridView.Sort(TeamsGridView.Columns["Name"], ListSortDirection.Ascending);
        }

        private void btnRPI_Click(object sender, EventArgs e)
        {
            TeamsGridView.DataSource = null;
            TeamsGridView.Rows.Clear();
            TeamsGridView.Refresh();

            TeamsGridView.AutoGenerateColumns = false;
            TeamsGridView.Columns.Clear();

            TeamsGridView.DataSource = Teams;

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

            TeamsGridView.DataSource = Teams;

            TeamsGridView.Columns.Clear();

            TeamsGridView.Columns.Add("SoSRank", "SoSRank");
            TeamsGridView.Columns.Add("Name", "Team");
            TeamsGridView.Columns.Add("SoS", "SoS");

            TeamsGridView.Columns[0].DataPropertyName = "StrengthOfScheduleRank";
            TeamsGridView.Columns[1].DataPropertyName = "Name";
            TeamsGridView.Columns[2].DataPropertyName = "StrengthOfSchedule";


            TeamsGridView.Sort(TeamsGridView.Columns["SoS"], ListSortDirection.Descending);
        }

        private void btnPyth_Click(object sender, EventArgs e)
        {
            TeamsGridView.DataSource = null;
            TeamsGridView.Rows.Clear();
            TeamsGridView.Refresh();

            TeamsGridView.AutoGenerateColumns = false;

            TeamsGridView.DataSource = Teams;

            TeamsGridView.Columns.Clear();

            TeamsGridView.Columns.Add("PythRPIRank", "PythRPIRank");
            TeamsGridView.Columns.Add("Name", "Team");
            TeamsGridView.Columns.Add("RPIRank", "RPIRank");
            TeamsGridView.Columns.Add("PythRPI", "PythRPI");
            TeamsGridView.Columns.Add("PythWin", "PythWin");
            TeamsGridView.Columns.Add("PythLosses", "PythLosses");
            TeamsGridView.Columns.Add("PythWinPercentage", "PythWinPercentage");
            TeamsGridView.Columns.Add("PythOWP", "PythOWP");
            TeamsGridView.Columns.Add("PythOOWP", "PythOOWP");

            TeamsGridView.Columns[0].DataPropertyName = "PythRPIRanking";
            TeamsGridView.Columns[1].DataPropertyName = "Name";
            TeamsGridView.Columns[2].DataPropertyName = "RPIRank";
            TeamsGridView.Columns[3].DataPropertyName = "PythRPI";
            TeamsGridView.Columns[4].DataPropertyName = "PythWins";
            TeamsGridView.Columns[5].DataPropertyName = "PythLosses";
            TeamsGridView.Columns[6].DataPropertyName = "PythWinPercentage";
            TeamsGridView.Columns[7].DataPropertyName = "PythOpponentsWinPercentage";
            TeamsGridView.Columns[8].DataPropertyName = "PythOpponentsOpponentWinPercentage";
            



            TeamsGridView.Sort(TeamsGridView.Columns["RPIRank"], ListSortDirection.Descending);
        }
    }
}