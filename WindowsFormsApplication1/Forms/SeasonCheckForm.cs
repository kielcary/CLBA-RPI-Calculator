using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication1.DataBaseContext;

namespace WindowsFormsApplication1
{
    public partial class SeasonCheckForm : Form
    {
        public int SeasonID { get; set; }
        public List<Season> SeasonList { get; set; }
        

        public SeasonCheckForm()
        {
            InitializeComponent();
            GetSeasonList();

            cmbSeason.DataSource = SeasonList;
            cmbSeason.DisplayMember = "Year";
            cmbSeason.ValueMember = "SeasonID";
            
        }

        private void GetSeasonList()
        {
            using (var Content = new DataClassesDataContext())
            {
                SeasonList = Content.Seasons.ToList();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.SeasonID = (int)cmbSeason.SelectedValue;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
