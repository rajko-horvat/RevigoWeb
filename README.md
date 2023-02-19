## Repository description
<p>This is the main REVIGO web service that implements the web interface and visualizations.</p>
<p>Since a lot of changes had to be made for .NET Core. Official release with binaries is planned for mid February 2023</p>

## How to compile and run
<p>To compile and run the web service you need the compiled RevigoCore library, a set of precompiled databases: 
	<a href="http://revigo.irb.hr/Databases/GeneOntology.xml.gz" target="_blank">Gene Ontology</a> and 
	<a href="http://revigo.irb.hr/Databases/SpeciesAnnotations.xml.gz" target="_blank">Species annotations</a>, 
	or build your own databases with <a href="https://github.com/rajko-horvat/RevigoGenerateDatabases">RevigoGenerateDatabases</a> command line utility.</p>
<p>The following JavaScript libraries are also needed: 
	<a href="https://jquery.com/download/">JQuery</a>, 
	<a href="https://jqueryui.com/download/">JQuery-UI</a> (with Accordion, Button, Tabs, Dialog, Datepicker, Selectmenu and Tooltip widgets), 
	<a href="https://d3js.org/">D3</a>, <a href="https://www.x3dom.org/nodes/">X3Dom</a>, <a href="https://github.com/LCweb-ita/LC-switch">LCSwitch</a> and 
	<a href="https://github.com/cytoscape/cytoscape.js">Cytoscape</a>.</p>
<p>For the Statistics functionality you also need the <a href="https://github.com/mysql-net/MySqlConnector/tree/v2.1">MySqlConnector 2.1 library</a> 
	(Under project -> Buld -> Compilation Symbols the 'WEB_STATISTICS' constant needs to be defined).</p>
<p>The web server can be run as a command line utility, worker service or windows service by specifying 'WINDOWS_SERVICE' or 'WORKER_SERVICE' constant.</p>

<p>To compile from command line: 
<ul>
	<li>Optional: Install <a href="https://visualstudio.microsoft.com/">Visual Studio Code</a> or <a href="https://visualstudio.microsoft.com/">Visual Studio for Windows</a> (You can also compile from Visual Studio for Windows)</li>
	<li>Install .NET core 6.0 from Microsoft (<a href="https://dotnet.microsoft.com/download">Install .NET for Windows</a>, <a href="https://learn.microsoft.com/en-us/dotnet/core/install/linux">Install .NET for Linux</a>)</li>
	<li>git clone https://github.com/rajko-horvat/RevigoCore</li>
	<li>git clone https://github.com/rajko-horvat/RevigoWeb</li>
	<li>Configure 'DefineConstants' section in RevigoWeb.csproj file (available constants: 'WEB_STATISTICS', 'WINDOWS_SERVICE', 'WORKER_SERVICE' or without any constant)</li>
	<li>Configure 'appsettings.json' file (rename from 'appsettings.example.json' or 'appsettings.Development.example.json'), specifically 'AppSettings' and 'AppPaths' section.</li>
	<li>dotnet build --configuration Release --os win-x64 RevigoWeb.csproj (For Linux use --os linux. See <a href="https://learn.microsoft.com/en-us/dotnet/core/rid-catalog">list of OS RIDs</a> for --os option)</li>
	<li>Copy entire wwwroot directory to the binary directory (for example: cp -r wwwroot ./bin/net6.0/linux-x64/)</li>
	<li>Run generated binary file (under RevigoWeb/bin/net6.0/) and enjoy.</li>
</ul></p>

## About REVIGO (REduce + VIsualize Gene Ontology) project
<p>Outcomes of high-throughput biological experiments are typically interpreted by statistical testing
for enriched gene functional categories defined by the Gene Ontology (GO). The resulting lists of GO terms 
may be large and highly redundant, and thus difficult to interpret.<p>
<p>REVIGO is a successful project to summarize long, unintelligible lists of Gene Ontology terms by finding a representative subset 
of the terms using a simple clustering algorithm that relies on semantic similarity measures.</p>
<p>For any further information about REVIGO project please see 
<a href="https://dx.doi.org/10.1371/journal.pone.0021800" target="_blank">published paper</a> and 
<a href="http://revigo.irb.hr/FAQ.aspx" target="_blank">Frequently Asked Questions page</a></p>