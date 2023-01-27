<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="RevigoWeb.WebForm1" %>

<%@ Register assembly="RevigoWeb" namespace="RevigoWeb" tagprefix="cc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        	<cc1:SpinnerControl ID="Spinner1" runat="server" SpinnerType="Spinner1" />
        </div>
    </form>
</body>
</html>
