using System;
using System.Globalization;
using IRB.Revigo.Worker;
using IRB.Revigo.Core;

namespace RevigoWeb
{
	public partial class Results : System.Web.UI.Page
	{
		protected int iJobID = -1;
		protected RevigoWorker oWorker = null;

		protected void Page_Load(object sender, EventArgs e)
		{
			string sJobID = Convert.ToString(Request.Params["JobID"]);

			if (string.IsNullOrEmpty(sJobID) || !int.TryParse(sJobID, out this.iJobID))
			{
				this.iJobID = -1;
			}

			if (Global.Jobs != null && Global.Jobs.ContainsKey(this.iJobID))
			{
				RevigoJob job = Global.Jobs.GetValueByKey(this.iJobID);

				job.ExtendExpiration(Global.SessionTimeout);
				this.oWorker = job.Worker;
			}

			if (this.oWorker != null)
			{
				if (string.IsNullOrEmpty(this.fldGOList.Value))
				{
					this.fldGOList.Value = this.oWorker.Data;
					this.fldCutoff.Value = this.oWorker.CutOff.ToString(CultureInfo.InvariantCulture);
					this.fldValueType.Value = this.oWorker.ValueType.ToString();
					this.fldMeasure.Value = this.oWorker.Measure.ToString();
					this.fldSpeciesTaxon.Value = this.oWorker.Annotations.TaxonID.ToString();
					this.fldRemoveObsolete.Value = this.oWorker.RemoveObsolete.ToString();
				}

				switch (Math.Abs(Environment.TickCount) % 10)
				{
					case 0:
						this.litLoader00.Visible = true;
						break;
					case 1:
						this.litLoader01.Visible = true;
						break;
					case 2:
						this.litLoader02.Visible = true;
						break;
					case 3:
						this.litLoader03.Visible = true;
						break;
					case 4:
						this.litLoader04.Visible = true;
						break;
					case 5:
						this.litLoader05.Visible = true;
						break;
					case 6:
						this.litLoader06.Visible = true;
						break;
					case 7:
						this.litLoader07.Visible = true;
						break;
					case 8:
						this.litLoader08.Visible = true;
						break;
					case 9:
						this.litLoader09.Visible = true;
						break;
					default:
						this.litLoader00.Visible = true;
						break;
				}
			}
			else
			{
				Response.Redirect("~/");
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (!((RevigoMasterPage)this.Master).BasicCompliance)
			{
				this.phAddThis.Visible = true;
			}

			base.OnPreRender(e);
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);

			if (this.oWorker == null)
			{
				Response.Redirect("~/");
			}
		}

		protected void lnkBack_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.fldGOList.Value))
			{
				int iNewJobID = -1;
				try
				{
					iNewJobID = Global.CreateNewJob(RequestSourceEnum.WebPage, this.fldGOList.Value, Convert.ToDouble(this.fldCutoff.Value, CultureInfo.InvariantCulture),
						(ValueTypeEnum)Enum.Parse(typeof(ValueTypeEnum), this.fldValueType.Value, true),
						Global.SpeciesAnnotations.GetByID(Convert.ToInt32(this.fldSpeciesTaxon.Value)),
						(SemanticSimilarityScoreEnum)Enum.Parse(typeof(SemanticSimilarityScoreEnum), this.fldMeasure.Value, true),
						bool.Parse(this.fldRemoveObsolete.Value));
				}
				catch { }

				if (Global.Jobs != null && Global.Jobs.ContainsKey(this.iJobID))
				{
					Global.Jobs.RemoveByKey(this.iJobID);
				}

				Response.Redirect(string.Format("/?JobID={0}", iNewJobID));
			}
		}
	}
}