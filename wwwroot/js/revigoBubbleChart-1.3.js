
function BubbleChart(elementID, rawData, valueColumns, defaultColorColumn, defaultSizeColumn)
{
	// elementID serves as id of the enclosing div element and prefix for all other id's

	// Inspired by Vue.js way
	var vm=this;

	// Currently selected mappings for color and size
	vm.colorColumn=defaultColorColumn;
	vm.sizeColumn=defaultSizeColumn;

	// Chart div
	$("#"+elementID)
		.append($("<div>", {id: elementID+"-chart-div", style: "float:left"})
			.append($("<div>", {id: elementID+"-chart"})));

	// Menu selector divs
	$("#"+elementID)
		.append($("<div>", {id: elementID+"-menu-div", style: "float:left"}));

	// Size menu selector
	$("#"+elementID+"-menu-div")
		.append($("<div>")
			.attr("id", elementID+"-size")
			.css("margin", "5px")
			.css("padding", "5px")
			.css("border", "1px solid silver")
			.css("border-radius", "4px")
			.html("Bubble size: ")
			.append($("<select>", {id: elementID+"-size-selector"}))
			.append("</br>")
			.append($("<div>", {id: elementID+"-size-legend"}).css("margin", "5px auto 0 auto")));

	// Color menu selector
	$("#"+elementID+"-menu-div")
		.append($("<div>")
			.attr("id", elementID+"-color")
			.css("margin", "5px")
			.css("padding", "5px")
			.css("border", "1px solid silver")
			.css("border-radius", "4px")
			.html("Bubble color: ")
			.append($("<select>", {id: elementID+"-color-selector"}))
			.append("</br>")
			.append($("<div>", {id: elementID+"-color-legend"}).css("margin", "5px auto 0 auto")));

	let sizeSelector=$("#"+elementID+"-size-selector");
	let colorSelector=$("#"+elementID+"-color-selector");

	valueColumns.forEach(function(d)
	{
		if (d==defaultColorColumn)
		{
			sizeSelector
				.append($("<option>", {id: elementID+"-size-"+d, value: d, disabled: ""}).text(d));

			colorSelector
				.append($("<option>", {id: elementID+"-color-"+d, value: d, selected: ""}).text(d));
		}
		else if (d==defaultSizeColumn)
		{
			sizeSelector
				.append($("<option>", {id: elementID+"-size-"+d, value: d, selected: ""}).text(d));

			colorSelector
				.append($("<option>", {id: elementID+"-color-"+d, value: d, disabled: ""}).text(d));
		}
		else
		{
			sizeSelector
				.append($("<option>", {id: elementID+"-size-"+d, value: d}).text(d));

			colorSelector
				.append($("<option>", {id: elementID+"-color-"+d, value: d}).text(d));
		}
	});

	// GO term selector
	$("#"+elementID+"-menu-div")
		.append($("<div>")
			.attr("id", elementID+"-go-terms-selector")
			.css("height", "300px")
			.css("width", "175px")
			.css("overflow-y", "auto")
			.css("margin", "5px")
			.css("padding", "5px")
			.css("border", "1px solid silver")
			.css("border-radius", "4px"));

	var axisMinX=Number.MAX_VALUE;
	var axisMaxX=Number.MIN_VALUE;
	var axisMinY=Number.MAX_VALUE;
	var axisMaxY=Number.MIN_VALUE;

	// Fill in the GO term selector and axis min and max values
	rawData.forEach(function(d)
	{
		axisMinX=Math.min(axisMinX, d.PC_0);
		axisMaxX=Math.max(axisMaxX, d.PC_0);

		axisMinY=Math.min(axisMinY, d.PC_1);
		axisMaxY=Math.max(axisMaxY, d.PC_1);

		if (d["Selected"]!=0)
		{
			$("#"+elementID+"-go-terms-selector")
				.append($("<input>", {
					type: "checkbox",
					id: elementID+"go-term-checkbox"+d.ID,
					value: d.ID,
					checked: ""
				}))
				.append($("<label>", {for: elementID+"go-term-checkbox"+d.ID})
					.html("("+d["Term ID"]+") "+d.Name))
				.append($("</br>"));
		}
		else
		{
			$("#"+elementID+"-go-terms-selector")
				.append($("<input>", {
					type: "checkbox",
					id: elementID+"go-term-checkbox"+d.ID,
					value: d.ID
				}))
				.append($("<label>", {for: elementID+"go-term-checkbox"+d.ID})
					.html("("+d["Term ID"]+") "+d.Name))
				.append($("</br>"));
		}
	});

	// set the dimensions and margins of the graph
	var margin={top: 10, right: 20, bottom: 40, left: 50};
	var width=700-margin.left-margin.right;
	var height=620-margin.top-margin.bottom;
	var tooltipLineOffset=2; // distance from tooltip line to tooltip text
	var axisMargin=0.15; // 15% larger axes than the range of data

	// https://www.youtube.com/watch?v=XmVPHq4NhMA
	this.drawSizeLegend=function(containerID)
	{
		// Format number for compact representation:
		//      |d| < 0.01 : exponential format (2e-8)
		//      0.01 < |d| < 10.0 : two significant digits
		//      |d| > 10.0 : round to nearest integer value
		function formatNumber(d)
		{
			return Math.abs(d)>0.01?
				Math.abs(d)>=10.0?
					Math.round(d):d.toPrecision(2):
				d.toExponential(0);
		};

		var legendheight=80;
		var legendwidth=100;
		var margin={top: 10, right: 70, bottom: 10, left: 2};

		var legendContainer=d3.select("#"+containerID);

		legendContainer.selectAll("*").remove();

		var minScale=vm.sizeScale.domain()[0];
		var midScale=((vm.sizeScale.type=='linear')? (0.5*(vm.sizeScale.domain()[1]+vm.sizeScale.domain()[0])):Math.exp(0.5*(Math.log(vm.sizeScale.domain()[1])+Math.log(vm.sizeScale.domain()[0]))));
		var maxScale=vm.sizeScale.domain()[1];

		legendContainer
			.style("width", legendwidth+"px")
			.style("height", legendheight+"px")
			.style("position", "relative")

		var svg=legendContainer
			.append("svg")
			.attr("width", legendwidth+"px")
			.attr("height", legendheight+"px")
			.style("position", "absolute")
			.style("left", "0px")
			.style("top", "0px");

		var g1=svg
			.append("g")
			.attr("transform", "translate("+(legendwidth-margin.left-margin.right+3)+" "+(margin.top)+")");

		var g11=g1
			.append("g")
			.attr("transform", "translate(0 0)");

		g11
			.append("circle")
			.attr("r", vm.sizeScale(minScale))
			.attr("stroke", "black")
			.attr("fill", "white");

		g11
			.append("text")
			.attr("dy", "0.32em")
			.attr("x", "25")
			.text(formatNumber(minScale));

		var g12=g1
			.append("g")
			.attr("transform", "translate(0 20)");

		g12
			.append("circle")
			.attr('r', vm.sizeScale(midScale))
			.attr('stroke', 'black')
			.attr('fill', 'white');

		g12
			.append("text")
			.attr('dy', '0.32em')
			.attr('x', 25)
			.text(formatNumber(midScale));

		var g13=g1
			.append("g")
			.attr("transform", "translate(0 40)");

		g13
			.append("circle")
			.attr('r', vm.sizeScale(maxScale))
			.attr('stroke', 'black')
			.attr('fill', 'white');

		g13
			.append("text")
			.text(formatNumber(maxScale))
			.attr('dy', '0.32em')
			.attr('x', 25);
	};

	// Generate legend with continuous colors from a prespecified scale
	// Strangely, this is not built in D3 by default!?
	// http://bl.ocks.org/syntagmatic/e8ccca52559796be775553b467593a9f
	this.drawColorLegend=function(containerID)
	{
		var legendheight=100;
		var legendwidth=80;
		var margin={top: 5, right: 60, bottom: 10, left: 2};

		$("#"+containerID).empty();

		var canvas=d3.select("#"+containerID)
			.style("height", legendheight+"px")
			.style("width", legendwidth+"px")
			.style("position", "relative")
			.append("canvas")
			.attr("height", legendheight-margin.top-margin.bottom)
			.attr("width", 1)
			.style("height", (legendheight-margin.top-margin.bottom)+"px")
			.style("width", (legendwidth-margin.left-margin.right)+"px")
			.style("border", "1px solid #000")
			.style("position", "absolute")
			.style("top", (margin.top)+"px")
			.style("left", (margin.left)+"px")
			.node();

		var ctx=canvas.getContext("2d");

		var legendscale=((vm.colorScale.type=='linear')? d3.scaleLinear():d3.scaleLog()).range([1, legendheight-margin.top-margin.bottom]).domain(vm.colorScale.domain());

		// Generate image with continuous scale colors. If too slow see faster solution bellow!
		// http://bl.ocks.org/mbostock/048d21cf747371b11884f75ad896e5a5
		// http://stackoverflow.com/questions/4899799
		//       /whats-the-best-way-to-set-a-single-pixel-in-an-html5-canvas   
		d3.range(legendheight).forEach(function(i)
		{
			ctx.fillStyle=vm.colorScale(legendscale.invert(i));
			ctx.fillRect(0, i, 1, 1);
		});

		var legendaxis=d3.axisRight()
			.scale(legendscale)
			.tickSize(6)
			.ticks(4);

		var svg=d3.select("#"+containerID)
			.append("svg")
			.attr("height", legendheight+"px")
			.attr("width", legendwidth+"px")
			.style("position", "absolute")
			.style("left", "0px")
			.style("top", "0px");

		svg
			.append("g")
			.attr("class", "axis")
			.attr("transform", "translate("+(legendwidth-margin.left-margin.right+3)+" "+(margin.top)+")")
			.call(legendaxis);
	};

	// Tooltips are created as needed and hidden/revealed later
	// In this way we allow for multiple tooltips to be visible at the same time!
	this.createTooltipControls=function(d)
	{
		// Tolltip text is positioned outside of the data circle, with additional margin of 2% data range
		var textX=vm.xAxis(d.PC_0)+vm.sizeScale(d[vm.sizeColumn])+0.02*(vm.xAxis.range()[1]-vm.xAxis.range()[0]);
		var textY=vm.yAxis(d.PC_1)-vm.sizeScale(d[vm.sizeColumn])+0.02*(vm.yAxis.range()[1]-vm.yAxis.range()[0]);

		// Group for the tooltip - line, text and the surrounding rectangle
		d3.select("#"+elementID+"-tooltips")
			.append("g")
			.attr("id", elementID+"-tooltip-"+d.ID)
			.attr("class", "tooltip")
			.on("mousedown", dragTooltip);

		// Create tooltip text
		d3.select("#"+elementID+"-tooltip-"+d.ID)
			.append("text")
			.attr("class", "popuptext")
			.attr("id", elementID+"-text-"+d.ID)
			.attr("x", textX)
			.attr("y", textY)
			.text("("+d["Term ID"]+") "+d.Name);

		// Tooltip rectangle box surrounding the tooltip text
		var rect=d3.select("#"+elementID+"-text-"+d.ID).node().getBBox();

		d3.select("#"+elementID+"-tooltip-"+d.ID)
			.insert("rect", "#"+elementID+"-text-"+d.ID) // insert before tooltip text so that it is bellow
			.attr("class", "popuprect")
			.attr("id", elementID+"-rect-"+d.ID)
			.attr("x", rect.x-3)
			.attr("y", rect.y-3)
			.attr("width", rect.width+6)
			.attr("height", rect.height+6);

		var x2=textX-tooltipLineOffset;
		var y2=textY+tooltipLineOffset;
		var intersect=findIntersect([vm.xAxis(d.PC_0), vm.yAxis(d.PC_1)], [x2, y2], vm.sizeScale(d[vm.sizeColumn]));

		// Create tooltip line connecting data circle with the text box
		d3.select("#"+elementID+"-tooltip-"+d.ID)
			.insert("line", "#"+elementID+"-rect-"+d.ID) // insert before tooltip rectangle so that it is bellow
			.attr("class", "popupline")
			.attr("id", elementID+"-line-"+d.ID)
			.attr("x1", intersect[0])
			.attr("y1", intersect[1])
			.attr("x2", x2)
			.attr("y2", y2-5)
			.attr("stroke", "black");
	};

	this.updateTooltipVisibility=function(id, visible)
	{
		if (visible)
		{
			if (!$("#"+elementID+"-text-"+id).hasClass('show'))
			{
				$("#"+elementID+"-text-"+id).addClass('show');
			}
			if (!$("#"+elementID+"-line-"+id).hasClass('show'))
			{
				$("#"+elementID+"-line-"+id).addClass('show');
			}
			if (!$("#"+elementID+"-rect-"+id).hasClass('show'))
			{
				$("#"+elementID+"-rect-"+id).addClass('show');
			}
		}
		else
		{
			if ($("#"+elementID+"-text-"+id).hasClass('show'))
			{
				$("#"+elementID+"-text-"+id).removeClass('show');
			}
			if ($("#"+elementID+"-line-"+id).hasClass('show'))
			{
				$("#"+elementID+"-line-"+id).removeClass('show');
			}
			if ($("#"+elementID+"-rect-"+id).hasClass('show'))
			{
				$("#"+elementID+"-rect-"+id).removeClass('show');
			}
		}
	};

	// Dragging a tooltip updates the coordinates of tooltip based on the position of the mouse
	var dragTooltip=function(event)
	{
		// dragTooltip operates on the tooltip group so we have to extract the id of the GO term from it
		var GOterm=d3.select(this).attr("id").replace(elementID+"-tooltip-", "");

		// Offsets for text and rectangle so that dragging is smooth
		var textOffsetX=d3.select("#"+elementID+"-text-"+GOterm).attr('x')-d3.pointer(event)[0];
		var textOffsetY=d3.select("#"+elementID+"-text-"+GOterm).attr('y')-d3.pointer(event)[1];
		var rectOffsetX=d3.select("#"+elementID+"-rect-"+GOterm).attr('x')-d3.pointer(event)[0];
		var rectOffsetY=d3.select("#"+elementID+"-rect-"+GOterm).attr('y')-d3.pointer(event)[1];

		d3.select(this)
			.on("mousemove", function(event)
			{
				// Update position of the tooltip text
				d3.select("#"+elementID+"-text-"+GOterm)
					.attr("x", function(d)
					{
						return d3.pointer(event)[0]+textOffsetX;
					})
					.attr("y", function(d)
					{
						return d3.pointer(event)[1]+textOffsetY;
					});

				// Update the position of the connecting line
				var x1=Number(d3.select("#"+elementID+"-circle-"+GOterm).attr("cx"));
				var y1=Number(d3.select("#"+elementID+"-circle-"+GOterm).attr("cy"));
				var r=Number(d3.select("#"+elementID+"-circle-"+GOterm).attr("r"));
				var x2=d3.pointer(event)[0]+textOffsetX-tooltipLineOffset;
				var y2=d3.pointer(event)[1]+textOffsetY+tooltipLineOffset;

				// Connecting line will begin at data circle edge, not at the center!
				var intersect=findIntersect([x1, y1], [x2, y2], r);

				d3.select("#"+elementID+"-line-"+GOterm)
					.attr("x2", x2)
					.attr("y2", y2-5)
					.attr("x1", intersect[0])
					.attr("y1", intersect[1]);

				// Update the position of the surrounding rectangle
				d3.select("#"+elementID+"-rect-"+GOterm)
					.attr("x", d3.pointer(event)[0]+rectOffsetX)
					.attr("y", d3.pointer(event)[1]+rectOffsetY);

			})
			.on("mouseup", function()
			{
				d3.select(this).on("mousemove", null);
			});
	};

	this.updateSizeAndColorScale=function()
	{
		// Update min and max values
		var sizeMin=Number.MAX_VALUE;
		var sizeMax=Number.MIN_VALUE;
		var colorMin=Number.MAX_VALUE;
		var colorMax=Number.MIN_VALUE;

		rawData.forEach(function(d)
		{
			sizeMin=Math.min(sizeMin, d[vm.sizeColumn]);
			sizeMax=Math.max(sizeMax, d[vm.sizeColumn]);

			colorMin=Math.min(colorMin, d[vm.colorColumn]);
			colorMax=Math.max(colorMax, d[vm.colorColumn]);
		});

		// Use linear or log scale
		if (vm.sizeColumn.startsWith("Log"))
		{
			vm.sizeScale=d3.scaleLog()
			vm.sizeScale.type='log';
		}
		else
		{
			vm.sizeScale=d3.scaleLinear();
			vm.sizeScale.type='linear';
		}
		// Add a scale for bubble size
		vm.sizeScale.domain([sizeMin, sizeMax]).range([5, 15]);

		if (vm.colorColumn.startsWith("Log"))
		{
			vm.colorScale=d3.scaleLog()
			vm.colorScale.type='log';
		}
		else
		{
			vm.colorScale=d3.scaleLinear();
			vm.colorScale.type='linear';
		}
		// Add a scale for bubble color
		vm.colorScale.domain([colorMin, colorMax]).range(['red', 'yellow']);

		// Draw legend for bubble size
		vm.drawSizeLegend(elementID+"-size-legend");

		// Draw legend for bubble color
		vm.drawColorLegend(elementID+"-color-legend");
	};

	this.updateBubbles=function()
	{
		rawData.forEach(function(d)
		{
			$("#"+elementID+"-circle-"+d.ID)
				.attr("r", vm.sizeScale(d[vm.sizeColumn]))
				.css("fill", vm.colorScale(d[vm.colorColumn]));

			if ($("#"+elementID+"-text-"+d.ID).length>0)
			{
				let text=d3.select("#"+elementID+"-text-"+d.ID);
				let textX=text.attr("x");
				let textY=text.attr("y");
				let x2=textX-tooltipLineOffset;
				let y2=textY+tooltipLineOffset;
				let intersect=findIntersect([vm.xAxis(d.PC_0), vm.yAxis(d.PC_1)], [x2, y2], vm.sizeScale(d[vm.sizeColumn]));
				d3.select("#"+elementID+"-line-"+d.ID)
					.attr("x1", intersect[0])
					.attr("y1", intersect[1])
					.attr("x2", x2)
					.attr("y2", y2-5);
			}
		});
	};

	vm.updateSizeAndColorScale();

	// Draw the chart and append the svg object to the container
	var svg=d3.select("#"+elementID+"-chart")
		.append("svg")
		.attr("width", width+margin.left+margin.right)
		.attr("height", height+margin.top+margin.bottom)
		.style("font-size", "0.8em")
		.append("g")
		.attr("transform", "translate("+margin.left+" "+margin.top+")");

	// X axis scale
	vm.xAxis=d3.scaleLinear()
		.domain([axisMinX-axisMargin*(axisMaxX-axisMinX), axisMaxX+axisMargin*(axisMaxX-axisMinX)])
		.range([0, width]);

	// Add X axis
	svg.append("g")
		.attr("transform", "translate(0 "+height+")")
		.call(d3.axisBottom(vm.xAxis));

	// Add X axis label
	svg.append("text")
		.attr("text-anchor", "end")
		.attr("x", width)
		.attr("y", height+margin.top+20)
		.text("Semantic space X");

	// Y axis scale
	vm.yAxis=d3.scaleLinear()
		.domain([axisMinY-axisMargin*(axisMaxY-axisMinY), axisMaxY+axisMargin*(axisMaxY-axisMinY)])
		.range([height, 0]);

	// Add Y axis
	svg.append("g")
		.call(d3.axisLeft(vm.yAxis));

	// Add Y axis label
	svg.append("text")
		.attr("text-anchor", "end")
		.attr("transform", "rotate(-90)")
		.attr("y", -margin.left+20)
		.attr("x", -margin.top)
		.text("Semantic space Y")

	// Add data circles
	var bubbleContainer=svg.append('g')
		.attr("id", elementID+"-bubbles");

	rawData.forEach(function(d)
	{
		bubbleContainer
			.append("circle")
			.attr("class", "bubbles")
			.attr("id", elementID+"-circle-"+d.ID)
			.attr("cx", vm.xAxis(d.PC_0))
			.attr("cy", vm.yAxis(d.PC_1))
			.attr("r", vm.sizeScale(d[vm.sizeColumn]))
			.style("fill", vm.colorScale(d[vm.colorColumn]))
			.on("mouseover", function()
			{
				if ($("#"+elementID+"-text-"+d.ID).length==0)
				{
					vm.createTooltipControls(d);
				}

				vm.updateTooltipVisibility(d.ID, true);
			})
			.on("mouseleave", function() {vm.updateTooltipVisibility(d.ID, d.Selected!=0);})
			.on("click", function()
			{
				if ($("#"+elementID+"-text-"+d.ID).length==0)
				{
					vm.createTooltipControls(d);
				}

				d.Selected=(d.Selected!=0)? 0:1;

				$("#"+elementID+"go-term-checkbox"+d.ID)[0].checked=d.Selected!=0;

				vm.updateTooltipVisibility(d.ID, d.Selected!=0);
			});
	});

	// Add a group for tooltips - it needs to be bellow data group so that labels are visible!
	svg.append("g")
		.attr("id", elementID+"-tooltips");

	// Add checkbox, color and size events at the end
	rawData.forEach(function(d)
	{
		$("#"+elementID+"go-term-checkbox"+d.ID)
			.on("change", function()
			{
				if ($("#"+elementID+"-text-"+d.ID).length==0)
				{
					vm.createTooltipControls(d);
				}

				d.Selected=(d.Selected!=0)? 0:1;
				vm.updateTooltipVisibility(d.ID, d.Selected!=0);
			});

		if (d.Selected!=0)
		{
			vm.createTooltipControls(d);
			vm.updateTooltipVisibility(d.ID, true);
		}
	});

	$("#"+elementID+"-size-selector").on("change", function()
	{
		let select=$("#"+elementID+"-size-selector");

		if (select[0].value!=vm.colorColumn)
		{
			$("#"+elementID+"-color-"+vm.sizeColumn).removeAttr("disabled");
			vm.sizeColumn=select[0].value;
			$("#"+elementID+"-color-"+vm.sizeColumn).attr("disabled", "");

			vm.updateSizeAndColorScale();
			vm.updateBubbles();
		}
	});

	$("#"+elementID+"-color-selector").on("change", function()
	{
		let select=$("#"+elementID+"-color-selector");

		if (select[0].value!=vm.sizeColumn)
		{
			$("#"+elementID+"-size-"+vm.colorColumn).removeAttr("disabled");
			vm.colorColumn=select[0].value;
			$("#"+elementID+"-size-"+vm.colorColumn).attr("disabled", "");

			vm.updateSizeAndColorScale();
			vm.updateBubbles();
		}
	});
}

// Find the intersect between a line and circle of radius r, if line starts at the circle center
// (coordinates x) and ends at coordinates y
function findIntersect(x, y, r)
{
	var M=(y[1]-x[1])/(y[0]-x[0]);
	var signX=Math.sign(y[0]-x[0]);
	var signY=Math.sign(y[1]-x[1]);
	return [x[0]+signX*(r/Math.sqrt(1+M*M)), x[1]+signY*(r/Math.sqrt(1+1/(M*M)))];
}

