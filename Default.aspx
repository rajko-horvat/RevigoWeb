<%@ Page Language="C#" MasterPageFile="~/RevigoMasterPage.master" AutoEventWireup="true" Inherits="RevigoWeb._Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MasterHeaderContent" runat="server">
    <title>Revigo summarizes and visualizes long lists of Gene Ontology terms</title>
    <style type="text/css">
        .ui-tooltip-content {
            font-size: 13px;
        }
        .ui-tooltip {
            max-width:400px;
            background-color: LightYellow;
        }
        /* select with custom icons */
        .ui-selectmenu-button.ui-button {
	        text-align: left;
	        white-space: nowrap;
	        width: 20em;
        }
        .ui-widget {
            font-size: 0.8rem;
            color:black;
        }
        .ui-selectmenu-menu .ui-menu.customicons .ui-menu-item-wrapper {
            padding: 0.1em;
        }
    </style>
    <asp:Literal ID="litSpeciesCSS" runat="server" EnableViewState="false" Mode="PassThrough"></asp:Literal>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MasterContent" runat="server">
    <div style="width:950px; margin-left: auto; margin-right: auto" >
        <p>Welcome, Revigo can take long lists of Gene Ontology terms and summarize them by removing redundant GO terms. 
            Read more about Revigo on our <a href='<%=Request.ApplicationPath %>FAQ.aspx#q01'>Frequently Asked Questions</a> page.</p>
        <p>Please enter a list of Gene Ontology IDs below, each on its own line. The GO IDs 
            may be followed by value which describes the GO term in a way meaningful to you. 
            The value(s) must have a dot '.' for a decimal separator.
            <img id="tooltip1" title="For instance, you may provide a p-value (statistical significance), a fold change, enrichment, or some directly measured quantity such as average signal intensity from microarrays, ion count from mass spec, or read count from RNA-seq. You may also provide more than one value per line, although only the first value will be used in GO term selection/clustering."
                alt="For instance, you may provide a p-value (statistical significance), a fold change, enrichment, or some directly measured quantity such as average signal intensity from microarrays, ion count from mass spec, or read count from RNA-seq. You may also provide more than one value per line, although only the first value will be used in GO term selection/clustering."
                src="<%=Request.ApplicationPath %>Images/dialog-information.png" style="vertical-align:middle;" />
            <span style="font-size:smaller">(Hoover with a mouse over an icon for additional info)</span>
        </p>

        <script type="text/javascript">
            function fetchExample(n)
            {
                $.get("GetExample.aspx?id="+n, function (data, status)
                {
                    if (status === "success") {
                        $('#<%= this.txtGOInput.ClientID %>').val(data);
                    }
                });
                return false;
            }
        </script>
        <em style="color:gray">Examples:</em>
        <ul style="margin-top: 0;">
            <li><a title="Highly expressed genes across all bacterial genomes, where expression level is predicted by codon usage." href="#"
                    onclick="return fetchExample(1);">Example #1</a>
            (<a href="http://www.plosgenetics.org/article/info:doi/10.1371/journal.pgen.1001004" target="_blank">Supek et al. (2010) PLoS Genet</a>), </li>
            <li><a title="Genes diferentially expressed in HeLa and Hep-2 human tumor cell lines, in comparison to other cell lines." href="#"
                    onclick="return fetchExample(2);">Example #2</a>
            (<a href="http://www.springerlink.com/content/b615211p833j4626/" target="_blank">Ester et al. (2010) Inv New Drug</a>), </li>
            <li><a title="Differentially expressed genes between breast cancer patients with poor vs. good prognosis." href="#"
                    onclick="return fetchExample(3);">Example #3</a>
            (<a href="http://www.nature.com/nature/journal/v415/n6871/full/415530a.html" target="_blank">Van&#39;t Veer et al. (2002) Nature</a>)</li>
        </ul>
        <p><asp:Label ID="lblError" ForeColor="Red" Font-Size="Large" runat="server" 
                Visible="False"></asp:Label></p>
        <asp:TextBox ID="txtGOInput" runat="server" style="width:100%" rows="20" TextMode="MultiLine"></asp:TextBox>
        <p/>

        <p>How large would you like the resulting list to be: 
        <asp:RadioButton ID="chkSimilarity0_9" Text="Large (0.9)" GroupName="cutoff" runat="server"/>
        <asp:RadioButton ID="chkSimilarity0_7" Text="Medium (0.7)" Checked="True" GroupName="cutoff" runat="server"/>
        <asp:RadioButton ID="chkSimilarity0_5" Text="Small (0.5)" GroupName="cutoff" runat="server"/>
        <span>
            <asp:RadioButton ID="chkSimilarity0_4" Text="Tiny (0.4)" GroupName="cutoff" 
                ToolTip="Warning! This setting may remove some GO terms which are not redundant. Please consider a more conservative setting before using this one." runat="server"/>
            <img id="tooltip2" src="<%=Request.ApplicationPath %>Images/dialog-warning.png" alt="Warning" title="Warning! This setting may remove some GO terms which are not redundant. Please consider a more conservative setting before using this one." style="vertical-align:middle;" />
        </span></p>

        <p>If provided, the values associated with GO terms represent: 
        <asp:DropDownList ID="lstValueType" runat="server">
            <asp:ListItem Value="PValue" Selected="True">P value</asp:ListItem>
            <asp:ListItem Value="Higher">Higher value is better</asp:ListItem>
            <asp:ListItem Value="Lower">Lower value is better</asp:ListItem>
            <asp:ListItem Value="HigherAbsolute">Higher absolute value is better</asp:ListItem>
            <asp:ListItem Value="HigherAbsLog2">Higher absolute log2 value is better</asp:ListItem>
        </asp:DropDownList></p>

        <h2>Advanced options:</h2>

        <p>Would you like to remove obsolete GO terms: <asp:DropDownList ID="lstRemoveObsolete" runat="server" >
            <asp:ListItem Value="true">Yes (default)</asp:ListItem>
            <asp:ListItem Value="false">No</asp:ListItem>
            </asp:DropDownList>
            <img id="tooltip5" src="<%=Request.ApplicationPath %>Images/dialog-information.png" alt="Remove obsolete GO terms" title="When the GO term is marked as obsolete it indicates that the term has been deprecated and should not be used. A GO term is obsoleted when it is out of scope, misleadingly named or defined, or describes a concept that would be better represented in another way and needs to be removed from the published ontology. In these cases, the term and ID still persist in the ontology, but the term is tagged as obsolete, and all relationships to other terms are removed. A comment is added to the term detailing the reason for the obsoletion and replacement terms are suggested, if possible." style="vertical-align:middle;" /></p>
        <p>What species would you like to work with:
        <asp:DropDownList ID="lstSpecies" runat="server"></asp:DropDownList>&nbsp;&nbsp;
        <img id="tooltip3" alt="Species" title="The chosen species is used to find the size of each GO term i.e. the percentage of genes annotated with the term. This quantity determines the size of bubbles in the vizualizations, thus indicating a more general GO term (larger) or a more specific one (smaller). The choice of database also has some influence on the GO term clustering/selection process, and on the bubble placement in the visualizations. If your organism is not available, select the closest relative, e.g. human or mouse should work for any mammal. The default choice (Whole UniProt database) should also suffice in most cases."
            src="<%=Request.ApplicationPath %>Images/dialog-information.png" style="vertical-align:middle;" /></p>

        <p>What semantic similarity measure would you like to use:
        <asp:DropDownList ID="lstMeasures" runat="server"></asp:DropDownList>&nbsp;&nbsp;
        <a href="http://funsimmat.bioinf.mpi-inf.mpg.de/help3.php" target="_blank">
        <img id="tooltip4" title="The default is normally a sensible choice. Click the icon to view a web page with detailed information on how these measures are computed."
            alt="Semantic similarity measure"
            src="<%=Request.ApplicationPath %>Images/dialog-information.png" style="vertical-align:middle;" /></a></p>

        <asp:Button ID="btnStart" runat="server" Text="Start Revigo" onclick="btnStart_Click" />

        <div style="margin-top: 35px">
            Revigo uses these databases which are periodically updated:
            <ul><li>The Gene Ontology database (<asp:HyperLink ID="lnkOntology" runat="server" NavigateUrl="#" Target="_blank" EnableViewState="false">Unavailable</asp:HyperLink>) 
            which is dated <asp:Label ID="lblOntologyDate" runat="server" Text="-" EnableViewState="false"></asp:Label>.</li>
            <li>The UniProt-to-GO mapping database from the EBI GOA project 
            (<asp:HyperLink ID="lnkGOA" runat="server" NavigateUrl="#" Target="_blank" EnableViewState="false">Unavailable</asp:HyperLink>) 
            which is dated <asp:Label ID="lblGOADate" runat="server" Text="-" EnableViewState="false"></asp:Label>.</li></ul>
        </div>
        <p/>

        <div class="tinybox">
            <p style="font-size: 0.9rem;"><strong>If you found Revigo useful in your work, please cite the following reference:</strong><br />
            Supek F, Bošnjak M, Škunca N, Šmuc T. &quot;<em>REVIGO summarizes and visualizes long lists of Gene Ontology terms</em>&quot;<br/>
            PLoS ONE 2011. <a href="http://dx.doi.org/10.1371/journal.pone.0021800" target="_blank">doi:10.1371/journal.pone.0021800</a></p>
            <p>Revigo is an iProject funded by the <a href="http://iprojekti.mzos.hr/arh_iprojekata.asp" target="_blank">
                Ministry of Science and Education, Republic of Croatia (2008-057)</a> and <a href="http://www.irb.hr/eng" target="_blank">Ruđer Bošković Institute</a>.<br />
                The Revigo project was implemented at the
                <a href="https://www.irb.hr/eng/Divisions/Division-of-Electronics" target="_blank">Division of electronics</a>, 
                <a href="http://www.irb.hr/eng" target="_blank">Ruđer Bošković Institute</a>.</p>
        </div>
        <div style="display:table; width:100%; text-align:center; margin-top: 20px;">Did you find Revigo useful in your work? Please share it with your colleagues.<br /><div style="margin-top:5px;" class="addthis_inline_share_toolbox"></div></div>
        <script type="text/javascript">
            $( function() {
                $.widget("custom.iconselectmenu", $.ui.selectmenu, {
                    _resizeMenu: function ()
                    {
                        $(this.menu).css("max-width", "550px");
                        $(this.menu).css("max-height", "300px");
                    },
                    _renderItem: function (ul, item)
                    {
                        var li = $("<li>");
                        var wrapper = $("<div>");
                        var imgwrapper = $("<div>", {style:"width:110px; text-align:center; display:inline-block;"});
                        $("<img>", { style: "vertical-align:middle;", src: item.element.attr("data-src"), title: item.label }).appendTo(imgwrapper);
                        wrapper.append(imgwrapper);
                        wrapper.append(item.label);

                        if (item.disabled)
                        {
                            li.addClass("ui-state-disabled");
                        }

                        return li.append(wrapper).appendTo(ul);
                    }
                });

                $('#<%=this.lstSpecies.ClientID %>').iconselectmenu();
            });

            $('#<% =this.btnStart.ClientID %>').button();
            $("#tooltip1").tooltip();
            $("#tooltip2").tooltip();
            $("#tooltip3").tooltip();
            $("#tooltip4").tooltip();
            $("#tooltip5").tooltip();
        </script>
    </div>
    <asp:PlaceHolder ID="phAddThis" runat="server" EnableTheming="false" EnableViewState="false" Visible="false">
        <script type="text/javascript" src="//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-601aabd7f2793f3b"></script>
    </asp:PlaceHolder>
</asp:Content>