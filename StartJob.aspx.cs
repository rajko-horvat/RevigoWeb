using System;
using System.Globalization;
using System.IO;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using IRB.Revigo.Core;

namespace RevigoWeb
{
	public partial class StartJob : System.Web.UI.Page
	{
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

			// finished parsing parameters, now start Revigo Job
			int iJobID = -1;
			try
			{
				iJobID = Global.StartNewJob(RequestSourceEnum.JubSubmitting, sUserData, dCutoff, eValueType, oAnnotations, eMeasure, bRemoveObsolete);
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

			Response.Clear();
			Response.ClearHeaders();
			Response.ContentType = "text/plain";
			StreamWriter writer = new StreamWriter(Response.OutputStream);

			writer.Write("{{\"jobid\": {0}, \"message\": \"{1}\"}}", iJobID, "");

			writer.Flush();
			Response.End(); // response end must not be in try block
		}

		private void ShowError(string text)
		{
			Response.Clear();
			Response.ClearHeaders();
			Response.ContentType = "text/plain";
			StreamWriter writer = new StreamWriter(Response.OutputStream);

			writer.Write("{{\"jobid\": -1, \"message\": \"{0}\"}}", text);

			writer.Flush();
			Response.End(); // response end must not be in try block
		}
	}
}