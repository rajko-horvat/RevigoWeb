using IRB.Revigo.Core;
using IRB.Revigo.Databases;
using IRB.Revigo.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
#if WEB_STATISTICS
using MySqlConnector;
#endif
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace IRB.RevigoWeb.Pages
{
    public class GetStatisticsModel : PageModel
	{
#if WEB_STATISTICS
		public ContentResult OnGet()
        {
			string sKey = WebUtilities.TypeConverter.ToString(Request.Query["key"]);

			if (string.IsNullOrEmpty(sKey) || sKey != Global.StatisticsKey)
			{
				return Content("{}", "application/json", Encoding.UTF8);
			}

			DBConnection oConnection = new DBConnection(Global.ConnectionString);

			string sContentType = "application/json";
			StringBuilder writer = new StringBuilder();

			if (oConnection != null && oConnection.IsConnected)
			{
				// parameters
				DateTime dtFrom;
				DateTime dtTo;
				string sFrom = WebUtilities.TypeConverter.ToString(Request.Query["from"]);
				string sTo = WebUtilities.TypeConverter.ToString(Request.Query["to"]);

				if (DateTime.TryParseExact(sFrom, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtFrom) &&
					DateTime.TryParseExact(sTo, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtTo))
				{
					// this includes all hours, minutes and seconds from chosen day
					dtTo = dtTo.AddDays(1.0);

					// check for limits
					if (dtFrom.Ticks < Global.MinStatTicks)
						dtFrom = new DateTime(Global.MinStatTicks);
					if (dtTo.Ticks > Global.MaxStatTicks + 1)
						dtTo = new DateTime(Global.MaxStatTicks + 1);

					try
					{
						int iMaxPoints = 100; // how many points we want in our graph
						DataTable dtResult = new DataTable();

						// compile data list
						StringBuilder sbSQL = new StringBuilder();
						int iTableIndex = 0;
						string sTableName = "stats";

						MySqlCommand oCommand = new MySqlCommand(
							string.Format("select count(*) from {0} where ({0}.DateTimeTicks>=?from) and ({0}.DateTimeTicks<?to);", sTableName),
							oConnection.Connection);
						oCommand.Parameters.Add("?from", MySqlDbType.Int64).Value = dtFrom.Ticks;
						oCommand.Parameters.Add("?to", MySqlDbType.Int64).Value = dtTo.Ticks;
						object count = oCommand.ExecuteScalar();

						if (count != null && count != DBNull.Value)
						{
							try
							{
								if (Convert.ToInt64(count) > 1200)
								{
									sTableName = "stats_h";
								}
							}
							catch { }
						}

						sbSQL.AppendFormat("select {0}.DateTimeTicks, SUM({0}.Count) AS Requests", sTableName);

						// add RequestSourceEnum
						FieldInfo[] aRequestSourceEnum = typeof(RequestSourceEnum).GetFields();
						for (int i = 1; i < aRequestSourceEnum.Length; i++)
						{
							sbSQL.AppendFormat(", (select SUM(stats{1}.Count) FROM {0} AS stats{1} WHERE stats{1}.RequestSource={2} and " +
								"(stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `RequestSource={3}`", sTableName, iTableIndex,
								Convert.ToInt32(aRequestSourceEnum[i].GetRawConstantValue()), aRequestSourceEnum[i].Name);
							iTableIndex++;
						}

						// add hardcoded Cutoff values
						int[] aCutoff = new int[] { 4, 5, 7, 9 }; // hardcoded values
						for (int i = 0; i < aCutoff.Length; i++)
						{
							sbSQL.AppendFormat(", (select SUM(stats{1}.Count) FROM {0} AS stats{1} WHERE stats{1}.Cutoff={2} and " +
								"(stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `Cutoff=0.{2}`", sTableName, iTableIndex, aCutoff[i]);
							iTableIndex++;
						}

						// add ValueTypeEnum values
						FieldInfo[] aValueTypeEnum = typeof(ValueTypeEnum).GetFields();
						for (int i = 1; i < aValueTypeEnum.Length; i++)
						{
							sbSQL.AppendFormat(", (select SUM(stats{1}.Count) FROM {0} AS stats{1} WHERE stats{1}.ValueType={2} and " +
								"(stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `ValueType={3}`", sTableName, iTableIndex,
								Convert.ToInt32(aValueTypeEnum[i].GetRawConstantValue()), aValueTypeEnum[i].Name);
							iTableIndex++;
						}

						// add SpeciesAnnotations values
						SpeciesAnnotationsList aSpeciesAnnotations = Global.SpeciesAnnotations;
						for (int i = 0; i < aSpeciesAnnotations.Items.Count; i++)
						{
							sbSQL.AppendFormat(", (select SUM(stats{1}.Count) FROM {0} AS stats{1} WHERE stats{1}.SpeciesTaxon={2} and " +
								"(stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `SpeciesTaxon=\"{2}\"`", sTableName, iTableIndex,
								aSpeciesAnnotations.Items[i].TaxonID);
							iTableIndex++;
						}

						// add SemanticSimilarityScoreEnum values
						FieldInfo[] aSemanticSimilarityScoreEnum = typeof(SemanticSimilarityScoreEnum).GetFields();
						for (int i = 1; i < aSemanticSimilarityScoreEnum.Length; i++)
						{
							sbSQL.AppendFormat(", (select SUM(stats{1}.Count) FROM {0} AS stats{1} WHERE stats{1}.Measure={2} and " +
								"(stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `SemanticSimilarityScore={3}`", sTableName, iTableIndex,
								Convert.ToInt32(aSemanticSimilarityScoreEnum[i].GetRawConstantValue()), aSemanticSimilarityScoreEnum[i].Name);
							iTableIndex++;
						}

						// add RemoveObsolete values, it's boolean, only {true, false} values
						sbSQL.AppendFormat(", (select SUM(stats{1}.Count) FROM {0} AS stats{1} WHERE stats{1}.RemoveObsolete={2} and " +
							"(stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `RemoveObsolete={3}`", sTableName, iTableIndex,
							0, "false");
						iTableIndex++;
						sbSQL.AppendFormat(", (select SUM(stats{1}.Count) FROM {0} AS stats{1} WHERE stats{1}.RemoveObsolete={2} and " +
							"(stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `RemoveObsolete={3}`", sTableName, iTableIndex,
							1, "true");
						iTableIndex++;

						// add report usage percentages (7)
						sbSQL.AppendFormat(", (select SUM(if(stats{1}.NSCount>0.0,(stats{1}.Table_BP+stats{1}.Table_CC+stats{1}.Table_MF)/(stats{1}.NSCount),0.0)) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `TableUsage`", sTableName, iTableIndex);
						iTableIndex++;
						sbSQL.AppendFormat(", (select SUM(if(stats{1}.NSCount>0.0,(stats{1}.Scatterplot_BP+stats{1}.Scatterplot_CC+stats{1}.Scatterplot_MF)/(stats{1}.NSCount),0.0)) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `ScatterplotUsage`", sTableName, iTableIndex);
						iTableIndex++;
						sbSQL.AppendFormat(", (select SUM(if(stats{1}.NSCount>0.0,(stats{1}.Scatterplot3D_BP+stats{1}.Scatterplot3D_CC+stats{1}.Scatterplot3D_MF)/(stats{1}.NSCount),0.0)) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `Scatterplot3DUsage`", sTableName, iTableIndex);
						iTableIndex++;
						sbSQL.AppendFormat(", (select SUM(if(stats{1}.NSCount>0.0,(stats{1}.Cytoscape_BP+stats{1}.Cytoscape_CC+stats{1}.Cytoscape_MF)/(stats{1}.NSCount),0.0)) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `CytoscapeUsage`", sTableName, iTableIndex);
						iTableIndex++;
						sbSQL.AppendFormat(", (select SUM(if(stats{1}.NSCount>0.0,(stats{1}.TreeMap_BP+stats{1}.TreeMap_CC+stats{1}.TreeMap_MF)/(stats{1}.NSCount),0.0)) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `TreeMapUsage`", sTableName, iTableIndex);
						iTableIndex++;
						sbSQL.AppendFormat(", (select SUM(stats{1}.TagClouds) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `TagCloudsUsage`", sTableName, iTableIndex);
						iTableIndex++;
						sbSQL.AppendFormat(", (select SUM(if(stats{1}.NSCount>0.0,(stats{1}.SimMat_BP+stats{1}.SimMat_CC+stats{1}.SimMat_MF)/(stats{1}.NSCount),0.0)) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `SimMatUsage`", sTableName, iTableIndex);
						iTableIndex++;

						// add BiologicalProcess size value
						sbSQL.AppendFormat(", (select SUM(stats{1}.BiologicalProcess)/SUM(stats{1}.Count) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `BiologicalProcess`", sTableName, iTableIndex);
						iTableIndex++;

						// add CellularComponent size value
						sbSQL.AppendFormat(", (select SUM(stats{1}.CellularComponent)/SUM(stats{1}.Count) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `CellularComponent`", sTableName, iTableIndex);
						iTableIndex++;

						// add MolecularFunction size value
						sbSQL.AppendFormat(", (select SUM(stats{1}.MolecularFunction)/SUM(stats{1}.Count) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `MolecularFunction`", sTableName, iTableIndex);
						iTableIndex++;

						// add ExecTicks value
						sbSQL.AppendFormat(", (select SUM(stats{1}.ExecTicks)/SUM(stats{1}.Count) FROM {0} AS stats{1} " +
							"where (stats{1}.DateTimeTicks={0}.DateTimeTicks)) as `ExecTicks`", sTableName, iTableIndex);
						iTableIndex++;

						sbSQL.AppendFormat(" from {0} where ({0}.DateTimeTicks>=?from) and ({0}.DateTimeTicks<?to) group by {0}.DateTimeTicks;", sTableName);

						// fetch the data
						MySqlDataAdapter oAdapter = new MySqlDataAdapter(sbSQL.ToString(), oConnection.Connection);
						oAdapter.SelectCommand.Parameters.Add("?from", MySqlDbType.Int64).Value = dtFrom.Ticks;
						oAdapter.SelectCommand.Parameters.Add("?to", MySqlDbType.Int64).Value = dtTo.Ticks;
						//oAdapter.SelectCommand.CommandTimeout = 240;
						oAdapter.Fill(dtResult);
						oAdapter.Dispose();

						if (dtResult.Rows.Count > 0)
						{
							List<string> aViewNames = new List<string>();
							List<List<string>> aViewColumns = new List<List<string>>();
							List<StringBuilder> aViews = new List<StringBuilder>();

							double[] aSumValues = new double[dtResult.Columns.Count - 1];
							for (int i = 0; i < aSumValues.Length; i++)
							{
								aSumValues[i] = 0.0;
							}

							// add RequestSourceEnum
							aViewNames.Add("Request source");
							List<string> aColumns = new List<string>();
							aColumns.Add("Time");
							for (int i = 1; i < aRequestSourceEnum.Length; i++)
							{
								aColumns.Add(string.Format("RequestSource={0}", aRequestSourceEnum[i].Name));
							}
							aViewColumns.Add(aColumns);

							// add hardcoded Cutoff values
							aViewNames.Add("Cutoff");
							aColumns = new List<string>();
							aColumns.Add("Time");
							for (int i = 0; i < aCutoff.Length; i++)
							{
								aColumns.Add(string.Format("Cutoff=0.{0}", aCutoff[i]));
							}
							aViewColumns.Add(aColumns);

							// add ValueTypeEnum values
							aViewNames.Add("Value type");
							aColumns = new List<string>();
							aColumns.Add("Time");
							for (int i = 1; i < aValueTypeEnum.Length; i++)
							{
								aColumns.Add(string.Format("ValueType={0}", aValueTypeEnum[i].Name));
							}
							aViewColumns.Add(aColumns);

							// add SpeciesAnnotations values
							aViewNames.Add("Species");
							aColumns = new List<string>();
							aColumns.Add("Time");
							for (int i = 0; i < aSpeciesAnnotations.Items.Count; i++)
							{
								aColumns.Add(string.Format("{0} [{1}]",
									WebUtilities.TypeConverter.StringToJSON(aSpeciesAnnotations.Items[i].SpeciesName), aSpeciesAnnotations.Items[i].TaxonID));
							}
							aViewColumns.Add(aColumns);

							// add SemanticSimilarityScoreEnum values
							aViewNames.Add("Semantic similarity score");
							aColumns = new List<string>();
							aColumns.Add("Time");
							for (int i = 1; i < aSemanticSimilarityScoreEnum.Length; i++)
							{
								aColumns.Add(string.Format("SemanticSimilarityScore={0}", aSemanticSimilarityScoreEnum[i].Name));
							}
							aViewColumns.Add(aColumns);

							// add RemoveObsolete values, it's boolean, only {true, false} values
							aViewNames.Add("Remove obsolete");
							aColumns = new List<string>();
							aColumns.Add("Time");
							aColumns.Add(string.Format("RemoveObsolete={0}", false));
							aColumns.Add(string.Format("RemoveObsolete={0}", true));
							aViewColumns.Add(aColumns);

							// add Report usage values
							aViewNames.Add("Report usage");
							aColumns = new List<string>();
							aColumns.Add("Time");
							aColumns.Add("Table");
							aColumns.Add("Scatterplot");
							aColumns.Add("Scatterplot3D");
							aColumns.Add("Cytoscape");
							aColumns.Add("TreeMap");
							aColumns.Add("TagClouds");
							aColumns.Add("SimMat");
							aViewColumns.Add(aColumns);

							// add namespaces and execution time
							aViewNames.Add("Data set size and execution time");
							aColumns = new List<string>();
							aColumns.Add("Time");
							aColumns.Add("BiologicalProcessTermCount/10");
							aColumns.Add("CellularComponentTermCount/10");
							aColumns.Add("MolecularFunctionTermCount/10");
							aColumns.Add("ExecutionTimeInSeconds");
							aViewColumns.Add(aColumns);

							for (int i = 0; i < aViewNames.Count; i++)
							{
								aViews.Add(new StringBuilder());
							}

							// distribute data through time domain
							int iPosition = 0;
							long lTickStep = 0; // default is row by row
							long lFromTicks = dtFrom.Ticks;
							long lToTicks = dtTo.Ticks;
							bool bFirstRow = true;

							if (dtResult.Rows.Count > iMaxPoints)
							{
								// divide time range by maximum points and round step to minutes
								lTickStep = (new TimeSpan(0, Math.Max(1, (int)Math.Ceiling(new TimeSpan((lToTicks - lFromTicks) / (long)iMaxPoints).TotalMinutes)), 0)).Ticks;
							}
							else
							{
								// round step to minutes
								lTickStep = new TimeSpan(0, 1, 0).Ticks;
							}

							// distribute data across time range
							while (iPosition < dtResult.Rows.Count)
							{
								DateTime dtDateTime = new DateTime(lFromTicks);
								long lNewFromTicks = lFromTicks + lTickStep;
								long lTicks;
								DataRow row;
								double[] aValues = new double[dtResult.Columns.Count - 1];

								for (int j = 0; j < aValues.Length; j++)
								{
									aValues[j] = 0.0;
								}

								while (iPosition < dtResult.Rows.Count && (lTicks = WebUtilities.TypeConverter.ToInt64((row = dtResult.Rows[iPosition])[0])) < lNewFromTicks)
								{
									for (int j = 0; j < aValues.Length; j++)
									{
										aValues[j] += WebUtilities.TypeConverter.ToDouble(row[j + 1]);
									}
									iPosition++;
								}
								lFromTicks = lNewFromTicks;

								// value 0 is total number of requests (use only for arithmetic center)
								int iValueIndex = 1;
								int iViewIndex = 0;
								double dRequestCount1 = Math.Max(1.0, aValues[0]);
								string sDateFormat = "d.M.yyyy. HH:mm:ss";

								// aggregate values
								for (int i = 0; i < aValues.Length; i++)
								{
									aSumValues[i] += aValues[i];
								}

								// add RequestSourceEnum
								StringBuilder sbData = aViews[iViewIndex];
								aColumns = aViewColumns[iViewIndex];
								iViewIndex++;
								if (!bFirstRow) sbData.Append(",");
								sbData.AppendFormat("[\"{0}\"", dtDateTime.ToString(sDateFormat));
								for (int i = 1; i < aRequestSourceEnum.Length; i++)
								{
									sbData.AppendFormat(",{0}", aValues[iValueIndex]);
									iValueIndex++;
								}
								sbData.Append("]");

								// add hardcoded Cutoff values
								sbData = aViews[iViewIndex];
								aColumns = aViewColumns[iViewIndex];
								if (!bFirstRow) sbData.Append(",");
								iViewIndex++;
								sbData.AppendFormat("[\"{0}\"", dtDateTime.ToString(sDateFormat));
								for (int i = 0; i < aCutoff.Length; i++)
								{
									sbData.AppendFormat(",{0}", aValues[iValueIndex]);
									iValueIndex++;
								}
								sbData.Append("]");

								// add ValueTypeEnum values
								sbData = aViews[iViewIndex];
								aColumns = aViewColumns[iViewIndex];
								iViewIndex++;
								if (!bFirstRow) sbData.Append(",");
								sbData.AppendFormat("[\"{0}\"", dtDateTime.ToString(sDateFormat));
								for (int i = 1; i < aValueTypeEnum.Length; i++)
								{
									sbData.AppendFormat(",{0}", aValues[iValueIndex]);
									iValueIndex++;
								}
								sbData.Append("]");

								// add SpeciesAnnotations values
								sbData = aViews[iViewIndex];
								aColumns = aViewColumns[iViewIndex];
								iViewIndex++;
								if (!bFirstRow) sbData.Append(",");
								sbData.AppendFormat("[\"{0}\"", dtDateTime.ToString(sDateFormat));
								for (int i = 0; i < aSpeciesAnnotations.Items.Count; i++)
								{
									sbData.AppendFormat(",{0}", aValues[iValueIndex]);
									iValueIndex++;
								}
								sbData.Append("]");

								// add SemanticSimilarityScoreEnum values
								sbData = aViews[iViewIndex];
								aColumns = aViewColumns[iViewIndex];
								iViewIndex++;
								if (!bFirstRow) sbData.Append(",");
								sbData.AppendFormat("[\"{0}\"", dtDateTime.ToString(sDateFormat));
								for (int i = 1; i < aSemanticSimilarityScoreEnum.Length; i++)
								{
									sbData.AppendFormat(",{0}", aValues[iValueIndex]);
									iValueIndex++;
								}
								sbData.Append("]");

								// add RemoveObsolete values, it's boolean, only {true, false} values
								sbData = aViews[iViewIndex];
								aColumns = aViewColumns[iViewIndex];
								iViewIndex++;
								if (!bFirstRow) sbData.Append(",");
								sbData.AppendFormat("[\"{0}\"", dtDateTime.ToString(sDateFormat));
								sbData.AppendFormat(",{0}", aValues[iValueIndex]);
								iValueIndex++;
								sbData.AppendFormat(",{0}", aValues[iValueIndex]);
								iValueIndex++;
								sbData.Append("]");

								// add Report usage values
								sbData = aViews[iViewIndex];
								aColumns = aViewColumns[iViewIndex];
								iViewIndex++;
								if (!bFirstRow) sbData.Append(",");
								sbData.AppendFormat("[\"{0}\"", dtDateTime.ToString(sDateFormat));
								for (int i = 0; i < 7; i++)
								{
									sbData.AppendFormat(",{0}", ((aValues[iValueIndex] / dRequestCount1)).ToString(CultureInfo.InvariantCulture));
									iValueIndex++;
								}
								sbData.Append("]");

								// add namespaces and execution time
								sbData = aViews[iViewIndex];
								aColumns = aViewColumns[iViewIndex];
								iViewIndex++;
								if (!bFirstRow) sbData.Append(",");
								sbData.AppendFormat("[\"{0}\"", dtDateTime.ToString(sDateFormat));
								// BiologicalProcess
								sbData.AppendFormat(",{0}", ((aValues[iValueIndex] / dRequestCount1) / 10.0).ToString(CultureInfo.InvariantCulture));
								iValueIndex++;
								// CellularComponent
								sbData.AppendFormat(",{0}", ((aValues[iValueIndex] / dRequestCount1) / 10.0).ToString(CultureInfo.InvariantCulture));
								iValueIndex++;
								// MolecularFunction
								sbData.AppendFormat(",{0}", ((aValues[iValueIndex] / dRequestCount1) / 10.0).ToString(CultureInfo.InvariantCulture));
								iValueIndex++;
								// ExecutionTimeInSeconds
								sbData.AppendFormat(",{0}", (new TimeSpan((long)(aValues[iValueIndex] / dRequestCount1))).TotalSeconds.ToString(CultureInfo.InvariantCulture));
								iValueIndex++;
								sbData.Append("]");

								bFirstRow = false;
							}

							int iSumPosition = 1;
							double dRequestCount = aSumValues[0];

							writer.AppendFormat("{{\"error\":\"\",\"maxDate\":\"{0}\",\"requestCount\":{1},\"views\":[",
								(new DateTime(Global.MaxStatTicks)).ToString("d.M.yyyy"), dRequestCount);

							for (int i = 0; i < aViews.Count; i++)
							{
								if (i > 0)
									writer.Append(",");

								StringBuilder sbColumns = new StringBuilder();
								StringBuilder sbDistribution = new StringBuilder();
								sbDistribution.Append("0.0");

								for (int j = 0; j < aViewColumns[i].Count; j++)
								{
									if (j > 0)
										sbColumns.Append(",");
									sbColumns.AppendFormat("\"{0}\"", WebUtilities.TypeConverter.StringToJSON(aViewColumns[i][j]));

									if (j > 0)
									{
										if (iSumPosition < aSumValues.Length - 4)
										{
											sbDistribution.AppendFormat(",{0}",
												(((double)aSumValues[iSumPosition] / dRequestCount) * 100.0).ToString("##0.0###", CultureInfo.InvariantCulture));
											iSumPosition++;
										}
										else
										{
											sbDistribution.Append(",\"NaN\"");
											iSumPosition++;
										}
									}
								}

								writer.AppendFormat("{{\"name\":\"{0}\",\"type\":\"{1}\",\"columns\":[{2}],\"distribution\":[{3}],\"data\":[{4}]}}",
									aViewNames[i], (i < aViews.Count - 1) ? "stack" : "bar", sbColumns.ToString(), sbDistribution.ToString(), aViews[i].ToString());
							}
							writer.AppendLine("]}");
						}
						else
						{
							writer.AppendFormat("{{\"error\":\"No data points are available for specified date range\",\"maxDate\":\"{0}\",\"requestCount\":0,\"views\":[]}}",
								(new DateTime(Global.MaxStatTicks)).ToString("d.M.yyyy"));
							writer.AppendLine();
						}
					}
					catch (Exception ex)
					{
						writer.AppendFormat("{{\"error\":\"Error accessing data\",\"maxDate\":\"{0}\",\"requestCount\":0,\"views\":[]}}",
							(new DateTime(Global.MaxStatTicks)).ToString("d.M.yyyy"));
						writer.AppendLine();
						Global.WriteToSystemLog(typeof(Global).FullName, ex.Message);
					}
				}
				else
				{
					writer.AppendFormat("{{\"error\":\"Invalid date range specified\",\"maxDate\":\"{0}\",\"requestCount\":0,\"views\":[]}}",
						(new DateTime(Global.MaxStatTicks)).ToString("d.M.yyyy"));
					writer.AppendLine();
				}
				oConnection.Close();
				
				return Content(writer.ToString(), sContentType, Encoding.UTF8);
			}

			writer.AppendFormat("{{\"error\":\"No database available\",\"maxDate\":{0},\"requestCount\":0,\"views\":[]}}",
				(new DateTime(Global.MaxStatTicks)).ToString("d.M.yyyy"));

			return Content(writer.ToString(), sContentType, Encoding.UTF8);
		}
#else
	public ContentResult OnGet()
    {
		return Content("{}", "application/json", Encoding.UTF8);
	}
#endif
	}
}
