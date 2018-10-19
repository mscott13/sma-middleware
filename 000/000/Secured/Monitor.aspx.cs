using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ACCPAC.Advantage;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using TableDependency.Enums;
using TableDependency.EventArgs;
using TableDependency.SqlClient;

namespace _000.Secured
{
    public partial class Monitor : System.Web.UI.Page
    {
        private bool monitor = true;
        private int orgVal;
        private int curVal;
        bool first_time = true;

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void Button1_Click(object sender, EventArgs e)
        {
 
            using (var tableDepend = new SqlTableDependency<SqlNotify>(ConfigurationManager.ConnectionStrings["AsmsGenericMaster"].ConnectionString, "tblARInvoices"))
            {
                tableDepend.OnChanged += TableDependency_Changed;
                tableDepend.OnError += TableDependency_OnError;

                tableDepend.Stop();
                tableDepend.Start();      
            }

        }
 
        public void Invoices(string idCust, string docNum, string desc, string feeCode, string amt)
        {
            var session = new Session();
            session.Init("", "XY", "XY1000", "62A");
            session.Open("ADMIN", "ADMIN", "SANLTD", DateTime.Today, 0);

            var mDBLinkCmpRW = session.OpenDBLink(DBLinkType.Company, DBLinkFlags.ReadWrite);

            var arInvoiceBatch = mDBLinkCmpRW.OpenView("AR0031");
            var arInvoiceHeader = mDBLinkCmpRW.OpenView("AR0032");
            var arInvoiceDetail = mDBLinkCmpRW.OpenView("AR0033");
            var arInvoicePaymentSchedules = mDBLinkCmpRW.OpenView("AR0034");
            var arInvoiceHeaderOptFields = mDBLinkCmpRW.OpenView("AR0402");
            var arInvoiceDetailOptFields = mDBLinkCmpRW.OpenView("AR0401");

            arInvoiceBatch.Compose(new ACCPAC.Advantage.View[] { arInvoiceHeader });
            arInvoiceHeader.Compose(new ACCPAC.Advantage.View[] { arInvoiceBatch, arInvoiceDetail, arInvoicePaymentSchedules, arInvoiceHeaderOptFields });
            arInvoiceDetail.Compose(new ACCPAC.Advantage.View[] { arInvoiceHeader, arInvoiceBatch, arInvoiceDetailOptFields });
            arInvoicePaymentSchedules.Compose(new ACCPAC.Advantage.View[] { arInvoiceHeader });
            arInvoiceHeaderOptFields.Compose(new ACCPAC.Advantage.View[] { arInvoiceHeader });
            arInvoiceDetailOptFields.Compose(new ACCPAC.Advantage.View[] { arInvoiceDetail });

            try
            {
                arInvoiceBatch.RecordCreate(ViewRecordCreate.Insert);
                arInvoiceBatch.Process();
                arInvoiceBatch.Read(false);
                arInvoiceHeader.RecordCreate(ViewRecordCreate.DelayKey);
                arInvoiceDetail.Cancel();
                arInvoiceHeader.Fields.FieldByName("IDCUST").SetValue(idCust, false);

                arInvoiceHeader.Process();
                arInvoiceHeader.Fields.FieldByName("IDINVC").SetValue(docNum, false);

                var temp = arInvoiceDetail.Exists;
                arInvoiceDetail.RecordClear();
                temp = arInvoiceDetail.Exists;
                arInvoiceDetail.RecordCreate(ViewRecordCreate.NoInsert);

                arInvoiceDetail.Process();
                arInvoiceDetail.Fields.FieldByName("TEXTDESC").SetValue(desc, false);
                arInvoiceDetail.Fields.FieldByName("IDACCTREV").SetValue(feeCode, false);
                arInvoiceDetail.Fields.FieldByName("AMTEXTN").SetValue(amt, false);

                arInvoiceDetail.Insert();

                arInvoiceDetail.Read(false);
                arInvoiceHeader.Insert();
                arInvoiceDetail.Read(false);
                arInvoiceDetail.Read(false);
                arInvoiceBatch.Read(false);
                arInvoiceHeader.RecordCreate(ViewRecordCreate.DelayKey);
                arInvoiceDetail.Cancel();

                session.Dispose();
            }
            catch (Exception ex)
            {
                session.Dispose();
            }
        }

        public void TableDependency_Changed(object sender, RecordChangedEventArgs<SqlNotify> es)
        {
          
            if(es.ChangeType !=ChangeType.None)
             {
                var values = es.Entity;
                var operation = es.ChangeType;

                var pId = values.CustomerID;
                var pInvoice = values.ARInvoiceID;
                var pAmt = values.Amount.ToString();
                var pFeeType= values.FeeType;

                string iv_customerId = "";

                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AsmsGenericMaster"].ConnectionString);
                SqlDataReader reader = null;
                SqlCommand cmd = new SqlCommand();

                int id = Convert.ToInt32(pId);

                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = " SELECT clientCompany, ccNum from client where clientId=@id";
                cmd.Parameters.AddWithValue("@id", id);

                reader = cmd.ExecuteReader();
                reader.Read();

                string companyName = reader[0].ToString();
                string cNum = reader[1].ToString();
                string temp = "";
                string fCode = "";

                for(int i=0; i<cNum.Length; i++)
                {
                    if (cNum[i] != '-')
                    {
                        temp += cNum[i];
                    }
                    else
                    {
                        i = cNum.Length;
                        cNum = temp;
                    }
                }

                if (pFeeType=="SLF")
                {
                    iv_customerId = cNum + "-L";
                    fCode = "10102-100"; 
                }
                else if (pFeeType == "RF")
                {
                    iv_customerId = cNum + "-R";
                    fCode = " 10100-100"; 
                }
                conn.Close();
                Invoices(iv_customerId, pInvoice.ToString(), companyName, fCode, pAmt);
            }
        }

        static void TableDependency_OnError(object sender,ErrorEventArgs ex)
        {

        }
    }
}
