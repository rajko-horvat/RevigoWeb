using IRB.Revigo.Core;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace IRB.RevigoWeb.Pages
{
	[IgnoreAntiforgeryToken]
	public class QueryJobModel : PageModel
    {
		private int iJobID = -1;
		private RevigoWorker oWorker = null;
		private GONamespaceEnum eNamespace = GONamespaceEnum.None;
		private string sNamespace = "_";
		private TermListVisualizer oVisualizer = null;

		private bool bAttachment = false;
		private bool bJSON = true;  // true by default

		public ContentResult OnGet()
		{
			return ProcessQuery();
		}

		public ContentResult OnPost()
		{
			return ProcessQuery();
		}

		private ContentResult ProcessQuery()
		{
			string sType = null;

			try
			{
				sType = WebUtilities.TypeConverter.ToString(Request.Query["type"]);
				if (!string.IsNullOrEmpty(sType))
				{
					sType = sType.ToLower();

					if (sType.StartsWith("a", StringComparison.InvariantCultureIgnoreCase))
					{
						this.bAttachment = true;
						this.bJSON = false;
						sType = sType.Substring(1);
					}
					else if (!sType.StartsWith("j", StringComparison.InvariantCultureIgnoreCase))
					{
						this.bJSON = false;
					}
				}

				string sJobID = WebUtilities.TypeConverter.ToString(Request.Query["JobID"]);

				if (!string.IsNullOrEmpty(sJobID) && int.TryParse(sJobID, out iJobID) &&
					Global.Jobs != null && Global.Jobs.ContainsKey(iJobID))
				{
					RevigoJob job = Global.Jobs.GetValueByKey(iJobID);
					oWorker = job.Worker;
					job.ExtendExpiration();
				}

				// this is a deal breaker, we have to positively indentify a job
				if (oWorker == null)
				{
					return ReturnError("Unknown job");
				}

				StringBuilder writer = new StringBuilder();
				string sContentType;
				GOTermList terms;
				bool bFirst;
				string sLabelOfValue;
				ContentResult oCheckParameterResult;

				Global.UpdateJobUsageStats(oWorker, sType, WebUtilities.TypeConverter.ToString(Request.Query["namespace"]));

				switch (sType)
				{
					case "jstatus":
						#region jstatus						
						writer.AppendFormat("{{\"running\":{0},\"progress\":{1},\"message\":\"{2}\"}}",
							oWorker.IsRunning || !oWorker.IsFinished ? 1 : 0,
							oWorker.Progress.ToString("##0.00", CultureInfo.InvariantCulture),
							string.IsNullOrEmpty(oWorker.ProgressText) ? "" : WebUtilities.TypeConverter.StringToJSON(oWorker.ProgressText));
						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					case "jinfo":
						#region jinfo
						if (oWorker.IsRunning || !oWorker.IsFinished)
						{
							return ReturnError("The Job did not yet complete");
						}

						writer.AppendFormat("{{\"warnings\":{0},\"errors\":{1},\"exectime\":{2}," +
							"\"HasBP\":{3},\"BPCount\":{4}," +
							"\"HasCC\":{5},\"CCCount\":{6}," +
							"\"HasMF\":{7},\"MFCount\":{8}," +
							"\"HasClouds\":{9}}}",
							oWorker.HasUserWarnings ? WebUtilities.TypeConverter.StringArrayToJSON(oWorker.UserWarnings) : "[]",
							oWorker.HasUserErrors ? WebUtilities.TypeConverter.StringArrayToJSON(oWorker.UserErrors) : "[]",
							(long)(oWorker.ExecutingTime.TotalSeconds),
							oWorker.HasBPVisualizer ? 1 : 0, oWorker.HasBPVisualizer ? oWorker.BPVisualizer.Terms.Length : 0,
							oWorker.HasCCVisualizer ? 1 : 0, oWorker.HasCCVisualizer ? oWorker.CCVisualizer.Terms.Length : 0,
							oWorker.HasMFVisualizer ? 1 : 0, oWorker.HasMFVisualizer ? oWorker.MFVisualizer.Terms.Length : 0,
							oWorker.HasClouds ? 1 : 0);
						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					case "jpinterm":
						#region Pin Term
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						int iTermID = -1;
						string sTemID = WebUtilities.TypeConverter.ToString(Request.Query["TermID"]);

						if (!string.IsNullOrEmpty(sTemID) && int.TryParse(sTemID, out iTermID) &&
							this.oWorker.ContainsTermID(iTermID))
						{
							this.oWorker.PinTerm(iTermID);

							writer.Append("{\"Success\":1}");
						}
						else
						{
							return ReturnError("Unknown TermID");
						}
						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					case "jtable":
						#region jtable
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, this.oWorker.AllProperties, this.oWorker.CutOff);

						writer.Append("{\"Columns\":[");

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"ID", "",
							"Number", -1, 0, 0, 0, 0);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Term ID", "This is the GO term ID",
							"Term", 0, 1, 1, 1, 1);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"CellTitle\":\"{2}\",\"Type\":\"{3}\",\"Decimals\":{4},\"Sortable\":{5},\"Filter\":{6},\"Condensed\":{7},\"Visible\":{8}}},",
							"Name", "Name of the GO term as defined in Gene ontology (hoover mouse over GO term name for description)", "Description",
							"String", 0, 1, 1, 0, 1);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Description", "",
							"String", 0, 0, 0, 0, 0);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Pin Term", "Use this to ensure that the term gets chosen as the cluster representative",
							"PinTerm", 0, 0, 0, 1, 1);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Value", "This is the value (if any) you provided alongside the GO term ID in your input data set",
							"Number", 4, 1, 1, 1, 1);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"LogSize", "The Log10(Number of annotations for GO Term ID in selected species in the EBI GOA database)",
							"Number", 3, 1, 1, 1, 1);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Frequency", "The proportion of this GO term in the underlying protein annotation database (by default, whole UniProt, but the user can select a single-species subset). Higher frequency implies more general terms, lower - more specific ones",
							"Percentage", 3, 1, 1, 1, 1);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Uniqueness", "Measures whether the term is an outlier when compared semantically to the whole list. Calculated as 1-(average semantic similarity of a term to all other terms). More unique terms tend to be less dispensable",
							"Number", 3, 1, 1, 1, 1);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Dispensability", "The semantic similarity threshold at which the term was removed from the list and assigned to a cluster. Cluster representatives always have dispensability less than the user-specified 'allowed similarity' cutoff",
							"Number", 3, 1, 1, 1, 1);

						writer.AppendFormat("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}}",
							"Representative", "",
							"Number", -1, 0, 0, 0, 0);

						writer.Append("],\"Rows\":[");

						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							if (i > 0)
								writer.Append(",");

							writer.Append("{");
							writer.AppendFormat("\"ID\":{0},", oTerm.ID);
							writer.AppendFormat("\"Term ID\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.FormattedID));
							writer.AppendFormat("\"Name\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.Name));
							writer.AppendFormat("\"Description\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.Description));
							writer.AppendFormat("\"Pin Term\":{0},", oProperties.Pinned ? 0 : (oProperties.Representative <= 0 ? -1 : oTerm.ID));
							writer.AppendFormat("\"Value\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Value));
							writer.AppendFormat("\"LogSize\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.LogAnnotationSize));
							writer.AppendFormat("\"Frequency\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.AnnotationFrequency * 100.0));
							writer.AppendFormat("\"Uniqueness\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Uniqueness));
							writer.AppendFormat("\"Dispensability\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Dispensability));
							writer.AppendFormat("\"Representative\":{0}", oProperties.Representative);
							writer.Append("}");
						}
						writer.Append("]}");
						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					case "table":
						#region table
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						if (!bAttachment)
						{
							sContentType = "text/plain";
						}
						else
						{
							sContentType = "application/octet-stream";
							Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=Revigo{0}Table.tsv", this.sNamespace));
						}

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);

						writer.Append("TermID\tName\tValue\t");
						for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
						{
							writer.AppendFormat("UserValue_{0}\t", c - 1);
						}
						writer.AppendLine("LogSize\tFrequency\tUniqueness\tDispensability\tRepresentative");

						// print the data
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							writer.AppendFormat("\"{0}\"\t", oTerm.FormattedID);
							writer.AppendFormat("\"{0}\"\t", oTerm.Name);
							writer.AppendFormat("{0}\t", oProperties.Value.ToString(CultureInfo.InvariantCulture));

							for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
							{
								writer.AppendFormat("{0}\t", oProperties.UserValues[c - 1].ToString(CultureInfo.InvariantCulture));
							}

							writer.AppendFormat("{0}\t",
								oProperties.LogAnnotationSize.ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t",
								(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));

							if (oProperties.Representative > 0)
							{
								writer.AppendFormat("{0}", oProperties.Representative);
							}
							else
							{
								writer.Append("null");
							}

							writer.AppendLine();
						}
						#endregion
						return Content(writer.ToString(), sContentType, Encoding.UTF8);

					case "jscatterplot":
						#region jscatterplot
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, this.oWorker.AllProperties, this.oWorker.CutOff);

						writer.Append("[");

						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							if (i > 0)
								writer.Append(",");

							writer.Append("{");
							writer.AppendFormat("\"ID\":{0},", oTerm.ID);
							writer.AppendFormat("\"Term ID\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.FormattedID));
							writer.AppendFormat("\"Name\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.Name));
							writer.AppendFormat("\"Description\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.Description));
							writer.AppendFormat("\"Pin Term\":{0},", oProperties.Pinned ? 0 : (oProperties.Representative <= 0 ? -1 : oTerm.ID));
							writer.AppendFormat("\"Value\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Value));
							writer.AppendFormat("\"LogSize\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.LogAnnotationSize));
							writer.AppendFormat("\"Frequency\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.AnnotationFrequency * 100.0));
							writer.AppendFormat("\"Uniqueness\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Uniqueness));
							writer.AppendFormat("\"Dispensability\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Dispensability));

							// 2D
							writer.AppendFormat("\"PC_0\":{0},", (oProperties.PC.Count > 0) ?
								WebUtilities.TypeConverter.DoubleToJSON(oProperties.PC[0]) : "0");
							writer.AppendFormat("\"PC_1\":{0},", (oProperties.PC.Count > 1) ?
								WebUtilities.TypeConverter.DoubleToJSON(oProperties.PC[1]) : "0");
							writer.AppendFormat("\"Selected\":{0},", (oProperties.Dispensability <= 0.05) ? 1 : 0);

							writer.AppendFormat("\"Representative\":{0}", oProperties.Representative);
							writer.Append("}");
						}
						writer.Append("]");
						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					case "scatterplot":
						#region scatterplot
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						Response.Clear();
						if (!bAttachment)
						{
							sContentType = "text/plain";
						}
						else
						{
							sContentType = "application/octet-stream";
							Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=Revigo{0}Scatterplot.tsv", this.sNamespace));
						}

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);

						writer.AppendLine("TermID\tName\tValue\tLogSize\tFrequency\tUniqueness\tDispensability\tPC_0\tPC_1\tRepresentative");

						// print the data
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							writer.AppendFormat("\"{0}\"\t", oTerm.FormattedID);
							writer.AppendFormat("\"{0}\"\t", oTerm.Name);
							writer.AppendFormat("{0}\t", oProperties.Value.ToString(CultureInfo.InvariantCulture));

							writer.AppendFormat("{0}\t",
								oProperties.LogAnnotationSize.ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t",
								(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));

							// 2D
							writer.AppendFormat("{0}\t", (oProperties.PC.Count > 0) ?
								oProperties.PC[0].ToString(CultureInfo.InvariantCulture) : "null");
							writer.AppendFormat("{0}\t", (oProperties.PC.Count > 1) ?
								oProperties.PC[1].ToString(CultureInfo.InvariantCulture) : "null");

							if (oProperties.Representative > 0)
							{
								writer.AppendFormat("{0}", oProperties.Representative);
							}
							else
							{
								writer.Append("null");
							}

							writer.AppendLine();
						}
						#endregion
						return Content(writer.ToString(), sContentType, Encoding.UTF8);

					case "rscatterplot":
						#region rscatterplot
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						if (!bAttachment)
						{
							sContentType = "text/plain";
						}
						else
						{
							sContentType = "application/octet-stream";
							Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=Revigo{0}Scatterplot.R", this.sNamespace));
						}

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);

						writer.AppendLine("# A plotting R script produced by the Revigo server at http://revigo.irb.hr/");
						writer.AppendLine("# If you found Revigo useful in your work, please cite the following reference:");
						writer.AppendLine("# Supek F et al. \"REVIGO summarizes and visualizes long lists of Gene Ontology");
						writer.AppendLine("# terms\" PLoS ONE 2011. doi:10.1371/journal.pone.0021800");
						writer.AppendLine();
						writer.AppendLine("# --------------------------------------------------------------------------");
						writer.AppendLine("# If you don't have the ggplot2 package installed, uncomment the following line:");
						writer.AppendLine("# install.packages( \"ggplot2\" );");
						writer.AppendLine("library( ggplot2 );");
						writer.AppendLine();
						writer.AppendLine("# --------------------------------------------------------------------------");
						writer.AppendLine("# If you don't have the scales package installed, uncomment the following line:");
						writer.AppendLine("# install.packages( \"scales\" );");
						writer.AppendLine("library( scales );");
						writer.AppendLine();
						writer.AppendLine("# --------------------------------------------------------------------------");
						writer.AppendLine("# Here is your data from Revigo. Scroll down for plot configuration options.");
						writer.AppendLine();

						writer.Append("revigo.names <- c(\"term_ID\",\"description\",\"frequency\",\"plot_X\",\"plot_Y\",\"log_size\",");
						if (oWorker.TermsWithValuesCount > 0)
						{
							sLabelOfValue = "value";
							writer.AppendFormat("\"{0}\",", sLabelOfValue);
						}
						else
						{
							sLabelOfValue = "uniqueness";
						}

						for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
						{
							writer.AppendFormat("\"UserValue_{0}\",", c - 1);
						}
						writer.AppendLine("\"uniqueness\",\"dispensability\");");

						// print the data
						writer.Append("revigo.data <- rbind(");
						bFirst = true;
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm curGOTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(curGOTerm.ID);
							bool isTermEliminated = oProperties.Dispensability > oWorker.CutOff;
							if (!isTermEliminated)
							{
								if (bFirst)
								{
									bFirst = false;
								}
								else
								{
									writer.AppendLine(",");
								}
								writer.Append("c(");
								writer.AppendFormat("\"{0}\",", curGOTerm.FormattedID);
								writer.AppendFormat("\"{0}\",", curGOTerm.Name);
								writer.AppendFormat("{0},",
									(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));

								writer.AppendFormat("{0},", (oProperties.PC.Count > 0) ?
									oProperties.PC[0].ToString(CultureInfo.InvariantCulture) : "null");
								writer.AppendFormat("{0},", (oProperties.PC.Count > 1) ?
									oProperties.PC[1].ToString(CultureInfo.InvariantCulture) : "null");

								writer.AppendFormat("{0},",
									oProperties.LogAnnotationSize.ToString(CultureInfo.InvariantCulture));

								if (oWorker.TermsWithValuesCount > 0)
								{
									writer.AppendFormat("{0},", oProperties.Value.ToString(CultureInfo.InvariantCulture));
								}

								for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
								{
									writer.AppendFormat("{0},", oProperties.UserValues[c - 1].ToString(CultureInfo.InvariantCulture));
								}

								writer.AppendFormat("{0},", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
								writer.AppendFormat("{0}", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));
								writer.Append(")");
							}
						}
						writer.AppendLine(");");
						writer.AppendLine();
						writer.AppendLine("one.data <- data.frame(revigo.data);");
						writer.AppendLine("names(one.data) <- revigo.names;");
						writer.AppendLine("one.data <- one.data [(one.data$plot_X != \"null\" & one.data$plot_Y != \"null\"), ];");
						writer.AppendLine("one.data$plot_X <- as.numeric( as.character(one.data$plot_X) );");
						writer.AppendLine("one.data$plot_Y <- as.numeric( as.character(one.data$plot_Y) );");
						writer.AppendLine("one.data$log_size <- as.numeric( as.character(one.data$log_size) );");
						if (oWorker.TermsWithValuesCount > 0)
						{
							writer.AppendFormat("one.data${0} <- as.numeric( as.character(one.data${0}) );", sLabelOfValue);
							writer.AppendLine();
						}
						writer.AppendLine("one.data$frequency <- as.numeric( as.character(one.data$frequency) );");
						writer.AppendLine("one.data$uniqueness <- as.numeric( as.character(one.data$uniqueness) );");
						writer.AppendLine("one.data$dispensability <- as.numeric( as.character(one.data$dispensability) );");
						writer.AppendLine("#head(one.data);");
						writer.AppendLine();
						writer.AppendLine();
						writer.AppendLine("# --------------------------------------------------------------------------");
						writer.AppendLine("# Names of the axes, sizes of the numbers and letters, names of the columns,");
						writer.AppendLine("# etc. can be changed below");
						writer.AppendLine();
						writer.AppendLine("p1 <- ggplot( data = one.data );");
						writer.AppendFormat("p1 <- p1 + geom_point( aes( plot_X, plot_Y, colour = {0}, size = log_size), alpha = I(0.6) );", sLabelOfValue);
						writer.AppendLine();
						writer.AppendFormat("p1 <- p1 + scale_colour_gradientn( colours = c(\"blue\", \"green\", \"yellow\", \"red\"), limits = c( min(one.data${0}), 0) );", sLabelOfValue);
						writer.AppendLine();
						writer.AppendLine("p1 <- p1 + geom_point( aes(plot_X, plot_Y, size = log_size), shape = 21, fill = \"transparent\", colour = I (alpha (\"black\", 0.6) ));");
						writer.AppendLine("p1 <- p1 + scale_size( range=c(5, 30)) + theme_bw(); # + scale_fill_gradientn(colours = heat_hcl(7), limits = c(-300, 0) );");
						writer.AppendLine("ex <- one.data [ one.data$dispensability < 0.15, ];");
						writer.AppendLine("p1 <- p1 + geom_text( data = ex, aes(plot_X, plot_Y, label = description), colour = I(alpha(\"black\", 0.85)), size = 3 );");
						writer.AppendLine("p1 <- p1 + labs (y = \"semantic space x\", x = \"semantic space y\");");
						writer.AppendLine("p1 <- p1 + theme(legend.key = element_blank()) ;");
						writer.AppendLine("one.x_range = max(one.data$plot_X) - min(one.data$plot_X);");
						writer.AppendLine("one.y_range = max(one.data$plot_Y) - min(one.data$plot_Y);");
						writer.AppendLine("p1 <- p1 + xlim(min(one.data$plot_X)-one.x_range/10,max(one.data$plot_X)+one.x_range/10);");
						writer.AppendLine("p1 <- p1 + ylim(min(one.data$plot_Y)-one.y_range/10,max(one.data$plot_Y)+one.y_range/10);");
						writer.AppendLine();
						writer.AppendLine();
						writer.AppendLine("# --------------------------------------------------------------------------");
						writer.AppendLine("# Output the plot to screen");
						writer.AppendLine();
						writer.AppendLine("p1;");
						writer.AppendLine();
						writer.AppendLine("# Uncomment the line below to also save the plot to a file.");
						writer.AppendLine("# The file type depends on the extension (default=pdf).");
						writer.AppendLine();
						writer.AppendLine("# ggsave(\"/path_to_your_file/revigo-plot.pdf\");");
						writer.AppendLine();
						#endregion
						return Content(writer.ToString(), sContentType, Encoding.UTF8);

					case "jscatterplot3d":
						#region jscatterplot3d
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, this.oWorker.AllProperties, this.oWorker.CutOff);

						writer.Append("[");

						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							if (i > 0)
								writer.Append(",");

							writer.Append("{");
							writer.AppendFormat("\"ID\":{0},", oTerm.ID);
							writer.AppendFormat("\"Term ID\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.FormattedID));
							writer.AppendFormat("\"Name\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.Name));
							writer.AppendFormat("\"Description\":\"{0}\",", WebUtilities.TypeConverter.StringToJSON(oTerm.Description));
							writer.AppendFormat("\"Pin Term\":{0},", oProperties.Pinned ? 0 : (oProperties.Representative <= 0 ? -1 : oTerm.ID));
							writer.AppendFormat("\"Value\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Value));
							writer.AppendFormat("\"LogSize\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.LogAnnotationSize));
							writer.AppendFormat("\"Frequency\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.AnnotationFrequency * 100.0));
							writer.AppendFormat("\"Uniqueness\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Uniqueness));
							writer.AppendFormat("\"Dispensability\":{0},", WebUtilities.TypeConverter.DoubleToJSON(oProperties.Dispensability));

							// 3D
							writer.AppendFormat("\"PC3_0\":{0},", (oProperties.PC3.Count > 0) ?
								WebUtilities.TypeConverter.DoubleToJSON(oProperties.PC3[0]) : "0");
							writer.AppendFormat("\"PC3_1\":{0},", (oProperties.PC3.Count > 1) ?
								WebUtilities.TypeConverter.DoubleToJSON(oProperties.PC3[1]) : "0");
							writer.AppendFormat("\"PC3_2\":{0},", (oProperties.PC3.Count > 2) ?
								WebUtilities.TypeConverter.DoubleToJSON(oProperties.PC3[2]) : "0");

							writer.AppendFormat("\"Representative\":{0}", oProperties.Representative);
							writer.Append("}");
						}
						writer.Append("]");
						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					case "scatterplot3d":
						#region scatterplot3d
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						if (!bAttachment)
						{
							sContentType = "text/plain";
						}
						else
						{
							sContentType = "application/octet-stream";
							Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=Revigo{0}Scatterplot3D.tsv", this.sNamespace));
						}

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);

						writer.AppendLine("TermID\tName\tValue\tLogSize\tFrequency\tUniqueness\tDispensability\tPC3_0\tPC3_1\tPC3_2\tRepresentative");

						// print the data
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							writer.AppendFormat("\"{0}\"\t", oTerm.FormattedID);
							writer.AppendFormat("\"{0}\"\t", oTerm.Name);
							writer.AppendFormat("{0}\t", oProperties.Value.ToString(CultureInfo.InvariantCulture));

							writer.AppendFormat("{0}\t",
								oProperties.LogAnnotationSize.ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t",
								(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));

							// 3D
							writer.AppendFormat("{0}\t", (oProperties.PC3.Count > 0) ?
								oProperties.PC3[0].ToString(CultureInfo.InvariantCulture) : "null");
							writer.AppendFormat("{0}\t", (oProperties.PC3.Count > 1) ?
								oProperties.PC3[1].ToString(CultureInfo.InvariantCulture) : "null");
							writer.AppendFormat("{0}\t", (oProperties.PC3.Count > 2) ?
								oProperties.PC3[2].ToString(CultureInfo.InvariantCulture) : "null");

							if (oProperties.Representative > 0)
							{
								writer.AppendFormat("{0}", oProperties.Representative);
							}
							else
							{
								writer.Append("null");
							}

							writer.AppendLine();
						}
						#endregion
						return Content(writer.ToString(), sContentType, Encoding.UTF8);

					case "jtreemap":
						#region jtree
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, this.oWorker.AllProperties, 0.10);
						int iObjectID = 0;
						int iGroupID = 0;
						int iCurrentRepresentativeID = -1;
						bFirst = true;

						writer.AppendFormat("{{\"id\":\"{0}\",\"name\":\"Group {1}\",\"children\":[", iObjectID++, iGroupID++);

						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm curTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(curTerm.ID);
							if (double.IsNaN(oProperties.Dispensability) || oProperties.Dispensability > oWorker.CutOff)
								continue;

							if (oProperties.Representative <= 0 && iCurrentRepresentativeID != curTerm.ID)
							{
								if (iCurrentRepresentativeID > 0)
								{
									writer.Append("]},");
								}
								iCurrentRepresentativeID = curTerm.ID;
								writer.AppendFormat("{{\"id\":\"{0}\",\"name\":\"{1}\",\"children\":[",
									curTerm.FormattedID.Replace(":", ""), WebUtilities.TypeConverter.StringToJSON(curTerm.Name));
								bFirst = true;
							}
							if (!bFirst)
								writer.Append(", ");
							bFirst = false;
							writer.AppendFormat("{{\"id\":\"{0}\",\"name\":\"{1}\",",
								curTerm.FormattedID.Replace(":", ""), WebUtilities.TypeConverter.StringToJSON(curTerm.Name));
							writer.AppendFormat("\"value\":{0},",
								oWorker.TermsWithValuesCount == 0 ?
								WebUtilities.TypeConverter.DoubleToJSON(oProperties.Uniqueness) :
								WebUtilities.TypeConverter.DoubleToJSON(oProperties.TransformedValue)); // was * 100.0 intially
							writer.AppendFormat("\"logSize\":{0}}}",
								WebUtilities.TypeConverter.DoubleToJSON(oProperties.LogAnnotationSize));
						}
						if (iCurrentRepresentativeID >= 0)
						{
							writer.Append("]}");
						}
						writer.Append("]}");
						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					case "treemap":
						#region tree
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						if (!bAttachment)
						{
							sContentType = "text/plain";
						}
						else
						{
							sContentType = "application/octet-stream";
							Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=Revigo{0}TreeMap.tsv", this.sNamespace));
						}

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, 0.1);

						writer.AppendLine("# WARNING - This exported Revigo data is only useful for the specific purpose of constructing a TreeMap visualization.");
						writer.AppendLine("# Do not use this table as a general list of non-redundant GO categories, as it sets an extremely permissive ");
						writer.AppendLine("# threshold to detect redundancies (c=0.10) and fill the 'representative' column, while normally c>=0.4 is recommended.");
						writer.AppendLine("# To export a reduced-redundancy set of GO terms, go to the Scatterplot or Table tab, and export from there.");

						writer.Append("TermID\tName\tFrequency\tValue\t");
						for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
						{
							writer.AppendFormat("UserValue_{0}\t", c - 1);
						}
						writer.AppendLine("Uniqueness\tDispensability\tRepresentative");

						// print the data
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm curGOTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(curGOTerm.ID);
							bool isTermEliminated = oProperties.Dispensability > oWorker.CutOff;
							if (isTermEliminated)
								continue; // will not output terms below the dispensability threshold at all

							writer.AppendFormat("\"{0}\"\t", curGOTerm.FormattedID);
							writer.AppendFormat("\"{0}\"\t", curGOTerm.Name);
							writer.AppendFormat("{0}\t", (oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));

							writer.AppendFormat("{0}\t", oProperties.Value.ToString(CultureInfo.InvariantCulture));

							for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
							{
								writer.AppendFormat("{0}\t", oProperties.UserValues[c - 1].ToString(CultureInfo.InvariantCulture));
							}

							writer.AppendFormat("{0}\t", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
							writer.AppendFormat("{0}\t", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));
							if (oProperties.Representative > 0)
							{
								writer.AppendFormat("\"{0}\"", Global.Ontology.Terms.GetValueByKey(oProperties.Representative).Name);
							}
							else
							{
								writer.Append("null");
							}
							writer.AppendLine();
						}
						#endregion
						return Content(writer.ToString(), sContentType, Encoding.UTF8);

					case "rtreemap":
						#region rtree
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						if (!bAttachment)
						{
							sContentType = "text/plain";
						}
						else
						{
							sContentType = "application/octet-stream";
							Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=Revigo{0}TreeMap.R", this.sNamespace));

						}

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, 0.1);

						writer.AppendLine("# A treemap R script produced by the Revigo server at http://revigo.irb.hr/");
						writer.AppendLine("# If you found Revigo useful in your work, please cite the following reference:");
						writer.AppendLine("# Supek F et al. \"REVIGO summarizes and visualizes long lists of Gene Ontology");
						writer.AppendLine("# terms\" PLoS ONE 2011. doi:10.1371/journal.pone.0021800");
						writer.AppendLine();
						writer.AppendLine("# author: Anton Kratz <anton.kratz@gmail.com>, RIKEN Omics Science Center, Functional Genomics Technology Team, Japan");
						writer.AppendLine("# created: Fri, Nov 02, 2012  7:25:52 PM");
						writer.AppendLine("# last change: Fri, Nov 09, 2012  3:20:01 PM");
						writer.AppendLine();
						writer.AppendLine("# -----------------------------------------------------------------------------");
						writer.AppendLine("# If you don't have the treemap package installed, uncomment the following line:");
						writer.AppendLine("# install.packages( \"treemap\" );");
						writer.AppendLine("library(treemap) 								# treemap package by Martijn Tennekes");
						writer.AppendLine();
						writer.AppendLine("# Set the working directory if necessary");
						writer.AppendLine("# setwd(\"C:/Users/username/workingdir\");");
						writer.AppendLine();
						writer.AppendLine("# --------------------------------------------------------------------------");
						writer.AppendLine("# Here is your data from Revigo. Scroll down for plot configuration options.");
						writer.AppendLine();

						writer.Append("revigo.names <- c(\"term_ID\",\"description\",\"frequency\",");
						if (oWorker.TermsWithValuesCount > 0)
						{
							sLabelOfValue = "value";
							writer.AppendFormat("\"{0}\",", sLabelOfValue);
						}
						else
						{
							sLabelOfValue = "uniqueness";
						}

						for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
						{
							writer.AppendFormat("\"UserValue_{0}\",", c - 1);
						}
						writer.AppendLine("\"uniqueness\",\"dispensability\",\"representative\");");

						// print the data
						writer.Append("revigo.data <- rbind(");
						bFirst = true;
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm curGOTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(curGOTerm.ID);
							bool isTermEliminated = oProperties.Dispensability > oWorker.CutOff;
							if (!isTermEliminated)
							{
								if (bFirst)
								{
									bFirst = false;
								}
								else
								{
									writer.AppendLine(",");
								}
								writer.Append("c(");
								writer.AppendFormat("\"{0}\",", curGOTerm.FormattedID);
								writer.AppendFormat("\"{0}\",", curGOTerm.Name);
								writer.AppendFormat("{0},",
									(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));

								if (oWorker.TermsWithValuesCount > 0)
								{
									writer.AppendFormat("{0},", Math.Abs(oProperties.Value).ToString(CultureInfo.InvariantCulture));
								}

								for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
								{
									writer.AppendFormat("{0},", oProperties.UserValues[c - 1].ToString(CultureInfo.InvariantCulture));
								}

								writer.AppendFormat("{0},", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
								writer.AppendFormat("{0},", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));
								if (oProperties.Representative > 0)
								{
									writer.AppendFormat("\"{0}\"", Global.Ontology.Terms.GetValueByKey(oProperties.Representative).Name);
								}
								else
								{
									writer.AppendFormat("\"{0}\"", Global.Ontology.Terms.GetValueByKey(curGOTerm.ID).Name);
								}
								writer.Append(")");
							}
						}
						writer.AppendLine(");");
						writer.AppendLine();

						writer.AppendLine("stuff <- data.frame(revigo.data);");
						writer.AppendLine("names(stuff) <- revigo.names;");
						writer.AppendLine();
						if (oWorker.TermsWithValuesCount > 0)
						{
							writer.AppendFormat("stuff${0} <- as.numeric( as.character(stuff${0}) );", sLabelOfValue);
							writer.AppendLine();
						}
						writer.AppendLine("stuff$frequency <- as.numeric( as.character(stuff$frequency) );");
						writer.AppendLine("stuff$uniqueness <- as.numeric( as.character(stuff$uniqueness) );");
						writer.AppendLine("stuff$dispensability <- as.numeric( as.character(stuff$dispensability) );");
						writer.AppendLine();
						writer.AppendLine("# by default, outputs to a PDF file");
						writer.AppendLine("pdf( file=\"revigo_treemap.pdf\", width=16, height=9 ) # width and height are in inches");
						writer.AppendLine();
						writer.AppendLine("# check the tmPlot command documentation for all possible parameters - there are a lot more");
						writer.AppendLine("treemap(");
						writer.AppendLine("  stuff,");
						writer.AppendLine("  index = c(\"representative\",\"description\"),");
						writer.AppendFormat("  vSize = \"{0}\",", sLabelOfValue);
						writer.AppendLine();
						writer.AppendLine("  type = \"categorical\",");
						writer.AppendLine("  vColor = \"representative\",");
						writer.AppendLine("  title = \"Revigo TreeMap\",");
						writer.AppendLine("  inflate.labels = FALSE,      # set this to TRUE for space-filling group labels - good for posters");
						writer.AppendLine("  lowerbound.cex.labels = 0,   # try to draw as many labels as possible (still, some small squares may not get a label)");
						writer.AppendLine("  bg.labels = \"#CCCCCCAA\",   # define background color of group labels");
						writer.AppendLine("								 # \"#CCCCCC00\" is fully transparent, \"#CCCCCCAA\" is semi-transparent grey, NA is opaque");
						writer.AppendLine("  position.legend = \"none\"");
						writer.AppendLine(")");
						writer.AppendLine();
						writer.AppendLine("dev.off()");
						writer.AppendLine();
						#endregion
						return Content(writer.ToString(), sContentType, Encoding.UTF8);

					case "jcytoscape":
						#region jcytoscape
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						OntoloGraph graph = oVisualizer.SimpleOntologram;
						double minSize = double.MaxValue;
						double maxSize = double.MinValue;
						double sizeMult = 0.0;
						double minWeight = double.MaxValue;
						double maxWeight = double.MinValue;
						double weightMult = 0.0;

						// determine min and max value for size
						for (int i = 0; i < graph.nodes.Count; ++i)
						{
							double dTemp = (double)graph.nodes[i].properties.GetValueByKey("LogSize");
							if (dTemp < minSize)
								minSize = dTemp;
							if (dTemp > maxSize)
								maxSize = dTemp;
						}
						sizeMult = 60.0 / (maxSize - minSize);

						// determine min and max value for weight
						for (int i = 0; i < graph.edges.Count; ++i)
						{
							double dTemp = (double)graph.edges[i].properties.GetValueByKey("similarity");
							if (dTemp < minWeight)
								minWeight = dTemp;
							if (dTemp > maxWeight)
								maxWeight = dTemp;
						}
						weightMult = 5.0 / (maxWeight - minWeight);

						writer.Append("[");
						for (int i = 0; i < graph.nodes.Count; ++i)
						{
							if (i > 0)
								writer.Append(",");

							writer.AppendFormat("{{\"data\":{{\"id\":\"GO:{0:d7}\",\"label\":\"{1}\",\"value\":{2},\"color\":\"{3}\",\"log_size\":{4}}}}}",
								graph.nodes[i].ID,
								WebUtilities.TypeConverter.StringToJSON(graph.nodes[i].properties.GetValueByKey("description").ToString().Replace("'", "")),
								WebUtilities.TypeConverter.DoubleToJSON(Math.Round((double)graph.nodes[i].properties.GetValueByKey("value"), 3)),
								WebUtilities.TypeConverter.StringToJSON(graph.nodes[i].properties.GetValueByKey("color").ToString()),
								10 + (int)Math.Floor(((double)graph.nodes[i].properties.GetValueByKey("LogSize") - minSize) * sizeMult));
						}
						for (int i = 0; i < graph.edges.Count; ++i)
						{
							writer.Append(",");
							writer.AppendFormat("{{\"data\":{{\"source\":\"GO:{0:d7}\",\"target\":\"GO:{1:d7}\",\"weight\":{2}}}}}",
								graph.edges[i].sourceID, graph.edges[i].destinationID,
								1 + (int)Math.Floor(((double)graph.edges[i].properties.GetValueByKey("similarity") - minWeight) * weightMult));
						}
						writer.Append("]");
						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					case "xgmml":
						#region xgmml
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						if (!bAttachment)
						{
							sContentType = "text/plain";
						}
						else
						{
							sContentType = "application/octet-stream";
							Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=Revigo{0}Cytoscape.xgmml", this.sNamespace));
						}
						using (MemoryStream memoryStream = new MemoryStream())
						{
							StreamWriter streamWriter1 = new StreamWriter(memoryStream);
							oVisualizer.SimpleOntologram.GraphToXGMML(streamWriter1);
							streamWriter1.Flush();
							writer.Append(Encoding.UTF8.GetString(memoryStream.ToArray()));
						}
						#endregion
						return Content(writer.ToString(), sContentType, Encoding.UTF8);

					case "simmat":
						#region simmat
						GetNamespace();
						oCheckParameterResult = CheckParameters();
						if (oCheckParameterResult != null)
							return oCheckParameterResult;

						if (!bAttachment)
						{
							sContentType = "text/plain";
						}
						else
						{
							sContentType = "application/octet-stream";
							Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=Revigo{0}SimilarityMatrix.tsv", this.sNamespace));
						}

						for (int i = 0; i < oVisualizer.Terms.Length; i++)
						{
							writer.AppendFormat("\t{0}", oVisualizer.Terms[i].FormattedID);
						}
						writer.AppendLine();
						for (int i = 0; i < oVisualizer.Terms.Length; i++)
						{
							writer.Append(oVisualizer.Terms[i].FormattedID);
							for (int j = 0; j < oVisualizer.Terms.Length; j++)
							{
								writer.AppendFormat("\t{0}", oVisualizer.Matrix.Matrix[i, j].ToString(CultureInfo.InvariantCulture));
							}
							writer.AppendLine();
						}
						#endregion
						return Content(writer.ToString(), sContentType, Encoding.UTF8);

					case "jclouds":
						#region jclouds
						if (oWorker.IsRunning || !oWorker.IsFinished)
						{
							return ReturnError("The Job did not yet complete");
						}

						if (oWorker.HasUserErrors || oWorker.HasDeveloperErrors)
						{
							return ReturnError("The Job has an errors, no data available.");
						}

						writer.Append("{");
						if (oWorker.Enrichments != null)
						{
							writer.Append("\"Enrichments\":[");

							double MIN_UNIT_SIZE = 1;
							double MAX_UNIT_SIZE = 9;
							double RANGE_UNIT_SIZE = MAX_UNIT_SIZE - MIN_UNIT_SIZE;
							double minFreq = 999999.0;
							double maxFreq = 0.0;

							for (int i = 0; i < oWorker.Enrichments.Count; i++)
							{
								double dFrequency = Math.Sqrt(oWorker.Enrichments[i].Value);
								if (dFrequency > 0.0)
								{
									minFreq = Math.Min(minFreq, dFrequency);
									maxFreq = Math.Max(maxFreq, dFrequency);
								}
							}

							if (minFreq > maxFreq)
							{
								double dTemp = minFreq;
								minFreq = maxFreq;
								maxFreq = dTemp;
							}
							if (minFreq == maxFreq)
							{
								maxFreq++;
							}
							double range = maxFreq - minFreq;
							bFirst = true;

							for (int i = 0; i < oWorker.Enrichments.Count; i++)
							{
								string sWord = oWorker.Enrichments[i].Key.Replace("'", "");
								double dFrequency = Math.Sqrt(oWorker.Enrichments[i].Value);

								if (dFrequency > 0.0)
								{
									if (!bFirst)
										writer.Append(",");

									int size = (int)Math.Ceiling(MIN_UNIT_SIZE + Math.Round(((dFrequency - minFreq) * RANGE_UNIT_SIZE) / range));
									writer.AppendFormat("{{\"Word\":\"{0}\",\"Size\":{1}}}", WebUtilities.TypeConverter.StringToJSON(sWord), size);
									bFirst = false;
								}
							}
							writer.Append("]");
						}

						if (oWorker.Correlations != null)
						{
							if (oWorker.Enrichments != null)
								writer.Append(",");

							writer.Append("\"Correlations\":[");

							double MIN_UNIT_SIZE = 1;
							double MAX_UNIT_SIZE = 9;
							double RANGE_UNIT_SIZE = MAX_UNIT_SIZE - MIN_UNIT_SIZE;
							double minFreq = 999999.0;
							double maxFreq = 0.0;
							for (int i = 0; i < oWorker.Correlations.Count; i++)
							{
								double dFrequency = oWorker.Correlations[i].Value;
								if (dFrequency > 0.0)
								{
									minFreq = Math.Min(minFreq, dFrequency);
									maxFreq = Math.Max(maxFreq, dFrequency);
								}
							}

							if (minFreq > maxFreq)
							{
								double dTemp = minFreq;
								minFreq = maxFreq;
								maxFreq = dTemp;
							}
							if (minFreq == maxFreq)
							{
								maxFreq++;
							}
							double range = maxFreq - minFreq;
							bFirst = true;

							for (int i = 0; i < oWorker.Correlations.Count; i++)
							{
								string sWord = oWorker.Correlations[i].Key.Replace("'", "");
								double dFrequency = oWorker.Correlations[i].Value;

								if (dFrequency > 0.0)
								{
									if (!bFirst)
										writer.Append(",");

									int size = (int)Math.Ceiling(MIN_UNIT_SIZE + Math.Round(((dFrequency - minFreq) * RANGE_UNIT_SIZE) / range));
									writer.AppendFormat("{{\"Word\":\"{0}\",\"Size\":{1}}}", WebUtilities.TypeConverter.StringToJSON(sWord), size);
									bFirst = false;
								}
							}
							writer.Append("]");
						}
						writer.Append("}");

						#endregion
						return Content(writer.ToString(), "application/json", Encoding.UTF8);

					default:
						return ReturnError("Unknown query type.");
				}
			}
			catch (ThreadAbortException) { throw; /* propagate */ }
			catch (Exception ex)
			{
				WebUtilities.Email.SendEmailNotification(oWorker, ex, sType);
				return ReturnError("Undefined error occured while querying the job.");
			}
		}

		private void GetNamespace()
		{
			string sNamespace = WebUtilities.TypeConverter.ToString(Request.Query["namespace"]);
			int iNamespace = -1;

			int.TryParse(sNamespace, out iNamespace);

			foreach (int value in Enum.GetValues(typeof(GONamespaceEnum)))
			{
				if (value == iNamespace)
				{
					this.eNamespace = (GONamespaceEnum)value;
					break;
				}
			}

			if (this.oWorker != null)
			{
				switch (eNamespace)
				{
					case GONamespaceEnum.BIOLOGICAL_PROCESS:
						this.oVisualizer = oWorker.BPVisualizer;
						this.sNamespace = "_BP_";
						break;
					case GONamespaceEnum.CELLULAR_COMPONENT:
						this.oVisualizer = oWorker.CCVisualizer;
						this.sNamespace = "_CC_";
						break;
					case GONamespaceEnum.MOLECULAR_FUNCTION:
						this.oVisualizer = oWorker.MFVisualizer;
						this.sNamespace = "_MF_";
						break;
					default:
						break;
				}
			}
		}

		private ContentResult CheckParameters()
		{
			if (oWorker.IsRunning || !oWorker.IsFinished)
			{
				return ReturnError("The Job did not yet complete");
			}

			if (oWorker.HasUserErrors || oWorker.HasDeveloperErrors)
			{
				return ReturnError("The Job has an errors, no data available.");
			}

			if (eNamespace == GONamespaceEnum.None || oVisualizer == null || oVisualizer.Terms.Length == 0)
			{
				return ReturnError(string.Format("The namespace {0} has no results.", GeneOntology.NamespaceToFriendlyString(eNamespace)));
			}

			return null;
		}

		private ContentResult ReturnError(string message)
		{
			StringBuilder writer = new StringBuilder();
			string sContentType = "application/json";

			if (this.bJSON)
			{
				writer.Append("{");
				if (message.StartsWith("["))
					writer.AppendFormat("\"error\":{0}", message);
				else
					writer.AppendFormat("\"error\":[\"{0}\"]", WebUtilities.TypeConverter.StringToJSON(message));
				writer.Append("}");
			}
			else
			{
				sContentType = "text/plain";
				writer.AppendFormat("error: {0}", message);
				writer.AppendLine();
			}

			return Content(writer.ToString(), sContentType, Encoding.UTF8);
		}
	}
}
