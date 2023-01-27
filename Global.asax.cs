using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using System.Data;
using System.Net.Mail;
using System.Globalization;
using System.Threading;
using IRB.Collections.Generic;
using IRB.Revigo.Worker;
using IRB.Revigo.Core;
using IRB.Revigo.Databases;
using MySql.Data.MySqlClient;

namespace RevigoWeb
{
	public class Global : System.Web.HttpApplication
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

		protected void Application_Start(object sender, EventArgs e)
		{
			// initialize log file
			try
			{
				oLog = new StreamWriter(new FileStream(HostingEnvironment.MapPath("~/App_Data/Logs/Errors.log"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
			}
			catch
			{
				oLog = null;
			}

			sConnectionString = ConfigurationManager.ConnectionStrings["MariaDBServer"].ConnectionString;

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
								lMinStatTicks = DBConnection.ToInt64(dtData.Rows[0][0]);
								lMaxStatTicks = DBConnection.ToInt64(dtData.Rows[0][1]);
							}
							break;
						}
						catch (Exception ex)
						{
							WriteToSystemLog(this.GetType().Name, "Error establishing connection: " + ex.Message);
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

			// try to load and intialize ontology object
			try
			{
				string sPath = ConfigurationManager.AppSettings["PathToGO"];
				if (!string.IsNullOrEmpty(sPath))
				{
					oOntology = GeneOntology.Deserialize(sPath);
				}
				else
				{
					WriteToSystemLog(this.GetType().Name, "Error: The path to Gene Ontology is empty");
				}
			}
			catch (Exception ex)
			{
				oOntology = null;
				WriteToSystemLog(this.GetType().Name, "Error loading Gene Ontology: " + ex.Message);
			}

			// try to load and initialize Species annotations object
			try
			{
				string sPath = ConfigurationManager.AppSettings["PathToSpeciesAnnotations"];
				if (!string.IsNullOrEmpty(sPath))
				{
					oSpeciesAnnotations = SpeciesAnnotationsList.Deserialize(sPath);
				}
				else
				{
					WriteToSystemLog(this.GetType().Name, "Error: The path to Species Annotations is empty");
				}
			}
			catch (Exception ex)
			{
				WriteToSystemLog(this.GetType().Name, "Error loading Species Annotations: " + ex.Message);
			}
		}

		void Application_Error(object sender, EventArgs e)
		{
			Exception ex = Server.GetLastError();

			if (ex is HttpRequestValidationException)
			{
				Server.ClearError();
				Response.Redirect("/Errors/RequestValidationError.html", false);
			}
			else if (ex.Message.StartsWith("The file ") && ex.Message.EndsWith(" does not exist."))
			{
				Server.ClearError();
				Response.Redirect("/Errors/Error404.aspx", false);
			}
			else
			{
				if (!ex.Message.Contains("Request timed out") && 
					!ex.Message.Contains("invalid webresource request") &&
					!ex.Message.Contains("potentially dangerous"))
				{
					SendEmailNotification(ex);
				}

				Server.ClearError();
				Response.Redirect("/Errors/GenericErrorPage.html", false);
			}
		}

		protected void Session_End(object sender, EventArgs e)
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
		}

		protected void Application_End(object sender, EventArgs e)
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

		public static int SessionTimeout
		{
			get
			{
				Configuration conf = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
				SessionStateSection section = (SessionStateSection)conf.GetSection("system.web/sessionState");
				int iTimeout = (int)section.Timeout.TotalMinutes;

				return iTimeout;
			}
		}

		public static int JobTimeout
		{
			get
			{
				int iTimeout;
				string sTimeout = ConfigurationManager.AppSettings["JobTimeout"];
				if (string.IsNullOrEmpty(sTimeout) || !int.TryParse(sTimeout, out iTimeout))
				{
					// default is 15 minutes
					iTimeout = 15;
				}
				return iTimeout;
			}
		}

		public static int CreateNewJob(RequestSourceEnum requestSource, string data, double cutoff, ValueTypeEnum valueType, SpeciesAnnotations annotations, SemanticSimilarityScoreEnum measure, bool removeObsolete)
		{
			int iJobID = -1;

			if (!bDisposing)
			{
				int iTimeout = Global.JobTimeout;
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

					oWorker = new RevigoWorker(iJobID, Global.Ontology, annotations, iTimeout, requestSource, data, cutoff, valueType, measure, removeObsolete);

					oJobs.Add(iJobID, new RevigoJob(iJobID, DateTime.Now.AddMinutes(iTimeout + 1), oWorker));
				}
			}

			return iJobID;
		}

		public static int StartNewJob(RequestSourceEnum requestSource, string data, double cutoff, ValueTypeEnum valueType, SpeciesAnnotations annotations, SemanticSimilarityScoreEnum measure, bool removeObsolete)
		{
			int iJobID = -1;

			if (!bDisposing)
			{
				int iTimeout = Global.JobTimeout;
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

					oWorker = new RevigoWorker(iJobID, Global.Ontology, annotations, iTimeout, requestSource, data, cutoff, valueType, measure, removeObsolete);
					oJobs.Add(iJobID, new RevigoJob(iJobID, DateTime.Now.AddMinutes(iTimeout + 1), oWorker));
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
					SendEmailNotification(worker);
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

		private static void SendEmailNotification(Exception ex)
		{
			string sEmailServer = ConfigurationManager.AppSettings["EmailServer"];
			string sEmailTo = ConfigurationManager.AppSettings["DeveloperEmailTo"];
			string sEmailCc = ConfigurationManager.AppSettings["DeveloperEmailCC"];

			if (!string.IsNullOrEmpty(sEmailServer) && !string.IsNullOrEmpty(sEmailTo))
			{
				StringBuilder sMessage = new StringBuilder();
				sMessage.AppendFormat("Error occured in the Revigo application ({0}).", ex.GetType().FullName);
				sMessage.AppendLine();
				sMessage.AppendLine();
				sMessage.AppendLine();

				sMessage.AppendFormat("Error message: {0}", ex.Message);
				sMessage.AppendLine();
				sMessage.AppendLine();

				sMessage.AppendFormat("Stack trace: {0}", ex.StackTrace);
				sMessage.AppendLine();

				SmtpClient client = new SmtpClient(sEmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(sEmailTo, sEmailTo, "Notice from Revigo", sMessage.ToString());
				if (!string.IsNullOrEmpty(sEmailCc))
					message.CC.Add(sEmailCc);

				try
				{
					client.Send(message);
				}
				catch (Exception ex1)
				{
					Global.WriteToSystemLog(typeof(Global).FullName, ex1.Message);
				}
			}
			else
			{
				Global.WriteToSystemLog(typeof(Global).FullName, string.Format("Error occured in the Revigo application. Error message: '{0}'. Stack trace: '{1}'.",
					ex.Message, ex.StackTrace));
			}
		}

		private static void SendEmailNotification(RevigoWorker worker)
		{
			string sEmailServer = ConfigurationManager.AppSettings["EmailServer"];
			string sEmailTo = ConfigurationManager.AppSettings["DeveloperEmailTo"];
			string sEmailCc = ConfigurationManager.AppSettings["DeveloperEmailCC"];

			if (!string.IsNullOrEmpty(sEmailServer) && !string.IsNullOrEmpty(sEmailTo))
			{
				StringBuilder sMessage = new StringBuilder();
				sMessage.AppendLine("Warning(s) and/or error(s) occured during processing of user data on http://revigo.irb.hr.");
				sMessage.AppendLine("The user data set has been attached.");
				sMessage.AppendLine();
				sMessage.AppendFormat("Parameters: CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}, RemoveObsolete = {4}",
					worker.CutOff, worker.ValueType, worker.Annotations.TaxonID, worker.Measure, worker.RemoveObsolete);
				sMessage.AppendLine();
				if (worker.HasDeveloperWarnings)
				{
					sMessage.AppendLine();
					sMessage.AppendFormat("Warnings: {0}", JoinStringArray(worker.DeveloperWarnings));
					sMessage.AppendLine();
				}
				if (worker.HasDeveloperErrors)
				{
					sMessage.AppendLine();
					sMessage.AppendFormat("Errors: {0}", JoinStringArray(worker.DeveloperErrors));
					sMessage.AppendLine();
				}

				SmtpClient client = new SmtpClient(sEmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(sEmailTo, sEmailTo, "Notice from Revigo", sMessage.ToString());
				if (!string.IsNullOrEmpty(sEmailCc))
					message.CC.Add(sEmailCc);
				MemoryStream oStream = new MemoryStream(Encoding.UTF8.GetBytes(worker.Data));
				message.Attachments.Add(new Attachment(oStream, "dataset.txt", "text/plain;charset=UTF-8"));

				try
				{
					client.Send(message);
				}
				catch (Exception ex)
				{
					Global.WriteToSystemLog(typeof(Global).FullName, ex.Message);
				}

				oStream.Close();
			}
			else
			{
				Global.WriteToSystemLog(typeof(Global).FullName, string.Format("Warning(s) and/or error(s) occured during processing of user data on http://revigo.irb.hr; " +
					"CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}; RemoveObsolete = {4}; Warnings: {5}; Errors: {6}; Dataset: {7}",
					worker.CutOff, worker.ValueType, worker.Annotations.TaxonID, worker.Measure, worker.RemoveObsolete,
					JoinStringArray(worker.DeveloperWarnings), JoinStringArray(worker.DeveloperErrors), worker.Data));
			}
		}

		public static string DoubleToJSON(double value)
		{
			if (double.IsNaN(value))
			{
				return "\"NaN\"";
			}

			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string StringToJSON(string text)
		{
			StringBuilder result = new StringBuilder();

			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];
				if (ch < '\x20')
				{
					switch (ch)
					{
						case '\b':
							result.Append("\\b");
							break;
						case '\f':
							result.Append("\\f");
							break;
						case '\n':
							result.Append("\\n");
							break;
						case '\r':
							result.Append("\\r");
							break;
						case '\t':
							result.Append("\\t");
							break;
						default:
							result.AppendFormat("\\u{0:x4}", (int)ch);
							break;
					}
				}
				else if (ch > '\xff')
				{
					result.AppendFormat("\\u{0:x4}", (int)ch);
				}
				else
				{
					switch (ch)
					{
						case '\"':
							result.Append("\\\"");
							break;
						case '/':
							result.Append("\\/");
							break;
						case '\\':
							result.Append("\\\\");
							break;
						default:
							result.Append(ch);
							break;
					}
				}
			}

			return result.ToString();
		}

		public static string JoinStringArray(List<string> lines)
		{
			StringBuilder sbTemp = new StringBuilder();

			for (int i = 0; i < lines.Count; i++)
			{
				sbTemp.AppendLine(lines[i]);
			}

			return sbTemp.ToString();
		}

		public static string StringArrayToJSON(List<string> lines)
		{
			StringBuilder sbTemp = new StringBuilder();

			sbTemp.Append("[");
			for (int i = 0; i < lines.Count; i++)
			{
				if (i > 0)
					sbTemp.Append(",");
				sbTemp.AppendFormat("\"{0}\"", Global.StringToJSON(lines[i]));
			}
			sbTemp.Append("]");

			return sbTemp.ToString();
		}

		public static string HtmlEncode(string text)
		{
			return HttpUtility.HtmlEncode(text.Replace('\"', '\'').Replace("\n", "\\n").Replace("\r", "\\r"));
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