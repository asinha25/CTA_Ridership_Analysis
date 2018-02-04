using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Data.SqlClient;

namespace CTA
{
    public partial class Form1 : Form
    {
        IReadOnlyList<BusinessTier.CTAStation> Stationslist;
        IReadOnlyList<BusinessTier.CTAStop> Stopslist;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //
            // setup GUI:
            //
            this.lstStations.Items.Add("");
            this.lstStations.Items.Add("[ Use File>>Load to display L stations... ]");
            this.lstStations.Items.Add("");

            this.lstStations.ClearSelected();

            toolStripStatusLabel1.Text = string.Format("Number of stations:  0");

            // 
            // open-close connect to get SQL Server started:
            //
            try
            {
                string filename = this.txtDatabaseFilename.Text;
                BusinessTier.Business bizTier;
                bizTier = new BusinessTier.Business(filename);
                bizTier.TestConnection();
            }
            catch
            {
                //
                // ignore any exception that occurs, goal is just to startup
                //
            }
        }

        //
        // File>>Exit:
        //
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //
        // File>>Load Stations:
        //
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //
            // clear the UI of any current results:
            //
            ClearStationUI(true /*clear stations*/);

            //
            // now load the stations from the database:
            //

            //SqlConnection db = null;

            try
            {
                string filename = this.txtDatabaseFilename.Text;
                BusinessTier.Business bizTier;
                bizTier = new BusinessTier.Business(filename);

                var stations = bizTier.GetStations();

                Stationslist = stations;

                foreach (var station in stations)
                {
                    this.lstStations.Items.Add(station.Name);
                }

                toolStripStatusLabel1.Text = string.Format("Number of stations: {0:#,##0}", stations.Count());
            }

            catch (Exception ex)
            {
                string msg = string.Format("Error: '{0}'.", ex.Message);
                MessageBox.Show(msg);
            }
        }

        private void lstStations_SelectedIndexChanged(object sender, EventArgs e)
        {
            // sometimes this event fires, but nothing is selected...
            if (this.lstStations.SelectedIndex < 0)   // so return now in this case:
                return;

            string Name = this.lstStations.Text;
            Name = Name.Replace("'", "''");

            //
            // clear GUI in case this fails:
            //
            ClearStationUI();

            int Index = this.lstStations.SelectedIndex;
            int list = Stationslist[Index].ID;

            try
            {
                string filename = this.txtDatabaseFilename.Text;
                BusinessTier.Business bizTier;
                bizTier = new BusinessTier.Business(filename);

                var riders = bizTier.getTotalRidership(Name);
                foreach (var r in riders)
                {
                    this.txtTotalRidership.Text = string.Format("{0:#,##0}", r.v1);
                    this.txtAvgDailyRidership.Text = string.Format("{0:#,##0}/day", r.v2);
                }

                this.txtPercentRidership.Text = string.Format("{0:0.00}%", bizTier.getRidersPer(Name));
                this.txtStationID.Text = Convert.ToString(list);

                this.txtSaturdayRidership.Text = String.Format("{0:#,##0}", bizTier.FindDays(Name, "A"));
                this.txtSundayHolidayRidership.Text = String.Format("{0:#,##0}", bizTier.FindDays(Name, "U"));
                this.txtWeekdayRidership.Text = String.Format("{0:#,##0}", bizTier.FindDays(Name, "W"));

                var stops = bizTier.GetStops(list);
                Stopslist = stops;

                foreach (var stop in stops)
                {
                    this.lstStops.Items.Add(stop.Name);
                }
            }

            catch (Exception ex)
            {
                string msg = string.Format("Error: '{0}'.", ex.Message);
                MessageBox.Show(msg);
            }
        }

        private void ClearStationUI(bool clearStatations = false)
        {
            ClearStopUI();

            this.txtTotalRidership.Clear();
            this.txtTotalRidership.Refresh();

            this.txtAvgDailyRidership.Clear();
            this.txtAvgDailyRidership.Refresh();

            this.txtPercentRidership.Clear();
            this.txtPercentRidership.Refresh();

            this.txtStationID.Clear();
            this.txtStationID.Refresh();

            this.txtWeekdayRidership.Clear();
            this.txtWeekdayRidership.Refresh();
            this.txtSaturdayRidership.Clear();
            this.txtSaturdayRidership.Refresh();
            this.txtSundayHolidayRidership.Clear();
            this.txtSundayHolidayRidership.Refresh();

            this.lstStops.Items.Clear();
            this.lstStops.Refresh();

            if (clearStatations)
            {
                this.lstStations.Items.Clear();
                this.lstStations.Refresh();
            }
        }

        //
        // user has clicked on a stop for more info:
        //
        private void lstStops_SelectedIndexChanged(object sender, EventArgs e)
        {
            // sometimes this event fires, but nothing is selected...
            if (this.lstStops.SelectedIndex < 0)   // so return now in this case:
                return;

            //
            // clear GUI in case this fails:
            //
            ClearStopUI();

            //
            // now display info about this stop:
            //
            int Index = this.lstStops.SelectedIndex;
            var stops = Stopslist[Index];

            try
            {
                if (stops.ADA)
                    this.txtAccessible.Text = "Yes";
                else
                    this.txtAccessible.Text = "No";

                // direction of travel:
                this.txtDirection.Text = stops.Direction;

                this.txtLocation.Text = string.Format("({0:00.0000}, {1:00.0000})", stops.Latitude, stops.Longitude);
                string filename = this.txtDatabaseFilename.Text;
                BusinessTier.Business bizTier;
                bizTier = new BusinessTier.Business(filename);
                var getstops = bizTier.GetStopinfo(stops.ID);

                // display colors:
                foreach (string s in getstops.lines)
                {
                    this.lstLines.Items.Add(s);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error: '{0}'.", ex.Message);
                MessageBox.Show(msg);
            }
        }

        private void ClearStopUI()
        {
            this.txtAccessible.Clear();
            this.txtAccessible.Refresh();

            this.txtDirection.Clear();
            this.txtDirection.Refresh();

            this.txtLocation.Clear();
            this.txtLocation.Refresh();

            this.lstLines.Items.Clear();
            this.lstLines.Refresh();
        }

        //
        // Top-10 stations in terms of ridership:
        //
        private void top10StationsByRidershipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            // clear the UI of any current results:
            //
            ClearStationUI(true /*clear stations*/);

            try
            {
                string filename = this.txtDatabaseFilename.Text;
                BusinessTier.Business bizTier;
                bizTier = new BusinessTier.Business(filename);

                var stations = bizTier.GetTopStations(10);

                // display stations:
                foreach (var station in stations)
                {
                    this.lstStations.Items.Add(station.Name);
                }
                toolStripStatusLabel1.Text = string.Format("Number of stations:  {0:#,##0}", stations.Count);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error: '{0}'.", ex.Message);
                MessageBox.Show(msg);
            }
        }

    }//class
}//namespace
