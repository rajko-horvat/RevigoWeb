using System;
using System.Globalization;
using System.IO;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using IRB.Revigo.Core;
using System.Configuration;
using System.Text;
using System.Net.Mail;
using System.Threading;

namespace RevigoWeb
{
	public partial class QueryJob : System.Web.UI.Page
	{
		private int iJobID = -1;
		private RevigoWorker oWorker = null;
		private GONamespaceEnum eNamespace = GONamespaceEnum.None;
		private string sNamespace = "_";
		private TermListVisualizer oVisualizer = null;

		private bool bAttachment = false;
		private bool bJSON = true;  // true by default

		protected void Page_Load(object sender, EventArgs e)
		{
			string sType = null;

			try
			{
				sType = Convert.ToString(Request.Params["type"]);
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

				string sJobID = Convert.ToString(Request.Params["JobID"]);

				if (!string.IsNullOrEmpty(sJobID) && int.TryParse(sJobID, out iJobID) &&
					Global.Jobs != null && Global.Jobs.ContainsKey(iJobID))
				{
					RevigoJob job = Global.Jobs.GetValueByKey(iJobID);
					oWorker = job.Worker;
					job.ExtendExpiration(bJSON ? Global.SessionTimeout : Global.JobTimeout);
				}

				// this is a deal breaker, we have to positively indentify a job
				if (oWorker == null)
				{
					ShowError("Unknown job");
					return;
				}

				StreamWriter writer;
				GOTermList terms;
				bool bFirst;
				string sLabelOfValue;

				Global.UpdateJobUsageStats(oWorker, sType, Convert.ToString(Request.Params["namespace"]));

				switch (sType)
				{
					case "jstatus":
						#region jstatus
						Response.Clear();
						Response.ClearHeaders();
						Response.ContentType = "application/json";
						writer = new StreamWriter(Response.OutputStream);
						writer.Write("{{\"running\":{0},\"progress\":{1},\"message\":\"{2}\"}}",
							oWorker.IsRunning || !oWorker.IsFinished ? 1 : 0,
							oWorker.Progress.ToString("##0.00", CultureInfo.InvariantCulture),
							string.IsNullOrEmpty(oWorker.ProgressText) ? "" : Global.StringToJSON(oWorker.ProgressText));
						writer.Flush();
						Response.End();
						#endregion
						break;
					case "jinfo":
						#region jinfo
						if (oWorker.IsRunning || !oWorker.IsFinished)
						{
							ShowError("The Job did not yet complete");
							break;
						}

						Response.Clear();
						Response.ClearHeaders();
						Response.ContentType = "application/json";
						writer = new StreamWriter(Response.OutputStream);
						writer.Write("{{\"warnings\":{0},\"errors\":{1},\"exectime\":{2}," +
							"\"HasBP\":{3},\"BPCount\":{4}," +
							"\"HasCC\":{5},\"CCCount\":{6}," +
							"\"HasMF\":{7},\"MFCount\":{8}," +
							"\"HasClouds\":{9}}}",
							oWorker.HasUserWarnings ? Global.StringArrayToJSON(oWorker.UserWarnings) : "[]",
							oWorker.HasUserErrors ? Global.StringArrayToJSON(oWorker.UserErrors) : "[]",
							(long)(oWorker.ExecutingTime.TotalSeconds),
							oWorker.HasBPVisualizer ? 1 : 0, oWorker.HasBPVisualizer ? oWorker.BPVisualizer.Terms.Length : 0,
							oWorker.HasCCVisualizer ? 1 : 0, oWorker.HasCCVisualizer ? oWorker.CCVisualizer.Terms.Length : 0,
							oWorker.HasMFVisualizer ? 1 : 0, oWorker.HasMFVisualizer ? oWorker.MFVisualizer.Terms.Length : 0,
							oWorker.HasClouds ? 1 : 0);
						writer.Flush();
						Response.End();
						#endregion
						break;
					case "jpinterm":
						#region Pin Term
						GetNamespace();
						if (CheckParameters())
							break;

						int iTermID = -1;
						string sTemID = Convert.ToString(Request.Params["TermID"]);

						if (!string.IsNullOrEmpty(sTemID) && int.TryParse(sTemID, out iTermID) &&
							this.oWorker.ContainsTermID(iTermID))
						{
							this.oWorker.PinTerm(iTermID);
							Response.Clear();
							Response.ClearHeaders();
							Response.ContentType = "application/json";
							writer = new StreamWriter(Response.OutputStream);

							writer.Write("{\"Success\":1}");

							writer.Flush();
							Response.End();
						}
						else
						{
							ShowError("Unknown TermID");
						}
						#endregion
						break;
					case "jtable":
						#region jtable
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						Response.ContentType = "application/json";
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, this.oWorker.AllProperties, this.oWorker.CutOff);

						writer.Write("{\"Columns\":[");

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"ID", "",
							"Number", -1, 0, 0, 0, 0);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Term ID", "This is the GO term ID",
							"Term", 0, 1, 1, 1, 1);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"CellTitle\":\"{2}\",\"Type\":\"{3}\",\"Decimals\":{4},\"Sortable\":{5},\"Filter\":{6},\"Condensed\":{7},\"Visible\":{8}}},",
							"Name", "Name of the GO term as defined in Gene ontology (hoover mouse over GO term name for description)", "Description",
							"String", 0, 1, 1, 0, 1);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Description", "",
							"String", 0, 0, 0, 0, 0);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Pin Term", "Use this to ensure that the term gets chosen as the cluster representative",
							"PinTerm", 0, 0, 0, 1, 1);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Value", "This is the value (if any) you provided alongside the GO term ID in your input data set",
							"Number", 4, 1, 1, 1, 1);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"LogSize", "The Log10(Number of annotations for GO Term ID in selected species in the EBI GOA database)",
							"Number", 3, 1, 1, 1, 1);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Frequency", "The proportion of this GO term in the underlying protein annotation database (by default, whole UniProt, but the user can select a single-species subset). Higher frequency implies more general terms, lower - more specific ones",
							"Percentage", 3, 1, 1, 1, 1);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Uniqueness", "Measures whether the term is an outlier when compared semantically to the whole list. Calculated as 1-(average semantic similarity of a term to all other terms). More unique terms tend to be less dispensable",
							"Number", 3, 1, 1, 1, 1);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}},",
							"Dispensability", "The semantic similarity threshold at which the term was removed from the list and assigned to a cluster. Cluster representatives always have dispensability less than the user-specified 'allowed similarity' cutoff",
							"Number", 3, 1, 1, 1, 1);

						writer.Write("{{\"Name\":\"{0}\",\"Title\":\"{1}\",\"Type\":\"{2}\",\"Decimals\":{3},\"Sortable\":{4},\"Filter\":{5},\"Condensed\":{6},\"Visible\":{7}}}",
							"Representative", "",
							"Number", -1, 0, 0, 0, 0);

						writer.Write("],\"Rows\":[");

						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							if (i > 0)
								writer.Write(",");

							writer.Write("{");
							writer.Write("\"ID\":{0},", oTerm.ID);
							writer.Write("\"Term ID\":\"{0}\",", Global.StringToJSON(oTerm.FormattedID));
							writer.Write("\"Name\":\"{0}\",", Global.StringToJSON(oTerm.Name));
							writer.Write("\"Description\":\"{0}\",", Global.StringToJSON(oTerm.Description));
							writer.Write("\"Pin Term\":{0},", oProperties.Pinned ? 0 : (oProperties.Representative <= 0 ? -1 : oTerm.ID));
							writer.Write("\"Value\":{0},", Global.DoubleToJSON(oProperties.Value));
							writer.Write("\"LogSize\":{0},", Global.DoubleToJSON(oProperties.LogAnnotationSize));
							writer.Write("\"Frequency\":{0},", Global.DoubleToJSON(oProperties.AnnotationFrequency * 100.0));
							writer.Write("\"Uniqueness\":{0},", Global.DoubleToJSON(oProperties.Uniqueness));
							writer.Write("\"Dispensability\":{0},", Global.DoubleToJSON(oProperties.Dispensability));
							writer.Write("\"Representative\":{0}", oProperties.Representative);
							writer.Write("}");
						}
						writer.Write("]}");

						writer.Flush();
						Response.End();
						#endregion
						break;
					case "table":
						#region table
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						if (!bAttachment)
						{
							Response.ContentType = "text/plain";
						}
						else
						{
							Response.ContentType = "application/octet-stream";
							Response.AddHeader("Content-Disposition", string.Format("attachment; filename=Revigo{0}Table.tsv", this.sNamespace));
						}
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);

						writer.Write("TermID\tName\tValue\t");
						for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
						{
							writer.Write("UserValue_{0}\t", c - 1);
						}
						writer.WriteLine("LogSize\tFrequency\tUniqueness\tDispensability\tRepresentative");

						// print the data
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							writer.Write("\"{0}\"\t", oTerm.FormattedID);
							writer.Write("\"{0}\"\t", oTerm.Name);
							writer.Write("{0}\t", oProperties.Value.ToString(CultureInfo.InvariantCulture));

							for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
							{
								writer.Write("{0}\t", oProperties.UserValues[c - 1].ToString(CultureInfo.InvariantCulture));
							}

							writer.Write("{0}\t",
								oProperties.LogAnnotationSize.ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t",
								(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));

							if (oProperties.Representative > 0)
							{
								writer.Write("{0}", oProperties.Representative);
							}
							else
							{
								writer.Write("null");
							}

							writer.WriteLine();
						}
						writer.Flush();
						Response.End();
						#endregion
						break;
					case "jscatterplot":
						#region jscatterplot
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						Response.ContentType = "application/json";
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, this.oWorker.AllProperties, this.oWorker.CutOff);

						writer.Write("[");

						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							if (i > 0)
								writer.Write(",");

							writer.Write("{");
							writer.Write("\"ID\":{0},", oTerm.ID);
							writer.Write("\"Term ID\":\"{0}\",", Global.StringToJSON(oTerm.FormattedID));
							writer.Write("\"Name\":\"{0}\",", Global.StringToJSON(oTerm.Name));
							writer.Write("\"Description\":\"{0}\",", Global.StringToJSON(oTerm.Description));
							writer.Write("\"Pin Term\":{0},", oProperties.Pinned ? 0 : (oProperties.Representative <= 0 ? -1 : oTerm.ID));
							writer.Write("\"Value\":{0},", Global.DoubleToJSON(oProperties.Value));
							writer.Write("\"LogSize\":{0},", Global.DoubleToJSON(oProperties.LogAnnotationSize));
							writer.Write("\"Frequency\":{0},", Global.DoubleToJSON(oProperties.AnnotationFrequency * 100.0));
							writer.Write("\"Uniqueness\":{0},", Global.DoubleToJSON(oProperties.Uniqueness));
							writer.Write("\"Dispensability\":{0},", Global.DoubleToJSON(oProperties.Dispensability));

							// 2D
							writer.Write("\"PC_0\":{0},", (oProperties.PC.Count > 0) ?
								Global.DoubleToJSON(oProperties.PC[0]) : "0");
							writer.Write("\"PC_1\":{0},", (oProperties.PC.Count > 1) ?
								Global.DoubleToJSON(oProperties.PC[1]) : "0");
							writer.Write("\"Selected\":{0},", (oProperties.Dispensability <= 0.05) ? 1 : 0);

							writer.Write("\"Representative\":{0}", oProperties.Representative);
							writer.Write("}");
						}
						writer.Write("]");

						writer.Flush();
						Response.End();
						#endregion
						break;
					case "scatterplot":
						#region scatterplot
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						if (!bAttachment)
						{
							Response.ContentType = "text/plain";
						}
						else
						{
							Response.ContentType = "application/octet-stream";
							Response.AddHeader("Content-Disposition", string.Format("attachment; filename=Revigo{0}Scatterplot.tsv", this.sNamespace));
						}
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);

						writer.WriteLine("TermID\tName\tValue\tLogSize\tFrequency\tUniqueness\tDispensability\tPC_0\tPC_1\tRepresentative");

						// print the data
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							writer.Write("\"{0}\"\t", oTerm.FormattedID);
							writer.Write("\"{0}\"\t", oTerm.Name);
							writer.Write("{0}\t", oProperties.Value.ToString(CultureInfo.InvariantCulture));

							writer.Write("{0}\t",
								oProperties.LogAnnotationSize.ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t",
								(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));

							// 2D
							writer.Write("{0}\t", (oProperties.PC.Count > 0) ?
								oProperties.PC[0].ToString(CultureInfo.InvariantCulture) : "null");
							writer.Write("{0}\t", (oProperties.PC.Count > 1) ?
								oProperties.PC[1].ToString(CultureInfo.InvariantCulture) : "null");

							if (oProperties.Representative > 0)
							{
								writer.Write("{0}", oProperties.Representative);
							}
							else
							{
								writer.Write("null");
							}

							writer.WriteLine();
						}
						writer.Flush();
						Response.End();
						#endregion
						break;
					case "rscatterplot":
						#region rscatterplot
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						if (!bAttachment)
						{
							Response.ContentType = "text/plain";
						}
						else
						{
							Response.ContentType = "application/octet-stream";
							Response.AddHeader("Content-Disposition", string.Format("attachment; filename=Revigo{0}Scatterplot.R", this.sNamespace));
						}
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);

						writer.WriteLine("# A plotting R script produced by the Revigo server at http://revigo.irb.hr/");
						writer.WriteLine("# If you found Revigo useful in your work, please cite the following reference:");
						writer.WriteLine("# Supek F et al. \"REVIGO summarizes and visualizes long lists of Gene Ontology");
						writer.WriteLine("# terms\" PLoS ONE 2011. doi:10.1371/journal.pone.0021800");
						writer.WriteLine();
						writer.WriteLine("# --------------------------------------------------------------------------");
						writer.WriteLine("# If you don't have the ggplot2 package installed, uncomment the following line:");
						writer.WriteLine("# install.packages( \"ggplot2\" );");
						writer.WriteLine("library( ggplot2 );");
						writer.WriteLine();
						writer.WriteLine("# --------------------------------------------------------------------------");
						writer.WriteLine("# If you don't have the scales package installed, uncomment the following line:");
						writer.WriteLine("# install.packages( \"scales\" );");
						writer.WriteLine("library( scales );");
						writer.WriteLine();
						writer.WriteLine("# --------------------------------------------------------------------------");
						writer.WriteLine("# Here is your data from Revigo. Scroll down for plot configuration options.");
						writer.WriteLine();

						writer.Write("revigo.names <- c(\"term_ID\",\"description\",\"frequency\",\"plot_X\",\"plot_Y\",\"log_size\",");
						if (oWorker.TermsWithValuesCount > 0)
						{
							sLabelOfValue = "value";
							writer.Write("\"{0}\",", sLabelOfValue);
						}
						else
						{
							sLabelOfValue = "uniqueness";
						}

						for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
						{
							writer.Write("\"UserValue_{0}\",", c - 1);
						}
						writer.WriteLine("\"uniqueness\",\"dispensability\");");

						// print the data
						writer.Write("revigo.data <- rbind(");
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
									writer.WriteLine(",");
								}
								writer.Write("c(");
								writer.Write("\"{0}\",", curGOTerm.FormattedID);
								writer.Write("\"{0}\",", curGOTerm.Name);
								writer.Write("{0},",
									(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));

								writer.Write("{0},", (oProperties.PC.Count > 0) ?
									oProperties.PC[0].ToString(CultureInfo.InvariantCulture) : "null");
								writer.Write("{0},", (oProperties.PC.Count > 1) ?
									oProperties.PC[1].ToString(CultureInfo.InvariantCulture) : "null");

								writer.Write("{0},",
									oProperties.LogAnnotationSize.ToString(CultureInfo.InvariantCulture));

								if (oWorker.TermsWithValuesCount > 0)
								{
									writer.Write("{0},", oProperties.Value.ToString(CultureInfo.InvariantCulture));
								}

								for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
								{
									writer.Write("{0},", oProperties.UserValues[c - 1].ToString(CultureInfo.InvariantCulture));
								}

								writer.Write("{0},", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
								writer.Write("{0}", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));
								writer.Write(")");
							}
						}
						writer.WriteLine(");");
						writer.WriteLine();
						writer.WriteLine("one.data <- data.frame(revigo.data);");
						writer.WriteLine("names(one.data) <- revigo.names;");
						writer.WriteLine("one.data <- one.data [(one.data$plot_X != \"null\" & one.data$plot_Y != \"null\"), ];");
						writer.WriteLine("one.data$plot_X <- as.numeric( as.character(one.data$plot_X) );");
						writer.WriteLine("one.data$plot_Y <- as.numeric( as.character(one.data$plot_Y) );");
						writer.WriteLine("one.data$log_size <- as.numeric( as.character(one.data$log_size) );");
						if (oWorker.TermsWithValuesCount > 0)
							writer.WriteLine("one.data${0} <- as.numeric( as.character(one.data${0}) );", sLabelOfValue);
						writer.WriteLine("one.data$frequency <- as.numeric( as.character(one.data$frequency) );");
						writer.WriteLine("one.data$uniqueness <- as.numeric( as.character(one.data$uniqueness) );");
						writer.WriteLine("one.data$dispensability <- as.numeric( as.character(one.data$dispensability) );");
						writer.WriteLine("#head(one.data);");
						writer.WriteLine();
						writer.WriteLine();
						writer.WriteLine("# --------------------------------------------------------------------------");
						writer.WriteLine("# Names of the axes, sizes of the numbers and letters, names of the columns,");
						writer.WriteLine("# etc. can be changed below");
						writer.WriteLine();
						writer.WriteLine("p1 <- ggplot( data = one.data );");
						writer.WriteLine("p1 <- p1 + geom_point( aes( plot_X, plot_Y, colour = {0}, size = log_size), alpha = I(0.6) );", sLabelOfValue);
						writer.WriteLine("p1 <- p1 + scale_colour_gradientn( colours = c(\"blue\", \"green\", \"yellow\", \"red\"), limits = c( min(one.data${0}), 0) );", sLabelOfValue);
						writer.WriteLine("p1 <- p1 + geom_point( aes(plot_X, plot_Y, size = log_size), shape = 21, fill = \"transparent\", colour = I (alpha (\"black\", 0.6) ));");
						writer.WriteLine("p1 <- p1 + scale_size( range=c(5, 30)) + theme_bw(); # + scale_fill_gradientn(colours = heat_hcl(7), limits = c(-300, 0) );");
						writer.WriteLine("ex <- one.data [ one.data$dispensability < 0.15, ];");
						writer.WriteLine("p1 <- p1 + geom_text( data = ex, aes(plot_X, plot_Y, label = description), colour = I(alpha(\"black\", 0.85)), size = 3 );");
						writer.WriteLine("p1 <- p1 + labs (y = \"semantic space x\", x = \"semantic space y\");");
						writer.WriteLine("p1 <- p1 + theme(legend.key = element_blank()) ;");
						writer.WriteLine("one.x_range = max(one.data$plot_X) - min(one.data$plot_X);");
						writer.WriteLine("one.y_range = max(one.data$plot_Y) - min(one.data$plot_Y);");
						writer.WriteLine("p1 <- p1 + xlim(min(one.data$plot_X)-one.x_range/10,max(one.data$plot_X)+one.x_range/10);");
						writer.WriteLine("p1 <- p1 + ylim(min(one.data$plot_Y)-one.y_range/10,max(one.data$plot_Y)+one.y_range/10);");
						writer.WriteLine();
						writer.WriteLine();
						writer.WriteLine("# --------------------------------------------------------------------------");
						writer.WriteLine("# Output the plot to screen");
						writer.WriteLine();
						writer.WriteLine("p1;");
						writer.WriteLine();
						writer.WriteLine("# Uncomment the line below to also save the plot to a file.");
						writer.WriteLine("# The file type depends on the extension (default=pdf).");
						writer.WriteLine();
						writer.WriteLine("# ggsave(\"/path_to_your_file/revigo-plot.pdf\");");
						writer.WriteLine();

						writer.Flush();
						Response.End();
						#endregion
						break;
					case "jscatterplot3d":
						#region jscatterplot3d
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						Response.ContentType = "application/json";
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, this.oWorker.AllProperties, this.oWorker.CutOff);

						writer.Write("[");

						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							if (i > 0)
								writer.Write(",");

							writer.Write("{");
							writer.Write("\"ID\":{0},", oTerm.ID);
							writer.Write("\"Term ID\":\"{0}\",", Global.StringToJSON(oTerm.FormattedID));
							writer.Write("\"Name\":\"{0}\",", Global.StringToJSON(oTerm.Name));
							writer.Write("\"Description\":\"{0}\",", Global.StringToJSON(oTerm.Description));
							writer.Write("\"Pin Term\":{0},", oProperties.Pinned ? 0 : (oProperties.Representative <= 0 ? -1 : oTerm.ID));
							writer.Write("\"Value\":{0},", Global.DoubleToJSON(oProperties.Value));
							writer.Write("\"LogSize\":{0},", Global.DoubleToJSON(oProperties.LogAnnotationSize));
							writer.Write("\"Frequency\":{0},", Global.DoubleToJSON(oProperties.AnnotationFrequency * 100.0));
							writer.Write("\"Uniqueness\":{0},", Global.DoubleToJSON(oProperties.Uniqueness));
							writer.Write("\"Dispensability\":{0},", Global.DoubleToJSON(oProperties.Dispensability));

							// 3D
							writer.Write("\"PC3_0\":{0},", (oProperties.PC3.Count > 0) ?
								Global.DoubleToJSON(oProperties.PC3[0]) : "0");
							writer.Write("\"PC3_1\":{0},", (oProperties.PC3.Count > 1) ?
								Global.DoubleToJSON(oProperties.PC3[1]) : "0");
							writer.Write("\"PC3_2\":{0},", (oProperties.PC3.Count > 2) ?
								Global.DoubleToJSON(oProperties.PC3[2]) : "0");

							writer.Write("\"Representative\":{0}", oProperties.Representative);
							writer.Write("}");
						}
						writer.Write("]");

						writer.Flush();
						Response.End();
						#endregion
						break;
					case "scatterplot3d":
						#region scatterplot3d
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						if (!bAttachment)
						{
							Response.ContentType = "text/plain";
						}
						else
						{
							Response.ContentType = "application/octet-stream";
							Response.AddHeader("Content-Disposition", string.Format("attachment; filename=Revigo{0}Scatterplot3D.tsv", this.sNamespace));
						}
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, oWorker.CutOff);

						writer.WriteLine("TermID\tName\tValue\tLogSize\tFrequency\tUniqueness\tDispensability\tPC3_0\tPC3_1\tPC3_2\tRepresentative");

						// print the data
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm oTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(oTerm.ID);

							writer.Write("\"{0}\"\t", oTerm.FormattedID);
							writer.Write("\"{0}\"\t", oTerm.Name);
							writer.Write("{0}\t", oProperties.Value.ToString(CultureInfo.InvariantCulture));

							writer.Write("{0}\t",
								oProperties.LogAnnotationSize.ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t",
								(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));

							// 3D
							writer.Write("{0}\t", (oProperties.PC3.Count > 0) ?
								oProperties.PC3[0].ToString(CultureInfo.InvariantCulture) : "null");
							writer.Write("{0}\t", (oProperties.PC3.Count > 1) ?
								oProperties.PC3[1].ToString(CultureInfo.InvariantCulture) : "null");
							writer.Write("{0}\t", (oProperties.PC3.Count > 2) ?
								oProperties.PC3[2].ToString(CultureInfo.InvariantCulture) : "null");

							if (oProperties.Representative > 0)
							{
								writer.Write("{0}", oProperties.Representative);
							}
							else
							{
								writer.Write("null");
							}

							writer.WriteLine();
						}
						writer.Flush();
						Response.End();
						#endregion
						break;
					case "jtreemap":
						#region jtree
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						Response.ContentType = "application/json";
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, this.oWorker.AllProperties, 0.10);
						int iObjectID = 0;
						int iGroupID = 0;
						int iCurrentRepresentativeID = -1;
						bFirst = true;

						writer.Write("{{\"id\":\"{0}\",\"name\":\"Group {1}\",\"children\":[", iObjectID++, iGroupID++);

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
									writer.Write("]},");
								}
								iCurrentRepresentativeID = curTerm.ID;
								writer.Write("{{\"id\":\"{0}\",\"name\":\"{1}\",\"children\":[",
									curTerm.FormattedID.Replace(":", ""), Global.StringToJSON(curTerm.Name));
								bFirst = true;
							}
							if (!bFirst)
								writer.Write(", ");
							bFirst = false;
							writer.Write("{{\"id\":\"{0}\",\"name\":\"{1}\",",
								curTerm.FormattedID.Replace(":", ""), Global.StringToJSON(curTerm.Name));
							writer.Write("\"value\":{0},",
								oWorker.TermsWithValuesCount == 0 ?
								Global.DoubleToJSON(oProperties.Uniqueness) :
								Global.DoubleToJSON(oProperties.TransformedValue)); // was * 100.0 intially
							writer.Write("\"logSize\":{0}}}",
								Global.DoubleToJSON(oProperties.LogAnnotationSize));
						}
						if (iCurrentRepresentativeID >= 0)
						{
							writer.Write("]}");
						}
						writer.Write("]}");

						writer.Flush();
						Response.End();
						#endregion
						break;
					case "treemap":
						#region tree
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						if (!bAttachment)
						{
							Response.ContentType = "text/plain";
						}
						else
						{
							Response.ContentType = "application/octet-stream";
							Response.AddHeader("Content-Disposition", string.Format("attachment; filename=Revigo{0}TreeMap.tsv", this.sNamespace));
						}
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, 0.1);

						writer.WriteLine("# WARNING - This exported Revigo data is only useful for the specific purpose of constructing a TreeMap visualization.");
						writer.WriteLine("# Do not use this table as a general list of non-redundant GO categories, as it sets an extremely permissive ");
						writer.WriteLine("# threshold to detect redundancies (c=0.10) and fill the 'representative' column, while normally c>=0.4 is recommended.");
						writer.WriteLine("# To export a reduced-redundancy set of GO terms, go to the Scatterplot or Table tab, and export from there.");

						writer.Write("TermID\tName\tFrequency\tValue\t");
						for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
						{
							writer.Write("UserValue_{0}\t", c - 1);
						}
						writer.WriteLine("Uniqueness\tDispensability\tRepresentative");

						// print the data
						for (int i = 0; i < terms.Count; i++)
						{
							GOTerm curGOTerm = terms[i];
							GOTermProperties oProperties = oWorker.AllProperties.GetValueByKey(curGOTerm.ID);
							bool isTermEliminated = oProperties.Dispensability > oWorker.CutOff;
							if (isTermEliminated)
								continue; // will not output terms below the dispensability threshold at all

							writer.Write("\"{0}\"\t", curGOTerm.FormattedID);
							writer.Write("\"{0}\"\t", curGOTerm.Name);
							writer.Write("{0}\t", (oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));

							writer.Write("{0}\t", oProperties.Value.ToString(CultureInfo.InvariantCulture));

							for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
							{
								writer.Write("{0}\t", oProperties.UserValues[c - 1].ToString(CultureInfo.InvariantCulture));
							}

							writer.Write("{0}\t", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
							writer.Write("{0}\t", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));
							if (oProperties.Representative > 0)
							{
								writer.Write("\"{0}\"", Global.Ontology.GetValueByKey(oProperties.Representative).Name);
							}
							else
							{
								writer.Write("null");
							}
							writer.WriteLine();
						}
						writer.Flush();
						Response.End();
						#endregion
						break;
					case "rtreemap":
						#region rtree
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						if (!bAttachment)
						{
							Response.ContentType = "text/plain";
						}
						else
						{
							Response.ContentType = "application/octet-stream";
							Response.AddHeader("Content-Disposition", string.Format("attachment; filename=Revigo{0}TreeMap.R", this.sNamespace));

						}
						writer = new StreamWriter(Response.OutputStream);

						terms = new GOTermList(oVisualizer.Terms);
						terms.FindClustersAndSortByThem(Global.Ontology, oWorker.AllProperties, 0.1);

						writer.WriteLine("# A treemap R script produced by the Revigo server at http://revigo.irb.hr/");
						writer.WriteLine("# If you found Revigo useful in your work, please cite the following reference:");
						writer.WriteLine("# Supek F et al. \"REVIGO summarizes and visualizes long lists of Gene Ontology");
						writer.WriteLine("# terms\" PLoS ONE 2011. doi:10.1371/journal.pone.0021800");
						writer.WriteLine();
						writer.WriteLine("# author: Anton Kratz <anton.kratz@gmail.com>, RIKEN Omics Science Center, Functional Genomics Technology Team, Japan");
						writer.WriteLine("# created: Fri, Nov 02, 2012  7:25:52 PM");
						writer.WriteLine("# last change: Fri, Nov 09, 2012  3:20:01 PM");
						writer.WriteLine();
						writer.WriteLine("# -----------------------------------------------------------------------------");
						writer.WriteLine("# If you don't have the treemap package installed, uncomment the following line:");
						writer.WriteLine("# install.packages( \"treemap\" );");
						writer.WriteLine("library(treemap) 								# treemap package by Martijn Tennekes");
						writer.WriteLine();
						writer.WriteLine("# Set the working directory if necessary");
						writer.WriteLine("# setwd(\"C:/Users/username/workingdir\");");
						writer.WriteLine();
						writer.WriteLine("# --------------------------------------------------------------------------");
						writer.WriteLine("# Here is your data from Revigo. Scroll down for plot configuration options.");
						writer.WriteLine();

						writer.Write("revigo.names <- c(\"term_ID\",\"description\",\"frequency\",");
						if (oWorker.TermsWithValuesCount > 0)
						{
							sLabelOfValue = "value";
							writer.Write("\"{0}\",", sLabelOfValue);
						}
						else
						{
							sLabelOfValue = "uniqueness";
						}

						for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
						{
							writer.Write("\"UserValue_{0}\",", c - 1);
						}
						writer.WriteLine("\"uniqueness\",\"dispensability\",\"representative\");");

						// print the data
						writer.Write("revigo.data <- rbind(");
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
									writer.WriteLine(",");
								}
								writer.Write("c(");
								writer.Write("\"{0}\",", curGOTerm.FormattedID);
								writer.Write("\"{0}\",", curGOTerm.Name);
								writer.Write("{0},",
									(oProperties.AnnotationFrequency * 100.0).ToString(CultureInfo.InvariantCulture));

								if (oWorker.TermsWithValuesCount > 0)
								{
									writer.Write("{0},", Math.Abs(oProperties.Value).ToString(CultureInfo.InvariantCulture));
								}

								for (int c = 1; c < oWorker.MinNumColsPerGoTerm; c++)
								{
									writer.Write("{0},", oProperties.UserValues[c - 1].ToString(CultureInfo.InvariantCulture));
								}

								writer.Write("{0},", oProperties.Uniqueness.ToString(CultureInfo.InvariantCulture));
								writer.Write("{0},", oProperties.Dispensability.ToString(CultureInfo.InvariantCulture));
								if (oProperties.Representative > 0)
								{
									writer.Write("\"{0}\"", Global.Ontology.GetValueByKey(oProperties.Representative).Name);
								}
								else
								{
									writer.Write("\"{0}\"", Global.Ontology.GetValueByKey(curGOTerm.ID).Name);
								}
								writer.Write(")");
							}
						}
						writer.WriteLine(");");
						writer.WriteLine();

						writer.WriteLine("stuff <- data.frame(revigo.data);");
						writer.WriteLine("names(stuff) <- revigo.names;");
						writer.WriteLine();
						if (oWorker.TermsWithValuesCount > 0)
							writer.WriteLine("stuff${0} <- as.numeric( as.character(stuff${0}) );", sLabelOfValue);
						writer.WriteLine("stuff$frequency <- as.numeric( as.character(stuff$frequency) );");
						writer.WriteLine("stuff$uniqueness <- as.numeric( as.character(stuff$uniqueness) );");
						writer.WriteLine("stuff$dispensability <- as.numeric( as.character(stuff$dispensability) );");
						writer.WriteLine();
						writer.WriteLine("# by default, outputs to a PDF file");
						writer.WriteLine("pdf( file=\"revigo_treemap.pdf\", width=16, height=9 ) # width and height are in inches");
						writer.WriteLine();
						writer.WriteLine("# check the tmPlot command documentation for all possible parameters - there are a lot more");
						writer.WriteLine("treemap(");
						writer.WriteLine("  stuff,");
						writer.WriteLine("  index = c(\"representative\",\"description\"),");
						writer.WriteLine("  vSize = \"{0}\",", sLabelOfValue);
						writer.WriteLine("  type = \"categorical\",");
						writer.WriteLine("  vColor = \"representative\",");
						writer.WriteLine("  title = \"Revigo TreeMap\",");
						writer.WriteLine("  inflate.labels = FALSE,      # set this to TRUE for space-filling group labels - good for posters");
						writer.WriteLine("  lowerbound.cex.labels = 0,   # try to draw as many labels as possible (still, some small squares may not get a label)");
						writer.WriteLine("  bg.labels = \"#CCCCCCAA\",   # define background color of group labels");
						writer.WriteLine("								 # \"#CCCCCC00\" is fully transparent, \"#CCCCCCAA\" is semi-transparent grey, NA is opaque");
						writer.WriteLine("  position.legend = \"none\"");
						writer.WriteLine(")");
						writer.WriteLine();
						writer.WriteLine("dev.off()");
						writer.WriteLine();

						writer.Flush();
						Response.End();
						#endregion
						break;
					case "jcytoscape":
						#region jcytoscape
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						Response.ContentType = "application/json";
						writer = new StreamWriter(Response.OutputStream);

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

						writer.Write("[");
						for (int i = 0; i < graph.nodes.Count; ++i)
						{
							if (i > 0)
								writer.Write(",");

							writer.Write("{{\"data\":{{\"id\":\"GO:{0:d7}\",\"label\":\"{1}\",\"value\":{2},\"color\":\"{3}\",\"log_size\":{4}}}}}",
								graph.nodes[i].ID,
								Global.StringToJSON(graph.nodes[i].properties.GetValueByKey("description").ToString().Replace("'", "")),
								Global.DoubleToJSON(Math.Round((double)graph.nodes[i].properties.GetValueByKey("value"), 3)),
								Global.StringToJSON(graph.nodes[i].properties.GetValueByKey("color").ToString()),
								10 + (int)Math.Floor(((double)graph.nodes[i].properties.GetValueByKey("LogSize") - minSize) * sizeMult));
						}
						for (int i = 0; i < graph.edges.Count; ++i)
						{
							writer.Write(",");
							writer.Write("{{\"data\":{{\"source\":\"GO:{0:d7}\",\"target\":\"GO:{1:d7}\",\"weight\":{2}}}}}",
								graph.edges[i].sourceID, graph.edges[i].destinationID,
								1 + (int)Math.Floor(((double)graph.edges[i].properties.GetValueByKey("similarity") - minWeight) * weightMult));
						}
						writer.Write("]");

						writer.Flush();
						Response.End();
						#endregion
						break;
					case "xgmml":
						#region xgmml
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						if (!bAttachment)
						{
							Response.ContentType = "text/plain";
						}
						else
						{
							Response.ContentType = "application/octet-stream";
							Response.AddHeader("Content-Disposition", string.Format("attachment; filename=Revigo{0}Cytoscape.xgmml", this.sNamespace));
						}
						writer = new StreamWriter(Response.OutputStream);
						oVisualizer.SimpleOntologram.GraphToXGMML(writer);
						writer.Flush();
						Response.End();
						#endregion
						break;
					case "simmat":
						#region simmat
						GetNamespace();
						if (CheckParameters())
							break;

						Response.Clear();
						Response.ClearHeaders();
						if (!bAttachment)
						{
							Response.ContentType = "text/plain";
						}
						else
						{
							Response.ContentType = "application/octet-stream";
							Response.AddHeader("Content-Disposition", string.Format("attachment; filename=Revigo{0}SimilarityMatrix.tsv", this.sNamespace));
						}
						writer = new StreamWriter(Response.OutputStream);

						for (int i = 0; i < oVisualizer.Terms.Length; i++)
						{
							writer.Write("\t{0}", oVisualizer.Terms[i].FormattedID);
						}
						writer.WriteLine();
						for (int i = 0; i < oVisualizer.Terms.Length; i++)
						{
							writer.Write(oVisualizer.Terms[i].FormattedID);
							for (int j = 0; j < oVisualizer.Terms.Length; j++)
							{
								writer.Write("\t{0}", oVisualizer.Matrix.Matrix[i, j].ToString(CultureInfo.InvariantCulture));
							}
							writer.WriteLine();
						}

						writer.Flush();
						Response.End();
						#endregion
						break;
					case "jclouds":
						#region jclouds
						if (oWorker.IsRunning || !oWorker.IsFinished)
						{
							ShowError("The Job did not yet complete");
							break;
						}

						if (oWorker.HasUserErrors || oWorker.HasDeveloperErrors)
						{
							ShowError("The Job has an errors, no data available.");
							break;
						}

						Response.Clear();
						Response.ClearHeaders();
						Response.ContentType = "application/json";
						writer = new StreamWriter(Response.OutputStream);

						writer.Write("{");
						if (oWorker.Enrichments != null)
						{
							writer.Write("\"Enrichments\":[");

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
										writer.Write(",");

									int size = (int)Math.Ceiling(MIN_UNIT_SIZE + Math.Round(((dFrequency - minFreq) * RANGE_UNIT_SIZE) / range));
									writer.Write("{{\"Word\":\"{0}\",\"Size\":{1}}}", Global.StringToJSON(sWord), size);
									bFirst = false;
								}
							}
							writer.Write("]");
						}

						if (oWorker.Correlations != null)
						{
							if (oWorker.Enrichments != null)
								writer.Write(",");

							writer.Write("\"Correlations\":[");

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
										writer.Write(",");

									int size = (int)Math.Ceiling(MIN_UNIT_SIZE + Math.Round(((dFrequency - minFreq) * RANGE_UNIT_SIZE) / range));
									writer.Write("{{\"Word\":\"{0}\",\"Size\":{1}}}", Global.StringToJSON(sWord), size);
									bFirst = false;
								}
							}
							writer.Write("]");
						}
						writer.Write("}");

						writer.Flush();
						Response.End();
						#endregion
						break;
					default:
						ShowError("Unknown query type.");
						break;
				}
			}
			catch (ThreadAbortException) { throw; /* propagate */ }
			catch (Exception ex)
			{
				SendEmailNotification(oWorker, ex, sType);
				ShowError("Undefined error occured while querying the job.");
			}
		}

		private void GetNamespace()
		{
			string sNamespace = Convert.ToString(Request.Params["namespace"]);
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

		private bool CheckParameters()
		{
			if (oWorker.IsRunning || !oWorker.IsFinished)
			{
				ShowError("The Job did not yet complete");
				return true;
			}

			if (oWorker.HasUserErrors || oWorker.HasDeveloperErrors)
			{
				ShowError("The Job has an errors, no data available.");
				return true;
			}

			if (eNamespace == GONamespaceEnum.None || oVisualizer == null || oVisualizer.Terms.Length == 0)
			{
				ShowError(string.Format("The namespace {0} has no results.", GeneOntology.NamespaceToFriendlyString(eNamespace)));
				return true;
			}

			return false;
		}

		private void SendEmailNotification(RevigoWorker worker, Exception ex, string qType)
		{
			string sEmailServer = ConfigurationManager.AppSettings["EmailServer"];
			string sEmailTo = ConfigurationManager.AppSettings["DeveloperEmailTo"];
			string sEmailCc = ConfigurationManager.AppSettings["DeveloperEmailCC"];

			if (!string.IsNullOrEmpty(sEmailServer) && !string.IsNullOrEmpty(sEmailTo))
			{
				StringBuilder sMessage = new StringBuilder();
				sMessage.AppendLine("Error occured during exporting of the job results on http://revigo.irb.hr.");
				sMessage.AppendLine("The user data set has been attached.");
				sMessage.AppendLine();
				sMessage.AppendFormat("Query type: '{0}'", Convert.ToString(qType));
				sMessage.AppendLine();
				sMessage.AppendLine();
				sMessage.AppendFormat("Parameters: CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}, RemoveObsolete = {4}",
					worker.CutOff, worker.ValueType, worker.Annotations.TaxonID, worker.Measure, worker.RemoveObsolete);
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
				MemoryStream oStream = new MemoryStream(Encoding.UTF8.GetBytes(worker.Data));
				message.Attachments.Add(new Attachment(oStream, "dataset.txt", "text/plain;charset=UTF-8"));

				try
				{
					client.Send(message);
				}
				catch (Exception ex1)
				{
					Global.WriteToSystemLog(this.GetType().FullName, ex1.Message);
				}

				oStream.Close();
			}
			else
			{
				Global.WriteToSystemLog(this.GetType().FullName, string.Format("Error occured during exporting of the job results on http://revigo.irb.hr; " +
					"CutOff = {0}, ValueType = {1}, SpeciesTaxon = {2}, Measure = {3}; Warnings: {4}; Errors: {5}; Dataset: {6}",
					worker.CutOff, worker.ValueType, worker.Annotations.TaxonID, worker.Measure,
					Global.JoinStringArray(worker.DeveloperWarnings), Global.JoinStringArray(worker.DeveloperErrors), worker.Data));
			}
		}

		private void ShowError(string message)
		{
			Response.Clear();
			Response.ClearHeaders();
			StreamWriter writer = new StreamWriter(Response.OutputStream);

			if (this.bJSON)
			{
				Response.ContentType = "application/json";
				writer.Write("{");
				if (message.StartsWith("["))
					writer.Write("\"error\":{0}", message);
				else
					writer.Write("\"error\":[\"{0}\"]", Global.StringToJSON(message));
				writer.Write("}");
			}
			else
			{
				Response.ContentType = "text/plain";
				writer.WriteLine("error: {0}", message);
			}
			writer.Flush();
			Response.End();
		}
	}
}
