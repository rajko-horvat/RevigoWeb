<%@ Page Language="C#" MasterPageFile="~/RevigoMasterPage.master" AutoEventWireup="true" 
    Inherits="RevigoWeb.Contact" CodeBehind="Contact.aspx.cs" %>

<asp:Content ContentPlaceHolderID="MasterHeaderContent" runat="server">
    <title>Contact page</title>
    <script type="text/jscript" src="https://www.google.com/recaptcha/api.js" async defer></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MasterContent" runat="server">
    <div style="max-width:950px; margin-left: auto; margin-right: auto" >
        <asp:Label ID="lblError" Font-Size="14pt" runat="server" ForeColor="Red" Visible="false" EnableViewState="false" />
	    <asp:Panel ID="pnlComment" runat="server">
            <div class="contactForm-container contactForm-container-full">
	        <noscript class="contactForm-error-noscript">Please enable JavaScript in your browser to complete this form.</noscript>
	        <div class="contactForm-field-container">
	            <div class="contactForm-field">
                    <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName" CssClass="contactForm-field-label">Your name <span class="contactForm-required-label">*</span></asp:Label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="contactForm-field-medium"></asp:TextBox>
	            </div>
	            <div class="contactForm-field">
                    <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="contactForm-field-label">E-mail address <span class="contactForm-required-label">*</span></asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="contactForm-field-medium"></asp:TextBox>
	            </div>
	            <div class="contactForm-field">
                    <asp:Label ID="lblMessage" runat="server" AssociatedControlID="txtMessage" CssClass="contactForm-field-label">Message <span class="contactForm-required-label">*</span></asp:Label>
                    <asp:TextBox ID="txtMessage" runat="server" CssClass="contactForm-field-medium" Rows="10" TextMode="MultiLine"></asp:TextBox>
	            </div>
	            <div class="contactForm-field">
                    <asp:Label ID="lblGDPR" runat="server" AssociatedControlID="chkGDPR" CssClass="contactForm-field-label">GDPR consent <span class="contactForm-required-label">*</span></asp:Label>
                    <asp:CheckBox ID="chkGDPR" runat="server" Text="I agree that this web page can process and store my personal information to be able to answer my inquiry." Checked="false" EnableViewState="false" />
	            </div>
	        </div>
	        <div class="contactForm-recaptcha-container" >
	            <div class="g-recaptcha" data-sitekey='<%=ConfigurationManager.AppSettings["RecaptchaPublicKey"] %>'></div>
	        </div>
	        <div class="contactForm-submit-container" >
	            <asp:Button ID="cmdSubmit" runat="server" Text="Send" OnClick="cmdSubmit_Click" />
	        </div>
            </div>
	    </asp:Panel>
        <script type="text/javascript">
            $('#<% =this.cmdSubmit.ClientID %>').button();
        </script>
    </div>
</asp:Content>