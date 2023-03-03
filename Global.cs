using System.Data;
using System.Net.Mail;
using System.Text;
using IRB.Collections.Generic;
using IRB.Revigo.Core;
using IRB.Revigo.Core.Databases;
using IRB.Revigo.Core.Worker;
#if WEB_STATISTICS
using MySqlConnector;
#endif

namespace IRB.RevigoWeb
{
	public class Global
	{
		private static bool bDisposing = false;
		private static string? sConnectionString = null;
		private static long lMinStatTicks = 0;
		private static long lMaxStatTicks = 0;
		private static string? sPathToLog = null;
		private static StreamWriter? oLog = null;
		private static GeneOntology? oOntology = null;
		private static SpeciesAnnotationList? oSpeciesAnnotations = null;
		private static BDictionary<int, RevigoJob> oJobs = new BDictionary<int, RevigoJob>();
		private static object oJobLock = new object();
		private static string? sEmailServer = null;
		private static string? sEmailFrom = null;
		private static string? sEmailTo = null;
		private static string? sEmailCC = null;
		private static string? sRecaptchaPublicKey = null;
		private static string? sRecaptchaPrivateKey = null;
		private static string? sStatisticsKey = null;
		private static TimeSpan tsJobTimeout = new TimeSpan(0, 15, 0);
		private static TimeSpan tsSessionTimeout = new TimeSpan(0, 30, 0);
		private static TimeSpan tsJobSessionTimeout = new TimeSpan(0, 20, 0);

		// JS control paths
		private static string? sPathToJQuery = null;
		private static string? sPathToJQueryUI = null;
		private static string? sPathToJQueryUICSS = null;
		private static string? sPathToD3 = null;
		private static string? sPathToX3Dom = null;
		private static string? sPathToX3DomCSS = null;
		private static string? sPathToLCSwitch = null;
		private static string? sPathToCytoscape = null;
		// Revigo JS control paths
		private static string? sPathToCSS = null;
		private static string? sPathToBubbleChart = null;
		private static string? sPathToBubbleChartCSS = null;
		private static string? sPathToTable = null;
		private static string? sPathToTableCSS = null;
		private static string? sPathToTreeMap = null;
		private static string? sPathToX3DScatterplot = null;
		private static string? sPathToCloudCSS = null;

		// Path to AddThis control
		private static string? sPathToAddThis = null;

		public static void StartApplication(IConfiguration? configuration)
		{
			if (configuration == null)
				return;

			// initialize log file
			try
			{
				sPathToLog = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToLog"]);
			}
			catch { }

			try
			{
				if (!string.IsNullOrEmpty(sPathToLog))
				{
					oLog = new StreamWriter(new FileStream(sPathToLog, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
				}
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

#if WEB_STATISTICS
			if (!string.IsNullOrEmpty(sConnectionString))
			{
				try
				{
					using (MySqlConnection oConnection = new MySqlConnection(sConnectionString))
					{
						oConnection.Open();

						// cache minTicks and maxTicks
						using (MySqlDataAdapter oAdapter = new MySqlDataAdapter("select Min(DateTimeTicks), Max(DateTimeTicks) from stats_h;", oConnection))
						{
							DataTable dtData = new DataTable();
							oAdapter.Fill(dtData);
							oAdapter.Dispose();

							if (dtData.Rows.Count > 0 && dtData.Columns.Count >= 2)
							{
								lMinStatTicks = WebUtilities.TypeConverter.ToInt64(dtData.Rows[0][0]);
								lMaxStatTicks = WebUtilities.TypeConverter.ToInt64(dtData.Rows[0][1]);
							}
						}
					}
				}
				catch (Exception ex)
				{
					WriteToSystemLog($"{typeof(Global).GetType().Name}.StartApplication", $"Message: '{ex.Message}', Stack trace: '{ex.StackTrace}'");
				}
			}
#endif
			try
			{
				sEmailServer = configuration.GetSection("AppSettings")["EmailServer"];
				sEmailFrom = configuration.GetSection("AppSettings")["EmailFrom"];
				sEmailTo = configuration.GetSection("AppSettings")["EmailTo"];
				sEmailCC = configuration.GetSection("AppSettings")["EmailCC"];

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
				tsJobTimeout = TimeSpan.Parse(configuration.GetSection("AppSettings")["JobTimeout"]);
				tsSessionTimeout = TimeSpan.Parse(configuration.GetSection("AppSettings")["SessionTimeout"]);
				tsJobSessionTimeout = TimeSpan.Parse(configuration.GetSection("AppSettings")["JobSessionTimeout"]);
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
				string? sPath = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToGO"]);
				if (!string.IsNullOrEmpty(sPath))
				{
					oOntology = GeneOntology.Deserialize(sPath);
				}
				else
				{
					WriteToSystemLog($"{typeof(Global).GetType().Name}.StartApplication", "Error: The path to Gene Ontology is empty");
				}
			}
			catch (Exception ex)
			{
				oOntology = null;
				WriteToSystemLog($"{typeof(Global).GetType().Name}.StartApplication", "Error loading Gene Ontology: " + ex.Message);
			}

			// try to load and initialize Species annotations object
			try
			{
				string? sPath = WebUtilities.TypeConverter.ToString(configuration.GetSection("AppPaths")["PathToSpeciesAnnotations"]);
				if (!string.IsNullOrEmpty(sPath))
				{
					oSpeciesAnnotations = SpeciesAnnotationList.Deserialize(sPath);
				}
				else
				{
					WriteToSystemLog($"{typeof(Global).GetType().Name}.StartApplication", "Error: The path to Species Annotations is empty");
				}
			}
			catch (Exception ex)
			{
				WriteToSystemLog($"{typeof(Global).GetType().Name}.StartApplication", "Error loading Species Annotations: " + ex.Message);
			}
		}

		public static void StopApplication()
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
			}

			if (oLog != null)
			{
				oLog.Close();
				oLog.Dispose();
				oLog = null;
			}
		}

		public static string? ConnectionString { get { return sConnectionString; } }

		public static long MinStatTicks { get { return lMinStatTicks; } }

		public static long MaxStatTicks { get { return lMaxStatTicks; } }

		public static GeneOntology? Ontology { get { return oOntology; } }

		public static SpeciesAnnotationList? SpeciesAnnotations { get { return oSpeciesAnnotations; } }

		public static BDictionary<int, RevigoJob> Jobs { get { return oJobs; } }

		public static TimeSpan JobTimeout { get { return tsJobTimeout; } }

		public static TimeSpan SessionTimeout { get { return tsSessionTimeout; } }

		public static TimeSpan JobSessionTimeout { get { return tsJobSessionTimeout; } }

		public static string? EmailServer { get { return sEmailServer; } }

		public static string? EmailFrom { get { return sEmailFrom; } }

		public static string? EmailTo { get { return sEmailTo; } }

		public static string? EmailCC { get { return sEmailCC; } }

		public static string? RecaptchaPublicKey { get { return sRecaptchaPublicKey; } }

		public static string? RecaptchaSecretKey { get { return sRecaptchaPrivateKey; } }

		public static string? StatisticsKey { get { return sStatisticsKey; } }

		// JS control paths
		public static string? PathToJQuery { get { return sPathToJQuery; } }
		public static string? PathToJQueryUI { get { return sPathToJQueryUI; } }
		public static string? PathToJQueryUICSS { get { return sPathToJQueryUICSS; } }
		public static string? PathToD3 { get { return sPathToD3; } }
		public static string? PathToX3Dom { get { return sPathToX3Dom; } }
		public static string? PathToX3DomCSS { get { return sPathToX3DomCSS; } }
		public static string? PathToLCSwitch { get { return sPathToLCSwitch; } }
		public static string? PathToCytoscape { get { return sPathToCytoscape; } }

		// Revigo JS control paths
		public static string? PathToCSS { get { return sPathToCSS; } }
		public static string? PathToBubbleChart { get { return sPathToBubbleChart; } }
		public static string? PathToBubbleChartCSS { get { return sPathToBubbleChartCSS; } }
		public static string? PathToTable { get { return sPathToTable; } }
		public static string? PathToTableCSS { get { return sPathToTableCSS; } }
		public static string? PathToTreeMap { get { return sPathToTreeMap; } }
		public static string? PathToX3DScatterplot { get { return sPathToX3DScatterplot; } }
		public static string? PathToCloudCSS { get { return sPathToCloudCSS; } }

		// Path to AddThis control
		public static string? PathToAddThis { get { return sPathToAddThis; } }

		public static int CreateNewJob(RequestSourceEnum requestSource, string data, double cutoff,
			ValueTypeEnum valueType, SpeciesAnnotations annotations, SemanticSimilarityTypeEnum measure, bool removeObsolete)
		{
			int iJobID = -1;

			if (!bDisposing && Global.Ontology != null)
			{
				iJobID = Math.Abs(Environment.TickCount);
				RevigoWorker oWorker;

				lock (oJobLock)
				{
					// remove old jobs
					for (int i = 0; i < oJobs.Count; i++)
					{
						if (oJobs[i].Value.IsExpired)
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

		public static int StartNewJob(RequestSourceEnum requestSource, string data, double cutoff, ValueTypeEnum valueType, SpeciesAnnotations annotations, SemanticSimilarityTypeEnum measure, bool removeObsolete)
		{
			int iJobID = -1;

			if (!bDisposing && Global.Ontology != null)
			{
				iJobID = Math.Abs(Environment.TickCount);
				RevigoWorker oWorker;

				lock (oJobLock)
				{
					// remove old jobs
					for (int i = 0; i < oJobs.Count; i++)
					{
						if (oJobs[i].Value.IsExpired)
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

		public static void RemoveJob(int jobID)
		{
			lock (oJobLock)
			{
				if (oJobs.ContainsKey(jobID))
				{
					oJobs.RemoveByKey(jobID);
				}
			}
		}

		public static void RemoveJob(RevigoJob job)
		{
			RemoveJob(job.ID);
		}

		private static void oWorker_OnFinish(object? sender, EventArgs e)
		{
			// update our statistics
			if (sender is RevigoWorker)
			{
				RevigoWorker worker = (RevigoWorker)sender;

#if WEB_STATISTICS
				if (!string.IsNullOrEmpty(sConnectionString))
				{
					try
					{
						using (MySqlConnection oConnection = new MySqlConnection(sConnectionString))
						{
							oConnection.Open();
							if (!worker.HasUserErrors && !worker.HasDeveloperErrors)
							{
								DateTime dtCreateTime = worker.CreateDateTime;

								// update our statistics
								using (MySqlCommand oCommand = new MySqlCommand(
									"insert into stats (DateTimeTicks, JobID, RequestSource, Cutoff, ValueType, SpeciesTaxon, Measure, RemoveObsolete, " +
									"BiologicalProcess, CellularComponent, MolecularFunction, ExecTicks, Count, NSCount) values " +
									"(?vDateTimeTicks, ?vJobID, ?vRequestSource, ?vCutoff, ?vValueType, ?vSpeciesTaxon, ?vMeasure, " +
									"?vRemoveObsolete, ?vBiologicalProcess, ?vCellularComponent, ?vMolecularFunction, ?vExecTicks, ?vCount, " +
									"?vNSCount);",
									oConnection))
								{
									oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = worker.CreateDateTime.Ticks;
									oCommand.Parameters.Add("?vJobID", MySqlDbType.Int32).Value = worker.JobID;
									oCommand.Parameters.Add("?vRequestSource", MySqlDbType.Int32).Value = (int)worker.RequestSource;
									oCommand.Parameters.Add("?vCutoff", MySqlDbType.Int32).Value = (int)(worker.CutOff * 10.0);
									oCommand.Parameters.Add("?vValueType", MySqlDbType.Int32).Value = (int)worker.ValueType;
									oCommand.Parameters.Add("?vSpeciesTaxon", MySqlDbType.Int32).Value = (int)((worker.Annotations == null) ? 0 : worker.Annotations.TaxonID);
									oCommand.Parameters.Add("?vMeasure", MySqlDbType.Int32).Value = (int)worker.SemanticSimilarity;
									oCommand.Parameters.Add("?vRemoveObsolete", MySqlDbType.Int32).Value = (int)(worker.RemoveObsolete ? 1 : 0);
									oCommand.Parameters.Add("?vBiologicalProcess", MySqlDbType.Int64).Value =
										(long)(worker.BPVisualizer.IsEmpty ? 0 : worker.BPVisualizer.Terms.Count);
									oCommand.Parameters.Add("?vCellularComponent", MySqlDbType.Int64).Value =
										(long)(worker.CCVisualizer.IsEmpty ? 0 : worker.CCVisualizer.Terms.Count);
									oCommand.Parameters.Add("?vMolecularFunction", MySqlDbType.Int64).Value =
										(long)(worker.MFVisualizer.IsEmpty ? 0 : worker.MFVisualizer.Terms.Count);
									oCommand.Parameters.Add("?vExecTicks", MySqlDbType.Int64).Value = worker.ExecutingTime.Ticks;
									oCommand.Parameters.Add("?vCount", MySqlDbType.Int64).Value = (long)1;
									oCommand.Parameters.Add("?vNSCount", MySqlDbType.Double).Value =
										(worker.BPVisualizer.IsEmpty ? 0.0 : 1.0) + (worker.MFVisualizer.IsEmpty ? 0.0 : 1.0) +
										(worker.CCVisualizer.IsEmpty ? 0.0 : 1.0);

									oCommand.ExecuteNonQuery();
								}

								// aggregate statistics by hours
								object? id = null;
								DateTime createDateTime = new DateTime(dtCreateTime.Year, dtCreateTime.Month, dtCreateTime.Day, dtCreateTime.Hour, 0, 0);
								using (MySqlCommand oCommand = new MySqlCommand("select ID " +
									"from stats_h " +
									"where (DateTimeTicks=?vDateTimeTicks) and (RequestSource=?vRequestSource) and (Cutoff=?vCutoff) and (ValueType=?vValueType) and " +
									"(SpeciesTaxon=?vSpeciesTaxon) and (Measure=?vMeasure) and (RemoveObsolete=?vRemoveObsolete) and (NSCount=?vNSCount);",
									oConnection))
								{
									oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = createDateTime.Ticks;
									oCommand.Parameters.Add("?vRequestSource", MySqlDbType.Int32).Value = (int)worker.RequestSource;
									oCommand.Parameters.Add("?vCutoff", MySqlDbType.Int32).Value = (int)(worker.CutOff * 10.0);
									oCommand.Parameters.Add("?vValueType", MySqlDbType.Int32).Value = (int)worker.ValueType;
									oCommand.Parameters.Add("?vSpeciesTaxon", MySqlDbType.Int32).Value = (int)((worker.Annotations == null) ? 0 : worker.Annotations.TaxonID);
									oCommand.Parameters.Add("?vMeasure", MySqlDbType.Int32).Value = (int)worker.SemanticSimilarity;
									oCommand.Parameters.Add("?vRemoveObsolete", MySqlDbType.Int32).Value = (int)(worker.RemoveObsolete ? 1 : 0);
									oCommand.Parameters.Add("?vNSCount", MySqlDbType.Double).Value =
										(worker.BPVisualizer.IsEmpty ? 0.0 : 1.0) + (worker.MFVisualizer.IsEmpty ? 0.0 : 1.0) +
										(worker.CCVisualizer.IsEmpty ? 0.0 : 1.0);
									//oCommand.CommandTimeout = 240;
									id = oCommand.ExecuteScalar();
								}

								if (id != null && id != DBNull.Value)
								{
									// we are aggregating
									using (MySqlCommand oCommand = new MySqlCommand(
										"update stats_h set BiologicalProcess=BiologicalProcess+?vBiologicalProcess, " +
										"CellularComponent=CellularComponent+?vCellularComponent, " +
										"MolecularFunction=MolecularFunction+?vMolecularFunction, " +
										"ExecTicks=ExecTicks+?vExecTicks, Count=Count+1 " +
										"where ID=?id;", oConnection))
									{
										oCommand.Parameters.Add("?vBiologicalProcess", MySqlDbType.Int64).Value =
											(long)(worker.BPVisualizer.IsEmpty ? 0 : worker.BPVisualizer.Terms.Count);
										oCommand.Parameters.Add("?vCellularComponent", MySqlDbType.Int64).Value =
											(long)(worker.CCVisualizer.IsEmpty ? 0 : worker.CCVisualizer.Terms.Count);
										oCommand.Parameters.Add("?vMolecularFunction", MySqlDbType.Int64).Value =
											(long)(worker.MFVisualizer.IsEmpty ? 0 : worker.MFVisualizer.Terms.Count);
										oCommand.Parameters.Add("?vExecTicks", MySqlDbType.Int64).Value = worker.ExecutingTime.Ticks;
										oCommand.Parameters.Add("?id", MySqlDbType.Int64).Value = Convert.ToInt64(id);

										oCommand.ExecuteNonQuery();
									}
								}
								else
								{
									// we are creating initial value
									using (MySqlCommand oCommand = new MySqlCommand(
										"insert into stats_h (DateTimeTicks, RequestSource, Cutoff, ValueType, SpeciesTaxon, Measure, RemoveObsolete, " +
										"BiologicalProcess, CellularComponent, MolecularFunction, ExecTicks, Count, NSCount) values " +
										"(?vDateTimeTicks, ?vRequestSource, ?vCutoff, ?vValueType, ?vSpeciesTaxon, ?vMeasure, ?vRemoveObsolete, " +
										"?vBiologicalProcess, ?vCellularComponent, ?vMolecularFunction, ?vExecTicks, ?vCount, ?vNSCount);",
										oConnection))
									{
										oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = createDateTime.Ticks;
										oCommand.Parameters.Add("?vRequestSource", MySqlDbType.Int32).Value = (int)worker.RequestSource;
										oCommand.Parameters.Add("?vCutoff", MySqlDbType.Int32).Value = (int)(worker.CutOff * 10.0);
										oCommand.Parameters.Add("?vValueType", MySqlDbType.Int32).Value = (int)worker.ValueType;
										oCommand.Parameters.Add("?vSpeciesTaxon", MySqlDbType.Int32).Value =
											WebUtilities.TypeConverter.ToInt32(worker.Annotations?.TaxonID);
										oCommand.Parameters.Add("?vMeasure", MySqlDbType.Int32).Value = (int)worker.SemanticSimilarity;
										oCommand.Parameters.Add("?vRemoveObsolete", MySqlDbType.Int32).Value = (int)(worker.RemoveObsolete ? 1 : 0);
										oCommand.Parameters.Add("?vBiologicalProcess", MySqlDbType.Int64).Value =
											(long)(worker.BPVisualizer.IsEmpty ? 0 : worker.BPVisualizer.Terms.Count);
										oCommand.Parameters.Add("?vCellularComponent", MySqlDbType.Int64).Value =
											(long)(worker.CCVisualizer.IsEmpty ? 0 : worker.CCVisualizer.Terms.Count);
										oCommand.Parameters.Add("?vMolecularFunction", MySqlDbType.Int64).Value =
											(long)(worker.MFVisualizer.IsEmpty ? 0 : worker.MFVisualizer.Terms.Count);
										oCommand.Parameters.Add("?vExecTicks", MySqlDbType.Int64).Value = worker.ExecutingTime.Ticks;
										oCommand.Parameters.Add("?vCount", MySqlDbType.Int64).Value = (long)1;
										oCommand.Parameters.Add("?vNSCount", MySqlDbType.Double).Value =
											(worker.BPVisualizer.IsEmpty ? 0.0 : 1.0) + (worker.MFVisualizer.IsEmpty ? 0.0 : 1.0) +
											(worker.CCVisualizer.IsEmpty ? 0.0 : 1.0);

										oCommand.ExecuteNonQuery();
									}

									lMaxStatTicks = createDateTime.Ticks;
								}
							}
						}
					}
					catch (Exception ex)
					{
						WriteToSystemLog($"{typeof(Global).GetType().Name}.oWorker_OnFinish", $"Message: '{ex.Message}', Stack trace: '{ex.StackTrace}'");
					}
				}
#endif

				// for debugging
				if (worker.HasDeveloperWarnings || worker.HasDeveloperErrors)
				{
					LogAndReportError("RevigoWorker", worker);
				}
			}
		}

		public static void UpdateJobUsageStats(RevigoWorker worker, string? type, string? ns)
		{
#if WEB_STATISTICS
			int iNamespace = -1;

			if (!string.IsNullOrEmpty(sConnectionString))
			{
				try
				{
					using (MySqlConnection oConnection = new MySqlConnection(sConnectionString))
					{
						oConnection.Open();
						string sNamespace = "";

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
						switch (type)
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

						// get previous field value
						object? value = null;
						using (MySqlCommand oCommand = new MySqlCommand(
							"select " + sField + " from stats where (DateTimeTicks=?vDateTimeTicks and JobID=?vJobID);",
							oConnection))
						{
							oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = worker.CreateDateTime.Ticks;
							oCommand.Parameters.Add("?vJobID", MySqlDbType.Int32).Value = worker.JobID;

							value = oCommand.ExecuteScalar();
						}

						if (value != null && value != DBNull.Value)
						{
							double dValue = Convert.ToDouble(value);
							if (dValue == 0.0)
							{
								// update the value
								using (MySqlCommand oCommand = new MySqlCommand(
									"update stats set " + sField + "=1.0 where (DateTimeTicks=?vDateTimeTicks) and (JobID=?vJobID);",
									oConnection))
								{
									oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = worker.CreateDateTime.Ticks;
									oCommand.Parameters.Add("?vJobID", MySqlDbType.Int32).Value = worker.JobID;
									oCommand.ExecuteNonQuery();
								}

								// update aggregated value
								DateTime createDateTime = new DateTime(worker.CreateDateTime.Year, worker.CreateDateTime.Month,
									worker.CreateDateTime.Day, worker.CreateDateTime.Hour, 0, 0);
								using (MySqlCommand oCommand = new MySqlCommand("update stats_h set " + sField + "=" + sField + "+1.0 " +
									"where (DateTimeTicks=?vDateTimeTicks) and (RequestSource=?vRequestSource) and (Cutoff=?vCutoff) and (ValueType=?vValueType) and " +
									"(SpeciesTaxon=?vSpeciesTaxon) and (Measure=?vMeasure) and (RemoveObsolete=?vRemoveObsolete) and (NSCount=?vNSCount);",
									oConnection))
								{
									oCommand.Parameters.Add("?vDateTimeTicks", MySqlDbType.Int64).Value = createDateTime.Ticks;
									oCommand.Parameters.Add("?vRequestSource", MySqlDbType.Int32).Value = (int)worker.RequestSource;
									oCommand.Parameters.Add("?vCutoff", MySqlDbType.Int32).Value = (int)(worker.CutOff * 10.0);
									oCommand.Parameters.Add("?vValueType", MySqlDbType.Int32).Value = (int)worker.ValueType;
									oCommand.Parameters.Add("?vSpeciesTaxon", MySqlDbType.Int32).Value = (int)((worker.Annotations == null) ? 0 : worker.Annotations.TaxonID);
									oCommand.Parameters.Add("?vMeasure", MySqlDbType.Int32).Value = (int)worker.SemanticSimilarity;
									oCommand.Parameters.Add("?vRemoveObsolete", MySqlDbType.Int32).Value = (int)(worker.RemoveObsolete ? 1 : 0);
									oCommand.Parameters.Add("?vNSCount", MySqlDbType.Double).Value =
										(worker.BPVisualizer.IsEmpty ? 0.0 : 1.0) + (worker.MFVisualizer.IsEmpty ? 0.0 : 1.0) +
										(worker.CCVisualizer.IsEmpty ? 0.0 : 1.0);
									//oCommand.CommandTimeout = 240;
									oCommand.ExecuteNonQuery();
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					WriteToSystemLog($"{typeof(Global).GetType().Name}.oWorker_OnFinish", $"Message: '{ex.Message}', Stack trace: '{ex.StackTrace}'");
				}
			}
#endif
		}

		public static void LogAndReportError(string source, Exception ex)
		{
			Global.WriteToSystemLog(source, string.Format("Error occured in the Revigo web service. Error message: '{0}'. Stack trace: '{1}'.",
				ex.Message, ex.StackTrace));

			if (!string.IsNullOrEmpty(Global.EmailServer) && !string.IsNullOrEmpty(Global.EmailFrom) && !string.IsNullOrEmpty(Global.EmailTo))
			{
				StringBuilder sMessage = new StringBuilder();
				sMessage.AppendFormat("Error ({0}) occured in the Revigo web service, source object '{1}'.", ex.GetType().FullName, source);
				sMessage.AppendLine();
				sMessage.AppendLine();

				sMessage.AppendFormat("Error message: {0}", ex.Message);
				sMessage.AppendLine();
				sMessage.AppendLine();

				sMessage.AppendFormat("Stack trace: {0}", ex.StackTrace);
				sMessage.AppendLine();

				SmtpClient client = new SmtpClient(Global.EmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(Global.EmailFrom, Global.EmailTo, "Notice from Revigo", sMessage.ToString());
				if (!string.IsNullOrEmpty(Global.EmailCC))
					message.CC.Add(Global.EmailCC);

				try
				{
					client.Send(message);
				}
				catch (Exception ex1)
				{
					WriteToSystemLog($"{typeof(Global).GetType().Name}.LogAndReportError", $"Message: '{ex1.Message}', Stack trace: '{ex1.StackTrace}'");
				}
			}
		}

		public static void LogAndReportError(string source, RevigoWorker worker)
		{
			WriteToSystemLog(source, string.Format("Warning(s) and/or error(s) occured in the Revigo web service while processing the user data; " +
				"CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}; RemoveObsolete = {4}; Warnings: {5}; Errors: {6}; Dataset: {7}",
				worker.CutOff, worker.ValueType, (worker.Annotations == null) ? 0 : worker.Annotations.TaxonID, worker.SemanticSimilarity, worker.RemoveObsolete,
				WebUtilities.TypeConverter.JoinStringArray(worker.DeveloperWarnings), WebUtilities.TypeConverter.JoinStringArray(worker.DeveloperErrors), worker.Data));

			if (!string.IsNullOrEmpty(Global.EmailServer) && !string.IsNullOrEmpty(Global.EmailFrom) && !string.IsNullOrEmpty(Global.EmailTo))
			{
				StringBuilder sMessage = new StringBuilder();
				sMessage.AppendFormat("Warning(s) and/or error(s) occured in the Revigo web service while processing the user data, source object: '{0}.", source);
				sMessage.AppendLine();
				sMessage.AppendLine("The user data set has been attached.");
				sMessage.AppendLine();
				sMessage.AppendFormat("Parameters: CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}, RemoveObsolete = {4}",
					worker.CutOff, worker.ValueType, (worker.Annotations == null) ? 0 : worker.Annotations.TaxonID, worker.SemanticSimilarity, worker.RemoveObsolete);
				sMessage.AppendLine();
				if (worker.HasDeveloperWarnings)
				{
					sMessage.AppendLine();
					sMessage.AppendFormat("Warnings: {0}", WebUtilities.TypeConverter.JoinStringArray(worker.DeveloperWarnings));
					sMessage.AppendLine();
				}
				if (worker.HasDeveloperErrors)
				{
					sMessage.AppendLine();
					sMessage.AppendFormat("Errors: {0}", WebUtilities.TypeConverter.JoinStringArray(worker.DeveloperErrors));
					sMessage.AppendLine();
				}

				SmtpClient client = new SmtpClient(Global.EmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(Global.EmailFrom, Global.EmailTo, "Notice from Revigo", sMessage.ToString());
				if (!string.IsNullOrEmpty(Global.EmailCC))
					message.CC.Add(Global.EmailCC);
				MemoryStream oStream = new MemoryStream(Encoding.UTF8.GetBytes(worker.Data));
				message.Attachments.Add(new Attachment(oStream, "dataset.txt", "text/plain;charset=UTF-8"));

				try
				{
					client.Send(message);
				}
				catch (Exception ex)
				{
					WriteToSystemLog($"{typeof(Global).GetType().Name}.LogAndReportError", $"Message: '{ex.Message}', Stack trace: '{ex.StackTrace}'");
				}

				oStream.Close();
			}
		}

		public static void LogAndReportError(string source, RevigoWorker worker, Exception ex)
		{
			Global.WriteToSystemLog(source, string.Format("Error occured in the Revigo web service while exporting the job results; " +
				"CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}; Warnings: {4}; Errors: {5}; Dataset: {6}",
				worker.CutOff, worker.ValueType, (worker.Annotations == null) ? 0 : worker.Annotations.TaxonID, worker.SemanticSimilarity,
				WebUtilities.TypeConverter.JoinStringArray(worker.DeveloperWarnings), WebUtilities.TypeConverter.JoinStringArray(worker.DeveloperErrors), worker.Data));

			if (!string.IsNullOrEmpty(Global.EmailServer) && !string.IsNullOrEmpty(Global.EmailFrom) && !string.IsNullOrEmpty(Global.EmailTo))
			{
				StringBuilder sMessage = new StringBuilder();
				sMessage.AppendFormat("Error occured in the Revigo web service while exporting the job results ({0}), source object '{1}'.", ex.GetType().FullName, source);
				sMessage.AppendLine();
				sMessage.AppendLine("The user data set has been attached.");
				sMessage.AppendLine();
				sMessage.AppendLine();
				sMessage.AppendFormat("Parameters: CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}, RemoveObsolete = {4}",
					worker.CutOff, worker.ValueType, (worker.Annotations == null) ? 0 : worker.Annotations.TaxonID, worker.SemanticSimilarity, worker.RemoveObsolete);
				sMessage.AppendLine();
				sMessage.AppendLine();

				sMessage.AppendFormat("Error message: {0}", ex.Message);
				sMessage.AppendLine();
				sMessage.AppendLine();

				sMessage.AppendFormat("Stack trace: {0}", ex.StackTrace);
				sMessage.AppendLine();

				SmtpClient client = new SmtpClient(Global.EmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(Global.EmailFrom, Global.EmailTo, "Notice from Revigo", sMessage.ToString());
				if (!string.IsNullOrEmpty(Global.EmailCC))
					message.CC.Add(Global.EmailCC);
				MemoryStream oStream = new MemoryStream(Encoding.UTF8.GetBytes(worker.Data));
				message.Attachments.Add(new Attachment(oStream, "dataset.txt", "text/plain;charset=UTF-8"));

				try
				{
					client.Send(message);
				}
				catch (Exception ex1)
				{
					WriteToSystemLog($"{typeof(Global).GetType().Name}.LogAndReportError", $"Message: '{ex1.Message}', Stack trace: '{ex1.StackTrace}'");
				}

				oStream.Close();
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