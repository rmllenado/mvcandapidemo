<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="Webforms._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Title</h2>
        <p>
            <%=ConfigurationManager.AppSettings["Title"] %>
        </p>
        <h2>DefaultConnectionString</h2>
        <p>
            <%=ConfigurationManager.ConnectionStrings["DefaultConnectionString"] %>
        </p>
        <h2>IdentityConnectionString</h2>
        <p>
            <%=ConfigurationManager.ConnectionStrings["IdentityConnectionString"] %>
        </p>
    </form>
</body>
</html>
