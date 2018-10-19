using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace _000.Secured
{
    public partial class ManageUsers : System.Web.UI.Page
    {
        private bool verified;
    
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    var roles = Roles.GetAllRoles();
                    int i = roles.Length;

                    int h = 0;
                    while (i > h)
                    {
                        ddlRoles.Items.Insert(h, new ListItem(roles[h]));
                        h++;
                    }
                    
                }
                catch (Exception ex)
                {
                    lblError.Text = ex.Message;
                }

            }
            //if (HttpContext.Current.User.Identity.IsAuthenticated)
            //{
            //    var a = HttpContext.Current.User.Identity;

            //}
            //else
            //{
            //    Response.Redirect("~/Defualt.aspx");
            //}

        }

        protected void btnAssignRole_Click(object sender, EventArgs e)
        {
            try {
                verified = (bool)Session["verify_state"];
                if (verified)
                {

                    Roles.AddUserToRole(txtUser.Text, ddlRoles.SelectedValue);
                    lblError.ForeColor = System.Drawing.ColorTranslator.FromHtml("#87CDA3");
                    lblError.Text = "Role Assigned";
                }
                else
                {
                    lblError.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FA7B7B");
                    lblError.Text = "Cannot Assign Role";
                }
            }catch(Exception ex)
            {
                lblError.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FA7B7B");
                lblError.Text = ex.Message;
            }

        }

        protected void btnChkName_Click(object sender, EventArgs e)
        {
            try
            {
                var user = Membership.GetUser(txtUser.Text);
                if (user != null)
                {
                    lblError2.ForeColor = System.Drawing.ColorTranslator.FromHtml("#87CDA3");
                    lblError2.Text = "Name Verified";
                    verified = true;
                    Session["verify_state"] = verified;


                    var s = Roles.GetRolesForUser("Junior");
                }
                else
                {
                    lblError2.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FA7B7B");
                    lblError2.Text = "Verification fail";
                    verified = false;
                    Session["verify_state"] = verified;
                }
            }
            catch(Exception ex)
            {
                lblError2.Text = ex.Message;
            }

        }
    }
}