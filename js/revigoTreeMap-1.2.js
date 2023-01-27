// elementID serves as id of the enclosing div element and prefix for all other id's
function TreeMap(elementID, treeData, fontSize, showValue, exportSvg)
{
    // Inspired by Vue.js way
    var vm = this;
	
	this.AddTextToContainer=function(container, text, yPos, yOffset, fontSize, maxLines, maxWidth, maxHeight)
	{
		var lineCount=0;
		var blockHeight=0;
		var blockWidth=0;
		
		if(text.length==0 || (maxHeight>0 && yPos+fontSize>maxHeight))
			return {width:blockWidth, height:blockHeight};
			
		while(true)
		{				
			var lastLine=(maxLines>0 && lineCount+1>=maxLines) || (maxHeight>0 && yPos+(fontSize+2)*2>=maxHeight);

			// split text
			var spacePos=0;
			var textSize=vm.getTextSize(text, fontSize);
			if(textSize.width>=maxWidth)
			{
				while(true)
				{
					var temp=text.indexOf(' ', spacePos);
					if(temp<0)
						break;
					var tempSize=vm.getTextSize(text.substring(0, temp), fontSize);
					if(tempSize.width>maxWidth)
						break;
					spacePos=temp+1;
					textSize=tempSize;
				}
				if(spacePos<=1)
				{
					// treat this line as last line
					lastLine=true;
					spacePos=text.length+1;
					textSize=vm.getTextSize(text, fontSize);
				}
			}
			else
			{
				lastLine=true;
			}
			
			var lineText=text;
			if(lastLine)
			{
				// does line fit into maxWidth
				if(textSize.width>maxWidth || spacePos>1)
				{
					if(spacePos>1)
					{
						lineText=text.substring(0, spacePos-1);
						textSize=vm.getTextSize(lineText+"...", fontSize);
					}
					// if it's last line add ellipsis
					while(textSize.width>maxWidth)
					{
						lineText=lineText.substring(0, lineText.length-1);
						textSize=vm.getTextSize(lineText+"...", fontSize);
					}
					lineText+="...";
				}
			}
			else
			{
				lineText=text.substring(0, spacePos-1);
				text=text.substring(spacePos);
				textSize=vm.getTextSize(lineText, fontSize);
			}
			
			if(lineCount==0 && lastLine)
			{
				// we want to skip rendering of text that's smaller than 3 characters and ends with ellipsis
				if(lineText.endsWith("..."))
				{
					 if(lineText.length<6)
						break;
				}
			}
			
			blockWidth=Math.max(blockWidth, textSize.width);
			
			var textSpan=container
				.append("tspan")
					.attr("x", container.attr("x")+"px")
					.attr("dy", ((lineCount==0)?yOffset:fontSize+2)+"px")
					.attr("font-size", fontSize+"px")
					.text(lineText);
						
			yPos+=fontSize+2;
			blockHeight+=fontSize+(lineCount>0?2:0);
			lineCount++;
			
			if(lastLine || text.length==0)
				break;
		}
						
		return {width:blockWidth, height:blockHeight};
	}
	
	this.getTextSize=function(text, fontSize)
	{
		var container=d3.select("#"+elementID+"_TextSizeTemp");
		container
			.attr("font-size", fontSize+"px")
			.text(text);
		var size = container.node().getBBox();
		return { width: size.width, height: size.height };
	}
	
	this.CreateToolTip=function(id, name, representativeID, representative, value, fontSize)
	{
		var container=d3.select("#"+elementID);
		var textContainer=container
			.append("div")
			.attr("id", elementID+"_ToolTip_"+id)
			.style("display", "none")
			.style("position", "absolute")
			.style("top", "0px")
			.style("left", "0px")
			.style("max-width", "250px")
			.style("padding", "5px")
			.style("border", "1px solid gray")
			.style("border-radius", "5px")
			.style("background", "ivory")
			.style("font-size", fontSize+"px");
		
		if(id!=representativeID)
		{
			textContainer
				.append("p")
				.style("margin-top", "0px")
				.text("("+id+") "+name);
		}
		else
		{
			textContainer
				.append("p")
				.style("margin-top", "0px")
				.style("color", "red")
				.text("("+id+", representative) "+name);
		}
		
		//textContainer.append("br");

		//if(id!=representativeID)
		//{
		//	textContainer
		//		.append("p")
		//		.text("Representative: ("+representativeID+") "+representative);
		//		
			//textContainer.append("br");
		//}
		
		textContainer
			.append("p")
			.style("margin-bottom", "0px")
			.text("Value: "+value);
	}
	
	this.DrawTreeMap=function(exportSvg, fontSize)
	{
		if(!exportSvg)
		{
			vm.svg
				.append("text")
				.attr("id", elementID+"_TextSizeTemp")
				.attr("x", -99999)
				.attr("y", -99999);
		}
		
		// Give the data to this cluster layout:
		// Here the size of each leaf is given in the 'valueName' field in input data
		var treeMapData=d3.hierarchy(treeData).sum(function(d){ return d[vm.valueName]});
		
		// Then d3.treemap computes the position of each element of the hierarchy
		var myTreeMap = d3.treemap()
			.size([vm.width, vm.height])
			.paddingInner(vm.padding)
			.paddingOuter(vm.padding)
			(treeMapData);

		// add Tree Map rectangles with their contents
		var tileParent="";
		var tileStylePos=0;
		var rectCount=0;
			
		// Add title for the main groups
		vm.svg.selectAll("groups")
			.data(treeMapData.descendants().filter(function(d){return d.depth==1}))
			.enter()
			.each(function(d, i) {
				vm.svg
					.append("g")
						.attr("id", elementID+"_Group_"+d.data.id)
					.append("rect")
						.attr("x", d.x0+1)
						.attr("y", d.y0+1)
						.attr("width", d.x1 - d.x0-2)
						.attr("height", d.y1 - d.y0-2)
						.attr("rx", 2)
						.style("stroke", "Gainsboro")
						.style("fill", "white");
			});
			
		vm.svg.selectAll("categories")
			.data(treeMapData.leaves())
			.enter()
			.each(function(d, i) {
				var maxWidth=d.x1-d.x0-vm.padding*6;
				var maxHeight=d.y1-d.y0-vm.padding*6;
				var container=d3.select("#"+elementID+"_Group_"+d.parent.data.id);
				
				if(!exportSvg)
				{
					// create tool tip
					vm.CreateToolTip(d.data.id, d.data.name, d.parent.data.id, d.parent.data.name, String(d.data[vm.valueName]), fontSize);
				}
				
				if(tileParent=="")
				{
					tileParent=d.parent.data.name;
				}
				else if(d.parent.data.name!=tileParent)
				{
					tileParent=d.parent.data.name;
					tileStylePos++;
					tileStylePos%=vm.tileStyles.length;
				}
				var tileStyle=vm.tileStyles[tileStylePos];
				
				var tileContainer=container.append("g");
				
				if(!exportSvg)
				{
					tileContainer
						.on("mouseenter", function(event) {
							var container=d3.select("#"+elementID+"_ToolTip_"+d.data.id);
							var wscrX=$(window).scrollLeft();
							var wscrY=$(window).scrollTop();
							var offset=$("#"+elementID).offset();
							var xPos=event.clientX-offset.left+wscrX+vm.padding*2;
							var yPos=event.clientY-offset.top+wscrY+vm.padding*2;
							
							container.style("top", yPos+"px");
							container.style("left", xPos+"px");
							container.style("display", "block");
						})
						.on("mousemove", function(event) {
							var container=d3.select("#"+elementID+"_ToolTip_"+d.data.id);
							var wscrX=$(window).scrollLeft();
							var wscrY=$(window).scrollTop();
							var offset=$("#"+elementID).offset();
							var xPos=event.clientX-offset.left+wscrX+vm.padding*2;
							var yPos=event.clientY-offset.top+wscrY+vm.padding*2;
							
							container.style("top", yPos+"px");
							container.style("left", xPos+"px");
						})
						.on("mouseleave", function() {
							d3.select("#"+elementID+"_ToolTip_"+d.data.id).style("display", "none");
						})
				}
				
				// special case when width or height is too low, compensate for padding
				var rectX=d.x0;
				var rectY=d.y0;
				var rectWidth=d.x1-d.x0;
				var rectHeight=d.y1-d.y0;
				if(rectWidth<1)
				{
					rectX-=vm.padding/2;
					rectWidth+=vm.padding;
				}
				if(rectHeight<1)
				{
					rectY-=vm.padding/2;
					rectHeight+=vm.padding;
				}
				tileContainer
					.append("rect")
						.attr("x", rectX)
						.attr("y", rectY)
						.attr("width", rectWidth)
						.attr("height", rectHeight)
						.attr("rx", 2)
						.style("fill", tileStyle.backcolor);
						
				if(exportSvg)
				{
					vm.defs
						.append("rect")
							.attr("id", "rect"+rectCount)
							.attr("x", rectX)
							.attr("y", rectY)
							.attr("width", rectWidth) 
							.attr("height", rectHeight);
							
					tileContainer
						.append("text")
							.attr("fill", tileStyle.color)
							.style("shape-outside", "url(#rect"+rectCount+")")
							.style("text-align", "center")
						//.append("tspan")
							.attr("x", rectX)
							.attr("y", rectY)
							.text("("+d.data.id+((d.data.id==d.parent.data.id)?", representative":"")+") "+d.data.name);
					
					rectCount++;
				}
				else
				{
					if(maxHeight>=12 && maxWidth>20)
					{
						var textContainer=tileContainer
							.append("text")
								.attr("x", d.x0+vm.padding*3+maxWidth/2)
								.attr("dominant-baseline", "hanging")
								.attr("text-anchor", "middle")
								.attr("fill", tileStyle.color)
								.style("cursor", "default");

						if(tileStyle.shadowColor!="")
							textContainer.style("text-shadow", "1px 1px "+tileStyle.shadowColor);

						var textSize=vm.AddTextToContainer(textContainer, String(d.data.name), 0, 0, fontSize, 0, maxWidth, maxHeight);
						if(textSize.height>0)
						{
							// center text
							textContainer
								.attr("y", d.y0+vm.padding*3+(maxHeight-textSize.height)/2);
						}
					}
				}
			});
			
		if(!exportSvg)
		{
			d3.select(elementID+"_TextSizeTemp").remove();
		}
	}
	
	// Draw the Tree Map
	vm.padding=2;
	if(showValue)
	{
		vm.valueName="value";
	}
	else
	{
		vm.valueName="logSize";
	}
	
	var mainContainer=d3.select("#"+elementID);
	var divContainer=mainContainer.append("div").attr("id", elementID+"_ValueSelector");
	
	if(!exportSvg)
	{
		divContainer
			.append("span")
				.text("Value to show:");
				
		var radioInput=divContainer
			.append("input")
				.attr("type", "radio")
				.attr("id", elementID+"_ValueType_0")
				.attr("name", elementID+"_ValueType")
				.on("change", function(event) {
					if(event.currentTarget.checked)
					{
						$("#"+elementID+"_SVG").children().remove();
						$("#"+elementID).children().filter(function(i,d){
							return d.id.startsWith(elementID+"_ToolTip");
						}).remove();
						vm.valueName="value";
						vm.DrawTreeMap(false, fontSize);
					}
				});
		if(showValue)
		{
			radioInput
				.attr("checked", "true")
		}
	
		divContainer
			.append("label")
				.attr("for", elementID+"_ValueType_0")
				.text("Value");
				
		radioInput1=divContainer
			.append("input")
				.attr("type", "radio")
				.attr("id", elementID+"_ValueType_1")
				.attr("name", elementID+"_ValueType")
				.on("change", function(event) {
					if(event.currentTarget.checked)
					{
						$("#"+elementID+"_SVG").children().remove();
						$("#"+elementID).children().filter(function(i,d){
							return d.id.startsWith(elementID+"_ToolTip");
						}).remove();
						vm.valueName="logSize";
						vm.DrawTreeMap(false, fontSize);
					}
				});
		if(!showValue)
		{
			radioInput1
				.attr("checked", "true")
		}

		divContainer
			.append("label")
				.attr("for", elementID+"_ValueType_1")
				.text("Log10(Size)");
	}
		
	// set the dimensions of the graph
	vm.width=$("#"+elementID).width();
	vm.height=$("#"+elementID).height()-$("#"+elementID+"_ValueSelector").height();

	// append the svg object to the container div
	if(exportSvg)
	{
		vm.svg=mainContainer
			.append("svg")
				.attr("id", elementID+"_SVG")
				.attr("width", vm.width)
				.attr("height", vm.height)
				.attr("viewBox", "0 0 "+vm.width+" "+vm.height)
                .attr("xmlns", "http://www.w3.org/2000/svg")
                .attr("xmlns:xlink", "http://www.w3.org/1999/xlink")
                .attr("version", "2.0")
				.attr("style", "font-size:"+fontSize+"px;font-family:Calibri,Verdana,Arial,sans-serif");
	}
	else
	{
		vm.svg=mainContainer
			.append("svg")
				.attr("id", elementID+"_SVG")
				.attr("width", vm.width)
				.attr("height", vm.height);
	}
	
	vm.defs=vm.svg
		.append("defs");
	
	if(exportSvg)
	{
		vm.svg=vm.svg
			.append("g");
	}

	vm.tileStyles=[{backcolor: "Crimson", color: "white", shadowColor: "gray"}, 
		{backcolor: "LightSeaGreen", color: "white", shadowColor: "gray"}, 
		{backcolor: "Tomato", color: "white", shadowColor: "gray"}, 
		{backcolor: "RoyalBlue ", color: "white", shadowColor: "gray"}, 
		{backcolor: "MediumSeaGreen", color: "white", shadowColor: "gray"}, 
		{backcolor: "DarkOrchid", color: "white", shadowColor: "gray"}, 
		{backcolor: "DarkOrange", color: "white", shadowColor: "gray"}, 
		{backcolor: "SlateBlue ", color: "white", shadowColor: "gray"}, 
		{backcolor: "LightSalmon", color: "black", shadowColor: "gainsboro"}, 
		{backcolor: "PaleTurquoise", color: "black", shadowColor: "gainsboro"}, 
		{backcolor: "Gold", color: "black", shadowColor: "gainsboro"}, 
		{backcolor: "SkyBlue", color: "black", shadowColor: "gainsboro"}, 
		{backcolor: "LightGreen", color: "black", shadowColor: "gainsboro"}, 
		{backcolor: "Violet", color: "white", shadowColor: "gray"}, 
		{backcolor: "Khaki", color: "black", shadowColor: "gainsboro"}, 
		{backcolor: "LightSkyBlue", color: "black", shadowColor: "gainsboro"}];

	vm.DrawTreeMap(exportSvg, fontSize);
}