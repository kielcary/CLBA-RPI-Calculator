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
        private int SeasonID;


        public CLBRPIForm()
        {
            InitializeComponent();

            using (var seasonForm = new SeasonCheckForm())
            {
                seasonForm.StartPosition = FormStartPosition.CenterScreen;
                var result = seasonForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SeasonID = seasonForm.SeasonID;
                }
            }

            using (var newDataForm = new CheckAndCommitForm())
            {
                newDataForm.StartPosition = FormStartPosition.CenterScreen;
                var result = newDataForm.ShowDialog();
            }

            TeamsGridView.AutoGenerateColumns = true;
            TeamsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;


            GetTeams();
            

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
            TeamsGridView.Columns[4].DataPropertyName = "StrengthOfScheduleRank";
            TeamsGridView.Columns[5].DataPropertyName = "RPI";
            TeamsGridView.Columns[6].DataPropertyName = "PreviousRPI";
            TeamsGridView.Columns[7].DataPropertyName = "StrengthOfSchedule";
            TeamsGridView.Columns[8].DataPropertyName = "WinningPercentage";
            TeamsGridView.Columns[9].DataPropertyName = "OpponentsWinPercentage";
            TeamsGridView.Columns[10].DataPropertyName = "OpponentsOpponentWinPercentage";


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
    }
}