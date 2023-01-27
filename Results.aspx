﻿<%@ Page Language="C#" MasterPageFile="~/RevigoMasterPage.master" AutoEventWireup="true" CodeBehind="Results.aspx.cs" 
    Inherits="RevigoWeb.Results" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MasterHeaderContent" runat="server">
    <link href="<%=Request.ApplicationPath %>css/tc.css" rel="stylesheet" type="text/css" />
    <link href="<%=Request.ApplicationPath %>css/revigoBubbleChart-1.1.css" rel="stylesheet" type="text/css" />
    <link href="<%=Request.ApplicationPath %>css/revigoTable-1.0.4.css" rel="stylesheet" type="text/css" />
    <link href="<%=Request.ApplicationPath %>css/x3dom-1.8.3.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="<%=Request.ApplicationPath %>js/d3-7.4.5.min.js"></script>
    <script type="text/javascript" src="<%=Request.ApplicationPath %>js/revigoBubbleChart-1.1.js"></script>
    <script type="text/javascript" src="<%=Request.ApplicationPath %>js/revigoTreeMap-1.2.js"></script>
    <script type="text/javascript" src="<%=Request.ApplicationPath %>js/revigoTable-1.0.5.js"></script>
    <script type="text/javascript" src="<%=Request.ApplicationPath %>js/revigox3dScatterplot-1.0.js"></script>
    <script type="text/javascript" src="<%=Request.ApplicationPath %>js/cytoscape-3.20.3.min.js"></script>
    <script type="text/javascript" src="<%=Request.ApplicationPath %>js/x3dom-full-1.8.3.min.js"></script>
    <style type="text/css">
        .no-close .ui-dialog-titlebar {
            display:none;
        }
        .ui-widget-content {
            border: none;
	        background: white;
	        color: #222222;
            padding:5px;
        }
        .no-close .ui-dialog-content {
            font-size: 10pt;
            overflow:hidden;
        }
    </style>
    <title>Revigo results</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MasterContent" runat="server">
    <asp:HiddenField ID="fldGOList" runat="server" />
    <asp:HiddenField ID="fldCutoff" runat="server" />
    <asp:HiddenField ID="fldValueType" runat="server" />
    <asp:HiddenField ID="fldSpeciesTaxon" runat="server" />
    <asp:HiddenField ID="fldMeasure" runat="server" />
    <asp:HiddenField ID="fldRemoveObsolete" runat="server" />
    <div style="margin-left: auto; margin-right: auto;" >
        <div id="pnlLoading" class="LoadingPanel">
            <div style="display:flex; justify-content: center; align-items: center;">
                <asp:Literal ID="litLoader00" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <svg class="Loader00" viewBox="0 0 100 100">
                  <g>
                    <circle style="fill:transparent;stroke:#0052ec; stroke-width:0.6rem; stroke-linecap: round; stroke-dasharray: 71 71;" cx="50" cy="50" r="45"/>
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 50 50" to="360 50 50" begin="0s" dur="1.4s" repeatCount="indefinite"/>
                  </g>
                </svg>
                </asp:Literal>
                <asp:Literal ID="litLoader01" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <div class='Loader01'>
                    <div class='block'>
                        <div class='item'></div>
                        <div class='item'></div>
                        <div class='item'></div>
                        <div class='item'></div>
                        <div class='item'></div>
                        <div class='item'></div>
                        <div class='item'></div>
                        <div class='item'></div>
                    </div>
                </div>
                </asp:Literal>
                <asp:Literal ID="litLoader02" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <svg class="Loader02" viewBox="0 0 280 280" fill="none">
                <g>
                    <line x1="60" y1="140" x2="220" y2="140" stroke="#000" stroke-width="4"/>
                    <circle cx="60" cy="140" r="5" fill="#000"/>
                    <circle cx="220" cy="140" r="5" fill="#000"/>
                </g>
                <g>
                    <path d="M109.957 122.655L140 105.309L170.043 122.655V157.345L140 174.691L109.957 157.345V122.655Z" stroke="#000" stroke-width="4"/>
                    <circle cx="140" cy="140" r="13" stroke="#f5d77b" stroke-width="4"/>
                    <circle cx="110" cy="192" r="13" stroke="#f7a78f" stroke-width="4"/>
                    <circle cx="170" cy="88" r="13" stroke="#82c7c5" stroke-width="4"/>
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 140 140" to="360 140 140" begin="0s" dur="6s" repeatCount="indefinite"/>
                </g>
                <g>
                    <circle cx="85" cy="232" r="8" stroke="#82c7c5" stroke-width="4"/>
                    <circle cx="110" cy="192" r="5" fill="#f7a78f"/>
                    <circle cx="185" cy="61" r="5" fill="#f5d77b"/>
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 140 140" to="360 140 140" begin="0s" dur="3s" repeatCount="indefinite"/>
                </g>
                </svg>
                </asp:Literal>
                <asp:Literal ID="litLoader03" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <svg class="Loader03" viewBox="0 0 100 100">
                    <circle class="spinner" style="fill:none;stroke:#dd2476; stroke-width:0.6rem; stroke-linecap: round;" cx="50" cy="50" r="45"/>
                </svg>
                </asp:Literal>
                <asp:Literal ID="litLoader04" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <div class="Loader04">
                  <div class="arc"></div>
                  <div class="arc"></div>
                  <div class="arc"></div>
                </div>
                </asp:Literal>
                <asp:Literal ID="litLoader05" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <svg class="Loader05" viewBox="0 0 100 100">
                <g>
                    <circle style="fill:black;opacity:1" cx="50" cy="50" r="3"/>
                    <circle style="fill:black;opacity:0.98" cx="50.24" cy="50.97" r="3"/>
                    <circle style="fill:black;opacity:0.96" cx="50.94" cy="51.77" r="3"/>
                    <circle style="fill:black;opacity:0.93" cx="52.01" cy="52.23" r="3"/>
                    <circle style="fill:black;opacity:0.91" cx="53.32" cy="52.24" r="3"/>
                    <circle style="fill:black;opacity:0.89" cx="54.7" cy="51.71" r="3"/>
                    <circle style="fill:black;opacity:0.87" cx="55.97" cy="50.63" r="3"/>
                    <circle style="fill:black;opacity:0.84" cx="56.93" cy="49.03" r="3"/>
                    <circle style="fill:black;opacity:0.82" cx="57.42" cy="47" r="3"/>
                    <circle style="fill:black;opacity:0.8" cx="57.28" cy="44.71" r="3"/>
                    <circle style="fill:black;opacity:0.78" cx="56.43" cy="42.34" r="3"/>
                    <circle style="fill:black;opacity:0.76" cx="54.82" cy="40.11" r="3"/>
                    <circle style="fill:black;opacity:0.73" cx="52.49" cy="38.26" r="3"/>
                    <circle style="fill:black;opacity:0.71" cx="49.55" cy="37.01" r="3"/>
                    <circle style="fill:black;opacity:0.69" cx="46.14" cy="36.54" r="3"/>
                    <circle style="fill:black;opacity:0.67" cx="42.5" cy="37.01" r="3"/>
                    <circle style="fill:black;opacity:0.64" cx="38.89" cy="38.49" r="3"/>
                    <circle style="fill:black;opacity:0.62" cx="35.58" cy="40.99" r="3"/>
                    <circle style="fill:black;opacity:0.6" cx="32.88" cy="44.44" r="3"/>
                    <circle style="fill:black;opacity:0.58" cx="31.05" cy="48.67" r="3"/>
                    <circle style="fill:black;opacity:0.56" cx="30.3" cy="53.47" r="3"/>
                    <circle style="fill:black;opacity:0.53" cx="30.82" cy="58.54" r="3"/>
                    <circle style="fill:black;opacity:0.51" cx="32.66" cy="63.54" r="3"/>
                    <circle style="fill:black;opacity:0.49" cx="35.84" cy="68.12" r="3"/>
                    <circle style="fill:black;opacity:0.47" cx="40.24" cy="71.93" r="3"/>
                    <circle style="fill:black;opacity:0.44" cx="45.66" cy="74.62" r="3"/>
                    <circle style="fill:black;opacity:0.42" cx="51.81" cy="75.94" r="3"/>
                    <circle style="fill:black;opacity:0.4" cx="58.34" cy="75.68" r="3"/>
                    <circle style="fill:black;opacity:0.38" cx="64.84" cy="73.75" r="3"/>
                    <circle style="fill:black;opacity:0.36" cx="70.86" cy="70.15" r="3"/>
                    <circle style="fill:black;opacity:0.33" cx="75.98" cy="65" r="3"/>
                    <circle style="fill:black;opacity:0.31" cx="79.8" cy="58.54" r="3"/>
                    <circle style="fill:black;opacity:0.29" cx="81.98" cy="51.12" r="3"/>
                    <circle style="fill:black;opacity:0.27" cx="82.28" cy="43.14" r="3"/>
                    <circle style="fill:black;opacity:0.24" cx="80.56" cy="35.1" r="3"/>
                    <circle style="fill:black;opacity:0.22" cx="76.81" cy="27.5" r="3"/>
                    <circle style="fill:black;opacity:0.2" cx="71.16" cy="20.88" r="3"/>
                    <circle style="fill:black;opacity:0.18" cx="63.86" cy="15.69" r="3"/>
                    <circle style="fill:black;opacity:0.16" cx="55.29" cy="12.37" r="3"/>
                    <circle style="fill:black;opacity:0.13" cx="45.92" cy="11.21" r="3"/>
                    <circle style="fill:black;opacity:0.11" cx="36.32" cy="12.41" r="3"/>
                    <circle style="fill:black;opacity:0.09" cx="27.07" cy="16.01" r="3"/>
                    <circle style="fill:black;opacity:0.07" cx="18.79" cy="21.9" r="3"/>
                    <circle style="fill:black;opacity:0.04" cx="12.03" cy="29.81" r="3"/>
                    <circle style="fill:black;opacity:0.02" cx="7.31" cy="39.36" r="3"/>
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 50 50" to="360 50 50" begin="0s" dur="1.4s" repeatCount="indefinite"></animateTransform>
                </g>
                </svg>
                </asp:Literal>
                <asp:Literal ID="litLoader06" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <div class="Loader06">
                  <div class="Loader06-chase">
                    <div class="Loader06-chase-dot"></div>
                    <div class="Loader06-chase-dot"></div>
                    <div class="Loader06-chase-dot"></div>
                    <div class="Loader06-chase-dot"></div>
                    <div class="Loader06-chase-dot"></div>
                    <div class="Loader06-chase-dot"></div>
                  </div>
                </div>
                </asp:Literal>
                <asp:Literal ID="litLoader07" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <svg class="Loader07" viewBox="0 0 200 200">
                  <circle cx="40" cy="100" r="20" fill="black">
                    <animate attributeName="r" begin="-0.32s" values="0;20;0" keyTimes="0;0.5;1" calcMode="spline" keySplines="0.5 0 0.5 1;0.5 0 0.5 1" dur="1.4s" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="100" r="20" fill="black">
                    <animate attributeName="r" begin="-0.16s" values="0;20;0" keyTimes="0;0.5;1" calcMode="spline" keySplines="0.5 0 0.5 1;0.5 0 0.5 1" dur="1.4s" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="160" cy="100" r="20" fill="black">
                    <animate attributeName="r" begin="0s" values="0;20;0" keyTimes="0;0.5;1" calcMode="spline" keySplines="0.5 0 0.5 1;0.5 0 0.5 1" dur="1.4s" repeatCount="indefinite"/>
                  </circle>
                </svg>
                </asp:Literal>
                <asp:Literal ID="litLoader08" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <svg class="Loader08" viewBox="0 0 200 200">
                  <circle cx="12" cy="100" r="12" fill="#324650">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="38" cy="100" r="10" fill="#324650" fill-opacity="0.8">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.1s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="60" cy="100" r="8" fill="#324650" fill-opacity="0.6">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.2s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="78" cy="100" r="6" fill="#324650" fill-opacity="0.4">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.3s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="92" cy="100" r="4" fill="#324650" fill-opacity="0.2">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.4s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="105" cy="100" r="4" fill="#489D8A" fill-opacity="0.2">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.4s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="119" cy="100" r="6" fill="#489D8A" fill-opacity="0.4">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.3s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="137" cy="100" r="8" fill="#489D8A" fill-opacity="0.6">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.2s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="159" cy="100" r="10" fill="#489D8A" fill-opacity="0.8">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.1s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="185" cy="100" r="12" fill="#489D8A">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
  
                  <circle cx="100" cy="12" r="12" fill="#E8A961">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="38" r="10" fill="#E8A961" fill-opacity="0.8">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.1s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="60" r="8" fill="#E8A961" fill-opacity="0.6">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.2s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="78" r="6" fill="#E8A961" fill-opacity="0.4">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.3s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="92" r="4" fill="#E8A961" fill-opacity="0.2">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.4s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="105" r="4" fill="#D34B4C" fill-opacity="0.2">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.4s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="119" r="6" fill="#D34B4C" fill-opacity="0.4">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.3s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="137" r="8" fill="#D34B4C" fill-opacity="0.6">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.2s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="159" r="10" fill="#D34B4C" fill-opacity="0.8">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0.1s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                  <circle cx="100" cy="185" r="12" fill="#D34B4C">
                    <animateTransform attributeName="transform" attributeType="XML" type="rotate" from="0 100 100" to="360 100 100" begin="0s" dur="2.2s" calcMode="spline" keySplines="1 0 .25 .25" repeatCount="indefinite"/>
                  </circle>
                </svg>
                </asp:Literal>
                <asp:Literal ID="litLoader09" runat="server" EnableViewState="false" Mode="PassThrough" Visible="false">
                <div class="Loader09-folding-cube">
                  <div class="Loader09-cube1 Loader09-cube"></div>
                  <div class="Loader09-cube2 Loader09-cube"></div>
                  <div class="Loader09-cube4 Loader09-cube"></div>
                  <div class="Loader09-cube3 Loader09-cube"></div>
                </div>
                </asp:Literal>
                <div style="display: block;">
                    <div style="display: block; text-align: center; font-size:large;">Revigo is currently processing your data, please wait...</div>
                    <div id="txtProgress" style="display: block; text-align: center; margin: 5px;">0%</div>
                </div>
            </div>
        </div>
        <div id="pnlBack" style="display:none;">
            <p><asp:LinkButton ID="lnkBack" runat="server" OnClick="lnkBack_Click">Return to the input page and change input parameters</asp:LinkButton></p>
        </div>
        <div id="ExportContainer" style="display:none;width:900px; height:600px;"></div>
        <script type="text/javascript">
            function ExportSvg(linkID, elementID, treeData)
            {
                var pValue=true;
                if(!$("#"+elementID+"_ValueType_0").is(":checked"))
                {
                    pValue=false;
                }
                var tempTreeMap = new TreeMap("ExportContainer", treeData, 10, pValue, true);
                var svg=document.getElementById("ExportContainer_SVG");

                // serialize svg to string
                var serializer = new XMLSerializer();
                var source = serializer.serializeToString(svg);

                // remove out SVG object after serialization
                $("#ExportContainer").children().remove();

                source="<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\r\n"+source;
                source=source.replaceAll("shape-outside", "shape-inside").replaceAll("&quot;", "");

                //convert svg source to URI data scheme.
                var url = "data:image/svg+xml;charset=utf-8,"+encodeURIComponent(source);

                //set url value to a element's href attribute.
                $("#"+linkID).attr("href", url);

                return true;
            }

            function ExportX3D(linkID, x3dElement)
            {
                let temp3dScatterplot = new x3dScatterplot("", 0, 0, x3dElement.chartData,
                    [x3dElement.selectedSizeOption, x3dElement.selectedColorOption],
                    x3dElement.selectedSizeOption, x3dElement.selectedColorOption, true);
                let x3d = temp3dScatterplot.X3D;

                x3d = "<!DOCTYPE X3D PUBLIC \"ISO//Web3D//DTD X3D 3.3//EN\" \"https://www.web3d.org/specifications/x3d-3.3.dtd\">\r\n" + x3d;
                x3d = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" + x3d;

                //convert X3D source to URI data scheme.
                var url = "data:model/x3d+xml;charset=utf-8," + encodeURIComponent(x3d);

                //set url value to a element's href attribute.
                $("#" + linkID).attr("href", url);

                return true;
            }
        </script>
        <div id="tabResults" class="ResultTabs" style="display:none;">
            <ul>
		        <li id="tabBP"><a href="#tabBPResults" style="font-size:10pt;">Biological Process (<span id="lblBPTab"></span>)</a></li>
	            <li id="tabCC"><a href="#tabCCResults" style="font-size:10pt;">Cellular Component (<span id="lblCCTab"></span>)</a></li>
    	        <li id="tabMF"><a href="#tabMFResults" style="font-size:10pt;">Molecular Function (<span id="lblMFTab"></span>)</a></li>
                <li id="tabCloud"><a href="#tabCloudResults" style="font-size:10pt;">Tag Clouds</a></li>
                <li><a id="tabReportLink" href="#tabReport" style="font-size:10pt;">Report<span id="lblReportTab" style="color:red;"></span></a></li>
    	    </ul>
            <div id="tabBPResults" class="ResultTab">
                <ul>
                    <li><a href="#tabResultsBPDescription" style="font-size:10pt;">Description</a></li>
                    <li><a href="#tabResultsBPTable" style="font-size:10pt;">Table</a></li>
		            <li><a href="#tabResultsBPScatterplot" style="font-size:10pt;">Scatterplot</a></li>
                    <li><a href="#tabResultsBPScatterplot3D" style="font-size:10pt;">3D Scatterplot</a></li>
		            <li><a href="#tabResultsBPCytoscape" style="font-size:10pt;">Interactive Graph</a></li>
	    	        <li><a href="#tabResultsBPTreeMap" style="font-size:10pt;">Tree Map</a></li>
    	        </ul>
                <div id="tabResultsBPDescription">
                    <div id="pnlBPDescription" style="margin: 10px auto 0 auto;">
                        <div style="display:table;"><img src="<%=Request.ApplicationPath %>Images/BiologicalProcesses.png" alt="Biological Process image" 
                            width="264" height="190" style="float:left; margin:0.5em;"/>
                        <div style="margin-top:0.5em; margin-bottom:0.5em; font-size:larger;"><b>Biological Processes</b></div><br />
                        <span style="font-size:larger;">The larger processes, or "biological programs" accomplished by 
                            multiple molecular activities. Examples of broad biological process terms are <i>DNA repair</i> or
                            <i>signal transduction</i>. Examples of more specific terms are <i>pyrimidine nucleobase biosynthetic process</i> 
                            or <i>glucose transmembrane transport</i>. Note that a biological process is not equivalent to a pathway. 
                            At present, the GO does not try to represent the dynamics or dependencies that would be required 
                            to fully describe a pathway.</span></div>
                        <p style="margin-top:1em; margin-bottom:1em; font-size:larger;"><b>Several tools assist in 
                        Biological Process analysis:</b></p>
                        <ul><li>The Table shows the cluster representatives and cluster members (indented smaller cursive text), 
                                additional statistical data and downloadable Term Similarity Matrix useful for further analysis.</li>
                            <li>The Scatterplot shows the cluster representatives (i.e. terms remaining after the redundancy reduction) 
                                in a two dimensional space derived by applying multidimensional scaling to a matrix of 
                                the GO term semantic similarities.</li>
                            <li>The 3D Scatterplot adds additional dimension while analyzing the cluster representatives and their semantic similarities.</li>
                            <li>The Interactive graph-based visualization. Each of the GO terms is a node in the graph, 
                                and 3% of the strongest GO term pairwise similarities are designated as edges in the graph. 
                                We found that the threshold value of 3% strikes a good balance between over-connected graphs 
                                with no visible subgroups on the one hand, and very fragmented graphs with 
                                too many small groups on the other hand. The placement of the nodes is determined 
                                by the ForceDirected layout algorithm as implemented in Cytoscape Web control.</li>
                            <li>The Tree Map shows a two-level hierarchy of GO terms - the cluster representatives 
                                from the Scatterplot and the Interactive graph are here joined into several very high-level groups.</li></ul>
                    </div>
                </div>
                <div id="tabResultsBPTable">
                    <div id="pnlBPTable" style="display:none; margin: 10px auto 0 auto;">
                        <div id="pnlBPTableContent"></div>
                        <p><a download="Revigo_BP_OnScreenTable.tsv" href="#" onclick='$(this).attr("href", revigoState.nsTabs[0].tabs[1].drawObj.GetEncodedContents()); return true;'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;width:22px;height:22px;vertical-align:middle;" />
                                Export table (as is shown on screen) to TSV<sup style="color:red">1,2</sup></a><br />
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aTable&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.BIOLOGICAL_PROCESS %>'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;margin-bottom:10px;width:22px;height:22px;vertical-align:middle;" />
                                Export raw table to TSV<sup style="color:red">1,2</sup></a><br />
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aSimMat&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.BIOLOGICAL_PROCESS %>'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;width:22px;height:22px;vertical-align:middle;" />
                                Export Term Similarity Matrix</a></p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlBPTableLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlBPTableMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsBPScatterplot">
                    <div id="pnlBPScatterplot" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlBPScatterplotContent" style="display:table; width: 900px; height:600px; border: 1px solid black; font-family: Calibri,Trebuchet,sans-serif; font-size: 9pt;">
                        </div>
                        <p style="font-size: 9pt; color: red; width: 900px;">The axes in the plot have no intrinsic meaning.
                            Revigo uses Multidimensional Scaling (MDS) to reduce the dimensionality of a matrix of the GO terms pairwise semantic similarities. 
                            The resulting projection may be highly non linear. The guiding principle is that semantically similar GO terms should remain close together in the plot. 
                            Repeated runs of Revigo may yield different arrangements, but the term distances will remain similar.</p>
                        <p><a style="margin-right:30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aRScatterplot&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.BIOLOGICAL_PROCESS %>'><img alt="R" src="<%=Request.ApplicationPath %>Images/development-r.png" style="vertical-align:middle; margin-right:4px;" />Export to R script for plotting<sup style="color:red">1</sup></a>
                            <a style="margin-right:30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aScatterplot&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.BIOLOGICAL_PROCESS %>'><img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to TSV<sup style="color:red">1,2</sup></a></p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlBPScatterplotLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlBPScatterplotMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsBPScatterplot3D">
                    <div id="pnlBPScatterplot3D" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlBPScatterplot3DContent" style="display:table; margin:0 auto 0 auto; font-family: Calibri,Trebuchet,sans-serif; font-size: 9pt;"></div>
                        <p style="font-size: 9pt; color: red; width: 900px;">The axes in the plot have no intrinsic meaning.
                            Revigo uses Multidimensional Scaling (MDS) to reduce the dimensionality of a matrix of the GO terms pairwise semantic similarities. 
                            The resulting projection may be highly non linear. The guiding principle is that semantically similar GO terms should remain close together in the plot. 
                            Repeated runs of Revigo may yield different arrangements, but the term distances will remain similar.</p>
	                    <h4>Interaction with plot (left mouse click inside plot):</h4>
	                    <ul>
                            <li>Hoover mouse over bubble to see a Term ID and description</li>
		                    <li>Hold left mouse button and move mouse to rotate</li>
		                    <li>Scroll (or hold right mouse button and move mouse) to pan (zoom)</li>
                            <li>Hold Ctrl key + left mouse button and move mouse to move view</li>
		                    <li>Press 'r' key to reset view</li>
		                    <li>View more navigation options on <a href="https://www.x3dom.org/documentation/interaction/" target="_blank">x3dom interaction</a> page</li>
	                    </ul>
                        <p><a style="margin-right:30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aScatterplot3D&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.BIOLOGICAL_PROCESS %>'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />
                                Export to TSV<sup style="color:red">1,2</sup></a>
                            <a id="ExportBPX3D" download="Revigo_BP_3DScatterplot.x3d" href="#" onclick='return ExportX3D("ExportBPX3D", revigoState.nsTabs[0].tabs[2].drawObj);'>
                                <img alt="X3D" src="<%=Request.ApplicationPath %>Images/x3d.png" style="width:24px;height:24px;vertical-align:middle; margin-right:4px;" />
                                Export to X3D<sup style="color:red">1</sup></a>
                        </p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlBPScatterplot3DLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlBPScatterplot3DMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsBPCytoscape">
                    <div id="pnlBPCytoscape" style="margin: 0 auto 0 auto; display: none;">
                        <p style="color: red;">The color of the bubble corresponds to the Value you provided alongside GO Term in your dataset.<br />
                            The size of the bubble corresponds to the LogSize value for the GO Term.</p>
                        <p style="font-size: 9pt">(Navigate with mouse. Click and hold to move view and drag items. Scroll to zoom.)</p>
                        <div id="pnlBPCytoscapeContent" style="width:900px; height:600px; border: 1px solid black;"></div>
                        <p><a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aXGMML&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.BIOLOGICAL_PROCESS %>'><img alt="XGMML" src="<%=Request.ApplicationPath %>Images/Cytoscape.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to Cytoscape XGMML for offline use</a></p>
                    </div>
                    <div id="pnlBPCytoscapeLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlBPCytoscapeMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsBPTreeMap">
                    <div id="pnlBPTreeMap" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlBPTreeMapContent" style="position:relative; width:900px; height:600px; border: 1px solid black; padding: 10px; margin:0px 40px 0px 40px; font-family: Calibri,Verdana,Arial,sans-serif; font-size: 12px;"></div>
                        <p><a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aTreeMap&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.BIOLOGICAL_PROCESS %>' style="margin-right: 30px;"><img alt="CSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to TSV<sup style="color:red">1,2</sup></a>
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aRTreeMap&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.BIOLOGICAL_PROCESS %>' style="margin-right: 30px;"><img alt="R" src="<%=Request.ApplicationPath %>Images/development-r.png" style="vertical-align:middle; margin-right:4px;" />Export to R script for plotting<sup style="color:red">2</sup></a>
                            <a id="ExportBPSVG" download="Revigo_BP_TreeMap.svg" href="#" onclick='return ExportSvg("ExportBPSVG", "pnlBPTreeMapContent", revigoState.nsTabs[0].treeMapData);'><img alt="SVG" src="<%=Request.ApplicationPath %>Images/inkscape.png" style="vertical-align:middle; margin-right:4px;" />Export to SVG<sup style="color:red">2</sup></a></p>
                        <p style="color: red;"><sup>1</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.<br />
                            <sup>2</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.</p>
                    </div>
                    <div id="pnlBPTreeMapLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlBPTreeMapMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
            </div>
            <div id="tabCCResults" class="ResultTab" style="display:none;">
                <ul>
                    <li><a href="#tabResultsCCDescription" style="font-size:10pt;">Description</a></li>
                    <li><a href="#tabResultsCCTable" style="font-size:10pt;">Table</a></li>
		            <li><a href="#tabResultsCCScatterplot" style="font-size:10pt;">Scatterplot</a></li>
                    <li><a href="#tabResultsCCScatterplot3D" style="font-size:10pt;">3D Scatterplot</a></li>
		            <li><a href="#tabResultsCCCytoscape" style="font-size:10pt;">Interactive Graph</a></li>
	    	        <li><a href="#tabResultsCCTreeMap" style="font-size:10pt;">Tree Map</a></li>
    	        </ul>
                <div id="tabResultsCCDescription">
                    <div id="pnlCCDescription" style="margin: 10px auto 0 auto;">
                        <div style="display:table;"><img src="<%=Request.ApplicationPath %>Images/CellularComponents.png" alt="Cellular Components image" 
                            width="264" height="190" style="float:left; margin:0.5em;"/>
                        <div style="margin-top:0.5em; margin-bottom:0.5em; font-size:larger;"><b>Cellular Components</b></div><br />
                        <span style="font-size:larger;">The locations relative to cellular structures in which 
                            a gene product performs a function, either cellular compartments (e.g., <i>mitochondrion</i>), 
                            or stable macromolecular complexes of which they are parts (e.g., the <i>ribosome</i>). 
                            Unlike the other aspects of GO, cellular component classes refer not to processes 
                            but rather a cellular anatomy.</span></div>
                        <p style="margin-top:1em; margin-bottom:1em; font-size:larger;"><b>Several tools assist in 
                        Cellular Component analysis:</b></p>
                        <ul><li>The Table shows the cluster representatives and cluster members (indented smaller cursive text), 
                                additional statistical data and downloadable Term Similarity Matrix useful for further analysis.</li>
                            <li>The Scatterplot shows the cluster representatives (i.e. terms remaining after the redundancy reduction) 
                                in a two dimensional space derived by applying multidimensional scaling to a matrix of 
                                the GO term semantic similarities.</li>
                            <li>The 3D Scatterplot adds additional dimension while analyzing the cluster representatives and their semantic similarities.</li>
                            <li>The Interactive graph-based visualization. Each of the GO terms is a node in the graph, 
                                and 3% of the strongest GO term pairwise similarities are designated as edges in the graph. 
                                We found that the threshold value of 3% strikes a good balance between over-connected graphs 
                                with no visible subgroups on the one hand, and very fragmented graphs with 
                                too many small groups on the other hand. The placement of the nodes is determined 
                                by the ForceDirected layout algorithm as implemented in Cytoscape Web control.</li>
                            <li>The Tree Map shows a two-level hierarchy of GO terms - the cluster representatives 
                                from the Scatterplot and the Interactive graph are here joined into several very high-level groups.</li></ul>
                    </div>
                </div>
                <div id="tabResultsCCTable">
                    <div id="pnlCCTable" style="display:none; margin: 10px auto 0 auto;">
                        <div id="pnlCCTableContent"></div>
                        <p><a download="Revigo_CC_OnScreenTable.tsv" href="#" onclick='$(this).attr("href", revigoState.nsTabs[1].tabs[1].drawObj.GetEncodedContents()); return true;'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;width:22px;height:22px;vertical-align:middle;" />
                                Export table (as is shown on screen) to TSV<sup style="color:red">1,2</sup></a><br />
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aTable&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.CELLULAR_COMPONENT %>'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;margin-bottom:10px;width:22px;height:22px;vertical-align:middle;" />
                                Export raw table to TSV<sup style="color:red">1,2</sup></a><br />
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aSimMat&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.CELLULAR_COMPONENT %>'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;width:22px;height:22px;vertical-align:middle;" />
                                Export Term Similarity Matrix</a></p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlCCTableLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlCCTableMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsCCScatterplot">
                    <div id="pnlCCScatterplot" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlCCScatterplotContent" style="display: table; width: 900px; border: 1px solid black; font-family: Calibri,Trebuchet,sans-serif; font-size: 9pt;"></div>
                        <p style="font-size: 9pt; color: red; width: 900px;">The axes in the plot have no intrinsic meaning. 
                            Revigo uses Multidimensional Scaling (MDS) to reduce the dimensionality of a matrix of the GO terms pairwise semantic similarities. 
                            The resulting projection may be highly non linear. The guiding principle is that semantically similar GO terms should remain close together in the plot. 
                            Repeated runs of Revigo may yield different arrangements, but the term distances will remain similar.</p>
                        <p><a style="margin-right: 30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aRScatterplot&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.CELLULAR_COMPONENT %>'><img alt="R" src="<%=Request.ApplicationPath %>Images/development-r.png" style="vertical-align:middle; margin-right:4px;" />Export to R script for plotting<sup style="color:red">1</sup></a>
                            <a style="margin-right:30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aScatterplot&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.CELLULAR_COMPONENT %>'><img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to TSV<sup style="color:red">1,2</sup></a></p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlCCScatterplotLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlCCScatterplotMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsCCScatterplot3D">
                    <div id="pnlCCScatterplot3D" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlCCScatterplot3DContent" style="display:table; margin:0 auto 0 auto; font-family: Calibri,Trebuchet,sans-serif; font-size: 9pt;"></div>
                        <p style="font-size: 9pt; color: red; width: 900px;">The axes in the plot have no intrinsic meaning.
                            Revigo uses Multidimensional Scaling (MDS) to reduce the dimensionality of a matrix of the GO terms pairwise semantic similarities. 
                            The resulting projection may be highly non linear. The guiding principle is that semantically similar GO terms should remain close together in the plot. 
                            Repeated runs of Revigo may yield different arrangements, but the term distances will remain similar.</p>
	                    <h4>Interaction with plot (left mouse click inside plot):</h4>
	                    <ul>
                            <li>Hoover mouse over bubble to see a Term ID and description</li>
		                    <li>Hold left mouse button and move mouse to rotate</li>
		                    <li>Scroll (or hold right mouse button and move mouse) to pan (zoom)</li>
                            <li>Hold Ctrl key + left mouse button and move mouse to move view</li>
		                    <li>Press 'r' key to reset view</li>
		                    <li>View more navigation options on <a href="https://www.x3dom.org/documentation/interaction/" target="_blank">x3dom interaction</a> page</li>
	                    </ul>
                        <p><a style="margin-right:30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aScatterplot3D&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.CELLULAR_COMPONENT %>'>
                            <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />
                            Export to TSV<sup style="color:red">1,2</sup></a>
                            <a id="ExportCCX3D" download="Revigo_CC_3DScatterplot.x3d" href="#" onclick='return ExportX3D("ExportCCX3D", revigoState.nsTabs[1].tabs[2].drawObj);'>
                                <img alt="X3D" src="<%=Request.ApplicationPath %>Images/x3d.png" style="width:24px;height:24px;vertical-align:middle; margin-right:4px;" />
                                Export to X3D<sup style="color:red">1</sup></a>
                        </p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlCCScatterplot3DLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlCCScatterplot3DMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsCCCytoscape">
                    <div id="pnlCCCytoscape" style="margin: 0 auto 0 auto; display: none;">
                        <p style="color: red;">The color of the bubble corresponds to the Value you provided alongside GO Term in your dataset.<br />
                            The size of the bubble corresponds to the LogSize value for the GO Term.</p>
                        <p style="font-size: 9pt">(Navigate with mouse. Click and hold to move view and drag items. Scroll to zoom.)</p>
                        <div id="pnlCCCytoscapeContent" style="width:900px; height:600px; border: 1px solid black;"></div>
                        <p><a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aXGMML&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.CELLULAR_COMPONENT %>'><img alt="XGMML" src="<%=Request.ApplicationPath %>Images/Cytoscape.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to Cytoscape XGMML for offline use</a></p>
                    </div>
                    <div id="pnlCCCytoscapeLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlCCCytoscapeMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsCCTreeMap">
                    <div id="pnlCCTreeMap" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlCCTreeMapContent" style="position:relative; width:900px; height:600px; border: 1px solid black; padding: 10px; margin:0px 40px 0px 40px; font-family: Calibri,Verdana,Arial,sans-serif; font-size: 12px;"></div>
                        <p><a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aTreeMap&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.CELLULAR_COMPONENT %>' style="margin-right: 30px;"><img alt="CSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to TSV<sup style="color:red">1,2</sup></a>
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aRTreeMap&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.CELLULAR_COMPONENT %>' style="margin-right: 30px;"><img alt="R" src="<%=Request.ApplicationPath %>Images/development-r.png" style="vertical-align:middle; margin-right:4px;" />Export to R script for plotting<sup style="color:red">2</sup></a>
                            <a id="ExportCCSVG" download="Revigo_CC_TreeMap.svg" href="#" onclick='return ExportSvg("ExportCCSVG", "pnlCCTreeMapContent", revigoState.nsTabs[1].treeMapData);'><img alt="SVG" src="<%=Request.ApplicationPath %>Images/inkscape.png" style="vertical-align:middle; margin-right:4px;" />Export to SVG<sup style="color:red">2</sup></a></p>
                        <p style="color: red;"><sup>1</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.<br />
                            <sup>2</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.</p>
                    </div>
                    <div id="pnlCCTreeMapLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlCCTreeMapMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
            </div>
            <div id="tabMFResults" class="ResultTab">
                <ul>
                    <li><a href="#tabResultsMFDescription" style="font-size:10pt;">Description</a></li>
                    <li><a href="#tabResultsMFTable" style="font-size:10pt;">Table</a></li>
		            <li><a href="#tabResultsMFScatterplot" style="font-size:10pt;">Scatterplot</a></li>
                    <li><a href="#tabResultsMFScatterplot3D" style="font-size:10pt;">3D Scatterplot</a></li>
		            <li><a href="#tabResultsMFCytoscape" style="font-size:10pt;">Interactive Graph</a></li>
	    	        <li><a href="#tabResultsMFTreeMap" style="font-size:10pt;">Tree Map</a></li>
    	        </ul>
                <div id="tabResultsMFDescription">
                    <div id="pnlMFDescription" style="margin: 10px auto 0 auto;">
                        <div style="display:table;"><img src="<%=Request.ApplicationPath %>Images/MolecularFunction.png" alt="Molecular Function image" 
                            width="264" height="190" style="float:left; margin:0.5em;"/>
                        <div style="margin-top:0.5em; margin-bottom:0.5em; font-size:larger;"><b>Molecular Functions</b></div><br />
                        <span style="font-size:larger;">Molecular-level activities performed by gene products. Molecular function terms describe 
                        activities that occur at the molecular level, such as "catalysis" or "transport". 
                        GO molecular function terms represent activities rather than the entities (molecules or complexes) 
                        that perform the actions, and do not specify where, when, or in what context the action takes place. 
                        Molecular functions generally correspond to activities that can be performed by 
                        individual gene products (i.e. a protein or RNA), but some activities are performed 
                        by molecular complexes composed of multiple gene products. Examples of broad functional 
                        terms are <i>catalytic activity</i> and <i>transporter activity</i>; examples of narrower functional terms 
                        are <i>adenylate cyclase activity</i> or <i>Toll-like receptor binding</i>. To avoid confusion 
                        between gene product names and their molecular functions, GO molecular functions 
                        are often appended with the word “activity” (a protein kinase would have 
                        the GO molecular function protein kinase activity).</span></div>
                        <p style="margin-top:1em; margin-bottom:1em; font-size:larger;"><b>Several tools assist in 
                        Molecular Function analysis:</b></p>
                        <ul><li>The Table view shows the cluster representatives and cluster members (indented smaller cursive text), 
                                additional statistical data and downloadable Term Similarity Matrix useful for further analysis.</li>
                            <li>The Scatterplot shows the cluster representatives (i.e. terms remaining after the redundancy reduction) 
                                in a two dimensional space derived by applying multidimensional scaling to a matrix of 
                                the GO term semantic similarities.</li>
                            <li>The 3D Scatterplot adds additional dimension while analyzing the cluster representatives and their semantic similarities.</li>
                            <li>The Interactive graph-based visualization. Each of the GO terms is a node in the graph, 
                                and 3% of the strongest GO term pairwise similarities are designated as edges in the graph. 
                                We found that the threshold value of 3% strikes a good balance between over-connected graphs 
                                with no visible subgroups on the one hand, and very fragmented graphs with 
                                too many small groups on the other hand. The placement of the nodes is determined 
                                by the ForceDirected layout algorithm as implemented in Cytoscape Web control.</li>
                            <li>The Tree Map shows a two-level hierarchy of GO terms - the cluster representatives 
                                from the Scatterplot and the Interactive graph are here joined into several very high-level groups.</li></ul>
                    </div>
                </div>
                <div id="tabResultsMFTable">
                    <div id="pnlMFTable" style="display:none; margin: 10px auto 0 auto;">
                        <div id="pnlMFTableContent"></div>
                        <p><a download="Revigo_MF_OnScreenTable.tsv" href="#" onclick='$(this).attr("href", revigoState.nsTabs[2].tabs[1].drawObj.GetEncodedContents()); return true;'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;width:22px;height:22px;vertical-align:middle;" />
                                Export table (as is shown on screen) to TSV<sup style="color:red">1,2</sup></a><br />
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aTable&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.MOLECULAR_FUNCTION %>'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;margin-bottom:10px;width:22px;height:22px;vertical-align:middle;" />
                                Export raw table to TSV<sup style="color:red">1,2</sup></a><br />
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aSimMat&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.MOLECULAR_FUNCTION %>'>
                                <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" style="margin:5px 4px 5px 0px;width:22px;height:22px;vertical-align:middle;" />
                                Export Term Similarity Matrix</a></p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlMFTableLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlMFTableMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsMFScatterplot">
                    <div id="pnlMFScatterplot" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlMFScatterplotContent" style="display: table; width: 900px; border: 1px solid black; font-family: Calibri,Trebuchet,sans-serif; font-size: 9pt;"></div>
                        <p style="font-size: 9pt; color: red; width: 900px;">The axes in the plot have no intrinsic meaning. 
                            Revigo uses Multidimensional Scaling (MDS) to reduce the dimensionality of a matrix of the GO terms pairwise semantic similarities. 
                            The resulting projection may be highly non linear. The guiding principle is that semantically similar GO terms should remain close together in the plot. 
                            Repeated runs of Revigo may yield different arrangements, but the term distances will remain similar.</p>
                        <p><a style="margin-right: 30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aRScatterplot&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.MOLECULAR_FUNCTION %>'><img alt="R" src="<%=Request.ApplicationPath %>Images/development-r.png" style="vertical-align:middle; margin-right:4px;" />Export to R script for plotting<sup style="color:red">1</sup></a>
                            <a style="margin-right:30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aScatterplot&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.MOLECULAR_FUNCTION %>'><img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to TSV<sup style="color:red">1,2</sup></a></p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlMFScatterplotLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlMFScatterplotMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsMFScatterplot3D">
                    <div id="pnlMFScatterplot3D" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlMFScatterplot3DContent" style="display:table; margin:0 auto 0 auto; font-family: Calibri,Trebuchet,sans-serif; font-size: 9pt;"></div>
                        <p style="font-size: 9pt; color: red; width: 900px;">The axes in the plot have no intrinsic meaning.
                            Revigo uses Multidimensional Scaling (MDS) to reduce the dimensionality of a matrix of the GO terms pairwise semantic similarities. 
                            The resulting projection may be highly non linear. The guiding principle is that semantically similar GO terms should remain close together in the plot. 
                            Repeated runs of Revigo may yield different arrangements, but the term distances will remain similar.</p>
	                    <h4>Interaction with plot (left mouse click inside plot):</h4>
	                    <ul>
                            <li>Hoover mouse over bubble to see a Term ID and description</li>
		                    <li>Hold left mouse button and move mouse to rotate</li>
		                    <li>Scroll (or hold right mouse button and move mouse) to pan (zoom)</li>
                            <li>Hold Ctrl key + left mouse button and move mouse to move view</li>
		                    <li>Press 'r' key to reset view</li>
		                    <li>View more navigation options on <a href="https://www.x3dom.org/documentation/interaction/" target="_blank">x3dom interaction</a> page</li>
	                    </ul>
                        <p><a style="margin-right:30px;" href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aScatterplot3D&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.MOLECULAR_FUNCTION %>'>
                            <img alt="TSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />
                            Export to TSV<sup style="color:red">1,2</sup></a>
                            <a id="ExportMFX3D" download="Revigo_MF_3DScatterplot.x3d" href="#" onclick='return ExportX3D("ExportMFX3D", revigoState.nsTabs[2].tabs[2].drawObj);'>
                                <img alt="X3D" src="<%=Request.ApplicationPath %>Images/x3d.png" style="width:24px;height:24px;vertical-align:middle; margin-right:4px;" />
                                Export to X3D<sup style="color:red">1</sup></a>
                        </p>
                        <p style="color: red;"><sup>1</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.<br />
                            <sup>2</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.</p>
                    </div>
                    <div id="pnlMFScatterplot3DLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlMFScatterplot3DMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsMFCytoscape">
                    <div id="pnlMFCytoscape" style="margin: 0 auto 0 auto; display: none;">
                        <p style="color: red;">The color of the bubble corresponds to the Value you provided alongside GO Term in your dataset.<br />
                            The size of the bubble corresponds to the LogSize value for the GO Term.</p>
                        <p style="font-size: 9pt">(Navigate with mouse. Click and hold to move view and drag items. Scroll to zoom.)</p>
                        <div id="pnlMFCytoscapeContent" style="width:900px; height:600px; border: 1px solid black;"></div>
                        <p><a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aXGMML&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.MOLECULAR_FUNCTION %>'><img alt="XGMML" src="<%=Request.ApplicationPath %>Images/Cytoscape.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to Cytoscape XGMML file for offline use</a></p>
                    </div>
                    <div id="pnlMFCytoscapeLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlMFCytoscapeMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
                <div id="tabResultsMFTreeMap">
                    <div id="pnlMFTreeMap" style="margin: 0 auto 0 auto; display: none;">
                        <div id="pnlMFTreeMapContent" style="position:relative; width:900px; height:600px; border: 1px solid black; padding: 10px; margin:0px 40px 0px 40px; font-family: Calibri,Verdana,Arial,sans-serif; font-size: 12px;"></div>
                        <p><a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aTreeMap&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.MOLECULAR_FUNCTION %>' style="margin-right: 30px;"><img alt="CSV" src="<%=Request.ApplicationPath %>Images/spreadsheet.png" width="22" height="22" style="vertical-align:middle; margin-right:4px;" />Export to TSV<sup style="color:red">1,2</sup></a>
                            <a href='<%=Request.ApplicationPath %>QueryJob.aspx?<%=string.Format("JobID={0}", this.iJobID) %>&type=aRTreeMap&namespace=<%=(int)IRB.Revigo.Databases.GONamespaceEnum.MOLECULAR_FUNCTION %>' style="margin-right: 30px;"><img alt="R" src="<%=Request.ApplicationPath %>Images/development-r.png" style="vertical-align:middle; margin-right:4px;" />Export to R script for plotting<sup style="color:red">2</sup></a>
                            <a id="ExportMFSVG" download="Revigo_MF_TreeMap.svg" href="#" onclick='return ExportSvg("ExportMFSVG", "pnlMFTreeMapContent", revigoState.nsTabs[2].treeMapData);'><img alt="SVG" src="<%=Request.ApplicationPath %>Images/inkscape.png" style="vertical-align:middle; margin-right:4px;" />Export to SVG<sup style="color:red">2</sup></a></p>
                        <p style="color: red;"><sup>1</sup> Please see the explanation for NULL values in "Representative" column on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q06'>Frequently Asked Questions</a> page.<br />
                            <sup>2</sup> Please see our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q07'>Frequently Asked Questions</a> page for choices on creating a high quality drawing.</p>
                    </div>
                    <div id="pnlMFTreeMapLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                        <div id="pnlMFTreeMapMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                            Please wait, loading data...
                        </div>
                    </div>
                </div>
            </div>
            <div id="tabCloudResults" class="ResultTab">
                <div id="pnlCloud1Image" style="display:none;">
                    <p><img src="<%=Request.ApplicationPath %>Images/cloud.png" width="60" height="60" alt="Cloud"/>Frequent keywords within your set of GO terms:</p>
                </div>
                <div id="pnlCloud1Content" class="termclouddiv" style="display:none;"></div>
                <div id="pnlCloud2Image" style="display:none;">
                    <p><img src="<%=Request.ApplicationPath %>Images/cloud.png" width="60" height="60" alt="Cloud"/>Keywords that correlate with the value you provided alongside GO terms:</p>
                </div>
                <div id="pnlCloud2Content" class="termclouddiv" style="display:none;"></div>
                <div id="pnlCloudLoading" style="display:table; position:relative; margin: 0 auto 0 auto; width: 900px; height:100px; border: 2px solid silver; border-radius: 6px;">
                    <div id="pnlCloudMessage" style="display:block; position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); font-size: larger; color:darkorange;">
                        Please wait, loading data...
                    </div>
                </div>
            </div>
            <div id="tabReport" class="ResultTab">
                <ul>
		            <li><a href="#tabReportMessages" style="font-size:10pt;">Messages/Warnings</a></li>
		            <li id="tabErrors"><a href="#tabReportErrors" style="font-size:10pt;">Errors</a></li>
    	        </ul>
                <div id="tabReportMessages" style="min-width:900px; max-width:1000px;">
                    <div>
                        <p>Revigo took <span id="lblExecTime">...</span> to process this query.</p>
                        <p id="lblWarnings" style="color:gray; font-size:larger; margin-bottom:5px;">Warnings:</p>
                        <div id="lstWarnings"></div>
                    </div>
                </div>
                <div id="tabReportErrors" style="min-width:900px; max-width:1000px;">
                    <p style="font-size: larger; color: red;">If this problem(s) persist please <asp:HyperLink ID="HyperLink2" runat="server" NavigateUrl="~/Contact.aspx">Contact Us</asp:HyperLink>.</p>
                    <div id="lstErrors"></div>
                </div>
            </div>
            <script type="text/javascript">
                var cytoscapeStyle=[
                    {
                        selector:'node',
                        style:{
                            'width':'mapData(log_size,10,70,10,70)',
                            'height':'mapData(log_size,10,70,10,70)',
                            'background-color':'data(color)',
                            'label':'data(label)',
                            'shape':'ellipse',
                            'border-width':1,
                            'border-style':'solid',
                            'border-color':'#000'}
                    },
                    {
                        selector:'edge',
                        style:{
                            'width':'mapData(weight,1,6,1,6)',
                            'line-color':'#ccc',
                            'curve-style':'bezier'
                        }
                    }
                ];
                var cytoscapeLayout={
                    name:'cose',
                    animate:false,
                    idealEdgeLength:100,
                    nodeOverlap:20,
                    refresh:20,
                    fit:true,
                    padding:10,
                    randomize:false,
                    componentSpacing:100,
                    nodeRepulsion:400000,
                    edgeElasticity:100,
                    nestingFactor:5,
                    gravity:80,
                    numIter:1000,
                    initialTemp:1000,
                    coolingFactor:0.95,
                    minTemp:1.0
                };

                var revigoState = {
                    id: "tabResults",
                    index: 0,
                    nsTabs: [
                        {
                            id: "tabBP", tabID: "tabBPResults", lblID: "lblBPTab", namespace: 1, index: 0, visible: 0, count: 0,
                            tabs: [
                                {
                                    id: "pnlBPTable", contentID: "pnlBPTableContent", loadingID: "pnlBPTableLoading",
                                    loadingMsgID: "pnlBPTableMessage", drawObj: undefined
                                },
								{
									id: "pnlBPScatterplot", contentID: "pnlBPScatterplotContent", loadingID: "pnlBPScatterplotLoading",
									loadingMsgID: "pnlBPScatterplotMessage", drawObj: undefined
								},
                                {
                                    id: "pnlBPScatterplot3D", contentID: "pnlBPScatterplot3DContent", loadingID: "pnlBPScatterplot3DLoading",
                                    loadingMsgID: "pnlBPScatterplot3DMessage", drawObj: undefined
                                },
                                {
                                    id: "pnlBPCytoscape", contentID: "pnlBPCytoscapeContent", loadingID: "pnlBPCytoscapeLoading",
                                    loadingMsgID: "pnlBPCytoscapeMessage", drawObj: undefined
                                },
                                {
                                    id: "pnlBPTreeMap", contentID: "pnlBPTreeMapContent", loadingID: "pnlBPTreeMapLoading",
                                    loadingMsgID: "pnlBPTreeMapMessage", drawObj: undefined
                                }
                            ],
                            tableErrorCount: 0,
                            tableTimer: undefined,
                            tableData: undefined,

                            scatterplotErrorCount: 0,
							scatterplotTimer: undefined,
							scatterplotData: undefined,

							scatterplot3DErrorCount: 0,
							scatterplot3DTimer: undefined,
							scatterplot3DData: undefined,

                            cytoscapeErrorCount: 0,
                            cytoscapeTimer: undefined,
                            cytoscapeData: undefined,

                            treeMapErrorCount: 0,
                            treeMapTimer: undefined,
                            treeMapData: undefined
                        },
                        {
                            id: "tabCC", tabID: "tabCCResults", lblID: "lblCCTab", namespace: 2, index: 0, visible: 0, count: 0,
                            tabs: [
                                {
                                    id: "pnlCCTable", contentID: "pnlCCTableContent", loadingID: "pnlCCTableLoading",
                                    loadingMsgID: "pnlCCTableMessage", drawObj: undefined
                                },
								{
									id: "pnlCCScatterplot", contentID: "pnlCCScatterplotContent", loadingID: "pnlCCScatterplotLoading",
									loadingMsgID: "pnlCCScatterplotMessage", drawObj: undefined
								},
                                {
                                    id: "pnlCCScatterplot3D", contentID: "pnlCCScatterplot3DContent", loadingID: "pnlCCScatterplot3DLoading",
                                    loadingMsgID: "pnlCCScatterplot3DMessage", drawObj: undefined
                                },
                                {
                                    id: "pnlCCCytoscape", contentID: "pnlCCCytoscapeContent", loadingID: "pnlCCCytoscapeLoading",
                                    loadingMsgID: "pnlCCCytoscapeMessage", drawObj: undefined
                                },
                                {
                                    id: "pnlCCTreeMap", contentID: "pnlCCTreeMapContent", loadingID: "pnlCCTreeMapLoading",
                                    loadingMsgID: "pnlCCTreeMapMessage", drawObj: undefined
                                }
                            ],
                            tableErrorCount: 0,
                            tableTimer: undefined,
                            tableData: undefined,

							scatterplotErrorCount: 0,
							scatterplotTimer: undefined,
							scatterplotData: undefined,

							scatterplot3DErrorCount: 0,
							scatterplot3DTimer: undefined,
							scatterplot3DData: undefined,

                            cytoscapeErrorCount: 0,
                            cytoscapeTimer: undefined,
                            cytoscapeData: undefined,

                            treeMapErrorCount: 0,
                            treeMapTimer: undefined,
                            treeMapData: undefined
                        },
                        {
                            id: "tabMF", tabID: "tabMFResults", lblID: "lblMFTab", namespace: 3, index: 0, visible: 0, count: 0,
                            tabs: [
                                {
                                    id: "pnlMFTable", contentID: "pnlMFTableContent", loadingID: "pnlMFTableLoading",
                                    loadingMsgID: "pnlMFTableMessage", drawObj: undefined
                                },
								{
									id: "pnlMFScatterplot", contentID: "pnlMFScatterplotContent", loadingID: "pnlMFScatterplotLoading",
									loadingMsgID: "pnlMFScatterplotMessage", drawObj: undefined
								},
                                {
                                    id: "pnlMFScatterplot3D", contentID: "pnlMFScatterplot3DContent", loadingID: "pnlMFScatterplot3DLoading",
                                    loadingMsgID: "pnlMFScatterplot3DMessage", drawObj: undefined
                                },
                                {
                                    id: "pnlMFCytoscape", contentID: "pnlMFCytoscapeContent", loadingID: "pnlMFCytoscapeLoading",
                                    loadingMsgID: "pnlMFCytoscapeMessage", drawObj: undefined
                                },
                                {
                                    id: "pnlMFTreeMap", contentID: "pnlMFTreeMapContent", loadingID: "pnlMFTreeMapLoading",
                                    loadingMsgID: "pnlMFTreeMapMessage", drawObj: undefined
                                }
                            ],
                            tableErrorCount: 0,
                            tableTimer: undefined,
                            tableData: undefined,

							scatterplotErrorCount: 0,
							scatterplotTimer: undefined,
							scatterplotData: undefined,

							scatterplot3DErrorCount: 0,
							scatterplot3DTimer: undefined,
							scatterplot3DData: undefined,

                            cytoscapeErrorCount: 0,
                            cytoscapeTimer: undefined,
                            cytoscapeData: undefined,

                            treeMapErrorCount: 0,
                            treeMapTimer: undefined,
                            treeMapData: undefined
                        }
                    ],
                    cloudTab: {
                        id: "tabCloud", tabID: "tabCloudResults", initialized: 0,
                        cloud1ImgID: "pnlCloud1Image", cloud1ContentID: "pnlCloud1Content",
                        cloud2ImgID: "pnlCloud2Image", cloud2ContentID: "pnlCloud2Content",
                        loadingID: "pnlCloudLoading", loadingMsgID: "pnlCloudMessage",

                        cloudErrorCount: 0,
                        cloudTimer: undefined,
                        cloudData: undefined
                    },

                    statusErrorCount: 0,
                    statusTimer: undefined,

                    pinTermErrorCount: 0,
					pinTermTimer: undefined,

                    infoErrorCount: 0,
                    infoTimer: undefined,
                    infoData: undefined
                };

                var allNodeState = 0;
                var inRefreshTabs = 0;

				LoadStatus();

				function LoadStatus()
                {
					$("#pnlLoading").css("display", "table");
					$("#pnlBack").css("display", "none");
					$("#tabResults").css("display", "none");

                    revigoState.statusErrorCount = 0;
                    LoadStatusRec();

                    function LoadStatusRec()
                    {
                        $.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jstatus")
                            .done(function (status)
                            {
                                if (status && !status.error)
                                {
                                    revigoState.statusErrorCount = 0;
                                    if (status.running == 1)
                                    {
                                        if (!revigoState.statusTimer)
											revigoState.statusTimer = setInterval(LoadStatusRec, 2000);

                                        $("#txtProgress").text(
                                            status.progress + "%" + (status.message ? " (" + status.message + ")" : ""));
                                    }
                                    else
                                    {
                                        if (revigoState.statusTimer)
                                            clearTimeout(revigoState.statusTimer);
                                        revigoState.statusTimer = undefined;

                                        $("#txtProgress").text("Finished processing job, loading results");
                                        LoadJob();
                                    }
                                }
                                else
                                {
                                    if (!revigoState.statusTimer)
										revigoState.statusTimer = setInterval(LoadStatusRec, 2000);

                                    revigoState.statusErrorCount++;
                                    if (revigoState.statusErrorCount > 10)
                                    {
                                        if (revigoState.statusTimer)
                                            clearTimeout(revigoState.statusTimer);
                                        revigoState.statusTimer = undefined;

                                        $("#txtProgress").text("Error parsing job status. Please refresh the page.");
                                    }
                                    else
                                    {
                                        $("#txtProgress").text("Error parsing job status. Retrying " + revigoState.statusErrorCount + " of 10.");
                                    }
                                }
                            })
                            .fail(function ()
                            {
                                if (!revigoState.statusTimer)
									revigoState.statusTimer = setInterval(LoadStatusRec, 2000);

                                revigoState.statusErrorCount++;
                                if (revigoState.statusErrorCount > 10)
                                {
                                    if (revigoState.statusTimer)
                                        clearTimeout(revigoState.statusTimer);
                                    revigoState.statusTimer = undefined;

                                    $("#txtProgress").text("Error querying job status. Please refresh the page.");
                                }
                                else
                                {
                                    $("#txtProgress").text("Error querying job status. Retrying " + revigoState.statusErrorCount + " of 10.");
                                }
                            });
                    }
                }

                function LoadJob()
                {
					revigoState.infoErrorCount = 0;

                    LoadJobRec();

                    function LoadJobRec()
                    {
                        $.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jinfo")
                            .done(function (info)
                            {
                                if (info && !info.error)
                                {
                                    if (revigoState.infoTimer)
                                        clearTimeout(revigoState.infoTimer);

                                    revigoState.infoErrorCount = 0;
                                    revigoState.infoTimer = undefined;
                                    revigoState.infoData = info;

                                    // populate results and intialize controls
									inRefreshTabs = 1;
                                    if ($("#tabResults").tabs("instance"))
                                        $("#tabResults").tabs("destroy");
                                    if ($("#tabReport").tabs("instance"))
                                        $("#tabReport").tabs("destroy");

                                    revigoState.nsTabs[0].visible = info.HasBP;
                                    revigoState.nsTabs[0].count = info.BPCount;
                                    revigoState.nsTabs[1].visible = info.HasCC;
                                    revigoState.nsTabs[1].count = info.CCCount;
                                    revigoState.nsTabs[2].visible = info.HasMF;
                                    revigoState.nsTabs[2].count = info.MFCount;
                                    revigoState.cloudTab.visible = info.HasClouds;

                                    for (let i = 0; i < revigoState.nsTabs.length; i++)
                                    {
                                        let nsTab = revigoState.nsTabs[i];

                                        if ($("#" + nsTab.tabID).tabs("instance"))
                                            $("#" + nsTab.tabID).tabs("destroy");

                                        if (nsTab.visible)
                                        {
                                            $("#" + nsTab.lblID).text(nsTab.count);
                                            $("#" + nsTab.id).removeClass("ui-state-hidden");
                                            $("#" + nsTab.id).css("display", "")
                                            $("#" + nsTab.tabID).removeClass("ui-state-hidden");
                                            $("#" + nsTab.tabID).css("display", "")
                                            $("#" + nsTab.tabID).tabs();
                                        }
                                        else
                                        {
                                            $("#" + nsTab.id).addClass("ui-state-hidden");
                                            $("#" + nsTab.id).css("display", "none")
                                            $("#" + nsTab.tabID).addClass("ui-state-hidden");
                                            $("#" + nsTab.tabID).css("display", "none")
                                        }

                                        for (let j = 0; j < nsTab.tabs.length; j++)
                                        {
                                            $("#" + nsTab.tabs[j].id).css("display", "none");
                                            $("#" + nsTab.tabs[j].contentID).empty();
                                            $("#" + nsTab.tabs[j].loadingID).css("display", "table");
                                            $("#" + nsTab.tabs[j].loadingMsgID).text("Please wait, loading data...");
                                            nsTab.tabs[j].drawObj = undefined;
                                        }

                                        nsTab.tableErrorCount = 0;
                                        nsTab.tableTimer = undefined;
                                        nsTab.tableData = undefined;

                                        nsTab.scatterplotErrorCount = 0;
                                        nsTab.scatterplotTimer = undefined;
										nsTab.scatterplotData = undefined;

										nsTab.scatterplot3DErrorCount = 0;
										nsTab.scatterplot3DTimer = undefined;
										nsTab.scatterplot3DData = undefined;

                                        nsTab.cytoscapeErrorCount = 0;
                                        nsTab.cytoscapeTimer = undefined;
                                        nsTab.cytoscapeData = undefined;

                                        nsTab.treeMapErrorCount = 0;
                                        nsTab.treeMapTimer = undefined;
                                        nsTab.treeMapData = undefined;
                                    }

                                    let cloudTab = revigoState.cloudTab;

                                    if (cloudTab.visible)
                                    {
                                        $("#" + cloudTab.id).removeClass("ui-state-hidden");
                                        $("#" + cloudTab.id).css("display", "")
                                        $("#" + cloudTab.tabID).removeClass("ui-state-hidden");
                                        $("#" + cloudTab.tabID).css("display", "")
                                    }
                                    else
                                    {
                                        $("#" + cloudTab.id).addClass("ui-state-hidden");
                                        $("#" + cloudTab.id).css("display", "none")
                                        $("#" + cloudTab.tabID).addClass("ui-state-hidden");
                                        $("#" + cloudTab.tabID).css("display", "none")
                                    }

                                    $("#" + cloudTab.cloud1ImgID).css("display", "none");
                                    $("#" + cloudTab.cloud1ContentID).css("display", "none");
                                    $("#" + cloudTab.cloud1ContentID).empty();
                                    $("#" + cloudTab.cloud2ImgID).css("display", "none");
                                    $("#" + cloudTab.cloud2ContentID).css("display", "none");
                                    $("#" + cloudTab.cloud2ContentID).empty();
                                    $("#" + cloudTab.loadingID).css("display", "table");
                                    $("#" + cloudTab.loadingMsgID).text("Please wait, loading data...");

                                    cloudTab.cloudErrorCount = 0;
                                    cloudTab.cloudTimer = undefined;
                                    cloudTab.cloudData = undefined;

                                    // populate report tab
                                    let minutes = Math.floor(info.exectime / 60);
                                    if (minutes > 0)
                                    {
                                        let seconds = info.exectime - minutes * 60;
                                        $("#lblExecTime").text(minutes + " minute(s) and " + seconds + " second(s)");
                                    }
                                    else
                                    {
                                        $("#lblExecTime").text(info.exectime + " second(s)");
                                    }

                                    // populate warning tab
                                    let wLength = info.warnings.length;
                                    $("#lstWarnings").empty();
                                    $("#lblWarnings").css("display", wLength > 0 ? "" : "none");
                                    if (wLength > 0)
                                    {
                                        var ulWarnings = $("<ul></ul>").appendTo("#lstWarnings");

                                        for (let i = 0; i < wLength; i++)
                                        {
                                            ulWarnings.append("<li>" + info.warnings[i] + "</li>");
                                        }
                                    }

                                    // populate error tab
                                    let eLength = info.errors.length;
                                    $("#lstErrors").empty();
                                    if (eLength > 0)
                                    {
                                        $("#tabErrors").css("display", "");
                                        $("#tabErrors").removeClass("ui-state-hidden");
                                        $("#tabReportErrors").css("display", "");
                                        $("#tabReportErrors").removeClass("ui-state-hidden");

                                        var ulErrors = $("<ul></ul>").appendTo("#lstErrors");

                                        for (let i = 0; i < eLength; i++)
                                        {
                                            ulErrors.append("<li>" + info.errors[i] + "</li>");
                                        }
                                    }
                                    else
                                    {
                                        $("#tabErrors").css("display", "none");
                                        $("#tabErrors").addClass("ui-state-hidden");
                                        $("#tabReportErrors").css("display", "none");
                                        $("#tabReportErrors").addClass("ui-state-hidden");
                                    }

                                    // populate warning/error label
                                    if (wLength > 0 && eLength > 0)
                                    {
                                        $("#lblReportTab").text(" (" + wLength + " Warning(s), " + eLength + " Error(s))");
                                    }
                                    else if (wLength > 0)
                                    {
                                        $("#lblReportTab").text(" (" + wLength + " Warning(s))");
                                    }
                                    else if (eLength > 0)
                                    {
                                        $("#lblReportTab").text(" (" + eLength + " Error(s))");
                                    }
                                    else
                                    {
                                        $("#lblReportTab").text("");
                                    }

                                    $("#tabReport").tabs();
                                    if (eLength > 0)
                                    {
                                        $("#tabReport").tabs("option", "active", 1);
                                    }

                                    // initialize main tabs
                                    $("#pnlLoading").css("display", "none");
                                    $("#pnlBack").css("display", "");
                                    $("#tabResults").css("display", "");
                                    $("#tabResults").tabs();

                                    // stupid tab control doesn't know how to properly initialize with hidden tabs
                                    if (!revigoState.nsTabs[0].visible && (revigoState.index === 0))
                                    {
                                        revigoState.index = 1;
                                    }
                                    if (!revigoState.nsTabs[1].visible && (revigoState.index === 1))
                                    {
                                        revigoState.index = 2;
                                    }
                                    if (!revigoState.nsTabs[2].visible && (revigoState.index === 2))
                                    {
                                        revigoState.index = 3;
                                    }
                                    if (!revigoState.cloudTab.visible && (revigoState.index === 3))
                                    {
                                        revigoState.index = 4;
                                    }

                                    // restore tab positions
                                    $("#tabResults").tabs("option", "active", revigoState.index);
                                    for (let i = 0; i < revigoState.nsTabs.length; i++)
                                    {
                                        let tab = revigoState.nsTabs[i];
                                        if (tab.visible)
                                            $("#" + tab.tabID).tabs("option", "active", tab.index);
                                    }

                                    inRefreshTabs = 0;

                                    // set tab activate listener, the same event fires when child tab controls change
                                    $("#tabResults").on("tabsactivate", function (event, ui)
                                    {
                                        RefreshTabs();
                                    });

                                    RefreshTabs();
                                }
                                else
                                {
                                    if (!revigoState.infoTimer)
                                    {
										revigoState.infoTimer = setInterval(LoadJobRec, 2000);
                                    }

                                    revigoState.infoErrorCount++;
                                    if (revigoState.infoErrorCount > 10)
                                    {
                                        if (revigoState.infoTimer)
                                            clearTimeout(revigoState.infoTimer);
                                        revigoState.infoTimer = undefined;

                                        $("#txtProgress").text("Error parsing job information. Please refresh the page.");
                                    }
                                    else
                                    {
                                        $("#txtProgress").text("Error parsing job information. Retrying " + revigoState.infoErrorCount + " of 10.");
                                    }
                                }
                            })
                            .fail(function ()
                            {
                                if (!revigoState.infoTimer)
                                {
									revigoState.infoTimer = setInterval(LoadJobRec, 2000);
                                }

                                revigoState.infoErrorCount++;
                                if (revigoState.infoErrorCount > 10)
                                {
                                    if (revigoState.infoTimer)
                                        clearTimeout(revigoState.infoTimer);
                                    revigoState.infoTimer = undefined;

                                    $("#txtProgress").text("Error querying job information. Please refresh the page.");
                                }
                                else
                                {
                                    $("#txtProgress").text("Error querying job information. Retrying " + revigoState.infoErrorCount + " of 10.");
                                }
                            });
                    }
                }

                function RefreshTabs()
                {
                    if (!inRefreshTabs)
                    {
                        // prevent recursion as a precaution
                        inRefreshTabs = true;

                        let tabIndex = $("#" + revigoState.id).tabs("option", "active");
                        revigoState.index = tabIndex;

                        if (tabIndex >= 0 && tabIndex < 3)
                        {
                            var tab = revigoState.nsTabs[tabIndex];
                            var index = $("#" + tab.tabID).tabs("option", "active");
                            tab.index = index;

                            // load results as needed
                            if (index == 1)
                            {
								LoadTable(tab, index - 1);
                            }
                            else if (index == 2)
                            {
								LoadScatterplot(tab, index - 1);
                            }
							else if (index == 3)
							{
								LoadScatterplot3D(tab, index - 1);
							}
                            else if (index == 4)
                            {
								LoadCytoscape(tab, index - 1);
                            }
                            else if (index == 5)
                            {
								LoadTreeMap(tab, index - 1);
                            }
                        }
                        else if (tabIndex == 3)
                        {
                            // load cloud results
                            LoadClouds();
                        }

                        inRefreshTabs = false;
                    }
                }

                // data loaders
				function LoadTable(tab, index)
                {
					var messageDiv = $("#" + tab.tabs[index].loadingMsgID);

                    if (!tab.tableData)
                    {
                        if (tab.tableErrorCount <= 10)
                        {
							messageDiv.text("Please wait, loading data...");
                            LoadTableRec();
                        }
                    }
                    else
                    {
                        InitTableCtrl(tab, index);
                    }

					function LoadTableRec()
                    {
						$.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jtable&namespace=" + tab.namespace)
                            .done(function (table)
                            {
                                if (table && !table.error)
                                {
                                    if (tab.tableTimer)
										clearTimeout(tab.tableTimer);
									tab.tableTimer = undefined;

                                    tab.tableData = table;
									InitTableCtrl(tab, index);
                                }
                                else
                                {
									if (!tab.tableTimer)
                                    {
										tab.tableTimer = setInterval(LoadTableRec, 2000);
                                    }

									tab.tableErrorCount++;
									if (tab.tableErrorCount > 10)
                                    {
										if (tab.tableTimer)
											clearTimeout(tab.tableTimer);
										tab.tableTimer = undefined;

										messageDiv.text("Error parsing Table data. Please refresh the page.");
                                    }
                                    else
                                    {
										messageDiv.text("Error parsing Table data. Retrying " + tab.tableErrorCount + " of 10.");
                                    }
                                }
                            })
                            .fail(function ()
                            {
								if (!tab.tableTimer)
                                {
									tab.tableTimer = setInterval(LoadTableRec, 2000);
                                }

								tab.tableErrorCount++;
								if (tab.tableErrorCount > 10)
                                {
									if (tab.tableTimer)
										clearTimeout(tab.tableTimer);
									tab.tableTimer = undefined;

									messageDiv.text("Error loading Table data. Please refresh the page.");
                                }
                                else
                                {
									messageDiv.text("Error loading Table data. Retrying " + tab.tableErrorCount + " of 10.");
                                }
                            });
                    }
                }

				function LoadScatterplot(tab, index)
				{
					var messageDiv = $("#" + tab.tabs[index].loadingMsgID);

                    if (!tab.scatterplotData)
                    {
                        if (tab.scatterplotErrorCount <= 10)
						{
							messageDiv.text("Please wait, loading data...");
							LoadScatterplotRec();
						}
					}
					else
					{
						InitScatterplotCtrl(tab, index);
					}

					function LoadScatterplotRec()
					{
						$.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jscatterplot&namespace=" + tab.namespace)
							.done(function (scatterplot)
							{
								if (scatterplot && !scatterplot.error)
                                {
                                    if (tab.scatterplotTimer)
										clearTimeout(tab.scatterplotTimer);
									tab.scatterplotTimer = undefined;

									tab.scatterplotData = scatterplot;
									InitScatterplotCtrl(tab, index);
								}
								else
								{
									if (!tab.scatterplotTimer)
									{
										tab.scatterplotTimer = setInterval(LoadScatterplotRec, 2000);
									}

                                    tab.scatterplotErrorCount++;
                                    if (tab.scatterplotErrorCount > 10)
									{
										if (tab.scatterplotTimer)
											clearTimeout(tab.scatterplotTimer);
										tab.scatterplotTimer = undefined;

										messageDiv.text("Error parsing Scatterplot data. Please refresh the page.");
									}
									else
									{
										messageDiv.text("Error parsing Scatterplot data. Retrying " + tab.scatterplotErrorCount + " of 10.");
									}
								}
							})
							.fail(function ()
							{
								if (!tab.scatterplotTimer)
								{
									tab.scatterplotTimer = setInterval(LoadScatterplotRec, 2000);
								}

								tab.scatterplotErrorCount++;
								if (tab.scatterplotErrorCount > 10)
								{
									if (tab.scatterplotTimer)
										clearTimeout(tab.scatterplotTimer);
									tab.scatterplotTimer = undefined;

									messageDiv.text("Error loading Scatterplot data. Please refresh the page.");
								}
								else
								{
									messageDiv.text("Error loading Scatterplot data. Retrying " + tab.scatterplotErrorCount + " of 10.");
								}
							});
					}
                }

				function LoadScatterplot3D(tab, index)
				{
					var messageDiv = $("#" + tab.tabs[index].loadingMsgID);

					if (!tab.scatterplot3DData)
					{
						if (tab.scatterplot3DErrorCount <= 10)
						{
							messageDiv.text("Please wait, loading data...");
							LoadScatterplot3DRec();
						}
					}
					else
					{
						InitScatterplot3DCtrl(tab, index);
					}

					function LoadScatterplot3DRec()
					{
						$.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jscatterplot3d&namespace=" + tab.namespace)
							.done(function (scatterplot3D)
							{
								if (scatterplot3D && !scatterplot3D.error)
								{
									if (tab.scatterplot3DTimer)
										clearTimeout(tab.scatterplot3DTimer);
									tab.scatterplot3DTimer = undefined;

									tab.scatterplot3DData = scatterplot3D;
									InitScatterplot3DCtrl(tab, index);
								}
								else
								{
									if (!tab.scatterplot3DTimer)
									{
										tab.scatterplot3DTimer = setInterval(LoadScatterplot3DRec, 2000);
									}

									tab.scatterplot3DErrorCount++;
									if (tab.scatterplot3DErrorCount > 10)
									{
										if (tab.scatterplot3DTimer)
											clearTimeout(tab.scatterplot3DTimer);
										tab.scatterplot3DTimer = undefined;

										messageDiv.text("Error parsing Scatterplot 3D data. Please refresh the page.");
									}
									else
									{
										messageDiv.text("Error parsing Scatterplot 3D data. Retrying " + tab.scatterplot3DErrorCount + " of 10.");
									}
								}
							})
							.fail(function ()
							{
								if (!tab.scatterplot3DTimer)
								{
									tab.scatterplot3DTimer = setInterval(LoadScatterplot3DRec, 2000);
								}

								tab.scatterplot3DErrorCount++;
								if (tab.scatterplot3DErrorCount > 10)
								{
									if (tab.scatterplot3DTimer)
										clearTimeout(tab.scatterplot3DTimer);
									tab.scatterplot3DTimer = undefined;

									messageDiv.text("Error loading Scatterplot 3D data. Please refresh the page.");
								}
								else
								{
									messageDiv.text("Error loading Scatterplot 3D data. Retrying " + tab.scatterplot3DErrorCount + " of 10.");
								}
							});
					}
                }

                function LoadCytoscape(tab, index)
                {
					var messageDiv = $("#" + tab.tabs[index].loadingMsgID);

                    if (!tab.cytoscapeData)
					{
                        if (tab.cytoscapeErrorCount <= 10)
                        {
                            messageDiv.text("Please wait, loading data...");
                            LoadCytoscapeRec();
                        }
					}
					else
					{
						InitCytoscapeCtrl(tab, index);
					}

					function LoadCytoscapeRec()
                    {
						$.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jcytoscape&namespace=" + tab.namespace)
							.done(function (cytoscape)
                            {
								if (cytoscape && !cytoscape.error)
                                {
                                    if (tab.cytoscapeTimer)
										clearTimeout(tab.cytoscapeTimer);
									tab.cytoscapeTimer = undefined;

                                    tab.cytoscapeData = cytoscape;
                                    InitCytoscapeCtrl(tab, index);
                                }
                                else
                                {
									if (!tab.cytoscapeTimer)
                                    {
										tab.cytoscapeTimer = setInterval(LoadCytoscapeRec, 2000);
                                    }

									tab.cytoscapeErrorCount++;
									if (tab.cytoscapeErrorCount > 10)
                                    {
										if (tab.cytoscapeTimer)
											clearTimeout(tab.cytoscapeTimer);
										tab.cytoscapeTimer = undefined;

                                        messageDiv.text("Error parsing Cytoscape data. Please refresh the page.");
                                    }
                                    else
                                    {
										messageDiv.text("Error parsing Cytoscape data. Retrying " + tab.cytoscapeErrorCount + " of 10.");
                                    }
                                }
                            })
                            .fail(function ()
                            {
								if (!tab.cytoscapeTimer)
                                {
									tab.cytoscapeTimer = setInterval(LoadCytoscapeRec, 2000);
                                }

								tab.cytoscapeErrorCount++;
								if (tab.cytoscapeErrorCount > 10)
                                {
									if (tab.cytoscapeTimer)
										clearTimeout(tab.cytoscapeTimer);
									tab.cytoscapeTimer = undefined;

									messageDiv.text("Error loading Cytoscape data. Please refresh the page.");
                                }
                                else
                                {
									messageDiv.text("Error loading Cytoscape data. Retrying " + tab.cytoscapeErrorCount + " of 10.");
                                }
                            });
                    }
                }

				function LoadTreeMap(tab, index)
                {
					var messageDiv = $("#" + tab.tabs[index].loadingMsgID);

                    if (!tab.treeMapData)
                    {
                        if (tab.treeMapErrorCount <= 10)
                        {
                            messageDiv.text("Please wait, loading data...");
                            LoadTreeMapRec();
                        }
					}
					else
					{
						InitTreeMapCtrl(tab, index);
					}

					function LoadTreeMapRec()
                    {
						$.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jtreemap&namespace=" + tab.namespace)
							.done(function (treeMap)
                            {
								if (treeMap && !treeMap.error)
                                {
									if (tab.treeMapTimer)
										clearTimeout(tab.treeMapTimer);
									tab.treeMapTimer = undefined;

									tab.treeMapData = treeMap;
									InitTreeMapCtrl(tab, index);
                                }
                                else
                                {
									if (!tab.treeMapTimer)
                                    {
										tab.treeMapTimer = setInterval(LoadTreeMapRec, 2000);
                                    }

									tab.treeMapErrorCount++;
									if (tab.treeMapErrorCount > 10)
                                    {
										if (tab.treeMapTimer)
											clearTimeout(tab.treeMapTimer);
										tab.treeMapTimer = undefined;

										messageDiv.text("Error parsing Tree Map data. Please refresh the page.");
                                    }
                                    else
                                    {
										messageDiv.text("Error parsing Tree Map data. Retrying " + tab.treeMapErrorCount + " of 10.");
                                    }
                                }
                            })
                            .fail(function ()
                            {
								if (!tab.treeMapTimer)
                                {
									tab.treeMapTimer = setInterval(LoadTreeMapRec, 2000);
                                }

								tab.treeMapErrorCount++;
								if (tab.treeMapErrorCount > 10)
                                {
									if (tab.treeMapTimer)
										clearTimeout(tab.treeMapTimer);
									tab.treeMapTimer = undefined;

									messageDiv.text("Error loading Tree Map data. Please refresh the page.");
                                }
                                else
                                {
									messageDiv.text("Error loading Tree Map data. Retrying " + tab.treeMapErrorCount + " of 10.");
                                }
                            });
                    }
                }

				function LoadClouds()
                {
                    var tab = revigoState.cloudTab;
					var messageDiv = $("#" + tab.loadingMsgID);

                    if (!tab.cloudData)
					{
                        if (tab.cloudErrorCount <= 10)
                        {
                            messageDiv.text("Please wait, loading data...");
                            LoadCloudsRec();
                        }
					}
					else
					{
						InitCloudCtrl(tab);
					}

					function LoadCloudsRec()
					{
						$.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jclouds")
							.done(function (clouds)
							{
								if (clouds && !clouds.error)
								{
									if (tab.cloudTimer)
										clearTimeout(tab.cloudTimer);
									tab.cloudTimer = undefined;

									tab.cloudData = clouds;
									InitCloudCtrl(tab);
								}
								else
								{
									if (!tab.cloudTimer)
									{
										tab.cloudTimer = setInterval(LoadCloudsRec, 2000);
									}

									tab.cloudErrorCount++;
									if (tab.cloudErrorCount > 10)
									{
										if (tab.cloudTimer)
											clearTimeout(tab.cloudTimer);
										tab.cloudTimer = undefined;

										messageDiv.text("Error parsing Cloud data. Please refresh the page.");
									}
									else
									{
										messageDiv.text("Error parsing Cloud data. Retrying " + tab.cloudErrorCount + " of 10.");
									}
								}
							})
							.fail(function ()
							{
								if (!tab.cloudTimer)
								{
									tab.cloudTimer = setInterval(LoadCloudsRec, 2000);
								}

								tab.cloudErrorCount++;
								if (tab.cloudErrorCount > 10)
								{
									if (tab.cloudTimer)
										tab.cloudTimer(tab.cloudTimer);
									tab.cloudTimer = undefined;

									messageDiv.text("Error loading Cloud data. Please refresh the page.");
								}
								else
								{
									messageDiv.text("Error loading Cloud data. Retrying " + tab.cloudErrorCount + " of 10.");
								}
							});
					}
				}

                function PinTermLoader(termID, namespace)
                {
                    $("#pnlLoading").css("display", "table");
                    $("#pnlBack").css("display", "none");
                    $("#tabResults").css("display", "none");

                    revigoState.pinTermErrorCount = 0;
                    PinTermLoaderRec();

                    function PinTermLoaderRec()
                    {
                        $.get("<%=Request.ApplicationPath%>QueryJob.aspx?jobid=<%=this.iJobID.ToString()%>&type=jpinterm&namespace=" + namespace + "&termid=" + termID)
                            .done(function (status)
                            {
                                if (status && !status.error)
                                {
                                    revigoState.pinTermErrorCount = 0;
                                    if (revigoState.pinTermTimer)
                                        clearTimeout(revigoState.pinTermTimer);
                                    revigoState.pinTermTimer = undefined;

                                    LoadStatus();
                                }
                                else
                                {
                                    if (!revigoState.pinTermTimer)
                                        revigoState.pinTermTimer = setInterval(PinTermLoaderRec, 2000);

                                    revigoState.pinTermErrorCount++;
                                    if (revigoState.pinTermErrorCount > 10)
                                    {
                                        if (revigoState.pinTermTimer)
                                            clearTimeout(revigoState.pinTermTimer);
                                        revigoState.pinTermTimer = undefined;

                                        $("#txtProgress").text("Error requesting pin term. Please refresh the page.");
                                    }
                                    else
                                    {
                                        $("#txtProgress").text("Error requesting pin term. Retrying " + revigoState.pinTermErrorCount + " of 10.");
                                    }
                                }
                            })
                            .fail(function ()
                            {
                                if (!revigoState.pinTermTimer)
                                    revigoState.pinTermTimer = setInterval(PinTermLoaderRec, 2000);

                                revigoState.pinTermErrorCount++;
                                if (revigoState.pinTermErrorCount > 10)
                                {
                                    if (revigoState.pinTermTimer)
                                        clearTimeout(revigoState.pinTermTimer);
                                    revigoState.pinTermTimer = undefined;

                                    $("#txtProgress").text("Error requesting pin term. Please refresh the page.");
                                }
                                else
                                {
                                    $("#txtProgress").text("Error requesting pin term. Retrying " + revigoState.pinTermErrorCount + " of 10.");
                                }
                            });
                    }
                }

                // control initializers
				function InitTableCtrl(tab, index)
                {
                    let subTab = tab.tabs[index];

					if (!subTab.drawObj)
					{
						$("#" + subTab.loadingMsgID).text("Please wait, loading view...");
						$("#" + subTab.loadingID).css("display", "none");
						$("#" + subTab.id).css("display", "");

						subTab.drawObj = new RevigoTable(subTab.contentID, tab.tableData);
						let jTable = $("#" + subTab.contentID + " table");
						jTable.on("pinterm", function (e, termID)
						{
							console.log("Pin term event: " + termID)
							PinTermLoader(termID, tab.namespace);
						});
					}
				}

				function InitScatterplotCtrl(tab, index)
                {
                    let subTab = tab.tabs[index];

                    if (!subTab.drawObj)
                    {
                        $("#" + subTab.loadingMsgID).text("Please wait, loading view...");
                        $("#" + subTab.loadingID).css("display", "none");
                        $("#" + subTab.id).css("display", "");

                        subTab.drawObj = new BubbleChart(subTab.contentID, tab.scatterplotData);
                        subTab.drawObj.draw('Value', 'LogSize');
                    }
                }

				function InitScatterplot3DCtrl(tab, index)
                {
                    let subTab = tab.tabs[index];

                    if (!subTab.drawObj)
                    {
						$("#" + subTab.loadingMsgID).text("Please wait, loading view...");
                        $("#" + subTab.loadingID).css("display", "none");
                        $("#" + subTab.id).css("display", "");

                        subTab.drawObj = new x3dScatterplot(subTab.contentID, 800, 600, tab.scatterplot3DData,
                            ['Value', 'LogSize', 'Frequency', 'Uniqueness', 'Dispensability'], 'LogSize', 'Value', false);

                        x3dom.reload();
                    }
                }

                function InitCytoscapeCtrl(tab, index)
                {
                    let subTab = tab.tabs[index];

                    if (!subTab.drawObj)
                    {
						$("#" + subTab.loadingMsgID).text("Please wait, loading view...");
						$("#" + subTab.loadingID).css("display", "none");
						$("#" + subTab.id).css("display", "");

						subTab.drawObj = cytoscape({
							container: document.getElementById(subTab.contentID), layout: cytoscapeLayout, style: cytoscapeStyle, elements: tab.cytoscapeData
                        });
                    }
                }

                function InitTreeMapCtrl(tab, index)
                {
                    let subTab = tab.tabs[index];

                    if (!subTab.drawObj)
                    {
						$("#" + subTab.loadingMsgID).text("Please wait, loading view...");
                        $("#" + subTab.loadingID).css("display", "none");
                        $("#" + subTab.id).css("display", "");

                        subTab.drawObj = new TreeMap(subTab.contentID, tab.treeMapData, 12, true, false);
                    }
                }

                function InitCloudCtrl(tab)
                {
					if (!tab.initialized)
                    {
						$("#" + tab.loadingMsgID).text("Please wait, loading view...");
                        $("#" + tab.loadingID).css("display", "none");

                        if (tab.cloudData.Enrichments && tab.cloudData.Enrichments.length > 0)
                        {
                            let enrichments = tab.cloudData.Enrichments;

                            var cloud = $("<div></div>").appendTo("#" + tab.cloud1ContentID);
                            cloud.addClass("term-cloud");

                            for (let i = 0; i < enrichments.length; i++)
                            {
                                let link = $("<a></a>").appendTo(cloud);
                                link.addClass("term-cloud-link");
                                link.attr("href", "http://www.ebi.ac.uk/QuickGO/search/" + enrichments[i].Word);
                                link.attr("target", "_blank");

                                let span = $("<span></span>").appendTo(link);
                                span.addClass("term-cloud-" + enrichments[i].Size);
                                span.text(enrichments[i].Word);

                                cloud.append(" ");
                            }

                            $("#" + tab.cloud1ImgID).css("display", "");
                            $("#" + tab.cloud1ContentID).css("display", "");
                        }

                        if (tab.cloudData.Correlations && tab.cloudData.Correlations.length > 0)
                        {
                            let correlations = tab.cloudData.Correlations;

                            var cloud = $("<div></div>").appendTo("#" + tab.cloud2ContentID);
                            cloud.addClass("term-cloud");

                            for (let i = 0; i < correlations.length; i++)
                            {
                                let link = $("<a></a>").appendTo(cloud);
                                link.addClass("term-cloud-link");
                                link.attr("href", "http://www.ebi.ac.uk/QuickGO/search/" + correlations[i].Word);
                                link.attr("target", "_blank");

                                let span = $("<span></span>").appendTo(link);
                                span.addClass("term-cloud-" + correlations[i].Size);
                                span.text(correlations[i].Word);

                                cloud.append(" ");
                            }

                            $("#" + tab.cloud2ImgID).css("display", "");
                            $("#" + tab.cloud2ContentID).css("display", "");
                        }
						tab.initialized = 1;
                    }
                }
			</script>
        </div>
        <div style="display:table; width:100%; text-align:center; margin-top: 20px;">Did you find Revigo useful in your work? Please share it with your colleagues.<br /><div style="margin-top:5px;" class="addthis_inline_share_toolbox"></div></div>
    </div>
    <asp:PlaceHolder ID="phAddThis" runat="server" EnableTheming="false" EnableViewState="false" Visible="false">
        <script type="text/javascript" src="//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-601aabd7f2793f3b"></script>
    </asp:PlaceHolder>
</asp:Content>