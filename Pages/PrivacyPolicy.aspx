<%@ Page Language="C#" MasterPageFile="~/RevigoMasterPage.master" AutoEventWireup="true" CodeBehind="PrivacyPolicy.aspx.cs"
    Inherits="RevigoWeb.PrivacyPolicy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MasterHeaderContent" runat="server">
    <title>Privacy Policy</title>
    <script type="text/javascript" src="<%=Request.ApplicationPath %>js/lc_switch-2.0.3.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MasterContent" runat="server">
    <div style="max-width:950px; margin-left: auto; margin-right: auto" >
        <h2>Privacy Policy</h2>

        <p>Ruđer Bošković Institute is committed to protecting your personal information. This privacy policy aims to help you to understand what information we may collect about you and how we use it.</p>

        <p>This privacy policy applies to anyone who uses our Revigo webpage and its services.</p>

        <p>Please carefully review this Privacy Policy and more generally our <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/TermsOfUse.aspx">Terms of Use</asp:HyperLink> relating to our services. Your use of Revigo web site and its services signifies your consent to our Privacy Policy and our Terms of Use.</p>

        <h2>What data do we collect?</h2>
        <ul>
            <li>We may use cookies to collect non-personally identifiable information in connection with the Revigo web site and its services,</li>
            <li>We may collect any non-personally identifiable telemetry data, such as the name of your internet service provider, the IP address of the computer you are using, the type of browser software and operating system that you use, the date and time you access our web site, the website address, if any, from which you linked directly to our web site, the website address, if any, to which you travel from our website, and other similar traffic-related information. We do not use such data in any way to create or maintain a personal profile of you,</li>
            <li>Non-personally identifiable data such as parameters you use and data set size when you use our Revigo web service,</li>
            <li>Your name and e-mail address when you contact us via our Contact form.</li>
        </ul>

        <h2>How do we collect your data?</h2>

        <p>You directly provide most of the data we collect. We collect data and process data when you:</p>
        <ul>
            <li>Use or view our Revigo web site or its services with your browser's cookies enabled,</li>
            <li>Submit a data set to our Revigo web service,</li>
            <li>Use our Contact form.</li>
        </ul>

        <h2>How will we use your data?</h2>

        <p>We collect your data so that we can:</p>
        <ul>
            <li>Process the data set you submit to our Revigo web service,</li>
            <li>Answer your query submitted via our Contact form,</li>
            <li>Analyze anonymous telemetry data about your visit to our Revigo web site and its services to improve our webpage and its services.</li>
        </ul>

        <h2>How do we store your data?</h2>

        <p>Ruđer Bošković Institute securely stores your data at our private servers, including the data you enter into our Contact form.</p>

        <p>Ruđer Bošković Institute does not keep the data set you submit to our Revigo web service, except for a session time which is currently one (1) hour or less. 
            If the data set you submit causes any error in the application, we may, privately store your non-personally identifiable data set for a purpose of debugging the service. 
            The message you send to us via our Contact form with your name and e-mail address we store privately for a purpose of answering your query, and later archive. 
            You may ask that your message, name and e-mail address be deleted from our archive.</p>

        <p>Ruđer Bošković Institute will not share your private data with any third party, except, with your permission, for a purpose of answering your message submitted via our Contact form.</p>

        <h2>What are your data protection rights?</h2>

        <p>Ruđer Bošković Institute would like to make sure you are fully aware of all of your data protection rights.</p>
        <p>Every user is entitled to the following:</p>
        <ul>
            <li>The right to access - You have the right to request us for copies of your personal data.</li>
            <li>The right to rectification - You have the right to request that we correct any information you believe is inaccurate. You also have the right to request us to complete information you believe is incomplete.</li>
            <li>The right to erasure - You have the right to request that we erase your personal data, under certain conditions.</li>
            <li>The right to restrict processing - You have the right to request that we restrict the processing of your personal data, under certain conditions.</li>
            <li>The right to object to processing - You have the right to object to our processing of your personal data, under certain conditions.</li>
            <li>The right to data portability - You have the right to request that we transfer the data that we have collected to another organization, or directly to you, under certain conditions.</li>
        </ul>

        <p>If you make a request, we have one month to respond to you. If you would like to exercise any of these rights, please <asp:HyperLink ID="HyperLink2" runat="server" NavigateUrl="~/Contact.aspx">Contact Us</asp:HyperLink>.</p>

        <h2>What are cookies?</h2>

        <p>Cookies are text files placed on your computer to collect standard Internet log information and visitor behavior information. When you visit our website or its services, we may collect information from you automatically through cookies or similar technology.
        For further information, visit www.allaboutcookies.org.</p>

        <h2>How do we use cookies?</h2>

        <p>Ruđer Bošković Institute uses cookies in a range of ways to improve your experience on our website and its services, including:</p>
        <ul>
            <li>Keeping you signed in,</li>
            <li>Understanding how you use our website and its services.</li>
        </ul>

        <h2>What types of cookies do we use?</h2>

        <p>There are a number of different types of cookies, however, our website uses:</p>
        <ul>
            <li>Functionality - We use these cookies so that we recognize you on our website and remember your previously selected preferences. These could include what language you prefer and location you are in. A mix of first-party and third-party cookies is used.</li>
        </ul>

        <h2>How to manage cookies</h2>

        <p>You can set your browser not to accept cookies, and the above website tells you how to remove cookies from your browser. However, in a few cases, some of our website features may not function as a result.</p>

        <h2>Privacy policies of other websites</h2>

        <p>Our webpage and its services contain links to other websites. Our privacy policy applies only to our website, so if you click on a link to another website, you should read their privacy policy.</p>

        <h2 id="telemetryOption">Telemetry data <asp:CheckBox ID="chkTelemetry" runat="server" OnCheckedChanged="chkTelemetry_CheckedChanged" AutoPostBack="True" /></h2>

        <p>This webpage uses external services to record information about your visit. 
            The information collected by these services can't be used to identify you in any way, 
            and the accumulated data is exclusively used for statistical purposes. 
            You can enable or disable all telemetry services by clicking On/Off button on this page. 
            If you want to prevent specific service to store cookies in your browser you can disable them on following links:</p>
        <ul>
            <li><a target="_blank" href="https://tools.google.com/dlpage/gaoptout">Google Analytics</a></li>
            <li><a target="_blank" href="https://datacloudoptout.oracle.com/">AddThis</a></li>
        </ul>

        <h2>Changes to our privacy policy</h2>

        <p>We keep this privacy policy under regular review and place any updates on this webpage.</p>

        <h2>How to contact us</h2>

        <p>If you have any questions about our privacy policy, the data we hold on you or you would like to exercise one of your data protection rights, please do not hesitate to <asp:HyperLink ID="HyperLink3" runat="server" NavigateUrl="~/Contact.aspx">Contact Us</asp:HyperLink>.</p>
    </div>
    <script type="text/javascript">
        lc_switch('#<%=this.chkTelemetry.ClientID %>');
        $('#<%=this.chkTelemetry.ClientID %>')[0].addEventListener('lcs-statuschange', (e) => {
            javascript:setTimeout('__doPostBack(\'<%=this.chkTelemetry.UniqueID %>\',\'\')', 0) });
    </script>
    <asp:PlaceHolder ID="phFocus" runat="server" Visible="false" EnableViewState="false">
        <script type="text/javascript">
            $('#telemetryOption')[0].scrollIntoView();
        </script>
    </asp:PlaceHolder>
</asp:Content>
