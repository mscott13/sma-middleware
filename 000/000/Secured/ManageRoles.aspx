<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageRoles.aspx.cs" Inherits="_000.Secured.CreateRole" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
      <div class="container">
           <div class="row">
               <div class="col-md-6">
                    <h1>Create new role</h1>
            <hr />
                    <div class="login_form">
                    <div class="s1">
                        <asp:Label ID="lbl_role" runat="server" Text="Role Name"></asp:Label>
                        <asp:TextBox ID="txtRole" runat="server" CssClass="form-control"></asp:TextBox>
                        
                  </div>
                  <asp:Button ID="Button1" runat="server" Text="Create Role" class="btn btn-default" OnClick="btnCreateRole_Click"/>
             <hr />
            <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
            </div>
               </div>
               <div class="col-md-6">
                    <h1>Delete role</h1>
            <hr />
                    <div class="login_form">
                    <div class="s1">
                        <asp:Label ID="lbl_role2" runat="server" Text="Role Name"></asp:Label>
                        <asp:TextBox ID="txtRole2" runat="server" CssClass="form-control"></asp:TextBox>
                        
                  </div>
                  <asp:Button ID="Button2" runat="server" Text="Delete Role" class="btn btn-default" OnClick="btnDeleteRole_Click"/>
             <hr />
            <asp:Label ID="lblError2" runat="server" Text=""></asp:Label>
            </div>
               </div>
           </div>
    </div>

                 

</asp:Content>
