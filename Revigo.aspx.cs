using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web.UI.WebControls;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using IRB.Revigo.Core;

namespace RevigoWeb
{
	public partial class RevigoREST : System.Web.UI.Page
	{
		protected RevigoWorker oWorker = null;

		protected void Page_Load(object sender, EventArgs e)
		{
			double dCutoff = 0.7;
			ValueTypeEnum eValueType = ValueTypeEnum.PValue;
			int iSpeciesTaxon = 0;
			SemanticSimilarityScoreEnum eMeasure = SemanticSimilarityScoreEnum.SIMREL;
			bool bRemoveObsolete = true;

			if (Global.SpeciesAnnotations == null)
			{
				ShowError("Species annotations object is not initialized.");
				return;
			}

			string sUserData = Convert.ToString(Request.Params["goList"]);
			if (string.IsNullOrEmpty(sUserData))
			{
				ShowError("The goList field is required");
				return;
			}

			// {0.4 - 0.9}
			// if the value is not provided 0.7 will be assumed
			string sCutOff = Convert.ToString(Request.Params["cutoff"]);
			if (!string.IsNullOrEmpty(sCutOff) && (!double.TryParse(sCutOff, NumberStyles.Any, CultureInfo.InvariantCulture, out dCutoff) || dCutoff < 0.4 || dCutoff > 0.9))
			{
				ShowError("The cutoff field has invalid value");
				return;
			}
			dCutoff = Math.Round(dCutoff, 1);

			// if the value is not provided pvalue will be assumed
			string sValueType = Convert.ToString(Request.Params["valueType"]);
			if (!string.IsNullOrEmpty(sValueType))
			{
				sValueType = sValueType.Trim();
				string[] aValueTypes = Enum.GetNames(typeof(ValueTypeEnum));
				bool bFound = false;
				foreach (string valueType in aValueTypes)
				{
					if (valueType.Equals(sValueType, StringComparison.CurrentCultureIgnoreCase))
					{
						eValueType = (ValueTypeEnum)Enum.Parse(typeof(ValueTypeEnum), sValueType, true);
						bFound = true;
						break;
					}
				}

				if (!bFound)
				{
					ShowError("The valueType field has invalid value");
					return;
				}
			}

			// One of the supported NCBI species taxon ID (0 for the whole UniProt database).
			// if the value is not provided 0 will be assumed
			string sSpeciesTaxon = Convert.ToString(Request.Params["speciesTaxon"]);
			if (!string.IsNullOrEmpty(sSpeciesTaxon))
			{
				if (!int.TryParse(sSpeciesTaxon, out iSpeciesTaxon) || !Global.SpeciesAnnotations.ContainsID(iSpeciesTaxon))
				{
					ShowError("The speciesTaxon field has invalid value");
					return;
				}
			}
			SpeciesAnnotations oAnnotations = Global.SpeciesAnnotations.GetByID(iSpeciesTaxon);

			// {SIMREL, LIN, RESNIK, JIANG}
			// if the value is not provided SIMREL will be assumed
			string sMeasure = Convert.ToString(Request.Params["measure"]);
			if (!string.IsNullOrEmpty(sMeasure))
			{
				sMeasure = sMeasure.Trim();
				string[] aMeasures = Enum.GetNames(typeof(SemanticSimilarityScoreEnum));
				bool bFound = false;
				foreach (string measure in aMeasures)
				{
					if (measure.Equals(sMeasure, StringComparison.CurrentCultureIgnoreCase))
					{
						eMeasure = (SemanticSimilarityScoreEnum)Enum.Parse(typeof(SemanticSimilarityScoreEnum), sMeasure, true);
						bFound = true;
						break;
					}
				}

				if (!bFound)
				{
					ShowError("The measure field has invalid value");
					return;
				}
			}

			string sRemoveObsolete = Convert.ToString(Request.Params["removeObsolete"]);
			if (!string.IsNullOrEmpty(sRemoveObsolete))
			{
				if (!bool.TryParse(sRemoveObsolete, out bRemoveObsolete))
				{
					ShowError("The removeObsolete field has invalid value");
					return;
				}
			}

			// finished parsing parameters, now invoke Revigo and fill the results
			int iJobID = -1;
			try
			{
				iJobID = Global.StartNewJob(RequestSourceEnum.RestfulAPI, sUserData, dCutoff, eValueType, oAnnotations, eMeasure, bRemoveObsolete);
			}
			catch (Exception ex)
			{
				Global.WriteToSystemLog(this.GetType().Name, ex.StackTrace);
				ShowError("Undefined error occured");
				return;
			}

			if (iJobID <= 0)
			{
				ShowError("Unable to start a new job, please try again later.");
				return;
			}

			this.oWorker = Global.Jobs.GetValueByKey(iJobID).Worker;

			// wait for a job to finish or a timeout to occur
			while (this.oWorker.IsRunning || !this.oWorker.IsFinished)
			{
				Thread.Sleep(500);
			}

			if (this.oWorker != null)
			{
				if (!oWorker.HasUserErrors && !oWorker.HasDeveloperErrors)
				{
					if (oWorker.HasBPVisualizer)
					{
						// Assign the Biological process controls
						TermListVisualizer oVisualizer = oWorker.BPVisualizer;

						this.dgBPTable.Visible = true;

						// render Data table
						GOTermList terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);
						DataTable dtResultTable = ToResultTable(oVisualizer.Namespace.ToString(), terms);
						this.dgBPTable.DataSource = dtResultTable;
						this.dgBPTable.DataBind();

						Global.UpdateJobUsageStats(oWorker, "table", "1");
					}
					if (oWorker.HasCCVisualizer)
					{
						// Assign the Biological process controls
						TermListVisualizer oVisualizer = oWorker.CCVisualizer;

						this.dgCCTable.Visible = true;

						// render Data table
						GOTermList terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);
						DataTable dtResultTable = ToResultTable(oVisualizer.Namespace.ToString(), terms);
						this.dgCCTable.DataSource = dtResultTable;
						this.dgCCTable.DataBind();

						Global.UpdateJobUsageStats(oWorker, "table", "2");
					}
					if (oWorker.HasMFVisualizer)
					{
						// Assign the Biological process controls
						TermListVisualizer oVisualizer = oWorker.MFVisualizer;

						this.dgMFTable.Visible = true;

						// render Data table
						GOTermList terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);
						DataTable dtResultTable = ToResultTable(oVisualizer.Namespace.ToString(), terms);
						this.dgMFTable.DataSource = dtResultTable;
						this.dgMFTable.DataBind();

						Global.UpdateJobUsageStats(oWorker, "table", "3");
					}						
				}
				else
				{
					ShowError(Global.JoinStringArray(oWorker.UserErrors));
				}

				if (oWorker.HasDeveloperWarnings || oWorker.HasDeveloperErrors)
				{
					SendEmailNotification();
				}
			}
		}

		private void SendEmailNotification()
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
					oWorker.CutOff, oWorker.ValueType, oWorker.Annotations.TaxonID, oWorker.Measure, oWorker.RemoveObsolete);
				sMessage.AppendLine();
				if (oWorker.HasDeveloperWarnings)
				{
					sMessage.AppendLine();
					sMessage.AppendFormat("Warnings: {0}", Global.JoinStringArray(oWorker.DeveloperWarnings));
					sMessage.AppendLine();
				}
				if (oWorker.HasDeveloperErrors)
				{
					sMessage.AppendLine();
					sMessage.AppendFormat("Errors: {0}", Global.JoinStringArray(oWorker.DeveloperErrors));
					sMessage.AppendLine();
				}

				SmtpClient client = new SmtpClient(sEmailServer);
				client.EnableSsl = false;
				MailMessage message = new MailMessage(sEmailTo, sEmailTo, "Notice from Revigo", sMessage.ToString());
				if (!string.IsNullOrEmpty(sEmailCc))
					message.CC.Add(sEmailCc);
				MemoryStream oStream = new MemoryStream(Encoding.UTF8.GetBytes(oWorker.Data));
				message.Attachments.Add(new Attachment(oStream, "dataset.txt", "text/plain;charset=UTF-8"));

				try
				{
					client.Send(message);
				}
				catch (Exception ex)
				{
					Global.WriteToSystemLog(this.GetType().FullName, ex.Message);
				}

				oStream.Close();
			}
			else
			{
				Global.WriteToSystemLog(this.GetType().FullName, string.Format("Warning(s) and/or error(s) occured during processing of user data on http://revigo.irb.hr; " +
					"CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}; Warnings: {4}; Errors: {5}; Dataset: {6}",
					oWorker.CutOff, oWorker.ValueType, oWorker.Annotations.TaxonID, oWorker.Measure, 
					Global.JoinStringArray(oWorker.DeveloperWarnings), Global.JoinStringArray(oWorker.DeveloperErrors), oWorker.Data));
			}
		}

		protected void dgTable_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			Repeater header = (Repeater)e.Item.FindControl("dgHeader");
			Repeater table = (Repeater)sender;
			if (header != null && table != null && table.DataSource != null && header.DataSource == null)
			{
				header.DataSource = ToHeaderTable((DataTable)table.DataSource);
				header.DataBind();
			}
		}

		protected DataTable ToHeaderTable(DataTable table)
		{
			DataTable dtHeader = new DataTable("Header");

			dtHeader.Columns.Add("Name", typeof(string));
			dtHeader.Columns.Add("Title", typeof(string));

			for (int i = 0; i < table.Columns.Count; i++)
			{
				DataColumn column = table.Columns[i];
				if (!column.ColumnName.StartsWith("?"))
				{
					DataRow row = dtHeader.NewRow();
					row[0] = column.ColumnName;
					row[1] = column.Caption;
					dtHeader.Rows.Add(row);
				}
			}

			return dtHeader;
		}

		private DataTable ToResultTable(string name, GOTermList terms)
		{
			DataTable dtResultTable = new DataTable(name);

			// hidden columns (preceded by question mark)
			dtResultTable.Columns.Add("?ID", typeof(int));
			dtResultTable.Columns.Add("?Description", typeof(string));

			// visible columns and labels
			dtResultTable.Columns.Add("Term ID", typeof(string));
			dtResultTable.Columns.Add("Name", typeof(string));
			dtResultTable.Columns.Add("Frequency", typeof(double));
			dtResultTable.Columns.Add("Value", typeof(double));
			dtResultTable.Columns.Add("Uniqueness", typeof(double));
			dtResultTable.Columns.Add("Dispensability", typeof(double));
			dtResultTable.Columns.Add("Eliminated", typeof(bool));
			dtResultTable.Columns.Add("Representative", typeof(int));

			// fill table with data
			for (int i = 0; i < terms.Count; i++)
			{
				GOTerm curGOTerm = terms[i];
				GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(curGOTerm.ID);
				DataRow row = dtResultTable.NewRow();
				bool isTermEliminated = oProperties.Dispensability > oWorker.CutOff;
				int iColumn = 0;

				// hidden columns
				row[iColumn] = curGOTerm.ID;
				iColumn++;
				row[iColumn] = curGOTerm.Description;
				iColumn++;

				// visible columns
				row[iColumn] = curGOTerm.FormattedID;
				iColumn++;
				row[iColumn] = curGOTerm.Name;
				iColumn++;
				row[iColumn] = oProperties.AnnotationFrequency * 100.0;
				iColumn++;
				row[iColumn] = oProperties.Value;
				iColumn++;
				row[iColumn] = oProperties.Uniqueness;
				iColumn++;
				row[iColumn] = oProperties.Dispensability;
				iColumn++;
				row[iColumn] = isTermEliminated;
				iColumn++;
				row[iColumn] = oProperties.Representative;
				iColumn++;

				dtResultTable.Rows.Add(row);
			}

			return dtResultTable;
		}

		private void ShowError(string text)
		{
			this.ErrorMessage.Text = text;
		}
	}
}
