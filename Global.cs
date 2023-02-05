using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using IRB.Collections.Generic;
using IRB.Revigo.Core;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using MySqlConnector;

namespace IRB.RevigoWeb
{
	public class Global
	{
		private static bool bDisposing = false;
		private static string sConnectionString = null;
		private static long lMinStatTicks = 0;
		private static long lMaxStatTicks = 0;
		private static GeneOntology oOntology = null;
		private static StreamWriter oLog = null;
		private static SpeciesAnnotationsList oSpeciesAnnotations = null;
		private static BDictionary<int, RevigoJob> oJobs = new BDictionary<int, RevigoJob>();
		private static object oJobLock = new object();
		private static object oConnectionLock = new object();
		private static string sEmailServer = null;
		private static string sEmailFrom = null;
		private static string sEmailTo = null;
		private static string sEmailCC = null;
		private static TimeSpan tsSessionTimeout = new TimeSpan(0, 30, 0);
		private static TimeSpan tsJobTimeout = new TimeSpan(0, 15, 0);
		private static string sRecaptchaPublicKey = null;
		private static string sRecaptchaPrivateKey = null;
		private static string sStatisticsKey = null;

		// JS control paths
		private static string sPathToJQuery = null;
		private static string sPathToJQueryUI = null;
		private static string sPathToJQueryUICSS = null;
		private static string sPathToD3 = null;
		private static string sPathToX3Dom = null;
		private static string sPathToX3DomCSS = null;
		private static string sPathToLCSwitch = null;
		private static string sPathToCytoscape = null;
		// Revigo JS control paths
		private static string sPathToCSS = null;
		private static string sPathToBubbleChart = null;
		private static string sPathToBubbleChartCSS = null;
		private static string sPathToTable = null;
		private static string sPathToTableCSS = null;
		private static string sPathToTreeMap = null;
		private static string sPathToX3DScatterplot = null;
		private static string sPathToCloudCSS = null;

		// Path to AddThis control
		private static string sPathToAddThis = null;

		public static void ApplicationStart(IConfiguration configuration)
		{
			// initialize log file
			try
			{
				oLog = new StreamWriter(new FileStream("Messages.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
			}
			catch
			{
				oLog = null;
			}

			try
			{
				sConnectionString = configuration.GetSection("DatabaseServer")["ConnectionString"];
			}
			catch { }

			DBConnection oConnection = new DBConnection(sConnectionString);
			if (oConnection != null && oConnection.IsConnected)
			{
				// try to reopen connection if its closed
				// try 10 times to open connection
				for (int i = 0; i < 10; i++)
				{
					if (oConnection.IsConnected)
					{
						// cache minTicks and maxTicks
						try
						{
							MySqlDataAdapter oAdapter = new MySqlDataAdapter("select Min(DateTimeTicks), Max(DateTimeTicks) from stats_h;", oConnection.Connection);
							DataTable dtData = new DataTable();
							oAdapter.Fill(dtData);
							oAdapter.Dispose();

							if (dtData.Rows.Count > 0 && dtData.Columns.Count >= 2)
							{
								lMinStatTicks = WebUtilities.TypeConverter.ToInt64(dtData.Rows[0][0]);
								lMaxStatTicks = WebUtilities.TypeConverter.ToInt64(dtData.Rows[0][1]);
							}
							break;
						}
						catch (Exception ex)
						{
							WriteToSystemLog(typeof(Global).Name, "Error establishing connection: " + ex.Message);
							lMinStatTicks = 0;
							lMaxStatTicks = 0;
						}
					}

					Thread.Sleep(100);

					try
					{
						oConnection.Close();
						oConnection.Open();
					}
					catch
					{ }
				}
			}

			try
			{
				sEmailServer = configuration.GetSection("AppSettings")["EmailServer"];
				sEmailTo = configuration.GetSection("AppSettings")["EmailFrom"];
				sEmailTo = configuration.GetSection("AppSettings")["EmailTo"];
				sEmailCC = configuration.GetSection("AppSettings")["EmailCC"];

			}
			catch { }

			try
			{
				tsJobTimeout = TimeSpan.Parse(configuration.GetSection("AppSettings")["JobTimeout"]);
				tsSessionTimeout = TimeSpan.Parse(configuration.GetSection("AppSettings")["SessionTimeout"]);
			}
			catch { }

			try
			{
				sRecaptchaPublicKey = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppSettings")["RecaptchaPublicKey"]);
				sRecaptchaPrivateKey = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppSettings")["RecaptchaPrivateKey"]);

			}
			catch { }

			try
			{
				sStatisticsKey = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppSettings")["StatisticsKey"]);

			}
			catch { }

			try
			{
				sPathToJQuery = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToJQuery"]);
				sPathToJQueryUI = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToJQueryUI"]);
				sPathToJQueryUICSS = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToJQueryUICSS"]);
				sPathToD3 = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToD3"]);
				sPathToX3Dom = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToX3Dom"]);
				sPathToX3DomCSS = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToX3DomCSS"]);
				sPathToLCSwitch = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToLCSwitch"]);
				sPathToCytoscape = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToCytoscape"]);

				sPathToCSS = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToCSS"]);
				sPathToBubbleChart = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToBubbleChart"]);
				sPathToBubbleChartCSS = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToBubbleChartCSS"]);
				sPathToTable = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToTable"]);
				sPathToTableCSS = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToTableCSS"]);
				sPathToTreeMap = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToTreeMap"]);
				sPathToX3DScatterplot = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToX3DScatterplot"]);
				sPathToCloudCSS = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToCloudCSS"]);

				sPathToAddThis = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToAddThis"]);
			}
			catch { }

			// try to load and intialize ontology object
			try
			{
                string sPath = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToGO"]);
                if (!string.IsNullOrEmpty(sPath))
                {
                    oOntology = GeneOntology.Deserialize(sPath);
                }
                else
                {
                    WriteToSystemLog(typeof(Global).Name, "Error: The path to Gene Ontology is empty");
                }
            }
            catch (Exception ex)
            {
                oOntology = null;
                WriteToSystemLog(typeof(Global).Name, "Error loading Gene Ontology: " + ex.Message);
            }

            // try to load and initialize Species annotations object
            try
            {
                string sPath = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToSpeciesAnnotations"]);
                if (!string.IsNullOrEmpty(sPath))
                {
                    oSpeciesAnnotations = SpeciesAnnotationsList.Deserialize(sPath);
                }
                else
                {
                    WriteToSystemLog(typeof(Global).Name, "Error: The path to Species Annotations is empty");
                }
            }
            catch (Exception ex)
            {
                WriteToSystemLog(typeof(Global).Name, "Error loading Species Annotations: " + ex.Message);
            }
        }

        /*protected void Session_End(object sender, EventArgs e)
        {
            // remove old jobs
            lock (oJobLock)
            {
                for (int i = 0; i < oJobs.Count; i++)
                {
                    if ((DateTime.Now - oJobs[i].Value.Expiration).TotalMinutes > 0)
                    {
                        oJobs.RemoveAt(i);
                        i--;
                    }
                }
            }
        }*/

        public static void ApplicationEnd()
        {
            bDisposing = true;

            if (oJobs != null)
            {
                for (int i = 0; i < oJobs.Count; i++)
                {
                    if (oJobs[i].Value.Worker.IsRunning)
                        oJobs[i].Value.Worker.Abort();
                }
                oJobs.Clear();
                oJobs = null;
            }

            if (oLog != null)
            {
                oLog.Close();
                oLog.Dispose();
                oLog = null;
            }
        }

        public static string ConnectionString
        {
            get { return sConnectionString; }
        }

        public static long MinStatTicks
        {
            get
            {
                return lMinStatTicks;
            }
        }

        public static long MaxStatTicks
        {
            get
            {
                return lMaxStatTicks;
            }
        }

        public static GeneOntology Ontology
        {
            get
            {
                return oOntology;
            }
        }

        public static SpeciesAnnotationsList SpeciesAnnotations
        {
            get
            {
                return oSpeciesAnnotations;
            }
        }

        public static BDictionary<int, RevigoJob> Jobs
        {
            get
            {
                return oJobs;
            }
        }

        public static TimeSpan SessionTimeout
        {
            get
            {
                return tsSessionTimeout;
            }
        }

        public static TimeSpan JobTimeout
        {
            get
            {
                return tsJobTimeout;
            }
        }

		public static string EmailServer
		{ 
			get{return sEmailServer; }
		}

		public static string EmailFrom
		{
			get { return sEmailFrom; }
		}

		public static string EmailTo
		{
			get { return sEmailTo; }
		}

		public static string EmailCC
		{
			get { return sEmailCC; }
		}

		public static string RecaptchaPublicKey
		{
			get { return sRecaptchaPublicKey; }
		}

		public static string RecaptchaSecretKey
		{
			get { return sRecaptchaPrivateKey; }
		}

		public static string StatisticsKey
		{
			get { return sStatisticsKey; }
		}

		// JS control paths
		public static string PathToJQuery { get { return sPathToJQuery; } }
		public static string PathToJQueryUI { get { return sPathToJQueryUI; } }
		public static string PathToJQueryUICSS { get { return sPathToJQueryUICSS; } }
		public static string PathToD3 { get { return sPathToD3; } }
		public static string PathToX3Dom { get { return sPathToX3Dom; } }
		public static string PathToX3DomCSS { get { return sPathToX3DomCSS; } }
		public static string PathToLCSwitch { get { return sPathToLCSwitch; } }
		public static string PathToCytoscape { get { return sPathToCytoscape; } }

		// Revigo JS control paths
		public static string PathToCSS { get { return sPathToCSS; } }
		public static string PathToBubbleChart { get { return sPathToBubbleChart; } }
		public static string PathToBubbleChartCSS { get { return sPathToBubbleChartCSS; } }
		public static string PathToTable { get { return sPathToTable; } }
		public static string PathToTableCSS { get { return sPathToTableCSS; } }
		public static string PathToTreeMap { get { return sPathToTreeMap; } }
		public static string PathToX3DScatterplot { get { return sPathToX3DScatterplot; } }
		public static string PathToCloudCSS { get { return sPathToCloudCSS; } }

		// Path to AddThis control
		public static string PathToAddThis { get { return sPathToAddThis; } }

		public static int CreateNewJob(RequestSourceEnum requestSource, string data, double cutoff, 
            ValueTypeEnum valueType, SpeciesAnnotations annotations, SemanticSimilarityScoreEnum measure, bool removeObsolete)
        {
            int iJobID = -1;

            if (!bDisposing)
            {
                iJobID = Math.Abs(Environment.TickCount);
                RevigoWorker oWorker;

                lock (oJobLock)
                {
                    // remove old jobs
                    for (int i = 0; i < oJobs.Count; i++)
                    {
                        if ((DateTime.Now - oJobs[i].Value.Expiration).TotalMinutes > 0)
                        {
                            oJobs.RemoveAt(i);
                            i--;
                        }
                    }

                    // get unique Job ID
                    while (oJobs.ContainsKey(iJobID) || iJobID < 1)
                    {
                        iJobID++;
                    }

                    oWorker = new RevigoWorker(iJobID, Global.Ontology, annotations, Global.JobTimeout, 
                        requestSource, data, cutoff, valueType, measure, removeObsolete);

                    oJobs.Add(iJobID, new RevigoJob(iJobID, DateTime.Now.AddMinutes(Global.SessionTimeout.TotalMinutes + 1.0), oWorker));
                }
            }

            return iJobID;
        }

        public static int StartNewJob(RequestSourceEnum requestSource, string data, double cutoff, ValueTypeEnum valueType, SpeciesAnnotations annotations, SemanticSimilarityScoreEnum measure, bool removeObsolete)
        {
            int iJobID = -1;

            if (!bDisposing)
            {
                iJobID = Math.Abs(Environment.TickCount);
                RevigoWorker oWorker;

                lock (oJobLock)
                {
                    // remove old jobs
                    for (int i = 0; i < oJobs.Count; i++)
                    {
                        if ((DateTime.Now - oJobs[i].Value.Expiration).TotalMinutes > 0)
                        {
                            oJobs.RemoveAt(i);
                            i--;
                        }
                    }

                    // get unique Job ID
                    while (oJobs.ContainsKey(iJobID) || iJobID < 1)
                    {
                        iJobID++;
                    }

                    oWorker = new RevigoWorker(iJobID, Global.Ontology, annotations, Global.JobTimeout, 
                        requestSource, data, cutoff, valueType, measure, removeObsolete);
                    oJobs.Add(iJobID, new RevigoJob(iJobID, DateTime.Now.AddMinutes(Global.SessionTimeout.TotalMinutes + 1.0), oWorker));
                }

                oWorker.OnFinish += oWorker_OnFinish;
                oWorker.Start();
            }

            return iJobID;
        }

        private static void oWorker_OnFinish(object sender, EventArgs e)
        {
            // update our statistics
            if (sender is RevigoWorker)
            {
                RevigoWorker worker = (RevigoWorker)sender;

                lock (oConnectionLock)
                {
                    DBConnection oConnection = new DBConnection(sConnectionString);
                    if (oConnection != null && oConnection.IsConnected)
                    {
                        if (!worker.HasUserErrors && !worker.HasDeveloperErrors)
                        {
                            try
                            {
                                DateTime dtCreateTime = worker.CreateDateTime;

                                // update our statistics
                                MySqlCommand oCommand = new MySqlCommand(
                                    "insert into stats (DateTimeTicks, JobID, RequestSource, Cutoff, ValueType, SpeciesTaxon, Measure, RemoveObsolete, " +
                                    "BiologicalProcess, CellularComponent, MolecularFunction, ExecTicks, Count, NSCount) values " +
                                    "(?vDateTimeTicks, ?vJobID, ?vRequestSource, ?vCutoff, ?vValueType, ?vSpeciesTaxon, ?vMeasure, " +
                                    "?vRemoveObsolete, ?vBiologicalProcess, ?vCellularComponent, ?vMolecularFunction, ?vExecTicks, ?vCount, " +
                                    "?vNSCount);",
                                    oConnection.Connection);
                                oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = worker.CreateDateTime.Ticks;
                                oCommand.Parameters.Add("?vJobID", MySqlDbType.Int32).Value = worker.JobID;
                                oCommand.Parameters.Add("?vRequestSource", MySqlDbType.Int32).Value = (int)worker.RequestSource;
                                oCommand.Parameters.Add("?vCutoff", MySqlDbType.Int32).Value = (int)(worker.CutOff * 10.0);
                                oCommand.Parameters.Add("?vValueType", MySqlDbType.Int32).Value = (int)worker.ValueType;
                                oCommand.Parameters.Add("?vSpeciesTaxon", MySqlDbType.Int32).Value = (int)worker.Annotations.TaxonID;
                                oCommand.Parameters.Add("?vMeasure", MySqlDbType.Int32).Value = (int)worker.Measure;
                                oCommand.Parameters.Add("?vRemoveObsolete", MySqlDbType.Int32).Value = (int)(worker.RemoveObsolete ? 1 : 0);
                                oCommand.Parameters.Add("?vBiologicalProcess", MySqlDbType.Int64).Value =
                                    (long)((worker.BPVisualizer != null) ? worker.BPVisualizer.Terms.Length : 0);
                                oCommand.Parameters.Add("?vCellularComponent", MySqlDbType.Int64).Value =
                                    (long)((worker.CCVisualizer != null) ? worker.CCVisualizer.Terms.Length : 0);
                                oCommand.Parameters.Add("?vMolecularFunction", MySqlDbType.Int64).Value =
                                    (long)((worker.MFVisualizer != null) ? worker.MFVisualizer.Terms.Length : 0);
                                oCommand.Parameters.Add("?vExecTicks", MySqlDbType.Int64).Value = worker.ExecutingTime.Ticks;
                                oCommand.Parameters.Add("?vCount", MySqlDbType.Int64).Value = (long)1;
                                oCommand.Parameters.Add("?vNSCount", MySqlDbType.Double).Value =
                                    (worker.HasBPVisualizer ? 1.0 : 0.0) + (worker.HasMFVisualizer ? 1.0 : 0.0) +
                                    (worker.HasCCVisualizer ? 1.0 : 0.0);

                                oCommand.ExecuteNonQuery();

                                // aggregate statistics by hours
                                DateTime createDateTime = new DateTime(dtCreateTime.Year, dtCreateTime.Month, dtCreateTime.Day, dtCreateTime.Hour, 0, 0);
                                oCommand = new MySqlCommand("select ID " +
                                    "from stats_h " +
                                    "where (DateTimeTicks=?vDateTimeTicks) and (RequestSource=?vRequestSource) and (Cutoff=?vCutoff) and (ValueType=?vValueType) and " +
                                    "(SpeciesTaxon=?vSpeciesTaxon) and (Measure=?vMeasure) and (RemoveObsolete=?vRemoveObsolete) and (NSCount=?vNSCount);",
                                    oConnection.Connection);
                                oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = createDateTime.Ticks;
                                oCommand.Parameters.Add("?vRequestSource", MySqlDbType.Int32).Value = (int)worker.RequestSource;
                                oCommand.Parameters.Add("?vCutoff", MySqlDbType.Int32).Value = (int)(worker.CutOff * 10.0);
                                oCommand.Parameters.Add("?vValueType", MySqlDbType.Int32).Value = (int)worker.ValueType;
                                oCommand.Parameters.Add("?vSpeciesTaxon", MySqlDbType.Int32).Value = (int)worker.Annotations.TaxonID;
                                oCommand.Parameters.Add("?vMeasure", MySqlDbType.Int32).Value = (int)worker.Measure;
                                oCommand.Parameters.Add("?vRemoveObsolete", MySqlDbType.Int32).Value = (int)(worker.RemoveObsolete ? 1 : 0);
                                oCommand.Parameters.Add("?vNSCount", MySqlDbType.Double).Value =
                                    (worker.HasBPVisualizer ? 1.0 : 0.0) + (worker.HasMFVisualizer ? 1.0 : 0.0) +
                                    (worker.HasCCVisualizer ? 1.0 : 0.0);
                                //oCommand.CommandTimeout = 240;
                                object id = oCommand.ExecuteScalar();

                                if (id != null && id != DBNull.Value)
                                {
                                    // we are aggregating
                                    oCommand = new MySqlCommand(
                                        "update stats_h set BiologicalProcess=BiologicalProcess+?vBiologicalProcess, " +
                                        "CellularComponent=CellularComponent+?vCellularComponent, " +
                                        "MolecularFunction=MolecularFunction+?vMolecularFunction, " +
                                        "ExecTicks=ExecTicks+?vExecTicks, Count=Count+1 " +
                                        "where ID=?id;", oConnection.Connection);
                                    oCommand.Parameters.Add("?vBiologicalProcess", MySqlDbType.Int64).Value = (long)worker.BPVisualizer.Terms.Length;
                                    oCommand.Parameters.Add("?vCellularComponent", MySqlDbType.Int64).Value = (long)worker.CCVisualizer.Terms.Length;
                                    oCommand.Parameters.Add("?vMolecularFunction", MySqlDbType.Int64).Value = (long)worker.MFVisualizer.Terms.Length;
                                    oCommand.Parameters.Add("?vExecTicks", MySqlDbType.Int64).Value = worker.ExecutingTime.Ticks;
                                    oCommand.Parameters.Add("?id", MySqlDbType.Int64).Value = Convert.ToInt64(id);

                                    oCommand.ExecuteNonQuery();
                                }
                                else
                                {
                                    // we are creating initial value
                                    oCommand = new MySqlCommand(
                                        "insert into stats_h (DateTimeTicks, RequestSource, Cutoff, ValueType, SpeciesTaxon, Measure, RemoveObsolete, " +
                                        "BiologicalProcess, CellularComponent, MolecularFunction, ExecTicks, Count, NSCount) values " +
                                        "(?vDateTimeTicks, ?vRequestSource, ?vCutoff, ?vValueType, ?vSpeciesTaxon, ?vMeasure, ?vRemoveObsolete, " +
                                        "?vBiologicalProcess, ?vCellularComponent, ?vMolecularFunction, ?vExecTicks, ?vCount, ?vNSCount);",
                                        oConnection.Connection);
                                    oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = createDateTime.Ticks;
                                    oCommand.Parameters.Add("?vRequestSource", MySqlDbType.Int32).Value = (int)worker.RequestSource;
                                    oCommand.Parameters.Add("?vCutoff", MySqlDbType.Int32).Value = (int)(worker.CutOff * 10.0);
                                    oCommand.Parameters.Add("?vValueType", MySqlDbType.Int32).Value = (int)worker.ValueType;
                                    oCommand.Parameters.Add("?vSpeciesTaxon", MySqlDbType.Int32).Value = (int)worker.Annotations.TaxonID;
                                    oCommand.Parameters.Add("?vMeasure", MySqlDbType.Int32).Value = (int)worker.Measure;
                                    oCommand.Parameters.Add("?vRemoveObsolete", MySqlDbType.Int32).Value = (int)(worker.RemoveObsolete ? 1 : 0);
                                    oCommand.Parameters.Add("?vBiologicalProcess", MySqlDbType.Int64).Value = (long)worker.BPVisualizer.Terms.Length;
                                    oCommand.Parameters.Add("?vCellularComponent", MySqlDbType.Int64).Value = (long)worker.CCVisualizer.Terms.Length;
                                    oCommand.Parameters.Add("?vMolecularFunction", MySqlDbType.Int64).Value = (long)worker.MFVisualizer.Terms.Length;
                                    oCommand.Parameters.Add("?vExecTicks", MySqlDbType.Int64).Value = worker.ExecutingTime.Ticks;
                                    oCommand.Parameters.Add("?vCount", MySqlDbType.Int64).Value = (long)1;
                                    oCommand.Parameters.Add("?vNSCount", MySqlDbType.Double).Value =
                                        (worker.HasBPVisualizer ? 1.0 : 0.0) + (worker.HasMFVisualizer ? 1.0 : 0.0) +
                                        (worker.HasCCVisualizer ? 1.0 : 0.0);

                                    oCommand.ExecuteNonQuery();

                                    lMaxStatTicks = createDateTime.Ticks;
                                }
                            }
                            catch
                            {
                                //Global.WriteToSystemLog(typeof(Global).FullName, "Error in oWorker_OnFinish: " + ex.Message);
                            }
                        }

                        oConnection.Close();
                    }
                }

                // for debugging
                if (worker.HasDeveloperWarnings || worker.HasDeveloperErrors)
                {
                    WebUtilities.Email.SendEmailNotification(worker);
                }
            }
        }

        public static void UpdateJobUsageStats(RevigoWorker worker, string type, string ns)
        {
            string sNamespace = null;
            int iNamespace = -1;

            lock (oConnectionLock)
            {
                DBConnection oConnection = new DBConnection(sConnectionString);
                if (oConnection != null && oConnection.IsConnected)
                {
                    if (!string.IsNullOrEmpty(ns) && int.TryParse(ns, out iNamespace))
                    {
                        switch (iNamespace)
                        {
                            case 1:
                                sNamespace = "_BP";
                                break;
                            case 2:
                                sNamespace = "_CC";
                                break;
                            case 3:
                                sNamespace = "_MF";
                                break;
                        }
                    }

                    string sField;
                    switch (type.ToLower())
                    {
                        case "jtable":
                        case "table":
                            if (string.IsNullOrEmpty(sNamespace))
                                return;
                            sField = "Table" + sNamespace;
                            break;
                        case "jscatterplot":
                        case "scatterplot":
                        case "rscatterplot":
                            if (string.IsNullOrEmpty(sNamespace))
                                return;
                            sField = "Scatterplot" + sNamespace;
                            break;
                        case "jscatterplot3d":
                        case "scatterplot3d":
                            if (string.IsNullOrEmpty(sNamespace))
                                return;
                            sField = "Scatterplot3D" + sNamespace;
                            break;
                        case "jtreemap":
                        case "treemap":
                        case "rtreemap":
                            if (string.IsNullOrEmpty(sNamespace))
                                return;
                            sField = "TreeMap" + sNamespace;
                            break;
                        case "jcytoscape":
                        case "xgmml":
                            if (string.IsNullOrEmpty(sNamespace))
                                return;
                            sField = "Cytoscape" + sNamespace;
                            break;
                        case "simmat":
                            if (string.IsNullOrEmpty(sNamespace))
                                return;
                            sField = "SimMat" + sNamespace;
                            break;
                        case "jclouds":
                            sField = "TagClouds";
                            break;
                        default:
                            return;
                    }

                    try
                    {
                        // get previous field value
                        MySqlCommand oCommand = new MySqlCommand(
                            "select " + sField + " from stats where (DateTimeTicks=?vDateTimeTicks and JobID=?vJobID);",
                            oConnection.Connection);
                        oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = worker.CreateDateTime.Ticks;
                        oCommand.Parameters.Add("?vJobID", MySqlDbType.Int32).Value = worker.JobID;

                        object value = oCommand.ExecuteScalar();

                        if (value != null && value != DBNull.Value)
                        {
                            double dValue = Convert.ToDouble(value);
                            if (dValue == 0.0)
                            {
                                // update the value
                                oCommand = new MySqlCommand(
                                    "update stats set " + sField + "=1.0 where (DateTimeTicks=?vDateTimeTicks) and (JobID=?vJobID);",
                                    oConnection.Connection);
                                oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = worker.CreateDateTime.Ticks;
                                oCommand.Parameters.Add("?vJobID", MySqlDbType.Int32).Value = worker.JobID;
                                oCommand.ExecuteNonQuery();

                                // update aggregated value
                                DateTime createDateTime = new DateTime(worker.CreateDateTime.Year, worker.CreateDateTime.Month,
                                    worker.CreateDateTime.Day, worker.CreateDateTime.Hour, 0, 0);
                                oCommand = new MySqlCommand("update stats_h set " + sField + "=" + sField + "+1.0 " +
                                    "where (DateTimeTicks=?vDateTimeTicks) and (RequestSource=?vRequestSource) and (Cutoff=?vCutoff) and (ValueType=?vValueType) and " +
                                    "(SpeciesTaxon=?vSpeciesTaxon) and (Measure=?vMeasure) and (RemoveObsolete=?vRemoveObsolete) and (NSCount=?vNSCount);",
                                    oConnection.Connection);
                                oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = createDateTime.Ticks;
                                oCommand.Parameters.Add("?vRequestSource", MySqlDbType.Int32).Value = (int)worker.RequestSource;
                                oCommand.Parameters.Add("?vCutoff", MySqlDbType.Int32).Value = (int)(worker.CutOff * 10.0);
                                oCommand.Parameters.Add("?vValueType", MySqlDbType.Int32).Value = (int)worker.ValueType;
                                oCommand.Parameters.Add("?vSpeciesTaxon", MySqlDbType.Int32).Value = (int)worker.Annotations.TaxonID;
                                oCommand.Parameters.Add("?vMeasure", MySqlDbType.Int32).Value = (int)worker.Measure;
                                oCommand.Parameters.Add("?vRemoveObsolete", MySqlDbType.Int32).Value = (int)(worker.RemoveObsolete ? 1 : 0);
                                oCommand.Parameters.Add("?vNSCount", MySqlDbType.Double).Value =
                                    (worker.HasBPVisualizer ? 1.0 : 0.0) + (worker.HasMFVisualizer ? 1.0 : 0.0) +
                                    (worker.HasCCVisualizer ? 1.0 : 0.0);
                                //oCommand.CommandTimeout = 240;
                                oCommand.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToSystemLog(typeof(Global).FullName, string.Format("Error in UpdateJobUsageStats: '{0}', Stack trace: '{1}'",
                            ex.Message, ex.StackTrace));
                    }
                    oConnection.Close();
                }
            }
        }

        public static void WriteToSystemLog(string source, string message)
        {
            if (oLog != null)
            {
                try
                {
                    oLog.WriteLine("[{0};{1}] {2}", DateTime.Now, source, message);
                    oLog.Flush();
                }
                catch
                { }
            }
        }
    }
}