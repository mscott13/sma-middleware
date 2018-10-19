<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="_000.Secured.ManageUsers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <div class="container">
           <div class="row">
               <div class="col-md-6">
                    <h1>Role Assignment</h1>
            <hr />
                    <div class="login_form">
                    <div class="s1">
                  
                        <asp:DropDownList ID="ddlRoles" runat="server" CssClass="form-control"></asp:DropDownList>
                        
                  </div>
                  <asp:Button ID="Button1" runat="server" Text="Assign Role" class="btn btn-default" OnClick="btnAssignRole_Click"/>
             <hr />
            <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
            </div>
               </div>


               <div class="col-md-6">
                    <h1>User</h1>
            <hr />
                    <div class="login_form">
                    <div class="s1">
                  
                        <asp:TextBox ID="txtUser" runat="server" CssClass="form-control"></asp:TextBox>  
                        
                  </div>
                        <asp:Button ID="btnChkName" runat="server" Text="Check Name" class="btn btn-default" OnClick="btnChkName_Click"/>
             <hr />
            <asp:Label ID="lblError2" runat="server" Text=""></asp:Label>
            </div>
               </div>
           </div>
         </div>
</asp:Content>
