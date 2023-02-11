using IRB.Revigo.Core;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using IRB.RevigoWeb;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace RevigoWeb.Pages
{
	[IgnoreAntiforgeryToken]
	public class IndexModel : PageModel
    {
        public string ErrorMessage = null;

		[BindProperty]
		public string txtGOInput { get; set; }
		[BindProperty]
		public string chkCutOff { get; set; }
		[BindProperty]
		public string lstValueType { get; set; }
		[BindProperty]
		public string chkRemoveObsolete { get; set; }
		[BindProperty]
		public string lstSpecies { get; set; }
		[BindProperty]
		public string lstSimilarity { get; set; }

		private bool bAutoStart = false;

		public void OnGet()
        {
			FillParameters();

			if (this.bAutoStart)
			{
				this.StartJob();
			}
		}

		public void OnPost()
		{
			this.bAutoStart = true;

			FillParameters();

			if (this.bAutoStart)
			{
				this.StartJob();
			}
		}

		private void StartJob()
		{
			// validate parameters
			string sUserData = this.txtGOInput;
			double dCutoff = 0.7;
			ValueTypeEnum eValueType = ValueTypeEnum.PValue;
			int iSpeciesTaxon = 0;
			SemanticSimilarityEnum eMeasure = SemanticSimilarityEnum.SIMREL;
			bool bRemoveObsolete = true;
			GDPRTypeEnum eGDPRType = GDPR.GetGDPRState(this.HttpContext);

			if (eGDPRType == GDPRTypeEnum.None && (ViewData["GDPRNoReload"] == null || !WebUtilities.TypeConverter.ToBoolean(ViewData["GDPRNoReload"])))
			{
				ShowError("Please first Accept our Terms Of Use.");
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

			if (string.IsNullOrEmpty(sUserData) || string.IsNullOrEmpty(sUserData.Trim()))
			{
				ShowError("The data you submitted is empty, plase fill out your data or use one of our examples.");
				return;
			}

			if (string.IsNullOrEmpty(this.chkCutOff))
			{
				ShowError("Please select appropriate similarity measure.");
				return;
			}

			if (this.chkCutOff.Equals("0.9"))
			{
				dCutoff = 0.9;
			}
			else if (this.chkCutOff.Equals("0.7"))
			{
				dCutoff = 0.7;
			}
			else if (this.chkCutOff.Equals("0.5"))
			{
				dCutoff = 0.5;
			}
			else if (this.chkCutOff.Equals("0.4"))
			{
				dCutoff = 0.4;
			}
			else
			{
				ShowError("Please select appropriate similarity measure.");
				return;
			}

			if (string.IsNullOrEmpty(this.lstValueType))
			{
				ShowError("Please select appropriate value representation.");
				return;
			}

			switch (this.lstValueType.ToLower())
			{
				case "pvalue":
					eValueType = ValueTypeEnum.PValue;
					break;
				case "higher":
					eValueType = ValueTypeEnum.Higher;
					break;
				case "lower":
					eValueType = ValueTypeEnum.Lower;
					break;
				case "higherabsolute":
					eValueType = ValueTypeEnum.HigherAbsolute;
					break;
				case "higherabslog2":
					eValueType = ValueTypeEnum.HigherAbsLog2;
					break;
				default:
					ShowError("Please select appropriate value representation.");
					return;
			}

			if (!int.TryParse(this.lstSpecies, out iSpeciesTaxon) || !Global.SpeciesAnnotations.ContainsID(iSpeciesTaxon))
			{
				ShowError("Please select appropriate species taxon.");
				return;
			}
			SpeciesAnnotations oAnnotations = Global.SpeciesAnnotations.GetByID(iSpeciesTaxon);

			string sSimilarity = this.lstSimilarity;
			bool bFound = false;
			Array aEnumValues = Enum.GetValues(typeof(SemanticSimilarityEnum));
			foreach (var value in aEnumValues)
			{
				if (value.ToString().Equals(sSimilarity, StringComparison.InvariantCultureIgnoreCase))
				{
					eMeasure = (SemanticSimilarityEnum)value;
					bFound = true;
					break;
				}
			}
			if (!bFound)
			{
				ShowError("Please select appropriate similarity measure to use.");
				return;
			}

			bRemoveObsolete = (string.IsNullOrEmpty(this.chkRemoveObsolete) || !this.chkRemoveObsolete.Equals("true") ? false : true);

			int iJobID = -1;
			try
			{
				// finished parsing parameters, now start Revigo job and redirect to result page
				iJobID = Global.StartNewJob(RequestSourceEnum.WebPage, sUserData, dCutoff, eValueType, oAnnotations, eMeasure, bRemoveObsolete);
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

			Response.Redirect(Url.Content(string.Format("~/Results?jobid={0}", iJobID)));
		}

		// we wan't to support old style job submitting, and returns from result page
		private void FillParameters()
		{
			string sUserData = null;
			string sCutOff = null;
			string sValueType = null;
			string sSpeciesTaxon = null;
			string sSimilarity = null;
			string sRemoveObsolete = null;
			string sAutoStart = null;

			if (Request.ContentType == "application/x-www-form-urlencoded")
			{
				sUserData = WebUtilities.TypeConverter.ToString(Request.Form["goList"]);
				if (string.IsNullOrEmpty(sUserData))
				{
					sUserData = WebUtilities.TypeConverter.ToString(Request.Form["inputGoList"]);
				}
			}
			if (!string.IsNullOrEmpty(sUserData))
			{
				// we use parameters from post
				sCutOff = WebUtilities.TypeConverter.ToString(Request.Form["cutoff"]);
				sValueType = WebUtilities.TypeConverter.ToString(Request.Form["valueType"]);
				if (string.IsNullOrEmpty(sValueType))
				{
					string sIsPValue = WebUtilities.TypeConverter.ToString(Request.Form["isPValue"]); // {no, yes} - deprecated, use "valueType"
					if (!string.IsNullOrEmpty(sIsPValue))
					{
						sIsPValue = sIsPValue.Trim();
						if (!sIsPValue.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
						{
							string sWhatIsBetter = WebUtilities.TypeConverter.ToString(Request.Form["whatIsBetter"]); // {higher, lower, absolute, abs_log} - deprecated, use "valueType"
							if (!string.IsNullOrEmpty(sWhatIsBetter))
							{
								switch (sWhatIsBetter.Trim().ToLower())
								{
									case "higher":
										sValueType = "Higher";
										break;
									case "lower":
										sValueType = "Lower";
										break;
									case "absolute":
										sValueType = "HigherAbsolute";
										break;
									case "abs_log":
										sValueType = "HigherAbsLog2";
										break;
									default:
										// we don't want to show errors here, just assume default value
										break;
								}
							}
						}
					}
				}
				sSpeciesTaxon = WebUtilities.TypeConverter.ToString(Request.Form["speciesTaxon"]);
				if (string.IsNullOrEmpty(sSpeciesTaxon))
				{
					sSpeciesTaxon = WebUtilities.TypeConverter.ToString(Request.Form["goSizes"]);
				}
				sSimilarity = WebUtilities.TypeConverter.ToString(Request.Form["measure"]);
				sRemoveObsolete = WebUtilities.TypeConverter.ToString(Request.Form["removeObsolete"]);
				sAutoStart = WebUtilities.TypeConverter.ToString(Request.Form["autoStart"]);
				ViewData["GDPRNoReload"] = true;
			}
			else
			{
				int iJobID = -1;
				string sJobID = WebUtilities.TypeConverter.ToString(Request.Query["JobID"]);

				if (!string.IsNullOrEmpty(sJobID) && int.TryParse(sJobID, out iJobID) && Global.Jobs != null && Global.Jobs.ContainsKey(iJobID))
				{
					RevigoWorker oWorker = Global.Jobs.GetValueByKey(iJobID).Worker;

					// we use parameters from Job
					sUserData = oWorker.Data;
					sCutOff = oWorker.CutOff.ToString(CultureInfo.InvariantCulture);
					sValueType = oWorker.ValueType.ToString();
					sSpeciesTaxon = oWorker.Annotations.TaxonID.ToString();
					sSimilarity = oWorker.SemanticSimilarity.ToString();
					sRemoveObsolete = oWorker.RemoveObsolete.ToString();

					Global.RemoveJob(iJobID);
					this.bAutoStart = false;
				}
			}

			if (!string.IsNullOrEmpty(sUserData))
			{
				this.txtGOInput = sUserData;
				this.bAutoStart = false;

				// {0.4 - 0.9}
				// if the value is not provided 0.7 will be assumed
				this.chkCutOff = null;

				if (!string.IsNullOrEmpty(sCutOff))
				{
					double dCutoff = 0.7;
					if (double.TryParse(sCutOff, NumberStyles.Any, CultureInfo.InvariantCulture, out dCutoff))
					{
						if (dCutoff < 0.45)
						{
							this.chkCutOff = "0.4";
						}
						else if (dCutoff >= 0.45 && dCutoff < 0.6)
						{
							this.chkCutOff = "0.5";
						}
						else if (dCutoff >= 0.6 && dCutoff < 0.8)
						{
							this.chkCutOff = "0.7";
						}
						else
						{
							this.chkCutOff = "0.9";
						}
					}
				}

				// if the value is not provided pvalue will be assumed
				this.lstValueType = null;
				if (!string.IsNullOrEmpty(sValueType))
				{
					switch (sValueType.Trim().ToLower())
					{
						case "pvalue":
							this.lstValueType = "PValue";
							break;
						case "higher":
							this.lstValueType = "Higher";
							break;
						case "lower":
							this.lstValueType = "Lower";
							break;
						case "higherabsolute":
							this.lstValueType = "HigherAbsolute";
							break;
						case "higherabslog2":
							this.lstValueType = "HigherAbsLog2";
							break;
					}
				}

				// {SIMREL, LIN, RESNIK, JIANG}
				// if the value is not provided SIMREL will be assumed
				this.lstSimilarity = null;
				if (!string.IsNullOrEmpty(sSimilarity))
				{
					sSimilarity = sSimilarity.Trim();

					Array aSimilarityTypes = Enum.GetValues(typeof(SemanticSimilarityEnum));
					foreach (SemanticSimilarityEnum value in aSimilarityTypes)
					{
						string sValue = value.ToString();
						if (sValue.Equals(sSimilarity, StringComparison.InvariantCultureIgnoreCase))
						{
							this.lstSimilarity = sValue;
							break;
						}
					}
				}

				// One of the supported NCBI species taxon ID (0 for the whole UniProt database).
				// if the value is not provided 0 will be assumed
				this.lstSpecies = null;
				if (!string.IsNullOrEmpty(sSpeciesTaxon))
				{
					int iSpeciesTaxon = 0;
					if (int.TryParse(sSpeciesTaxon, out iSpeciesTaxon))
					{
						for (int i = 0; i < Global.SpeciesAnnotations.Items.Count; i++)
						{
							SpeciesAnnotations annotations = Global.SpeciesAnnotations.Items[i];
							if (annotations.TaxonID == iSpeciesTaxon)
							{
								this.lstSpecies = annotations.TaxonID.ToString();
							}
						}
					}
				}

				bool bRemoveObsolete = true;
				if (!string.IsNullOrEmpty(sRemoveObsolete))
				{
					if (!bool.TryParse(sRemoveObsolete, out bRemoveObsolete))
					{
						bRemoveObsolete = true;
					}
				}
				this.chkRemoveObsolete = bRemoveObsolete ? "true" : "false";

				if (!string.IsNullOrEmpty(sAutoStart))
				{
					if (!bool.TryParse(sAutoStart, out this.bAutoStart))
					{
						this.bAutoStart = false;
					}
				}
			}
		}

		private void ShowError(string text)
		{
			this.ErrorMessage = text;
		}
	}
}
