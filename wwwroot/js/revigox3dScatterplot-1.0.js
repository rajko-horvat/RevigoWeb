// elementID serves as id of the enclosing div element and prefix for all other id's
//
// Stupid X3D standard is case sensitive, which doesn't make any sense to me!
// For export function output everything to exportOutput property string
function x3dScatterplot(elementID, width, height, data, optionValues, defaultSizeOption, defaultColorOption, exportX3D)
{
	// Inspired by Vue.js way
	var vm = this;
	vm.X3D = "";
	var x3dScene;
	var mainContainer;

	// 3D constants
	var axisDirectionVectors = { x: [1.0, 0.0, 0.0], y: [0.0, 1.0, 0.0], z: [0.0, 0.0, 1.0] };
	var axisRotationVectors = { x: [1.0, 1.0, 0.0, Math.PI], y: [0.0, 0.0, 0.0, 0.0], z: [0.0, 1.0, 1.0, Math.PI] };
	var axisSize = 100.0;
	var axisMargin = 5.0;
	var axisRadius = 0.2;
	var nTicks = 10.0;
	var tickRadius = 0.05;
	var bubbleSizeFrom = 0.2;
	var bubbleSizeTo = 1.2;
	var fontSize = 1.0;
	var centerOfRotation = [axisSize / 2.0, axisSize / 2.0, axisSize / 2.0];
	var viewPosition = [axisSize * 1.5, axisSize / 3.0, axisSize * 1.5];
	var viewOrientation = [0.0, 1.0, 0.0, Math.PI / 4.0];
	var fieldOfView = 1.0;

	function Draw3DScatterplot()
	{
		// determine minimum and maximum coordinate values
		let xMin = Number.MAX_VALUE;
		let xMax = Number.MIN_VALUE;
		let yMin = Number.MAX_VALUE;
		let yMax = Number.MIN_VALUE;
		let zMin = Number.MAX_VALUE;
		let zMax = Number.MIN_VALUE;

		for (let i = 0; i < vm.chartData.length; i++)
		{
			let item = vm.chartData[i];
			let x = item["PC3_0"];
			let y = item["PC3_1"];
			let z = item["PC3_2"];

			xMin = Math.min(xMin, x);
			xMax = Math.max(xMax, x);
			yMin = Math.min(yMin, y);
			yMax = Math.max(yMax, y);
			zMin = Math.min(zMin, z);
			zMax = Math.max(zMax, z);
		}

		// axes ranges
		let xRange = xMax - xMin;
		let yRange = yMax - yMin;
		let zRange = zMax - zMin;

		// scale axes, but preserve proportions
		let maxRange = Math.max(xRange, yRange, zRange);
		let axisMulti = (axisSize - axisMargin - axisMargin) / maxRange;

		// center axes
		let xCenter = (axisSize - xRange * axisMulti) / 2.0;
		let yCenter = (axisSize - yRange * axisMulti) / 2.0;
		let zCenter = (axisSize - zRange * axisMulti) / 2.0;

		// draw axes
		if (!exportX3D)
		{
			let tickSize = axisSize / nTicks;

			// axes group
			let axesGroup = x3dScene
				.append("Group")
				.attr("id", elementID + "_Axes");

			// x axis, tick direction z, colour blue
			let xGroup = axesGroup
				.append("Group")
				.attr("id", elementID + "_xAxis");

			let axisShape = xGroup
				.append("Transform")
				.attr("id", elementID + "_xAxisDomain").attr("rotation", axisRotationVectors["x"].join(" "))
				.attr("translation", axisDirectionVectors["x"].map(function (d) { return d * axisSize / 2.0; }).join(" "))
				.append("Shape");

			axisShape
				.append("Cylinder").attr("radius", axisRadius).attr("height", axisSize);
			axisShape
				.append("Appearance").append("Material").attr("diffuseColor", "0.0 0.0 1.0");

			for (let axisPos = 0.0, i = 0; axisPos < axisSize; axisPos += tickSize, i++)
			{
				let tickShape = xGroup
					.append("Transform").attr("id", elementID + "_xAxisTick" + i)
					.attr("translation", axisDirectionVectors["x"].map(function (d) { return (tickSize + axisPos) * d; }).join(" "))
					.append("Transform")
					.attr("translation", axisDirectionVectors["z"].map(function (d) { return d * axisSize / 2.0; }).join(" "))
					.attr("rotation", axisRotationVectors["z"].join(" "))
					.append("Shape");

				tickShape
					.append("Cylinder").attr("radius", tickRadius).attr("height", axisSize);
				tickShape
					.append("Appearance").append("Material").attr("diffuseColor", "0.9 0.9 0.9");
			}

			// y axis, tick direction x and z, colour red
			let yGroup = axesGroup.append("Group")
				.attr("id", elementID + "_yAxis");

			let yShape = yGroup
				.append("Transform").attr("id", elementID + "_yAxisDomain").attr("rotation", axisRotationVectors["y"].join(" "))
				.attr("translation", axisDirectionVectors["y"].map(function (d) { return d * axisSize / 2.0; }).join(" "))
				.append("Shape");

			yShape.append("Cylinder").attr("radius", axisRadius).attr("height", axisSize);
			yShape.append("Appearance").append("Material").attr("diffuseColor", "1.0 0.0 0.0");

			// we want y axis ticks to be oriented to x and z side
			for (let axisPos = 0.0, i = 0; axisPos < axisSize; axisPos += tickSize, i++)
			{
				let tickShape = yGroup
					.append("Transform").attr("id", elementID + "_yAxisTick" + i)
					.attr("translation", axisDirectionVectors["y"].map(function (d) { return (tickSize + axisPos) * d; }).join(" "))
					.append("Transform")
					.attr("translation", axisDirectionVectors["x"].map(function (d) { return d * axisSize / 2.0; }).join(" "))
					.attr("rotation", axisRotationVectors["x"].join(" "))
					.append("Shape");

				tickShape.append("Cylinder").attr("radius", tickRadius).attr("height", axisSize);
				tickShape.append("Appearance").append("Material").attr("diffuseColor", "0.9 0.9 0.9");

				tickShape = yGroup
					.append("Transform").attr("id", elementID + "_yAxisTick" + (nTicks + i))
					.attr("translation", axisDirectionVectors["y"].map(function (d) { return (tickSize + axisPos) * d; }).join(" "))
					.append("Transform")
					.attr("translation", axisDirectionVectors["z"].map(function (d) { return d * axisSize / 2.0; }).join(" "))
					.attr("rotation", axisRotationVectors["z"].join(" "))
					.append("Shape");

				tickShape.append("Cylinder").attr("radius", tickRadius).attr("height", axisSize);
				tickShape.append("Appearance").append("Material").attr("diffuseColor", "0.9 0.9 0.9");
			}

			// z axis, tick direction x, colour black
			let zGroup = axesGroup.append("Group")
				.attr("id", elementID + "_zAxis");

			let zShape = zGroup
				.append("Transform").attr("id", elementID + "_zAxisDomain").attr("rotation", axisRotationVectors["z"].join(" "))
				.attr("translation", axisDirectionVectors["z"].map(function (d) { return d * axisSize / 2.0; }).join(" "))
				.append("Shape");

			zShape.append("Cylinder").attr("radius", axisRadius).attr("height", axisSize);
			zShape.append("Appearance").append("Material").attr("diffuseColor", "0.0 0.0 0.0");

			for (let axisPos = 0.0, i = 0; axisPos < axisSize; axisPos += tickSize, i++)
			{
				let tickShape = zGroup
					.append("Transform").attr("id", elementID + "_zAxisTick" + i)
					.attr("translation", axisDirectionVectors["z"].map(function (d) { return (tickSize + axisPos) * d; }).join(" "))
					.append("Transform")
					.attr("translation", axisDirectionVectors["x"].map(function (d) { return d * axisSize / 2.0; }).join(" "))
					.attr("rotation", axisRotationVectors["x"].join(" "))
					.append("Shape");

				tickShape.append("Cylinder").attr("radius", tickRadius).attr("height", axisSize);
				tickShape.append("Appearance").append("Material").attr("diffuseColor", "0.9 0.9 0.9");
			}
		}
		else
		{
			let tickSize = axisSize / nTicks;

			// axes group
			vm.X3D += "<Group>\r\n";

			// x axis, tick direction z, colour blue
			vm.X3D += "<Group>\r\n"
			vm.X3D += "<Transform rotation='" + axisRotationVectors["x"].join(" ") + "'" +
				" translation='" + axisDirectionVectors["x"].map(function (d) { return d * axisSize / 2.0; }).join(" ") + "'>\r\n";
			vm.X3D += "<Shape>\r\n";
			vm.X3D += "<Cylinder radius='" + axisRadius + "' height='" + axisSize + "'></Cylinder>\r\n";
			vm.X3D += "<Appearance><Material diffuseColor='0.0 0.0 1.0'></Material></Appearance>\r\n";
			vm.X3D += "</Shape>\r\n";
			vm.X3D += "</Transform>\r\n";

			for (let axisPos = 0.0, i = 0; axisPos < axisSize; axisPos += tickSize, i++)
			{
				vm.X3D += "<Transform translation='" + axisDirectionVectors["x"].map(function (d) { return (tickSize + axisPos) * d; }).join(" ") + "'>\r\n"
				vm.X3D += "<Transform translation='" + axisDirectionVectors["z"].map(function (d) { return d * axisSize / 2.0; }).join(" ") + "'" +
					" rotation='" + axisRotationVectors["z"].join(" ") + "'>\r\n";
				vm.X3D += "<Shape>\r\n";
				vm.X3D += "<Cylinder radius='" + tickRadius + "' height='" + axisSize + "'></Cylinder>\r\n";
				vm.X3D += "<Appearance><Material diffuseColor='0.9 0.9 0.9'></Material></Appearance>\r\n";
				vm.X3D += "</Shape>\r\n";
				vm.X3D += "</Transform>\r\n";
				vm.X3D += "</Transform>\r\n";
			}
			vm.X3D += "</Group>\r\n"

			// y axis, tick direction x and z, colour red
			vm.X3D += "<Group>\r\n"
			vm.X3D += "<Transform rotation='" + axisRotationVectors["y"].join(" ") + "'" +
				" translation='" + axisDirectionVectors["y"].map(function (d) { return d * axisSize / 2.0; }).join(" ") + "'>\r\n";
			vm.X3D += "<Shape>\r\n";
			vm.X3D += "<Cylinder radius='" + axisRadius + "' height='" + axisSize + "'></Cylinder>\r\n";
			vm.X3D += "<Appearance><Material diffuseColor='1.0 0.0 0.0'></Material></Appearance>\r\n";
			vm.X3D += "</Shape>\r\n";
			vm.X3D += "</Transform>\r\n";

			// we want y axis ticks to be oriented to x and z side
			for (let axisPos = 0.0, i = 0; axisPos < axisSize; axisPos += tickSize, i++)
			{
				vm.X3D += "<Transform translation='" + axisDirectionVectors["y"].map(function (d) { return (tickSize + axisPos) * d; }).join(" ") + "'>\r\n"
				vm.X3D += "<Transform translation='" + axisDirectionVectors["x"].map(function (d) { return d * axisSize / 2.0; }).join(" ") + "'" +
					" rotation='" + axisRotationVectors["x"].join(" ") + "'>\r\n";
				vm.X3D += "<Shape>\r\n";
				vm.X3D += "<Cylinder radius='" + tickRadius + "' height='" + axisSize + "'></Cylinder>\r\n";
				vm.X3D += "<Appearance><Material diffuseColor='0.9 0.9 0.9'></Material></Appearance>\r\n";
				vm.X3D += "</Shape>\r\n";
				vm.X3D += "</Transform>\r\n";
				vm.X3D += "</Transform>\r\n";

				vm.X3D += "<Transform translation='" + axisDirectionVectors["y"].map(function (d) { return (tickSize + axisPos) * d; }).join(" ") + "'>\r\n"
				vm.X3D += "<Transform translation='" + axisDirectionVectors["z"].map(function (d) { return d * axisSize / 2.0; }).join(" ") + "'" +
					" rotation='" + axisRotationVectors["z"].join(" ") + "'>\r\n";
				vm.X3D += "<Shape>\r\n";
				vm.X3D += "<Cylinder radius='" + tickRadius + "' height='" + axisSize + "'></Cylinder>\r\n";
				vm.X3D += "<Appearance><Material diffuseColor='0.9 0.9 0.9'></Material></Appearance>\r\n";
				vm.X3D += "</Shape>\r\n";
				vm.X3D += "</Transform>\r\n";
				vm.X3D += "</Transform>\r\n";
			}
			vm.X3D += "</Group>\r\n"

			// z axis, tick direction x, colour black
			vm.X3D += "<Group>\r\n"
			vm.X3D += "<Transform rotation='" + axisRotationVectors["z"].join(" ") + "'" +
				" translation='" + axisDirectionVectors["z"].map(function (d) { return d * axisSize / 2.0; }).join(" ") + "'>\r\n";
			vm.X3D += "<Shape>\r\n";
			vm.X3D += "<Cylinder radius='" + axisRadius + "' height='" + axisSize + "'></Cylinder>\r\n";
			vm.X3D += "<Appearance><Material diffuseColor='0.0 0.0 0.0'></Material></Appearance>\r\n";
			vm.X3D += "</Shape>\r\n";
			vm.X3D += "</Transform>\r\n";

			for (let axisPos = 0.0, i = 0; axisPos < axisSize; axisPos += tickSize, i++)
			{
				vm.X3D += "<Transform translation='" + axisDirectionVectors["z"].map(function (d) { return (tickSize + axisPos) * d; }).join(" ") + "'>\r\n"
				vm.X3D += "<Transform translation='" + axisDirectionVectors["x"].map(function (d) { return d * axisSize / 2.0; }).join(" ") + "'" +
					" rotation='" + axisRotationVectors["x"].join(" ") + "'>\r\n";
				vm.X3D += "<Shape>\r\n";
				vm.X3D += "<Cylinder radius='" + tickRadius + "' height='" + axisSize + "'></Cylinder>\r\n";
				vm.X3D += "<Appearance><Material diffuseColor='0.9 0.9 0.9'></Material></Appearance>\r\n";
				vm.X3D += "</Shape>\r\n";
				vm.X3D += "</Transform>\r\n";
				vm.X3D += "</Transform>\r\n";
			}
			vm.X3D += "</Group>\r\n"

			// close axes group
			vm.X3D += "</Group>\r\n"
		}

		//  min, max and range for bubble size and color
		let bubbleSizeMin = valueRanges[vm.selectedSizeOption + "_Min"];
		let bubbleSizeRange = valueRanges[vm.selectedSizeOption + "_Range"];
		let bubbleSizeMulti = (bubbleSizeTo - bubbleSizeFrom) / bubbleSizeRange;

		let bubbleColorMin = valueRanges[vm.selectedColorOption + "_Min"];
		let bubbleColorRange = valueRanges[vm.selectedColorOption + "_Range"];
		let bubbleColorMulti = 1.0 / bubbleColorRange;

		// draw bubbles and labels
		let bubbleGroup;

		// bubble group
		if (!exportX3D)
		{
			bubbleGroup = x3dScene.append("group")
				.attr("id", elementID + "_Bubbles");
		}
		else
		{
			vm.X3D += "<Group>\r\n"
		}

		for (let i = 0; i < vm.chartData.length; i++)
		{
			let item = vm.chartData[i];
			let x = item["PC3_0"];
			let y = item["PC3_1"];
			let z = item["PC3_2"];
			let size = item[vm.selectedSizeOption];
			let color = item[vm.selectedColorOption];
			let bubbleRadius = bubbleSizeFrom + (size - bubbleSizeMin) * bubbleSizeMulti;
			let colorRGB = d3.rgb(d3.interpolateRdYlGn((color - bubbleColorMin) * bubbleColorMulti));

			x -= xMin;
			x *= axisMulti;
			x += xCenter;

			y -= yMin;
			y *= axisMulti;
			y += yCenter;

			z -= zMin;
			z *= axisMulti;
			z += zCenter;

			if (!exportX3D)
			{
				let bubbleShape = bubbleGroup
					.append("Transform").attr("id", elementID + "_Bubble" + i).attr("translation", x + " " + y + " " + z)
					.append("Shape");
				bubbleShape
					.attr("onmouseover", "handle3DSMouseOver(event,\"" + elementID + "\"," + i + ")")
					.attr("onmouseout", "handle3DSMouseOut(event,\"" + elementID + "\"," + i + ")");

				bubbleShape.append("Sphere").attr("id", elementID + "_BubbleSphere" + i).attr("radius", bubbleRadius);
				bubbleShape.append("Appearance").append("Material").attr("id", elementID + "_BubbleMaterial" + i).attr("diffuseColor", (colorRGB["r"] / 255.0) + " " + (colorRGB["g"] / 255.0) + " " + (colorRGB["b"] / 255.0));

				CreateToolTip(i, "(" + item["Term ID"] + ") " + item["Name"], 11);
			}
			else
			{
				vm.X3D += "<Transform translation='" + x + " " + y + " " + z + "'>\r\n";
				vm.X3D += "<Shape>\r\n";
				vm.X3D += "<Sphere radius='" + bubbleRadius + "'></Sphere>\r\n";
				vm.X3D += "<Appearance><Material diffuseColor='" + (colorRGB["r"] / 255.0) + " " + (colorRGB["g"] / 255.0) + " " + (colorRGB["b"] / 255.0) +
					"'></Material></Appearance>\r\n";
				vm.X3D += "</Shape>\r\n";
				vm.X3D += "</Transform>\r\n";

				vm.X3D += "<Transform translation='" + x + " " + y + " " + z + "'>\r\n";
				vm.X3D += "<Billboard axisOfRotation='0.0 0.0 0.0'>\r\n";
				vm.X3D += "<Transform translation='" + bubbleRadius + " " + (-bubbleRadius) + " " + bubbleRadius + "'>\r\n";
				vm.X3D += "<Shape>\r\n";
				let text = ("(" + item["Term ID"] + ") " + item["Name"]).replaceAll("'", "&apos;").replaceAll('"', "&quot;");
				vm.X3D += "<Text string='\"" + text + "\"'>" +
					"<FontStyle size='" + fontSize + "'></FontStyle></Text>\r\n";
				vm.X3D += "<Appearance><Material diffuseColor='0.2 0.2 0.2'></Material></Appearance>\r\n";
				vm.X3D += "</Shape>\r\n";
				vm.X3D += "</Transform>\r\n";
				vm.X3D += "</Billboard>\r\n";
				vm.X3D += "</Transform>\r\n";
			}
		}

		// close bubble group
		if (exportX3D)
		{
			vm.X3D += "</Group>\r\n"
		}
	}

	function CreateToolTip(id, value, fontSize)
	{
		mainContainer
			.append("div")
			.attr("id", elementID + "_Tooltip" + id)
			.style("display", "none")
			.style("position", "absolute")
			.style("top", "0px")
			.style("left", "0px")
			.style("max-width", "250px")
			.style("padding", "5px")
			.style("border", "1px solid gray")
			.style("border-radius", "4px")
			.style("background", "ivory")
			.style("opacity", "0.85")
			.style("font-size", fontSize + "px")
			.append("p")
			.style("margin", "auto 0px auto 0px")
			.text(value);
	}

	function UpdateBubbleSize()
	{
		let bubbleSizeMin = valueRanges[vm.selectedSizeOption + "_Min"];
		let bubbleSizeRange = valueRanges[vm.selectedSizeOption + "_Range"];
		let bubbleSizeMulti = (bubbleSizeTo - bubbleSizeFrom) / bubbleSizeRange;

		for (let i = 0; i < vm.chartData.length; i++)
		{
			let item = vm.chartData[i];
			let size = item[vm.selectedSizeOption];
			let bubbleRadius = bubbleSizeFrom + (size - bubbleSizeMin) * bubbleSizeMulti;

			d3.select("#" + elementID + "_BubbleSphere" + i).attr("radius", bubbleRadius);
		}
	}

	function UpdateBubbleColor()
	{
		let bubbleColorMin = valueRanges[vm.selectedColorOption + "_Min"];
		let bubbleColorRange = valueRanges[vm.selectedColorOption + "_Range"];
		let bubbleColorMulti = 1.0 / bubbleColorRange;

		for (let i = 0; i < vm.chartData.length; i++)
		{
			let item = vm.chartData[i];
			let color = item[vm.selectedColorOption];

			let colorRGB = d3.rgb(d3.interpolateRdYlGn((color - bubbleColorMin) * bubbleColorMulti));
			d3.select("#" + elementID + "_BubbleMaterial" + i).attr("diffuseColor", (colorRGB["r"] / 255.0) + " " + (colorRGB["g"] / 255.0) + " " + (colorRGB["b"] / 255.0));
		}
	}

	// render 3D ScatterPlot
	vm.chartData = data;
	vm.width = width;
	vm.height = height;

	// selected size and color values
	vm.selectedSizeOption = defaultSizeOption;
	vm.selectedColorOption = defaultColorOption;

	// calculate minimum, maximum and range for values
	var valueRanges = {};

	// define names
	for (let i = 0; i < optionValues.length; i++)
	{
		let valueName = optionValues[i];

		valueRanges[valueName + "_Min"] = Number.MAX_VALUE;
		valueRanges[valueName + "_Max"] = Number.MIN_VALUE;
		valueRanges[valueName + "_Range"] = 0.0;
	}

	// iterate through vm.chartData and determine minimum and maximum values
	for (let i = 0; i < vm.chartData.length; i++)
	{
		let dataItem = vm.chartData[i];

		for (let j = 0; j < optionValues.length; j++)
		{
			let valueName = optionValues[j];
			valueRanges[valueName + "_Min"] = Math.min(valueRanges[valueName + "_Min"], dataItem[valueName]);
			valueRanges[valueName + "_Max"] = Math.max(valueRanges[valueName + "_Max"], dataItem[valueName]);
		}
	}

	// and finally determine value ranges
	for (let i = 0; i < optionValues.length; i++)
	{
		let valueName = optionValues[i];

		valueRanges[valueName + "_Range"] = valueRanges[valueName + "_Max"] - valueRanges[valueName + "_Min"];
	}

	if (!exportX3D)
	{
		mainContainer = d3.select("#" + elementID)
			.style("position", "relative");

		let padding = 10;
		let optionContainer = mainContainer.append("div")
			.attr("id", elementID + "_ChartOptions")
			.style("width", width - padding * 2 + "px")
			.style("border", "1px solid gray").style("border-radius", "4px")
			.style("padding", padding + "px").style("margin-bottom", "10px");

		let chartContainer = mainContainer.append("div")
			.attr("id", elementID + "_Chart")
			.style("width", width + "px").style("height", height + "px")
			.style("border", "1px solid gray");

		// append options
		let optionSize = optionContainer
			.append("span").style("margin-right", "40px").text("Bubble size: ")
			.append("select").attr("id", elementID + "_SizeOption")
			.on("change", function (event)
			{
				let newValue = parseInt(event.target.value);
				let oldValue = 0;
				for (let i = 0; i < optionValues.length; i++)
				{
					if (optionValues[i] === vm.selectedSizeOption)
					{
						oldValue = i;
						break;
					}
				}

				if (!isNaN(newValue) && newValue >= 0 && newValue < optionValues.length)
				{
					d3.select("#" + elementID + "_ColorOption" + oldValue).attr("disabled", null);
					d3.select("#" + elementID + "_ColorOption" + newValue).attr("disabled", 1);

					vm.selectedSizeOption = optionValues[newValue];
					UpdateBubbleSize();
					//console.log("Changed 1 to " + value);
				}
				else
				{
					event.target.value = oldValue;
				}
			});

		// fill size options
		for (let i = 0; i < optionValues.length; i++)
		{
			let temp = optionSize.append("option").attr("id", elementID + "_SizeOption" + i).attr("value", i).text(optionValues[i]);
			if (optionValues[i] === vm.selectedSizeOption)
			{
				temp.attr("selected", "1");
			}
			if (optionValues[i] === vm.selectedColorOption)
			{
				temp.attr("disabled", "1");
			}
		}

		let spanColor = optionContainer
			.append("span").text("Bubble color: ");

		let optionColor = spanColor
			.append("select").attr("id", elementID + "_ColorOption")
			.on("change", function (event)
			{
				let newValue = parseInt(event.target.value);
				let oldValue = 0;
				for (let i = 0; i < optionValues.length; i++)
				{
					if (optionValues[i] === vm.selectedColorOption)
					{
						oldValue = i;
						break;
					}
				}

				if (!isNaN(newValue) && newValue >= 0 && newValue < optionValues.length)
				{
					d3.select("#" + elementID + "_SizeOption" + oldValue).attr("disabled", null);
					d3.select("#" + elementID + "_SizeOption" + newValue).attr("disabled", 1);

					vm.selectedColorOption = optionValues[newValue];
					UpdateBubbleColor();
				}
				else
				{
					event.target.value = oldValue;
				}
			});

		spanColor
			.append("img").attr("id", elementID + "_ColorOptionTip")
			.attr("title", "The bubble color ranges from Red (smallest values) through Yellow to Green (largest values)")
			.attr("src", "Images/dialog-information.png").style("vertical-align", "bottom");

		$("#" + elementID + "_ColorOptionTip").tooltip();

		// fill color options
		for (let i = 0; i < optionValues.length; i++)
		{
			let temp = optionColor.append("option").attr("id", elementID + "_ColorOption" + i).attr("value", i).text(optionValues[i]);
			if (optionValues[i] === vm.selectedColorOption)
			{
				temp.attr("selected", "1");
			}
			if (optionValues[i] === vm.selectedSizeOption)
			{
				temp.attr("disabled", "1");
			}
		}

		// append the x3d object to the container div
		x3dScene = chartContainer
			.append("X3D")
			.attr("id", elementID + "_x3d")
			.attr("width", width + "px")
			.attr("height", height + "px")
			.attr("showLog", "false")
			.attr("showStat", "false")
			.attr("useGeoCache", "false")
			.append("Scene");

		// Disable gamma correction
		x3dScene.append("Environment")
			.attr("gammaCorrectionDefault", "none");

		// Add a white background
		x3dScene.append("Background")
			.attr("groundColor", "1 1 1")
			.attr("skyColor", "1 1 1");

		// set initial view point
		x3dScene.append("Viewpoint")
			.attr("id", elementID + "_Viewpoint")
			.attr("centerOfRotation", centerOfRotation.join(" "))
			.attr("position", viewPosition.join(" "))
			.attr("orientation", viewOrientation.join(" "))
			.attr("fieldOfView", fieldOfView);
		//.attr("set_bind", "true");
	}
	else
	{
		vm.X3D += "<X3D profile='Immersive' version='3.3' xmlns:xsd='http://www.w3.org/2001/XMLSchema-instance'" +
			" xsd:noNamespaceSchemaLocation='http://www.web3d.org/specifications/x3d-3.3.xsd'>\r\n";
		vm.X3D += "<Scene>\r\n";

		// Disable gamma correction, not supported in standalone X3D viewers
		//vm.X3D += "<Environment gammaCorrectionDefault='none'></Environment>\r\n";

		// Add a white background
		vm.X3D += "<Background groundColor='1.0 1.0 1.0' skyColor='1.0 1.0 1.0'></Background>\r\n";

		// set initial view point
		vm.X3D += "<Viewpoint" +
			" centerOfRotation='" + centerOfRotation.join(" ") + "'" +
			" position='" + viewPosition.join(" ") + "'" +
			" orientation='" + viewOrientation.join(" ") + "'" +
			" fieldOfView='" + fieldOfView + "'></Viewpoint>\r\n";
	}

	Draw3DScatterplot();

	if (exportX3D)
	{
		vm.X3D += "</Scene>\r\n";
		vm.X3D += "</X3D>\r\n";
	}
}

function handle3DSMouseOver(event,elementID,id)
{
	let padding = 4;
	//let wscrX=$(window).scrollLeft();
	//let wscrY=$(window).scrollTop();
	let offset = $("#" + elementID).offset();
	let offset1 = $("#" + elementID + "_Chart").offset();
	let xPos = offset1.left - offset.left + event.layerX + padding * 2;
	let yPos = offset1.top - offset.top + event.layerY + padding * 2;

	let toolTip = d3.select("#" + elementID + "_Tooltip" + id);
	toolTip.style("top", yPos+"px");
	toolTip.style("left", xPos+"px");
	toolTip.style("display", "block");
}

function handle3DSMouseOut(event,elementID,id)
{
	$("#"+elementID+"_Tooltip"+id).css("display", "none");
}