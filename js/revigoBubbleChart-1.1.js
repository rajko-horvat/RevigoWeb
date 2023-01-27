
function BubbleChart(elementID, rawData) {

    // elementID serves as id of the enclosing div element and prefix for all other id's

    // Inspired by Vue.js way
    var vm = this;

    // Initialize internal variable for tracking selected GO terms
    vm.selectedGOterms = rawData.map(function (d, i) { return d["Selected"] ? d["Term ID"] : null })
        .filter(function (d) { return d });

    // Currently selected mappings for color and size
    vm.colorColumn = 'Value';
    vm.sizeColumn = 'LogSize';

    // Chart div
    $("#"+elementID)
        .append($("<div>",{id:elementID+"-chart-div",style:"float:left"})
            .append($("<div>",{id:elementID+"-chart"})));

    // Menu selector divs
    $("#"+elementID)
        .append($("<div>",{id:elementID+"-menu-div",style:"float:left"}));

    // Color menu selector
    $("#"+elementID+"-menu-div")
        .append($("<div>")
            .html("Color: ")
            .append("</br>")
            .append($("<select>",{name:elementID+"-color-selector",
                                  id:elementID+"-color-selector"})
                .append($("<option>",{value:"LogSize"})
                    .html("LogSize"))
                .append($("<option>",{value:"Value",selected:"selected"})
                    .html("Value"))))
        .append($("</br>"));

    // Color legend
    $("#"+elementID+"-menu-div")
        .append($("<div>",{id:elementID+"-legend-color"}));

    // Size menu selector
    $("#"+elementID+"-menu-div")
        .append($("<div>")
            .html("Size: ")
            .append("</br>")
            .append($("<select>",{name:elementID+"-size-selector",
                                  id:elementID+"-size-selector"})
                .append($("<option>",{value:"LogSize",selected:"selected"})
                    .html("LogSize"))
                .append($("<option>",{value:"Value"})
                    .html("Value"))))
        .append($("</br>"));

    // Size legend
    $("#"+elementID+"-menu-div")
        .append($("<div>",{id:elementID+"-legend-size"}))
        .append($("</br>"));

    // GO term selector
    $("#"+elementID+"-menu-div")
            .append($("<div>")
                .html("Select: ")
                .append("</br>")
                .append($("<form>",{name:elementID+"-go-terms-selector",
                                    id:elementID+"-go-terms-selector",
                                    style:"height:300px;width:200px;overflow-y:auto"})));

    // Fill in the GO term selector
    rawData.forEach( function(d,i) {
        $("#"+elementID+"-go-terms-selector")
            .append($("<input>",{type:"checkbox",
                               name:"go-term",
                               value:d.ID,
                               checked: vm.selectedGOterms.includes(d.ID) ? true : false}))
            .append($("<label>",{for:d.ID})
                .html(d.Name))
            .append($("</br>"));
    });

    document.getElementById(elementID+'-color-selector').addEventListener('change', function() {
        vm.colorColumn = this.value;
        vm.draw(vm.colorColumn,vm.sizeColumn);
    });


    document.getElementById(elementID+'-size-selector').addEventListener('change', function() {
        vm.sizeColumn = this.value;
        vm.draw(vm.colorColumn,vm.sizeColumn);
    });

    document.getElementById(elementID+'-go-terms-selector').addEventListener('change', function() {

        var checkboxes = 
            document.querySelectorAll('#'+elementID+'-go-terms-selector input[name="go-term"]:checked');

        vm.selectedGOterms = [];
        checkboxes.forEach((checkbox) => {
          vm.selectedGOterms.push(checkbox.value);
        });

        // Select appropriate GO terms on chart!
        rawData.forEach( function(d,i) {
            if (vm.selectedGOterms.includes(d.id)) { 
                vm.persistTooltip(d.id);
            } else {
                vm.unpersistTooltip(d.id);
            }
        });

    });


    // set the dimensions and margins of the graph
    var margin = {top: 10, right: 20, bottom: 40, left: 50},
        width = 700 - margin.left - margin.right,
        height = 620 - margin.top - margin.bottom;

    var tooltipLineOffset = 2; // distance from tooltip line to tooltip text
    var axisMargin = 0.15; // 15% larger axes than the range of data

    this.draw = function(color,size) {

        // Visualization data where color and size are mapped from one of the columns in original data
        vm.data = rawData.map(function(d) {
                return {'id': d['Term ID'],
                        'name': d['Name'],
                        'X': d['PC_0'],
                        'Y': d['PC_1'],
                        'color': d[color],
                        'size': d[size]};
        });

        // Remove all children elements of the chart div!
        d3.select("#"+elementID+"-chart").selectAll("*").remove();

        // append the svg object to the body of the page
        var svg = d3.select("#"+elementID+"-chart")
            .append("svg")
            .attr("width", width + margin.left + margin.right)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform",
                  "translate(" + margin.left + "," + margin.top + ")");

        // Add X axis
        var minX = Math.min(...vm.data.map(function(d){return d.X}));
        var maxX = Math.max(...vm.data.map(function(d){return d.X}));
        vm.x = d3.scaleLinear()
            .domain([minX-axisMargin*(maxX-minX),maxX+axisMargin*(maxX-minX)])
            .range([ 0, width ]);

        svg.append("g")
            .attr("transform", "translate(0," + height + ")")
            .call(d3.axisBottom(vm.x));

        // Add X axis label
        svg.append("text")
          .attr("text-anchor", "end")
          .attr("x", width)
          .attr("y", height + margin.top + 20)
          .text("Semantic space X");

        // Add Y axis
        var minY = Math.min(...vm.data.map(function(d){return d.Y}));
        var maxY = Math.max(...vm.data.map(function(d){return d.Y}));
        vm.y = d3.scaleLinear()
            .domain([minY-axisMargin*(maxY-minY),maxY+axisMargin*(maxY-minY)])
            .range([ height, 0]);

        svg.append("g")
            .call(d3.axisLeft(vm.y));

        // Y axis label
        svg.append("text")
          .attr("text-anchor", "end")
          .attr("transform", "rotate(-90)")
          .attr("y", -margin.left+20)
          .attr("x", -margin.top)
          .text("Semantic space Y")

        // If data is between 0 and 1 use log scale, otherwise use linear scale!
        var colorWithinOne = vm.data.map(x=>x['color']).every( x => x>=0 && x<= 1 );
        var sizeWithinOne = vm.data.map(x=>x['size']).every( x => x>=0 && x<=1 );

        if (colorWithinOne) {
            vm.scaleColor = d3.scaleLog()
            vm.scaleColor.type = 'log';
        } else {
            vm.scaleColor = d3.scaleLinear();
            vm.scaleColor.type = 'linear';
        }

        if (sizeWithinOne) {
            vm.scaleSize = d3.scaleLog()
            vm.scaleSize.type = 'log';
        } else {
            vm.scaleSize = d3.scaleLinear();
            vm.scaleSize.type = 'linear';
        }

        // Add a scale for bubble size
        vm.scaleSize = vm.scaleSize 
            .domain([Math.min(...vm.data.map(function(d){return d['size']})),
                     Math.max(...vm.data.map(function(d){return d['size']}))])
            .range([ 5, 15]);

        // Add a scale for bubble color
        vm.scaleColor = vm.scaleColor
            .domain([Math.min(...vm.data.map(function(d){return d['color']})),
                     Math.max(...vm.data.map(function(d){return d['color']}))])
            .range(['red','yellow']);

        // Add data circles
        svg.append('g')
            .attr("id",elementID+"-data")
            .selectAll("dot")
            .data(vm.data)
            .enter()
            .append("circle")
            .attr("class", "bubbles")
            .attr("id",function(d){return elementID+"-circle-"+d['id'].replace(":","-")})
            .attr("cx", function (d) { return vm.x(d['X']); } )
            .attr("cy", function (d) { return vm.y(d['Y']); } )
            .attr("r", function (d) { return vm.scaleSize(d['size']); } )
            .style("fill", function (d) { return vm.scaleColor(d['color']); } )
            .on("mouseover", function(event, d) {return vm.showTooltip(d['id'])} )
            .on("mouseleave", function(event, d) {return vm.hideTooltip(d['id'])} )
            .on("click", function(event, d) {return vm.toggleTooltip(d['id'])} )

        // Add a group for tooltips - it needs to be bellow data group so that labels are visible!
        svg.append("g")
            .attr("id",elementID+"-tooltips");

        // Toggle tooltips passed as an argument
        vm.selectedGOterms.forEach(function(id){
            vm.persistTooltip(id);
        });  

        // Draw legend for Values (node color)
        vm.drawColorLegend("#"+elementID+"-legend-color", vm.scaleColor); 

        // Draw legend for LogSize (circle radius)
        vm.drawSizeLegend("#"+elementID+"-legend-size", vm.scaleSize);

    }

    // Generate legend with continuous colors from a prespecified scale
    // Strangely, this is not built in D3 by default!?
    // http://bl.ocks.org/syntagmatic/e8ccca52559796be775553b467593a9f
    this.drawColorLegend = function(selector_id, colorscale) {

      var legendheight = 100,
          legendwidth = 80,
          margin = {top: 5, right: 60, bottom: 10, left: 2};
      
      d3.select(selector_id).selectAll("*").remove();

      var canvas = d3.select(selector_id)
        .style("height", legendheight + "px")
        .style("width", legendwidth + "px")
        .style("position", "relative")
        .append("canvas")
        .attr("height", legendheight - margin.top - margin.bottom)
        .attr("width", 1)
        .style("height", (legendheight - margin.top - margin.bottom) + "px")
        .style("width", (legendwidth - margin.left - margin.right) + "px")
        .style("border", "1px solid #000")
        .style("position", "absolute")
        .style("top", (margin.top) + "px")
        .style("left", (margin.left) + "px")
        .node();

      var ctx = canvas.getContext("2d");

      var legendscale;
      if (colorscale.type == 'linear') {
          legendscale = d3.scaleLinear();
      } else {
          legendscale = d3.scaleLog();
      }
      legendscale = legendscale
        .range([1, legendheight - margin.top - margin.bottom])
        .domain(colorscale.domain());

      // Generate image with continuous scale colors. If too slow see faster solution bellow!
      // http://bl.ocks.org/mbostock/048d21cf747371b11884f75ad896e5a5
      // http://stackoverflow.com/questions/4899799
      //       /whats-the-best-way-to-set-a-single-pixel-in-an-html5-canvas   
      d3.range(legendheight).forEach(function(i) {
        ctx.fillStyle = colorscale(legendscale.invert(i));
        ctx.fillRect(0,i,1,1);
      });

      var legendaxis = d3.axisRight()
        .scale(legendscale)
        .tickSize(6)
        .ticks(4);

      var svg = d3.select(selector_id)
        .append("svg")
        .attr("height", (legendheight) + "px")
        .attr("width", (legendwidth) + "px")
        .style("position", "absolute")
        .style("left", "0px")
        .style("top", "0px");

      svg
        .append("g")
        .attr("class", "axis")
        .attr("transform", "translate(" + (legendwidth - margin.left - margin.right + 3) + 
                                    "," + (margin.top) + ")")
        .call(legendaxis);

    }

    // https://www.youtube.com/watch?v=XmVPHq4NhMA
    this.drawSizeLegend = function(selector_id, sizescale) {

      var legendheight = 80,
          legendwidth = 100,
          margin = {top: 10, right: 70, bottom: 10, left: 2};

      d3.select(selector_id).selectAll("*").remove();

      d3.select(selector_id)
        .style("height", legendheight + "px")
        .style("width", legendwidth + "px")
        .style("position", "relative")

      var svg = d3.select(selector_id)
        .append("svg")
        .attr("height", (legendheight) + "px")
        .attr("width", (legendwidth) + "px")
        .style("position", "absolute")
        .style("left", "0px")
        .style("top", "0px");

      svg
        .append("g")
        .attr("class", "axis")
        .attr("transform", "translate(" + (legendwidth - margin.left - margin.right + 3) + 
                                    "," + (margin.top) + ")")
        .call( function(selection) {

            // Format number for compact representation:
            //      |d| < 0.01 : exponential format (2e-8)
            //      0.01 < |d| < 10.0 : two significant digits
            //      |d| > 10.0 : round to nearest integer value
            var formatNumber = function(d) {
                return Math.abs(d) > 0.01 ? 
                            Math.abs(d) >= 10.0 ? 
                                Math.round(d) : d.toPrecision(2) : 
                           d.toExponential(0);
            };

            var minScale = sizescale.domain()[0];

            var midScale;
            if (sizescale.type == 'linear') {
                midScale = 0.5*(sizescale.domain()[1] + sizescale.domain()[0]);
            } else {
                // Middle of the scale is in the middle of the log scale!
                midScale = Math.exp(0.5*(Math.log(sizescale.domain()[1]) + Math.log(sizescale.domain()[0])));
            }

            var maxScale = sizescale.domain()[1];

            var groups = selection.selectAll('g').data([
                formatNumber(minScale),
                formatNumber(midScale),
                formatNumber(maxScale)
            ]);

            var groupsEnter = groups.enter().append('g');

            groupsEnter
                .merge(groups)
                .attr('transform',(d,i) => 'translate(0,'+i*20+')');

            groups.exit().remove();

            groupsEnter
                  .append('circle')
                  .merge(groups.select('circle'))
                  .attr('r',sizescale)
                  .attr('stroke','black')
                  .attr('fill','white')

            groupsEnter
                  .append('text')
                  .merge(groups.select('text'))
                  .text(d => d)
                  .attr('dy','0.32em')
                  .attr('x',25)

        });
    }

    // Tooltips are created as needed and hidden/revealed later
    // In this way we allow for multiple tooltips to be visible at the same time!
    this.createTooltip = function(id,visibilityClass) {

        // visibilityClass: Either "show" or "persist" which determine whether tooltip is persistent.

        var GOterm = id.replace(":","-");

        // Retrieve data point for which tooltip is created
        var d = vm.data.filter(function(d){return d['id']==id})[0];

        // Tolltip text is positioned outside of the data circle, with additional margin of 2% data range
        var textX = vm.x(d['X'])+vm.scaleSize(d['size'])+0.02*(vm.x.range()[1]-vm.x.range()[0]);
        var textY = vm.y(d['Y'])-vm.scaleSize(d['size'])+0.02*(vm.y.range()[1]-vm.y.range()[0]);

        // Group for the tooltip - line, text and the surrounding rectangle
        d3.select("#"+elementID+"-tooltips")
          .append("g")
          .attr("id",elementID+"-tooltip-"+GOterm)
          .attr("class","tooltip")
                .on("mousedown", dragTooltip);

        // Create tooltip text
        d3.select("#"+elementID+"-tooltip-"+GOterm)
            .append("text")
            .attr("class","popuptext "+visibilityClass) 
            .attr("id",elementID+"-text-"+GOterm) 
            .attr("x",textX)
            .attr("y",textY)
            .text(d['name'] );

        // Tooltip rectangle box surrounding the tooltip text
        var rect = document.getElementById(elementID+"-text-"+GOterm).getBBox();
        d3.select("#"+elementID+"-tooltip-"+GOterm)
          .insert("rect","#"+elementID+"-text-"+GOterm) // insert before tooltip text so that it is bellow
          .attr("class","popuprect "+visibilityClass)
          .attr("id",elementID+"-rect-"+GOterm)
          .attr("x",rect.x-3)
          .attr("y",rect.y-3)
          .attr("width",rect.width+6)
          .attr("height",rect.height+6);

        var x2 = textX-tooltipLineOffset;
        var y2 = textY+tooltipLineOffset;
        var intersect = findIntersect([vm.x(d['X']), vm.y(d['Y'])], [x2,y2], vm.scaleSize(d['size']));

        // Create tooltip line connecting data circle with the text box
        d3.select("#"+elementID+"-tooltip-"+GOterm)
            .insert("line","#"+elementID+"-rect-"+GOterm) // insert before tooltip rectangle so that it is bellow
            .attr("class","popupline "+visibilityClass)
            .attr("id",elementID+"-line-"+GOterm)
            .attr("x1", intersect[0] )
            .attr("y1", intersect[1] )
            .attr("x2",x2)
            .attr("y2",y2-5)
            .attr("stroke","black");
    } 

    // Showing a tooltip is done by attaching a corresponding CSS class to the tooltip text and line
    this.showTooltip = function(id) {

        var GOterm = id.replace(":","-");

        if ($("#"+elementID+"-text-"+GOterm).length==0) {
            vm.createTooltip(id,"show");
        } else {
            if(!$("#"+elementID+"-text-"+GOterm).hasClass('show')){
                $("#"+elementID+"-text-"+GOterm).addClass('show');
             }
            if(!$("#"+elementID+"-line-"+GOterm).hasClass('show')){
                $("#"+elementID+"-line-"+GOterm).addClass('show');
             }
            if(!$("#"+elementID+"-rect-"+GOterm).hasClass('show')){
                $("#"+elementID+"-rect-"+GOterm).addClass('show');
             }
        }
    }

    this.persistTooltip = function(id) {

        var GOterm = id.replace(":","-");

        if ($("#"+elementID+"-text-"+GOterm).length==0) {
            vm.createTooltip(id,"persist");
        } else {
            if(!$("#"+elementID+"-text-"+GOterm).hasClass('persist')){
                $("#"+elementID+"-text-"+GOterm).addClass('persist');
             }
            if(!$("#"+elementID+"-line-"+GOterm).hasClass('persist')){
                $("#"+elementID+"-line-"+GOterm).addClass('persist');
             }
            if(!$("#"+elementID+"-rect-"+GOterm).hasClass('persist')){
                $("#"+elementID+"-rect-"+GOterm).addClass('persist');
             }
        }
    }

    // Hidding a tooltip is done by removing a corresponding CSS class to the tooltip text and line
    this.hideTooltip = function(id) {

        var GOterm = id.replace(":","-");

        if($("#"+elementID+"-text-"+GOterm).hasClass('show')){
            $("#"+elementID+"-text-"+GOterm).removeClass('show');
        }

        if($("#"+elementID+"-line-"+GOterm).hasClass('show')){
            $("#"+elementID+"-line-"+GOterm).removeClass('show');
        }

        if($("#"+elementID+"-rect-"+GOterm).hasClass('show')){
            $("#"+elementID+"-rect-"+GOterm).removeClass('show');
        }

    }

    this.unpersistTooltip = function(id) {

        var GOterm = id.replace(":","-");

        if($("#"+elementID+"-text-"+GOterm).hasClass('persist')){
            $("#"+elementID+"-text-"+GOterm).removeClass('persist');
        }

        if($("#"+elementID+"-line-"+GOterm).hasClass('persist')){
            $("#"+elementID+"-line-"+GOterm).removeClass('persist');
        }

        if($("#"+elementID+"-rect-"+GOterm).hasClass('persist')){
            $("#"+elementID+"-rect-"+GOterm).removeClass('persist');
        }

    }

    // Tooltip toggling is triggered upon click and makes a tooltip persistent
    this.toggleTooltip = function(id) {

        var GOterm = id.replace(":","-");

        if ($("#"+elementID+"-text-"+GOterm).length==0) {
            vm.createTooltip(id,"persist");
        } else {
            $("#"+elementID+"-text-"+GOterm).toggleClass("persist");
            $("#"+elementID+"-line-"+GOterm).toggleClass("persist");
            $("#"+elementID+"-rect-"+GOterm).toggleClass("persist");
        }

        // Select checkboxes which correspond to selected terms on chart!
        var selectedItems = [...document.querySelectorAll("#"+elementID+"-tooltips text.persist")]
                                            .map(x=>x.id.replace(elementID+"-text-","").replace("-",":"));
        if (selectedItems) {

            rawData.forEach( function(d,i) {
                if ( selectedItems.includes(id) ) {
                    document.querySelectorAll('#'+elementID+'-go-terms-selector input[value="'+id+'"]')[0]
                        .checked = true;
                } else {
                    document.querySelectorAll('#'+elementID+'-go-terms-selector input[value="'+id+'"]')[0]
                        .checked = false;
                }
            });

            // Assign a global variable which stores currently selected terms!
            vm.selectedGOterms = selectedItems;

        }

    }

    // Dragging a tooltip updates the coordinates of tooltip based on the position of the mouse
    var dragTooltip = function(event) {

        // dragTooltip operates on the tooltip group so we have to extract the id of the GO term from it
        var GOterm = d3.select(this).attr("id").replace(elementID+"-tooltip-","");

        // Offsets for text and rectangle so that dragging is smooth
        var textOffsetX = d3.select("#"+elementID+"-text-"+GOterm).attr('x') - d3.pointer(event)[0];
        var textOffsetY = d3.select("#"+elementID+"-text-"+GOterm).attr('y') - d3.pointer(event)[1];
        var rectOffsetX = d3.select("#"+elementID+"-rect-"+GOterm).attr('x') - d3.pointer(event)[0];
        var rectOffsetY = d3.select("#"+elementID+"-rect-"+GOterm).attr('y') - d3.pointer(event)[1];

        d3.select(this)
            .on("mousemove", function(event){

                // Update position of the tooltip text
                d3.select("#"+elementID+"-text-"+GOterm)
                    .attr("x",function(d){
                        return d3.pointer(event)[0] + textOffsetX;
                    })
                    .attr("y",function(d){
                        return d3.pointer(event)[1] + textOffsetY;
                    });

                // Update the position of the connecting line
                var x1 = Number(d3.select("#"+elementID+"-circle-"+GOterm).attr("cx"));
                var y1 = Number(d3.select("#"+elementID+"-circle-"+GOterm).attr("cy"));
                var r = Number(d3.select("#"+elementID+"-circle-"+GOterm).attr("r"));
                var x2 = d3.pointer(event)[0]+textOffsetX-tooltipLineOffset;
                var y2 = d3.pointer(event)[1]+textOffsetY+tooltipLineOffset;

                // Connecting line will begin at data circle edge, not at the center!
                var intersect = findIntersect([x1,y1],[x2,y2],r);

                d3.select("#"+elementID+"-line-"+GOterm)
                    .attr("x2", x2)
                    .attr("y2", y2-5)
                    .attr("x1", intersect[0])
                    .attr("y1", intersect[1]);

                // Update the position of the surrounding rectangle
                d3.select("#"+elementID+"-rect-"+GOterm)
                  .attr("x",d3.pointer(event)[0]+rectOffsetX)
                  .attr("y",d3.pointer(event)[1]+rectOffsetY);

            })
            .on("mouseup", function(){
                d3.select(this).on("mousemove", null); 
            });
    };

}

// Find the intersect between a line and circle of radius r, if line starts at the circle center
// (coordinates x) and ends at coordinates y
function findIntersect(x, y, r) {
    var M = (y[1]-x[1])/(y[0]-x[0]);
    var signX = Math.sign(y[0]-x[0]);
    var signY = Math.sign(y[1]-x[1]);
    return [x[0] + signX*(r/Math.sqrt(1+M*M)), x[1] + signY*(r/Math.sqrt(1+1/(M*M)))];
}

