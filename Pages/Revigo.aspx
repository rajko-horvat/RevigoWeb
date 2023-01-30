<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Revigo.aspx.cs" Inherits="RevigoWeb.RevigoREST" ViewStateMode="Disabled" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <link rel="icon" type="image/png" href="<%=Request.ApplicationPath %>favicon.png" />
    <title>Revigo RESTful results</title>
</head>
<body>
    <form id="form1" runat="server" enableviewstate="false">
        <asp:Label ID="ErrorMessage" runat="server" CssClass="ErrorMessage" Text="" EnableViewState="false"></asp:Label>
        <asp:Repeater ID="dgBPTable" runat="server" OnItemDataBound="dgTable_ItemDataBound" EnableViewState="False" Visible="false">
            <HeaderTemplate>
                <table id="BiologicalProcess">
                <tr>
                    <asp:Repeater ID="dgHeader" runat="server">
                        <ItemTemplate>
                            <th><%# Eval("Name")%></th>
                        </ItemTemplate>
                    </asp:Repeater>
                </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Eval("Term ID") %></td>
                    <td><%# Eval("Name") %></td>
                    <td><%# Convert.ToDouble(Eval("Frequency")).ToString("#####0.000", System.Globalization.CultureInfo.InvariantCulture) %>%</td>
                    <td><%# Convert.ToDouble(Eval("Value")).ToString("#####0.0000", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Convert.ToDouble(Eval("Uniqueness")).ToString("#####0.00", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Convert.ToDouble(Eval("Dispensability")).ToString("#####0.00", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Eval("Eliminated") %></td>
                    <td><%# Eval("Representative") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></table></FooterTemplate>
        </asp:Repeater>
        <asp:Repeater ID="dgCCTable" runat="server" OnItemDataBound="dgTable_ItemDataBound" EnableViewState="False" Visible="false">
            <HeaderTemplate>
                <table id="CellularComponent">
                <tr>
                    <asp:Repeater ID="dgHeader" runat="server">
                        <ItemTemplate>
                            <th><%# Eval("Name")%></th>
                        </ItemTemplate>
                    </asp:Repeater>
                </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Eval("Term ID") %></td>
                    <td><%# Eval("Name") %></td>
                    <td><%# Convert.ToDouble(Eval("Frequency")).ToString("#####0.000", System.Globalization.CultureInfo.InvariantCulture) %>%</td>
                    <td><%# Convert.ToDouble(Eval("Value")).ToString("#####0.0000", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Convert.ToDouble(Eval("Uniqueness")).ToString("#####0.00", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Convert.ToDouble(Eval("Dispensability")).ToString("#####0.00", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Eval("Eliminated") %></td>
                    <td><%# Eval("Representative") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></table></FooterTemplate>
        </asp:Repeater>
        <asp:Repeater ID="dgMFTable" runat="server" OnItemDataBound="dgTable_ItemDataBound" EnableViewState="False" Visible="false">
            <HeaderTemplate>
                <table id="MolecularFunction">
                <tr>
                    <asp:Repeater ID="dgHeader" runat="server">
                        <ItemTemplate>
                            <th><%# Eval("Name")%></th>
                        </ItemTemplate>
                    </asp:Repeater>
                </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Eval("Term ID") %></td>
                    <td><%# Eval("Name") %></td>
                    <td><%# Convert.ToDouble(Eval("Frequency")).ToString("#####0.000", System.Globalization.CultureInfo.InvariantCulture) %>%</td>
                    <td><%# Convert.ToDouble(Eval("Value")).ToString("#####0.0000", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Convert.ToDouble(Eval("Uniqueness")).ToString("#####0.00", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Convert.ToDouble(Eval("Dispensability")).ToString("#####0.00", System.Globalization.CultureInfo.InvariantCulture) %></td>
                    <td><%# Eval("Eliminated") %></td>
                    <td><%# Eval("Representative") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></table></FooterTemplate>
        </asp:Repeater>
    </form>
</body>
</html>
