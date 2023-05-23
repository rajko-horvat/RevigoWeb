function RevigoTable(containerName, tableData)
{
	var rtCtrl = this;

	rtCtrl.containerName = containerName;
	rtCtrl.jContainer = $("#" + containerName);
	rtCtrl.data = tableData;
	rtCtrl.jTBody = undefined;
	rtCtrl.tableCache = { columns: [], rows: [] };
	rtCtrl.allNodeState = 1; // -1 for all collapsed, 1 for all expanded, 0 for undefined

	function UpdateTBody()
	{
		// construct table tbody from cache and replace instead of old
		var jNewTBody = $("<tbody></tbody>");

		for (let i = 0, count = 0; i < rtCtrl.tableCache.rows.length; i++)
		{
			let tableRow = rtCtrl.tableCache.rows[i];
			let evenRow = (count & 1) === 1;

			// non visible rows are also added, but not displayed
			if (!tableRow.visible)
				tableRow.jRow.css("display", "none");
			else
			{
				tableRow.jRow.css("display", "");
				count++;
			}

			tableRow.jRow.removeClass("odd");
			tableRow.jRow.removeClass("even");
			if (evenRow)
				tableRow.jRow.addClass("even");
			else
				tableRow.jRow.addClass("odd");

			tableRow.jRow.appendTo(jNewTBody);

			for (let j = 0; j < tableRow.children.length; j++)
			{
				let childRow = tableRow.children[j];

				childRow.jRow.removeClass("odd");
				childRow.jRow.removeClass("even");
				if (evenRow)
					childRow.jRow.addClass("even");
				else
					childRow.jRow.addClass("odd");

				// non visible child rows are also added, but not displayed
				if (!tableRow.expanded || !tableRow.visible)
					childRow.jRow.css("display", "none");
				else
					childRow.jRow.css("display", "");

				childRow.jRow.appendTo(jNewTBody);
			}
		}

		if (rtCtrl.jTBody)
		{
			rtCtrl.jTBody.empty();
			rtCtrl.jTBody.remove();
			rtCtrl.jTBody = undefined;
		}

		jNewTBody.appendTo(rtCtrl.jTable);
		rtCtrl.jTBody = jNewTBody;
	}

	rtCtrl.GetEncodedContents = function ()
	{
		var content = "";

		// column header, ignore pinterm column types
		for (let i = 0, count = 0; i < rtCtrl.data.Columns.length; i++)
		{
			let column = rtCtrl.data.Columns[i];
			let columnType = column.Type.toLowerCase();

			if (column.Visible && columnType != "pinterm")
			{
				if (count > 0)
					content += "\t";
				content += column.Name.replace(" ", "");
				count++;
			}
		}
		content += "\tRepresentative\r\n";

		for (let i = 0, count = 0; i < rtCtrl.tableCache.rows.length; i++)
		{
			let tableRow = rtCtrl.tableCache.rows[i];

			// skip hidden rows
			if (tableRow.visible)
			{
				for (let j = 0, count = 0; j < rtCtrl.data.Columns.length; j++)
				{
					let column = rtCtrl.data.Columns[j];
					let columnType = column.Type.toLowerCase();

					if (column.Visible && columnType != "pinterm")
					{
						if (count > 0)
							content += "\t";

						if (columnType === "string")
							content += '"' + tableRow.row[column.Name] + '"';
						else
							content += tableRow.row[column.Name];

						count++;
					}
				}
				content += "\tnull\r\n";

				if (tableRow.expanded)
				{
					for (let j = 0; j < tableRow.children.length; j++)
					{
						let childRow = tableRow.children[j];

						if (childRow.visible)
						{
							for (let k = 0, count = 0; k < rtCtrl.data.Columns.length; k++)
							{
								let column = rtCtrl.data.Columns[k];
								let columnType = column.Type.toLowerCase();

								if (column.Visible && column.Type != "pinterm")
								{
									if (count > 0)
										content += "\t";

									if (columnType === "string")
										content += '"' + childRow.row[column.Name] + '"';
									else
										content += childRow.row[column.Name];

									count++;
								}
							}
							// for children put representative ID, which is always at column 0
							content += "\t" + tableRow.parsedRow[0] + "\r\n";
						}
					}
				}
			}
		}

		return "data:text/plain;charset=utf-8," + encodeURIComponent(content);
	}

	function ContructCache()
	{
		// cache columns
		for (let i = 0, count=0; i < rtCtrl.data.Columns.length; i++)
		{
			// sorted: 0 - not sorted, -1 - sorted descending, 1 - sorted ascending
			let col = rtCtrl.data.Columns[i];
			if (col.Visible)
			{
				rtCtrl.tableCache.columns.push({ ordinal: count, sorted: 0, filter: undefined });
				count++;
			}
			else
				rtCtrl.tableCache.columns.push({ ordinal: -1, sorted: 0, filter: undefined });
		}

		// cache rows
		for (let i = 0; i < rtCtrl.data.Rows.length; i++)
		{
			var oRow = rtCtrl.data.Rows[i];

			if (oRow.Representative <= 0)
			{
				let parentID = oRow.ID;
				var aChildren = [];

				// append children (if available)
				for (let j = 0; j < rtCtrl.data.Rows.length; j++)
				{
					let oChildRow = rtCtrl.data.Rows[j];

					if (oChildRow.Representative === parentID)
					{
						let tableRow = $("<tr></tr>");
						// parse all values as string (used for searching)
						let parsedData = [];
						let aFilterPos = [];
						let aFilterLen = [];

						// append visible columns
						for (let k = 0, count = 0; k < rtCtrl.data.Columns.length; k++)
						{
							let column = rtCtrl.data.Columns[k];
							let type = column.Type.toLowerCase();
							let sValue = FormatColumn(oChildRow[column.Name], type, column.Decimals);
							parsedData[k] = sValue;
							aFilterPos[k] = -1;
							aFilterLen[k] = 0;

							if (column.Visible)
							{
								let jTd = $("<td></td>").appendTo(tableRow);
								jTd.addClass("child");
								if (count === 0)
									jTd.addClass("padding");
								if (column.CellTitle)
									jTd.attr("title", oChildRow[column.CellTitle]);

								if (type === "term")
								{
									let jA = $("<a></a>").appendTo(jTd);
									jA.attr("href", "https://www.ebi.ac.uk/QuickGO/search/" + sValue);
									jA.attr("target", "_blank");
									jA.text(sValue);
								}
								else if (type === "number" || type === "percentage")
								{
									jTd.addClass("alignRight");
									jTd.text(sValue);
								}
								else if (type === "pinterm")
								{									
									jTd.addClass("alignCenter");
									jTd.text(sValue);
									let value = oChildRow[column.Name];
									if (value >= 0)
									{
										let jImg = $("<img/>").appendTo(jTd);
										jImg.attr("src", value === 0 ? "Images/Locked.svg" : "Images/Unlocked.svg");
										jImg.attr("alt", value === 0 ? "Pinned" : "Unpinned");
										jImg.attr("title", value === 0 ? "Unpin as the cluster representative" : "Pin as the cluster representative");
										jImg.css("width", "1.2rem");
										jImg.css("height", "1.2rem");
										jImg.css("vertical-align", "middle");
										jImg.css("cursor", "pointer");

										let termID = oChildRow[rtCtrl.data.Columns[0].Name];

										// attach event
										jTd.on("click", function (e)
										{
											//console.log("Clicked pin action on: " + oChildRow[rtCtrl.data.Columns[0].Name]);

											rtCtrl.jTable.trigger("pinterm", [termID]);
										});
									}
								}
								else
								{
									jTd.text(sValue);
								}
								count++;
							}
						}

						aChildren.push({ ordinal: j, row: oChildRow, parsedRow: parsedData, jRow: tableRow, filterPos: aFilterPos, filterLen: aFilterLen, visible: 1 });
					}
				}

				// append visible columns
				var tableRow = $("<tr></tr>");
				let jGroup = $("<td></td>").appendTo(tableRow);
				let cachedRow = { ordinal: i, row: oRow, parsedRow: [], jRow: tableRow, expanded: 1, filterPos: [], filterLen: [],children: aChildren, visible: 1 };

				if (aChildren.length > 0)
				{
					jGroup.attr("rowspan", aChildren.length + 1);
					jGroup.css("vertical-align", "top");
					jGroup.css("text-align", "center");

					let jImg = $("<img/>").appendTo(jGroup);
					jImg.attr("id", rtCtrl.containerName + "R" + cachedRow.ordinal + "C0");
					jImg.attr("src", "Images/Minus.svg");
					jImg.attr("alt", "Minus");
					jImg.attr("title", "Collapse node");
					jImg.css("width", "1.0rem");
					jImg.css("height", "1.0rem");
					jImg.tooltip();
					jImg.on("click", function ()
					{
						cachedRow.expanded = !cachedRow.expanded;
						if (cachedRow.expanded)
						{
							let img = $("#" + rtCtrl.containerName + "R" + cachedRow.ordinal + "C0");
							img.attr("alt", "Minus");
							img.attr("src", "Images/Minus.svg");
							img.attr("title", "Collapse node");

							for (let i = 0; i < cachedRow.children.length; i++)
							{
								cachedRow.children[i].jRow.css("display", "");
							}
							cachedRow.jRow.children("td").first().attr("rowspan", cachedRow.children.length + 1);

							// node state is undefined here
							if (allNodeState != 1)
							{
								allNodeState = 1;
								img = $("#" + rtCtrl.containerName + "NodeState");
								img.attr("src", "Images/NoPlus-Minus.svg");
								img.attr("alt", "Collapse all nodes");
								img.attr("title", "Collapse all nodes");
							}
						}
						else
						{
							cachedRow.jRow.children("td").first().attr("rowspan", "");

							let img = $("#" + rtCtrl.containerName + "R" + cachedRow.ordinal + "C0");
							img.attr("alt", "Plus");
							img.attr("src", "Images/Plus.svg");
							img.attr("title", "Expand node");

							for (let i = 0; i < cachedRow.children.length; i++)
							{
								cachedRow.children[i].jRow.css("display", "none");
							}
							// node state is undefined here
							if (allNodeState != 1)
							{
								allNodeState = 1;
								img = $("#" + rtCtrl.containerName + "NodeState");
								img.attr("src", "Images/NoPlus-Minus.svg");
								img.attr("alt", "Collapse all nodes");
								img.attr("title", "Collapse all nodes");
							}
						}
					});
				}

				for (let j = 0, count = 0; j < rtCtrl.data.Columns.length; j++)
				{
					let column = rtCtrl.data.Columns[j];
					let type = column.Type.toLowerCase();
					let sValue = FormatColumn(oRow[column.Name], type, column.Decimals);

					// store all values as string (used for searching)
					cachedRow.parsedRow[j] = sValue;
					cachedRow.filterPos[j] = -1;
					cachedRow.filterLen[j] = 0;

					if (column.Visible)
					{
						let jTd = $("<td></td>").appendTo(tableRow);
						if (column.CellTitle)
							jTd.attr("title", oRow[column.CellTitle]);

						if (type === "term")
						{
							let jA = $("<a></a>").appendTo(jTd);
							jA.attr("href", "http://amigo.geneontology.org/amigo/term/" + sValue);
							jA.attr("target", "_blank");
							jA.text(sValue);
						}
						else if (type === "number" || type === "percentage")
						{
							jTd.addClass("alignRight");
							jTd.text(sValue);
						}
						else if (type === "pinterm")
						{
							jTd.addClass("alignCenter");
							jTd.text(sValue);
							let value = oRow[column.Name];
							if (value >= 0)
							{
								let jImg = $("<img/>").appendTo(jTd);
								jImg.attr("src", value === 0 ? "Images/Locked.svg" : "Images/Unlocked.svg");
								jImg.attr("alt", value === 0 ? "Pinned" : "Unpinned");
								jImg.attr("title", value === 0 ? "Unpin as the cluster representative" : "Pin as the cluster representative");
								jImg.css("width", "1.2rem");
								jImg.css("height", "1.2rem");
								jImg.css("vertical-align", "middle");

								let termID = oRow[rtCtrl.data.Columns[0].Name];

								// attach event
								jTd.on("click", function (e)
								{
									//console.log("Clicked pin action on: " + oChildRow[rtCtrl.data.Columns[0].Name]);

									rtCtrl.jTable.trigger("pinterm", [termID]);
								});
							}
						}
						else
						{
							jTd.text(sValue);
						}

						if (count === 0 && aChildren.length > 0)
						{
							jTd.addClass("expandable");
						}
						count++;
					}
				}

				rtCtrl.tableCache.rows.push(cachedRow);
			}
		}
	}

	function FormatColumn(value, type, decimals)
	{
		var retVal = "";
		//type = type.toLowerCase();

		// we support only "string", "number", "percentage" for now
		if (type === "number")
		{
			if (value === undefined || isNaN(value) || ((typeof value) === "string" && value.toLowerCase() === "nan"))
				retVal = "NaN";
			else if (decimals >= 0)
				retVal = value.toFixed(decimals);
			else
				retVal = value.toString();
		}
		else if (type === "percentage")
		{
			if (value === undefined || isNaN(value) || ((typeof value) === "string" && value.toLowerCase() === "nan"))
				retVal = "NaN";
			else if (decimals >= 0)
				retVal = value.toFixed(decimals) + "%";
			else
				retVal = value.toString() + "%";
		}
		else if (type === "pinterm")
		{
			retval = "";
		}
		else
		{
			retVal = value.toString();
		}

		return retVal;
	}

	rtCtrl.jContainer.empty();

	ContructCache();

	// create table and required rows/columns
	rtCtrl.jTable = $("<table></table>").appendTo(rtCtrl.jContainer);
	rtCtrl.jTable.addClass("resultTable");
	
	// append visible columns (thead)
	var jThead = $("<thead></thead>").appendTo(rtCtrl.jTable);
	var jHeadRow = $("<tr></tr>").appendTo(jThead);
	var jFilterRow = $("<tr></tr>").appendTo(jThead);

	let jGroup = $("<th></th>").appendTo(jHeadRow);
	jGroup.css("text-align", "center");
	jGroup.addClass("condensed");
	jGroup.addClass("pointer");
	let jImg = $("<img/>").appendTo(jGroup);
	jImg.attr("id", rtCtrl.containerName + "NodeState");
	jImg.attr("src", "Images/NoPlus-Minus.svg");
	jImg.attr("alt", "Collapse all nodes");
	jImg.attr("title", "Collapse all nodes");
	jImg.css("width", "1.2rem");
	jImg.css("height", "1.2rem");
	jImg.css("vertical-align", "middle");
	jImg.tooltip();
	jImg.on("click", function ()
	{
		if (rtCtrl.allNodeState === -1)
		{
			// all nodes are collapsed, expand them
			for (let i = 0; i < rtCtrl.tableCache.rows.length; i++)
			{
				let row = rtCtrl.tableCache.rows[i];

				if (!row.expanded && row.children.length > 0)
				{
					row.expanded = 1;

					let img = $("#" + rtCtrl.containerName + "R" + row.ordinal + "C0");
					img.attr("alt", "Minus");
					img.attr("src", "Images/Minus.svg");
					img.attr("title", "Collapse node");

					if (row.visible)
					{
						for (let i = 0; i < row.children.length; i++)
						{
							row.children[i].jRow.css("display", "");
						}
					}

					row.jRow.children("td").first().attr("rowspan", row.children.length + 1);

					// update node state to expanded
					rtCtrl.allNodeState = 1;
					img = $("#" + rtCtrl.containerName + "NodeState");
					img.attr("src", "Images/NoPlus-Minus.svg");
					img.attr("alt", "Collapse all nodes");
					img.attr("title", "Collapse all nodes");
				}
			}
		}
		else
		{
			// (rtCtrl.allNodeState === 1) all nodes are expanded, collapse them
			// (rtCtrl.allNodeState === 0) nodes are in undefined state, collapse them by default
			for (let i = 0; i < rtCtrl.tableCache.rows.length; i++)
			{
				let row = rtCtrl.tableCache.rows[i];

				if (row.expanded && row.children.length > 0)
				{
					row.expanded = 0;

					let img = $("#" + rtCtrl.containerName + "R" + row.ordinal + "C0");
					img.attr("alt", "Plus");
					img.attr("src", "Images/Plus.svg");
					img.attr("title", "Expand node");

					if (row.visible)
					{
						for (let i = 0; i < row.children.length; i++)
						{
							row.children[i].jRow.css("display", "none");
						}
					}

					row.jRow.children("td").first().attr("rowspan", "");

					// update node state to collapsed
					rtCtrl.allNodeState = -1;
					img = $("#" + rtCtrl.containerName + "NodeState");
					img.attr("src", "Images/Plus-NoMinus.svg");
					img.attr("alt", "Expand all nodes");
					img.attr("title", "Expand all nodes");
				}
			}
		}
	});

	// add filter row
	let jFilter = $("<td></td>").appendTo(jFilterRow);
	jFilter.css("text-align", "center");
	jImg = $("<img/>").appendTo(jFilter);
	jImg.attr("src", "Images/Funnel.svg");
	jImg.attr("alt", "Filter");
	jImg.attr("title", "Please enter filtering term for a column. Press and hold Alt key then click on this icon to clear all filters");
	jImg.css("width", "1.1rem");
	jImg.css("height", "1.1rem");
	jImg.css("vertical-align", "middle");
	jImg.tooltip();
	jImg.on("click", function (e)
	{
		if (e.altKey)
		{
			// clear all filters
			let children = jFilterRow[0].children;
			for (let i = 1; i < children.length; i++)
			{
				let input = $(children[i]).children("input");
				if (input)
				{
					input.val("");
					input.trigger("change");
				}
			}
		}
	});

	for (let i = 0; i < rtCtrl.data.Columns.length; i++)
	{
		let column = rtCtrl.data.Columns[i];

		if (column.Visible)
		{
			let jTh = $("<th></th>").appendTo(jHeadRow);

			jTh.text(column.Name);
			jTh.addClass("ui-widget-header");
			if (column.Condensed)
				jTh.addClass("condensed");

			if (column.Title)
			{
				jTh.attr("title", column.Title);
				jTh.tooltip();

				let jImg = $("<img/>").appendTo(jTh);
				jImg.attr("src", "Images/Info.svg");
				jImg.attr("alt", "Info");
				jImg.css("width", "1.2rem");
				jImg.css("height", "1.2rem");
				jImg.css("vertical-align", "middle");
				jImg.css("margin", "0.2rem");
			}
			if (column.Sortable)
			{
				jTh.addClass("pointer");
				let jImg = $("<img/>").appendTo(jTh);
				jImg.attr("id", rtCtrl.containerName + "C" + i + "SortDir");
				jImg.attr("src", "Images/Arrows.svg");
				jImg.attr("alt", "Sort direction");
				jImg.attr("title", "Press and hold Alt key then click on column header to clear sort");
				jImg.css("width", "1rem");
				jImg.css("height", "1rem");
				jImg.css("vertical-align", "middle");
				jImg.css("margin", "0.2rem");

				let cachedColumn = rtCtrl.tableCache.columns[i];
				jTh.on("click", function (e)
				{
					let ordinal = cachedColumn.ordinal;
					let columns = rtCtrl.tableCache.columns;
					//console.log("Column " + ordinal + " needs sorting");

					for (let j = 0; j < columns.length; j++)
					{
						if (columns[j].ordinal != ordinal)
						{
							if (columns[j].sorted != 0)
							{
								columns[j].sorted = 0;
								$("#" + rtCtrl.containerName + "C" + j + "SortDir").attr("src", "Images/Arrows.svg");
							}
						}
					}

					if (cachedColumn.sorted === 0 || cachedColumn.sorted === -1)
					{
						cachedColumn.sorted = 1;
						$("#" + rtCtrl.containerName + "C" + i + "SortDir").attr("src", "Images/Arrows-up.svg");
					}
					else
					{
						cachedColumn.sorted = -1;
						$("#" + rtCtrl.containerName + "C" + i + "SortDir").attr("src", "Images/Arrows-down.svg");
					}

					let dir = cachedColumn.sorted;
					let type = column.Type.toLowerCase();
					let colName = column.Name;

					if (e.altKey)
					{
						// reset sorting to original order
						cachedColumn.sorted = 0;
						$("#" + rtCtrl.containerName + "C" + i + "SortDir").attr("src", "Images/Arrows.svg");

						rtCtrl.tableCache.rows.sort(function (x, y) { return x.ordinal - y.ordinal });
						for (let j = 0; j < rtCtrl.tableCache.rows.length; j++)
							rtCtrl.tableCache.rows[j].children.sort(function (x, y) { return x.ordinal - y.ordinal });
					}
					else
					{
						if (type === "number" || type === "percentage")
						{
							// sort numbers
							rtCtrl.tableCache.rows.sort(function (x, y)
							{
								// x <  y return -1
								// x >  y return  1
								// x == y return  0
								let value1 = x.row[colName];
								let value2 = y.row[colName];

								// handle NaNs
								let nan1 = value1 === undefined || value1 === "NaN" || isNaN(value1);
								let nan2 = value2 === undefined || value2 === "NaN" || isNaN(value2);

								if (nan1 && nan2)
									return 0;
								if (nan1 && !nan2)
									return -dir;
								if (!nan1 && nan2)
									return dir;

								return (value1 - value2) * dir;
							});

							// sort children
							for (let j = 0; j < rtCtrl.tableCache.rows.length; j++)
							{
								let row = rtCtrl.tableCache.rows[j];

								if (row.children.length > 0)
								{
									row.children.sort(function (x, y)
									{
										// x <  y return -1
										// x >  y return  1
										// x == y return  0
										let value1 = x.row[colName];
										let value2 = y.row[colName];

										// handle NaNs
										let nan1 = value1 === undefined || value1 === "NaN" || isNaN(value1);
										let nan2 = value2 === undefined || value2 === "NaN" || isNaN(value2);

										if (nan1 && nan2)
											return 0;
										if (nan1 && !nan2)
											return -dir;
										if (!nan1 && nan2)
											return dir;

										return (value1 - value2) * dir;
									});
								}
							}
						}
						else
						{
							// sort strings
							rtCtrl.tableCache.rows.sort(function (x, y)
							{
								// x <  y return -1
								// x >  y return  1
								// x == y return  0
								let value1 = x.row[colName];
								let value2 = y.row[colName];

								// handle NaNs
								let nan1 = value1 === undefined || (typeof value1) !== "string";
								let nan2 = value2 === undefined || (typeof value2) !== "string";

								if (nan1 && nan2)
									return 0;
								if (nan1 && !nan2)
									return -dir;
								if (!nan1 && nan2)
									return dir;

								// case insensitive compare
								value1 = value1.toLowerCase();
								value2 = value2.toLowerCase();

								if (value1 < value2)
									return -dir;
								if (value1 > value2)
									return dir;

								return 0;
							});

							// sort children
							for (let j = 0; j < rtCtrl.tableCache.rows.length; j++)
							{
								let row = rtCtrl.tableCache.rows[j];

								if (row.children.length > 0)
								{
									row.children.sort(function (x, y)
									{
										// x <  y return -1
										// x >  y return  1
										// x == y return  0
										let value1 = x.row[colName];
										let value2 = y.row[colName];

										// handle NaNs
										let nan1 = value1 === undefined || (typeof value1) !== "string";
										let nan2 = value2 === undefined || (typeof value2) !== "string";

										if (nan1 && nan2)
											return 0;
										if (nan1 && !nan2)
											return -dir;
										if (!nan1 && nan2)
											return dir;

										// case insensitive compare
										value1 = value1.toLowerCase();
										value2 = value2.toLowerCase();

										if (value1 < value2)
											return -dir;
										if (value1 > value2)
											return dir;

										return 0;
									});
								}
							}
						}
					}

					UpdateTBody();
				});
			}

			if (column.Filter)
			{
				let cachedColumn = rtCtrl.tableCache.columns[i];
				let jTd = $("<td></td>").appendTo(jFilterRow);
				let jInput = $("<input />").appendTo(jTd);
				let type = column.Type.toLowerCase();
				jInput.addClass("filter");
				jInput.attr("type", "search");
				if (type === "number" || type === "percentage")
				{
					jInput.addClass("alignRight");
				}
				else if (type === "pinterm")
				{
					jInput.addClass("alignCenter");
				}
				jInput.on("change keyup", function (e)
				{
					//console.log("Column " + i + " filter changed to: " + e.target.value);

					if (e.target.value != cachedColumn.filter)
					{
						cachedColumn.filter = e.target.value;

						// we filter by row, and then by column
						let rows = rtCtrl.tableCache.rows;
						let cols = rtCtrl.tableCache.columns;

						for (let j = 0; j < rows.length; j++)
						{
							let row = rows[j];
							row.visible = 1;

							for (let k = 0; k < cols.length; k++)
							{
								let col = cols[k];

								if (col.filter && col.filter.length > 0)
								{
									let type = column.Type.toLowerCase();
									var filterPos;
									var filterLen;

									if (type === "number" || type === "percentage")
									{
										// To do: we want to support less than '<' and higher than '>' operators

										filterPos = row.parsedRow[k].indexOf(col.filter);
										filterLen = col.filter.length;
									}
									else
									{
										filterPos = row.parsedRow[k].indexOf(col.filter);
										filterLen = col.filter.length;
									}

									row.filterPos[k] = filterPos;
									row.filterLen[k] = filterLen;
									if (filterPos >= 0)
									{
										let jCol = $(row.jRow[0].children[col.ordinal + 1]);
										let colText = row.parsedRow[k];

										jCol.empty();
										$(document.createTextNode(colText.substring(0, filterPos))).appendTo(jCol);
										let jSpan = $("<span></span>").appendTo(jCol);
										jSpan.css("background-color", "yellow");
										jSpan.text(colText.substring(filterPos, filterPos + filterLen));
										$(document.createTextNode(colText.substring(filterPos + filterLen))).appendTo(jCol);
									}
									else
									{
										let jCol = $(row.jRow[0].children[col.ordinal + 1]);
										jCol.empty();
										jCol.text(row.parsedRow[k]);
									}

									// children can also contain filter
									var childCount = 0;
									for (let l = 0; l < row.children.length; l++)
									{
										let child = row.children[l];
										var childFilterPos;
										var childFilterLen;

										if (type === "number" || type === "percentage")
										{
											// To do: we want to support less than '<' and higher than '>' operators

											childFilterPos = child.parsedRow[k].indexOf(col.filter);
											childFilterLen = filterLen;
										}
										else
										{
											childFilterPos = child.parsedRow[k].indexOf(col.filter);
											childFilterLen = filterLen;
										}

										child.filterPos[k] = childFilterPos;
										child.filterLen[k] = childFilterLen;
										if (childFilterPos >= 0)
										{
											let jCol = $(child.jRow[0].children[col.ordinal]);
											let colText = child.parsedRow[k];

											jCol.empty();
											$(document.createTextNode(colText.substring(0, childFilterPos))).appendTo(jCol);
											let jSpan = $("<span></span>").appendTo(jCol);
											jSpan.css("background-color", "yellow");
											jSpan.text(colText.substring(childFilterPos, childFilterPos + childFilterLen));
											$(document.createTextNode(colText.substring(childFilterPos + childFilterLen))).appendTo(jCol);

											childCount++;
										}
										else
										{
											let jCol = $(child.jRow[0].children[col.ordinal]);
											jCol.empty();
											jCol.text(child.parsedRow[k]);
										}
									}

									row.visible = row.filterPos[k] >= 0 || childCount > 0;

									if (!row.visible)
										break;
								}
								else
								{
									// unhighlight selection
									if (row.filterPos[k] >= 0)
									{
										row.filterPos[k] = -1;
										row.filterLen[k] = 0;

										let jCol = $(row.jRow[0].children[col.ordinal + 1]);
										jCol.empty();
										jCol.text(row.parsedRow[k]);
									}
									for (let l = 0; l < row.children.length; l++)
									{
										let child = row.children[l];

										if (child.filterPos[k] >= 0)
										{
											child.filterPos[k] = -1;
											child.filterLen[k] = 0;

											let jCol = $(child.jRow[0].children[col.ordinal]);
											jCol.empty();
											jCol.text(child.parsedRow[k]);
										}
									}
								}
							}
						}

						UpdateTBody();
					}
				});
			}
			else
				$("<td></td>").appendTo(jFilterRow);
		}
	}

	UpdateTBody();
}