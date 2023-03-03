using IRB.Revigo.Core;
using IRB.Revigo.Core.Databases;
using IRB.Revigo.Core.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Globalization;

namespace IRB.RevigoWeb.Pages
{
	[IgnoreAntiforgeryToken]
	public class RevigoModel : PageModel
	{
		public string? ErrorMessage = null;

		public RevigoWorker? oWorker = null;
		public DataTable? BPTable = null;
		public DataTable? CCTable = null;
		public DataTable? MFTable = null;

		public void OnGet()
		{
			ProcessJob();
		}

		public void OnPost()
		{
			ProcessJob();
		}

		private void ProcessJob()
		{
			double dCutoff = 0.7;
			ValueTypeEnum eValueType = ValueTypeEnum.PValue;
			int iSpeciesTaxon = 0;
			SemanticSimilarityTypeEnum eMeasure = SemanticSimilarityTypeEnum.SIMREL;
			bool bRemoveObsolete = true;

			if (Request.ContentType != "application/x-www-form-urlencoded")
			{
				ShowError("Tre request must be sent as a form.");
				return;
			}

			if (Global.Ontology == null)
			{
				ShowError("Gene Ontology object is not initialized.");
				return;
			}

			if (Global.SpeciesAnnotations == null)
			{
				ShowError("Species annotations object is not initialized.");
				return;
			}

			string sUserData = Convert.ToString(Request.Form["goList"]);
			if (string.IsNullOrEmpty(sUserData))
			{
				ShowError("The goList field is required");
				return;
			}

			// {0.4 - 0.9}
			// if the value is not provided 0.7 will be assumed
			string sCutOff = Convert.ToString(Request.Form["cutoff"]);
			if (!string.IsNullOrEmpty(sCutOff) && (!double.TryParse(sCutOff, NumberStyles.Any, CultureInfo.InvariantCulture, out dCutoff) || dCutoff < 0.4 || dCutoff > 0.9))
			{
				ShowError("The cutoff field has invalid value");
				return;
			}
			dCutoff = Math.Round(dCutoff, 1);

			// if the value is not provided pvalue will be assumed
			string sValueType = Convert.ToString(Request.Form["valueType"]);
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
			string sSpeciesTaxon = Convert.ToString(Request.Form["speciesTaxon"]);
			if (!string.IsNullOrEmpty(sSpeciesTaxon))
			{
				if (!int.TryParse(sSpeciesTaxon, out iSpeciesTaxon) || !Global.SpeciesAnnotations.ContainsID(iSpeciesTaxon))
				{
					ShowError("The speciesTaxon field has invalid value");
					return;
				}
			}
			SpeciesAnnotations? oAnnotations = Global.SpeciesAnnotations.GetByID(iSpeciesTaxon);
			if (oAnnotations == null)
			{
				ShowError("The speciesTaxon field has invalid value");
				return;
			}

			// {SIMREL, LIN, RESNIK, JIANG}
			// if the value is not provided SIMREL will be assumed
			string sMeasure = Convert.ToString(Request.Form["measure"]);
			if (!string.IsNullOrEmpty(sMeasure))
			{
				sMeasure = sMeasure.Trim();
				string[] aMeasures = Enum.GetNames(typeof(SemanticSimilarityTypeEnum));
				bool bFound = false;
				foreach (string measure in aMeasures)
				{
					if (measure.Equals(sMeasure, StringComparison.CurrentCultureIgnoreCase))
					{
						eMeasure = (SemanticSimilarityTypeEnum)Enum.Parse(typeof(SemanticSimilarityTypeEnum), sMeasure, true);
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

			string sRemoveObsolete = Convert.ToString(Request.Form["removeObsolete"]);
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
				Global.WriteToSystemLog($"{this.GetType().Name}.ProcessJob", $"Message: '{ex.Message}', Stack trace: '{ex.StackTrace}'");
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
					if (!oWorker.BPVisualizer.IsEmpty)
					{
						// Assign the Biological process controls
						NamespaceVisualizer oVisualizer = oWorker.BPVisualizer;

						// render Data table
						RevigoTermCollection terms = oVisualizer.Terms.FindClustersAndSortByThem(Global.Ontology, oWorker.CutOff);
						BPTable = ToResultTable(oVisualizer.Namespace.ToString(), terms);

						Global.UpdateJobUsageStats(oWorker, "table", "1");
					}
					if (!oWorker.CCVisualizer.IsEmpty)
					{
						// Assign the Biological process controls
						NamespaceVisualizer oVisualizer = oWorker.CCVisualizer;

						// render Data table
						RevigoTermCollection terms = oVisualizer.Terms.FindClustersAndSortByThem(Global.Ontology, oWorker.CutOff);
						CCTable = ToResultTable(oVisualizer.Namespace.ToString(), terms);

						Global.UpdateJobUsageStats(oWorker, "table", "2");
					}
					if (!oWorker.MFVisualizer.IsEmpty)
					{
						// Assign the Biological process controls
						NamespaceVisualizer oVisualizer = oWorker.MFVisualizer;

						// render Data table
						RevigoTermCollection terms = oVisualizer.Terms.FindClustersAndSortByThem(Global.Ontology, oWorker.CutOff);
						MFTable = ToResultTable(oVisualizer.Namespace.ToString(), terms);

						Global.UpdateJobUsageStats(oWorker, "table", "3");
					}
				}
				else
				{
					ShowError(WebUtilities.TypeConverter.JoinStringArray(oWorker.UserErrors));
				}

				if (oWorker.HasDeveloperWarnings || oWorker.HasDeveloperErrors)
				{
					Global.LogAndReportError("RevigoWorker", oWorker);
				}
			}

			Global.RemoveJob(iJobID);
		}

		private DataTable ToResultTable(string name, RevigoTermCollection terms)
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
			dtResultTable.Columns.Add("Representative", typeof(string));

			if (oWorker != null)
			{
				// fill table with data
				for (int i = 0; i < terms.Count; i++)
				{
					RevigoTerm term = terms[i];

					DataRow row = dtResultTable.NewRow();
					int iColumn = 0;

					// hidden columns
					row[iColumn] = term.GOTerm.ID;
					iColumn++;
					row[iColumn] = term.GOTerm.Description;
					iColumn++;

					// visible columns
					row[iColumn] = term.GOTerm.FormattedID;
					iColumn++;
					row[iColumn] = term.GOTerm.Name;
					iColumn++;
					row[iColumn] = term.AnnotationFrequency * 100.0;
					iColumn++;
					row[iColumn] = term.Value;
					iColumn++;
					row[iColumn] = term.Uniqueness;
					iColumn++;
					row[iColumn] = term.Dispensability;
					iColumn++;
					row[iColumn] = term.Dispensability > oWorker.CutOff;
					iColumn++;
					row[iColumn] = (term.RepresentativeID > 0) ? term.RepresentativeID.ToString() : "null";
					iColumn++;

					dtResultTable.Rows.Add(row);
				}
			}

			return dtResultTable;
		}

		private void ShowError(string message)
		{
			this.ErrorMessage = message;
		}
	}
}
