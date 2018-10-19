using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace _000.Secured
{
    public partial class CreateRole : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnCreateRole_Click(object sender, EventArgs e)
        {
            try
            {
                if (Roles.RoleExists(txtRole.Text))
                {
                    var x = Roles.GetAllRoles();
                    lblError.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FA7B7B");
                    lblError.Text = "Role Exists";
                    
                }
                else
                {
                    Roles.CreateRole(txtRole.Text);
                    lblError.ForeColor = System.Drawing.ColorTranslator.FromHtml("#87CDA3");
                    lblError.Text = "Role Created";
                   
                }
               
            }
            catch(Exception ex)
            {

            }
        }

        protected void btnDeleteRole_Click(object sender, EventArgs e)
        {
            try
            {
                if (Roles.RoleExists(txtRole2.Text))
                {
                    Roles.DeleteRole(txtRole2.Text);
                    lblError2.ForeColor = System.Drawing.ColorTranslator.FromHtml("#87CDA3");
                    lblError2.Text = "Role Deleted";
                }
                else
                {
                    Roles.DeleteRole(txtRole2.Text);
                    lblError2.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FA7B7B");
                    lblError2.Text = "No roles found. Deletion fail";
                }
            }catch(Exception ex)
            {
                lblError2.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FA7B7B");
                lblError2.Text = ex.Message;
            }
        }
    }
}