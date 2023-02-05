## Repository description
<p>This is the main REVIGO web service that implements the web interface and visualizations.</p>

## How to compile and run
<p>To compile and run the web service you need the Visual Studio Code (Visual Studio is also fine), RevigoCore library, a set of precompiled databases available 
	<a href="http://revigo.irb.hr/Databases/GeneOntology.xml.gz" target="_blank">here</a> (Gene Ontology) and 
	<a href="http://revigo.irb.hr/Databases/SpeciesAnnotations.xml.gz" target="_blank">here</a> (Species annotations), 
	or build your databases with <a href="https://github.com/rajko-horvat/RevigoGenerateDatabases">RevigoGenerateDatabases command line utility</a>.</p>
	<p>The following JavaScript libraries are also needed: 
	<a href="https://jquery.com/download/">JQuery</a>, 
	<a href="https://jqueryui.com/download/">JQuery-UI</a> (with Accordion, Button, Tabs, Dialog, Datepicker, Selectmenu and Tooltip widgets), 
	<a href="https://d3js.org/">D3</a>, <a href="https://www.x3dom.org/nodes/">X3Dom</a>, <a href="https://github.com/LCweb-ita/LC-switch">LCSwitch</a> and 
	<a href="https://github.com/cytoscape/cytoscape.js">Cytoscape</a>.</p>
	<p>For the Statistics functionality you also need the <a href="https://github.com/mysql-net/MySqlConnector/tree/v2.1">MySqlConnector 2.1 library</a> 
	(Under project -> Buld -> Compilation Symbols the 'STATISTICS' keyword needs to be defined).</p>
	<p>The 'appsettings.json' (Rename from 'appsettings.example.json') also needs to be configured properly, specifically 'AppSettings' and 'AppPaths' section.</p>

## About REVIGO (REduce + VIsualize Gene Ontology) project
<p>Outcomes of high-throughput biological experiments are typically interpreted by statistical testing
for enriched gene functional categories defined by the Gene Ontology (GO). The resulting lists of GO terms 
may be large and highly redundant, and thus difficult to interpret.<p>
<p>REVIGO is a successful project to summarize long, unintelligible lists of Gene Ontology terms by finding a representative subset 
of the terms using a simple clustering algorithm that relies on semantic similarity measures.</p>
<p>For any further information about REVIGO project please see 
<a href="https://dx.doi.org/10.1371/journal.pone.0021800" target="_blank">published paper</a> and 
<a href="http://revigo.irb.hr/FAQ.aspx" target="_blank">Frequently Asked Questions page</a></p>