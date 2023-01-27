using System;
using System.Web.UI.WebControls;
using System.Text;
using System.Globalization;
using IRB.Revigo.Worker;
using IRB.Revigo.Core;
using IRB.Revigo.Databases;

namespace RevigoWeb
{
	public partial class _Default : System.Web.UI.Page
	{
		bool bAutoStart = false;

		protected override void OnLoad(EventArgs e)
		{
			this.lblError.Visible = false;

			// fill measure list
			int iTemp = 0;
			string sTemp = this.lstMeasures.SelectedValue;
			this.lstMeasures.Items.Clear();
			foreach (SemanticSimilarityScore value in SemanticSimilarityScore.Values)
			{
				string sValue = value.EnumValue.ToString();
				this.lstMeasures.Items.Add(new ListItem(value.ToString(), sValue));
				if (!string.IsNullOrEmpty(sTemp) && sTemp.Equals(sValue))
				{
					this.lstMeasures.SelectedIndex = iTemp;
				}
				iTemp++;
			}
			if (string.IsNullOrEmpty(sTemp))
				this.lstMeasures.SelectedIndex = 0;

			// fill species list
			sTemp = this.lstSpecies.SelectedValue;
			this.lstSpecies.Items.Clear();
			StringBuilder sbSpeciesCSS = new StringBuilder();
			sbSpeciesCSS.AppendLine("<style type=\"text/css\">");
			if (Global.SpeciesAnnotations != null)
			{
				for (int i = 0; i < Global.SpeciesAnnotations.Items.Count; i++)
				{
					SpeciesAnnotations annotations = Global.SpeciesAnnotations.Items[i];
					string sValue = annotations.TaxonID.ToString();
					if (annotations.TaxonID == 0)
					{
						ListItem item = new ListItem(annotations.SpeciesName, sValue);
						item.Attributes.Add("data-src", string.Format("{0}Images/Species/{1}.jpg", Request.ApplicationPath, sValue));

						sbSpeciesCSS.AppendLine(string.Format(".ui-icon.img{0} {{", sValue));
						sbSpeciesCSS.AppendLine(string.Format("background: url('{0}Images/Species/{1}.jpg') 0 0 no-repeat;", Request.ApplicationPath, sValue));
						sbSpeciesCSS.AppendLine("}");
						this.lstSpecies.Items.Add(item);
					}
					else
					{
						ListItem item = new ListItem(string.Format("{0} ({1})", annotations.SpeciesName, sValue), sValue);
						item.Attributes.Add("data-src", string.Format("{0}Images/Species/{1}.jpg", Request.ApplicationPath, sValue));

						sbSpeciesCSS.AppendLine(string.Format(".ui-icon.img{0} {{", sValue));
						sbSpeciesCSS.AppendLine(string.Format("background: url('{0}Images/Species/{1}.jpg') 0 0 no-repeat;", Request.ApplicationPath, sValue));
						sbSpeciesCSS.AppendLine("}");
						this.lstSpecies.Items.Add(item);
					}
					if (!string.IsNullOrEmpty(sTemp))
					{
						if (sTemp.Equals(sValue))
							this.lstSpecies.SelectedIndex = i;
					}
					else if (annotations.TaxonID == 0)
					{
						this.lstSpecies.SelectedIndex = i;
					}
				}
			}
			else
			{
				this.lstSpecies.Items.Add("Species annotations not loaded");
				this.lstSpecies.SelectedIndex = 0;
			}
			sbSpeciesCSS.AppendLine("</style>");
			//litSpeciesCSS.Text = sbSpeciesCSS.ToString();

			if (Global.Ontology != null)
			{
				this.lblOntologyDate.Text = Global.Ontology.Date.ToString("D", CultureInfo.GetCultureInfo("en-US"));
				this.lnkOntology.Target = "_blank";
				string link = Global.Ontology.Link;
				if (!string.IsNullOrEmpty(link))
				{
					this.lnkOntology.NavigateUrl = link;
					string[] aLink = link.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
					this.lnkOntology.Text = aLink[aLink.Length - 1];
				}
			}

			if (Global.SpeciesAnnotations != null)
			{
				this.lblGOADate.Text = Global.SpeciesAnnotations.Date.ToString("D", CultureInfo.GetCultureInfo("en-US"));
				this.lnkGOA.Target = "_blank";
				string link = Global.SpeciesAnnotations.Link;
				if (!string.IsNullOrEmpty(link))
				{
					this.lnkGOA.NavigateUrl = link;
					string[] aLink = link.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
					this.lnkGOA.Text = aLink[aLink.Length - 1];
				}
			}

			if (!this.IsPostBack)
				FillParameters();

			if (this.bAutoStart)
			{
				this.btnStart_Click(this, EventArgs.Empty);
			}

			base.OnLoad(e);
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (!((RevigoMasterPage)this.Master).BasicCompliance)
			{
				this.phAddThis.Visible = true;
			}

			base.OnPreRender(e);
		}

		protected void btnStart_Click(object sender, EventArgs e)
		{
			// validate parameters
			string sUserData = this.txtGOInput.Text.Trim();
			double dCutoff = 0.7;
			ValueTypeEnum eValueType = ValueTypeEnum.PValue;
			int iSpeciesTaxon = 0;
			SemanticSimilarityScoreEnum eMeasure = SemanticSimilarityScoreEnum.SIMREL;
			bool bRemoveObsolete = true;

			if (!((RevigoMasterPage)this.Master).GDPRCompliance)
			{
				ShowError("Please first Accept our Terms Of Use.");
				return;
			}

			if(Global.SpeciesAnnotations == null)
			{
				ShowError("Species annotations object is not initialized.");
				return;
			}

			if (string.IsNullOrEmpty(sUserData))
			{
				ShowError("The data you submitted is empty, plase fill out your data or use one of our examples.");
				return;
			}

			if (this.chkSimilarity0_9.Checked)
			{
				dCutoff = 0.9;
			}
			else if (this.chkSimilarity0_7.Checked)
			{
				dCutoff = 0.7;
			}
			else if (this.chkSimilarity0_5.Checked)
			{
				dCutoff = 0.5;
			}
			else if (this.chkSimilarity0_4.Checked)
			{
				dCutoff = 0.4;
			}
			else
			{
				ShowError("Please select appropriate similarity measure.");
				return;
			}

			switch (this.lstValueType.SelectedValue.ToLower())
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

			if (!int.TryParse(this.lstSpecies.SelectedValue, out iSpeciesTaxon) || !Global.SpeciesAnnotations.ContainsID(iSpeciesTaxon))
			{
				ShowError("Please select appropriate species taxon.");
				return;
			}
			SpeciesAnnotations oAnnotations = Global.SpeciesAnnotations.GetByID(iSpeciesTaxon);

			string sMeasure = this.lstMeasures.SelectedValue;
			bool bFound = false;
			Array aEnumValues = Enum.GetValues(typeof(SemanticSimilarityScoreEnum));
			foreach (var value in aEnumValues)
			{
				if (value.ToString().Equals(sMeasure, StringComparison.InvariantCultureIgnoreCase))
				{
					eMeasure = (SemanticSimilarityScoreEnum)value;
					bFound = true;
					break;
				}
			}
			if (!bFound)
			{
				ShowError("Please select appropriate similarity measure to use.");
				return;
			}

			bRemoveObsolete = this.lstRemoveObsolete.SelectedIndex == 0;

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

			Response.Redirect(string.Format("~/Results.aspx?jobid={0}", iJobID));
		}

		// we wan't to support old style job submitting, and returns from result page
		private void FillParameters()
		{
			string sUserData = null;
			string sCutOff = null;
			string sValueType = null;
			string sSpeciesTaxon = null;
			string sMeasure = null;
			string sRemoveObsolete = null;
			string sAutoStart = null;

			sUserData = Convert.ToString(Request.Params["goList"]);
			if (string.IsNullOrEmpty(sUserData))
			{
				sUserData = Convert.ToString(Request.Params["inputGoList"]);
			}
			if (!string.IsNullOrEmpty(sUserData))
			{
				// we use parameters from post
				sCutOff = Convert.ToString(Request.Params["cutoff"]);
				sValueType = Convert.ToString(Request.Params["valueType"]);
				if (string.IsNullOrEmpty(sValueType))
				{
					string sIsPValue = Convert.ToString(Request.Params["isPValue"]); // {no, yes} - deprecated, use "valueType"
					if (!string.IsNullOrEmpty(sIsPValue))
					{
						sIsPValue = sIsPValue.Trim();
						if (!sIsPValue.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
						{
							string sWhatIsBetter = Convert.ToString(Request.Params["whatIsBetter"]); // {higher, lower, absolute, abs_log} - deprecated, use "valueType"
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
				sSpeciesTaxon = Convert.ToString(Request.Params["speciesTaxon"]);
				if (string.IsNullOrEmpty(sSpeciesTaxon))
				{
					sSpeciesTaxon = Convert.ToString(Request.Params["goSizes"]);
				}
				sMeasure = Convert.ToString(Request.Params["measure"]);
				sRemoveObsolete = Convert.ToString(Request.Params["removeObsolete"]);
				sAutoStart = Convert.ToString(Request.Params["autoStart"]);
			}
			else
			{
				int iJobID = -1;
				string sJobID = Convert.ToString(Request.Params["JobID"]);

				if (!string.IsNullOrEmpty(sJobID) && int.TryParse(sJobID, out iJobID) && Global.Jobs != null && Global.Jobs.ContainsKey(iJobID))
				{
					RevigoWorker oWorker = Global.Jobs.GetValueByKey(iJobID).Worker;

					// we use parameters from Job
					sUserData = oWorker.Data;
					sCutOff = oWorker.CutOff.ToString(CultureInfo.InvariantCulture);
					sValueType = oWorker.ValueType.ToString();
					sSpeciesTaxon = oWorker.Annotations.TaxonID.ToString();
					sMeasure = oWorker.Measure.ToString();
					sRemoveObsolete = oWorker.RemoveObsolete.ToString();

					Global.Jobs.RemoveByKey(iJobID);
				}
			}

			if (!string.IsNullOrEmpty(sUserData))
			{
				this.txtGOInput.Text = sUserData;

				// {0.4 - 0.9}
				// if the value is not provided 0.7 will be assumed
				this.chkSimilarity0_4.Checked = false;
				this.chkSimilarity0_5.Checked = false;
				this.chkSimilarity0_7.Checked = false;
				this.chkSimilarity0_9.Checked = false;

				if (!string.IsNullOrEmpty(sCutOff))
				{
					double dCutoff = 0.7;
					if (double.TryParse(sCutOff, NumberStyles.Any, CultureInfo.InvariantCulture, out dCutoff))
					{
						if (dCutoff < 0.45)
						{
							this.chkSimilarity0_4.Checked = true;
						}
						else if (dCutoff >= 0.45 && dCutoff < 0.6)
						{
							this.chkSimilarity0_5.Checked = true;
						}
						else if (dCutoff >= 0.6 && dCutoff < 0.8)
						{
							this.chkSimilarity0_7.Checked = true;
						}
						else
						{
							this.chkSimilarity0_9.Checked = true;
						}
					}
					else
					{
						this.chkSimilarity0_7.Checked = true;
					}
				}
				else
				{
					this.chkSimilarity0_7.Checked = true;
				}

				// if the value is not provided pvalue will be assumed
				this.lstValueType.SelectedIndex = 0;
				if (!string.IsNullOrEmpty(sValueType))
				{
					sValueType = sValueType.Trim();

					for (int i = 0; i < this.lstValueType.Items.Count; i++)
					{
						if (this.lstValueType.Items[i].Value.Equals(sValueType, StringComparison.InvariantCultureIgnoreCase))
						{
							this.lstValueType.SelectedIndex = i;
							break;
						}
					}
				}

				// {SIMREL, LIN, RESNIK, JIANG}
				// if the value is not provided SIMREL will be assumed
				this.lstMeasures.SelectedIndex = 0;
				if (!string.IsNullOrEmpty(sMeasure))
				{
					sMeasure = sMeasure.Trim();

					for (int i = 0; i < this.lstMeasures.Items.Count; i++)
					{
						if (this.lstMeasures.Items[i].Value.Equals(sMeasure, StringComparison.InvariantCultureIgnoreCase))
						{
							this.lstMeasures.SelectedIndex = i;
							break;
						}
					}
				}

				// One of the supported NCBI species taxon ID (0 for the whole UniProt database).
				// if the value is not provided 0 will be assumed
				int iSpeciesTaxon = 0;
				if (!string.IsNullOrEmpty(sSpeciesTaxon))
				{
					if (!int.TryParse(sSpeciesTaxon, out iSpeciesTaxon))
					{
						iSpeciesTaxon = 0;
					}
				}
				for (int i = 0; i < this.lstSpecies.Items.Count; i++)
				{
					if (Convert.ToInt32(this.lstSpecies.Items[i].Value) == iSpeciesTaxon)
					{
						this.lstSpecies.SelectedIndex = i;
						break;
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
				this.lstRemoveObsolete.SelectedIndex = bRemoveObsolete ? 0 : 1;

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
			this.lblError.Visible = true;
			this.lblError.Text = text;
		}

		public string GetMyVersion()
		{
			Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

			return string.Format("{0}.{1}. (build {2}{3:0000})", ver.Major, ver.Minor, ver.Build, ver.Revision);
		}
	}
}
