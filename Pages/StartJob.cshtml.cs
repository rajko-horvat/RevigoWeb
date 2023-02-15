using IRB.Revigo.Core;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace IRB.RevigoWeb.Pages
{
	[IgnoreAntiforgeryToken]
	public class StartJobModel : PageModel
    {
        public ContentResult OnGet()
        {
			return ProcessJob();
        }

		public ContentResult OnPost()
		{
			return ProcessJob();
		}

		private ContentResult ProcessJob()
		{
			double dCutoff = 0.7;
			ValueTypeEnum eValueType = ValueTypeEnum.PValue;
			int iSpeciesTaxon = 0;
			SemanticSimilarityEnum eMeasure = SemanticSimilarityEnum.SIMREL;
			bool bRemoveObsolete = true;

			if (Request.ContentType != "application/x-www-form-urlencoded")
			{
				return ReturnError("Tre request must be sent as a form.");
			}

			if (Global.SpeciesAnnotations == null)
			{
				return ReturnError("Species annotations object is not initialized.");
			}

			string sUserData = Convert.ToString(Request.Form["goList"]);
			if (string.IsNullOrEmpty(sUserData))
			{
				return ReturnError("The goList field is required");
			}

			// {0.4 - 0.9}
			// if the value is not provided 0.7 will be assumed
			string sCutOff = Convert.ToString(Request.Form["cutoff"]);
			if (!string.IsNullOrEmpty(sCutOff) && (!double.TryParse(sCutOff, NumberStyles.Any, CultureInfo.InvariantCulture, out dCutoff) || dCutoff < 0.4 || dCutoff > 0.9))
			{
				return ReturnError("The cutoff field has invalid value");
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
					return ReturnError("The valueType field has invalid value");
				}
			}

			// One of the supported NCBI species taxon ID (0 for the whole UniProt database).
			// if the value is not provided 0 will be assumed
			string sSpeciesTaxon = Convert.ToString(Request.Form["speciesTaxon"]);
			if (!string.IsNullOrEmpty(sSpeciesTaxon))
			{
				if (!int.TryParse(sSpeciesTaxon, out iSpeciesTaxon) || !Global.SpeciesAnnotations.ContainsID(iSpeciesTaxon))
				{
					return ReturnError("The speciesTaxon field has invalid value");
				}
			}
			SpeciesAnnotations? oAnnotations = Global.SpeciesAnnotations.GetByID(iSpeciesTaxon);
			if (oAnnotations == null)
				return ReturnError("The speciesTaxon field has invalid value");

			// {SIMREL, LIN, RESNIK, JIANG}
			// if the value is not provided SIMREL will be assumed
			string sMeasure = Convert.ToString(Request.Form["measure"]);
			if (!string.IsNullOrEmpty(sMeasure))
			{
				sMeasure = sMeasure.Trim();
				string[] aMeasures = Enum.GetNames(typeof(SemanticSimilarityEnum));
				bool bFound = false;
				foreach (string measure in aMeasures)
				{
					if (measure.Equals(sMeasure, StringComparison.CurrentCultureIgnoreCase))
					{
						eMeasure = (SemanticSimilarityEnum)Enum.Parse(typeof(SemanticSimilarityEnum), sMeasure, true);
						bFound = true;
						break;
					}
				}

				if (!bFound)
				{
					return ReturnError("The measure field has invalid value");
				}
			}

			string sRemoveObsolete = Convert.ToString(Request.Form["removeObsolete"]);
			if (!string.IsNullOrEmpty(sRemoveObsolete))
			{
				if (!bool.TryParse(sRemoveObsolete, out bRemoveObsolete))
				{
					return ReturnError("The removeObsolete field has invalid value");
				}
			}

			// finished parsing parameters, now start Revigo Job
			int iJobID = -1;
			try
			{
				iJobID = Global.StartNewJob(RequestSourceEnum.JobSubmitting, sUserData, dCutoff, eValueType, oAnnotations, eMeasure, bRemoveObsolete);
			}
			catch (Exception ex)
			{
				Global.WriteToSystemLog($"{this.GetType().Name}.ProcessJob", $"Message: '{ex.Message}', Stack trace: '{ex.StackTrace}'");
				return ReturnError("Undefined error occured");
			}

			if (iJobID <= 0)
			{
				return ReturnError("Unable to start a new job, please try again later.");
			}

			StringBuilder writer = new StringBuilder();
			string sContentType = "application/json";

			writer.AppendFormat("{{\"jobid\": {0}, \"message\": \"{1}\"}}", iJobID, "");

			return Content(writer.ToString(), sContentType, Encoding.UTF8);
		}

		private ContentResult ReturnError(string message)
		{
			StringBuilder writer = new StringBuilder();
			string sContentType = "application/json";

			writer.AppendFormat("{{\"jobid\": -1, \"message\": \"{0}\"}}", message);

			return Content(writer.ToString(), sContentType, Encoding.UTF8);
		}
	}
}
