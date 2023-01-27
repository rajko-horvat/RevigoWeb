<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error404.aspx.cs" Inherits="RevigoWeb.Errors.Error404" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" >
  <head>
    <meta http-equiv="Content-Type" content="text/html;charset=UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="icon" type="image/png" href="/favicon.png" />
    <script type="text/javascript" src="/js/jquery-3.6.0.min.js"></script>
    <link href="/js/jquery-ui-1.13.1/jquery-ui.css" rel="stylesheet"/>
    <script type="text/javascript" src="/js/jquery-ui-1.13.1/jquery-ui.js"></script>
    <link href="/css/styles.css" rel="stylesheet" type="text/css" />

    <title>REVIGO error page</title>
  </head>

	<body style="background-image: url('/Images/wavy_top_short_fade.png'); background-repeat: no-repeat">
		<form id="form1" runat="server">
			<div>
				<table class="logo">
				  <tr>
					  <td>
						  <a href="/"><img src="/Images/revigo_logo_transparent_2.png" alt="Revigo Logo" width="402" height="114" /></a>
					  </td>
					  <td style="width:95px;">
						  <a href="http://www.irb.hr/eng" target="_blank"><img id="IRBLogo" alt="Ruđer Bošković Institute" src="/Images/irb_logo_new_95x82.png" title="Ruđer Bošković Institute" /></a>
					  </td>
				  </tr>
				</table>
			</div>
			<div style="margin-top:3em; margin-left: auto; margin-right: auto; display: table;">
				<h2><img src="/Images/PullingHair.jpg" alt="Pulling Hair" align="middle" width="192" height="136" /> Oops, the page you are trying to access does not exist.</h2>
				<p style="text-align:center;">If you think this should not happen you can <a href="/Contact.aspx">Contact Us</a> with details.</p>
			</div>
		</form>
	</body>
</html>
