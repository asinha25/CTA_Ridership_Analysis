//
// Name: Aditya Sinha
// NetID: asinha25
// Course: CS 341 | Project #08
//
// **BusinessTierLogic.cs**
//
// BusinessTier:  business logic, acting as interface between UI and data store.
//

using System;
using System.Collections.Generic;
using System.Data;

namespace BusinessTier
{
    //
    // Business:
    //
    public class Business
    {
        //
        // Fields:
        //
        private string _DBFile;
        private DataAccessTier.Data dataTier;

        ///
        /// <summary>
        /// Constructs a new instance of the business tier.  The format
        /// of the filename should be either |DataDirectory|\filename.mdf,
        /// or a complete Windows pathname.
        /// </summary>
        /// <param name="DatabaseFilename">Name of database file</param>
        /// 
        public Business(string DatabaseFilename)
        {
            _DBFile = DatabaseFilename;

            dataTier = new DataAccessTier.Data(DatabaseFilename);
        }

        ///
        /// <summary>
        ///  Opens and closes a connection to the database, e.g. to
        ///  startup the server and make sure all is well.
        /// </summary>
        /// <returns>true if successful, false if not</returns>
        /// 
        public bool TestConnection()
        {
            return dataTier.OpenCloseConnection();
        }

        ///
        /// <summary>
        /// Returns all the CTA Stations, ordered by name.
        /// </summary>
        /// <returns>Read-only list of CTAStation objects</returns>
        /// 
        public IReadOnlyList<CTAStation> GetStations()
        {
            List<CTAStation> stations = new List<CTAStation>();

            try
            {
                //
                // TODO!
                //
                string sql = string.Format(@"
                SELECT * 
                FROM Stations 
                ORDER BY Name ASC;
                ");
                DataSet data = dataTier.ExecuteNonScalarQuery(sql);
                foreach (DataRow row in data.Tables["TABLE"].Rows)
                {
                    var stationID = Convert.ToInt32(row["StationID"]);
                    var stationName = Convert.ToString(row["Name"]);
                    var station = new CTAStation(stationID, stationName);                                        
                    stations.Add(station);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in Business.GetStations: '{0}'", ex.Message);
                throw new ApplicationException(msg);
            }
            return stations;
        }

        public long FindDays(string Name, string Day)
        {
            long num = new long();
            try
            {
                string sql = string.Format(@"
                SELECT Riderships.StationID, TypeOfDay, Sum(DailyTotal) AS Total
                FROM Stations
                INNER JOIN Riderships
                ON Stations.StationID = Riderships.StationID
                WHERE Name = '{0}'
                GROUP BY Riderships.TypeOfDay, Riderships.StationID
                ORDER BY Riderships.TypeOfDay;
                ", Name);

                DataSet dataResult = dataTier.ExecuteNonScalarQuery(sql);

                if (Day == "A")
                {
                    num = Convert.ToInt32(dataResult.Tables["TABLE"].Rows[0]["Total"]);
                }
                if (Day == "U")
                {
                    num = Convert.ToInt32(dataResult.Tables["TABLE"].Rows[1]["Total"]);
                }
                if (Day == "W")
                {
                    num = Convert.ToInt32(dataResult.Tables["TABLE"].Rows[2]["Total"]);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in Business.FindDays: '{0}'", ex.Message);
                throw new ApplicationException(msg);
            }
            return num;
        }

        ///
        /// <summary>
        /// Returns the CTA Stops associated with a given station,
        /// ordered by name.
        /// </summary>
        /// <returns>Read-only list of CTAStop objects</returns>
        ///
        public IReadOnlyList<CTAStop> GetStops(int stationID)
        {
            List<CTAStop> stops = new List<CTAStop>();

            try
            {
                //
                // TODO!
                //
                string sql = string.Format(@"
                SELECT * FROM Stops
                WHERE StationID = {0} 
                ORDER BY Name ASC;", stationID);

                DataSet data = dataTier.ExecuteNonScalarQuery(sql);

                foreach (DataRow row in data.Tables["TABLE"].Rows)
                {
                    var stopID = Convert.ToInt32(row["StopID"]);
                    var stopName = Convert.ToString(row["Name"]);
                    var stationID2 = Convert.ToInt32(row["StationID"]);
                    var direction = Convert.ToString(row["Direction"]);
                    var ada = Convert.ToBoolean(row["ADA"]);
                    var latitude = Convert.ToDouble(row["Latitude"]);
                    var longitude = Convert.ToDouble(row["Longitude"]);

                    var stop = new CTAStop(stopID, stopName, stationID2, direction, ada, latitude, longitude);
                    stops.Add(stop);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in Business.GetStops: '{0}'", ex.Message);
                throw new ApplicationException(msg);
            }
            return stops;
        }

        public DataCTA GetStopinfo(int ID)
        {
            List<string> temp = new List<string>();
            var DataCTA = new DataCTA(temp);
            try
            {
                String sql = string.Format(@"
                SELECT Color FROM Lines 
                INNER JOIN StopDetails 
                ON Lines.LineID = StopDetails.LineID
                INNER JOIN Stops 
                ON StopDetails.StopID = Stops.StopID   
                WHERE Stops.StopID = {0} 
                ORDER BY Color ASC", ID);

                DataSet data = dataTier.ExecuteNonScalarQuery(sql);

                foreach (DataRow row in data.Tables["TABLE"].Rows)
                {
                    temp.Add(row["Color"].ToString());
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in Business.GetStopsInfo: '{0}'", ex.Message);
                throw new ApplicationException(msg);
            }
            return DataCTA;
        }

        public IReadOnlyList<TotalRiders> getTotalRidership(string name)
        {
            List<TotalRiders> riders = new List<TotalRiders>();

            try
            {
                string sql = string.Format(@"
                SELECT SUM(CONVERT(bigint, DailyTotal)) AS Total, 
                AVG(CONVERT(bigint, DailyTotal)) AS Average
                FROM Riderships
                INNER JOIN Stations
                ON Riderships.StationID = Stations.StationID
                WHERE Stations.Name ='{0}'", name);

                DataSet data = dataTier.ExecuteNonScalarQuery(sql);
                foreach (DataRow ds in data.Tables["TABLE"].Rows)
                {
                    var count = Convert.ToInt32(ds["Total"]);
                    var avg = Convert.ToDouble(ds["Average"]);
                    var ride = new TotalRiders(count, avg);
                    riders.Add(ride);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in Business.getTotalRidership: '{0}'", ex.Message);
                throw new ApplicationException(msg);
            }
            return riders;
        }

        public Double getRidersPer(string name)
        {
            Double dataTotal;

            try
            {
                string query = string.Format(@"
                SELECT SUM(CONVERT(Float, DailyTotal)) AS TotalRiders
                FROM Riderships
                INNER JOIN Stations 
                ON Riderships.StationID = Stations.StationID
                WHERE Name = '{0}'", name);

                string query2 = string.Format(@"
                SELECT SUM(CONVERT(float,Riderships.DailyTotal)) 
                FROM Riderships");

                dataTotal = ((Double)dataTier.ExecuteScalarQuery(query) / (Double)dataTier.ExecuteScalarQuery(query2)) * 100;
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in Business.getRidersPer: '{0}'", ex.Message);
                throw new ApplicationException(msg);
            }
            return dataTotal;
        }

        ///
        /// <summary>
        /// Returns the top N CTA Stations by ridership, 
        /// ordered by name.
        /// </summary>
        /// <returns>Read-only list of CTAStation objects</returns>
        /// 
        public IReadOnlyList<CTAStation> GetTopStations(int N)
        {
            if (N < 1)
                throw new ArgumentException("GetTopStations: N must be positive");

            List<CTAStation> stations = new List<CTAStation>();

            try
            {
                //
                // TODO!
                //
                string sql = string.Format(@"
                SELECT TOP {0} Riderships.DailyTotal AS Total, Stations.Name AS Name
                FROM Riderships
                INNER JOIN Stations 
                ON Stations.StationID = Riderships.StationID ", N);

                DataSet data = dataTier.ExecuteNonScalarQuery(sql);
                foreach (DataRow rs in data.Tables["TABLE"].Rows)
                {
                    var total = Convert.ToInt32(rs["Total"]);
                    var stationName2 = Convert.ToString(rs["Name"]);
                    var station = new CTAStation(total, stationName2);
                    stations.Add(station);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in Business.GetTopStations: '{0}'", ex.Message);
                throw new ApplicationException(msg);
            }
            return stations;
        }

    }//class
}//namespace
