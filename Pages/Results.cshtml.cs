using IRB.Revigo.Core;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace IRB.RevigoWeb.Pages
{
	public class ResultsModel : PageModel
	{
		[BindProperty]
		public string? lnkBack { get; set; }

		private int iJobID = -1;
		RevigoWorker? oWorker = null;

		public string JobID
		{
			get
			{
				return this.iJobID.ToString();
			}
		}

		public RevigoWorker? Worker
		{
			get
			{
				return this.oWorker;
			}
		}

		[BindProperty]
		public string? fldGOList { get; set; }
		[BindProperty]
		public string? fldCutoff { get; set; }
		[BindProperty]
		public string? fldValueType { get; set; }
		[BindProperty]
		public string? fldSpeciesTaxon { get; set; }
		[BindProperty]
		public string? fldMeasure { get; set; }
		[BindProperty]
		public string? fldRemoveObsolete { get; set; }


		public void OnGet()
		{
			string sJobID = Convert.ToString(Request.Query["JobID"]);

			if (string.IsNullOrEmpty(sJobID) || !int.TryParse(sJobID, out this.iJobID))
			{
				this.iJobID = -1;
			}

			if (Global.Jobs != null && Global.Jobs.ContainsKey(this.iJobID))
			{
				RevigoJob job = Global.Jobs.GetValueByKey(this.iJobID);

				job.ExtendExpiration();
				oWorker = job.Worker;
			}

			if (oWorker == null)
			{
				Response.Redirect(Url.Content("~/"));
			}
		}

		public void OnPost()
		{
			if (!string.IsNullOrEmpty(lnkBack))
			{
				if (Global.SpeciesAnnotations != null &&
					!string.IsNullOrEmpty(this.fldGOList) && this.fldValueType != null && this.fldSpeciesTaxon != null && 
					this.fldMeasure != null && this.fldRemoveObsolete != null)
				{
					int iNewJobID = -1;
					try
					{
						SpeciesAnnotations? oAnnotations = Global.SpeciesAnnotations.GetByID(Convert.ToInt32(this.fldSpeciesTaxon));
						if (oAnnotations != null)
						{
							iNewJobID = Global.CreateNewJob(RequestSourceEnum.WebPage, this.fldGOList,
								Convert.ToDouble(this.fldCutoff, CultureInfo.InvariantCulture),
								(ValueTypeEnum)Enum.Parse(typeof(ValueTypeEnum), this.fldValueType, true),
								oAnnotations,
								(SemanticSimilarityEnum)Enum.Parse(typeof(SemanticSimilarityEnum), this.fldMeasure, true),
								bool.Parse(this.fldRemoveObsolete));
						}
					}
					catch { }

					if (Global.Jobs != null && Global.Jobs.ContainsKey(this.iJobID))
					{
						Global.Jobs.RemoveByKey(this.iJobID);
					}

					Response.Clear();
					Response.Redirect(Url.Content("~/") + string.Format("?JobID={0}", iNewJobID));
				}
			}
		}
	}
}
