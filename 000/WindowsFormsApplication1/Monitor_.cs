using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using ACCPAC.Advantage;
using TableDependency.Enums;
using TableDependency.EventArgs;
using TableDependency.SqlClient;
using System.Data.SqlClient;

using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using AccpacCOMAPI;

namespace SyncMon
{
    public partial class Monitor_ : Form, AccpacErrors
    {
        //Batch Types
        public const string TYPE_APPROVAL = "Type Approval";
        public const string RENEWAL_REG = "Renewals - Reg Fees - For ";
        public const string RENEWAL_SPEC = "Renewals - Spec Fees - For ";
        public const string MAJ = "Maj";
        public const string NON_MAJ = "Non Maj";
        public const string CREDIT_NOTE = "Credit Note";

        public const int INVOICE = 4;
        public const int CREDIT_MEMO = 5;
        public const int RECEIPT = 11;
        public const string ONE_DAY = "1";
        public const int RESET_FREQUENCY = 86400;
        public const string SAGE_COMPANY = "SMALTD";

        private int Code = 21;
        bool hidden = false;
        private bool monitorRunning = false;

        static string smaDbserv = "Data Source=ERP-SRVR\\ASMSDEV;Initial Catalog=ASMSGenericMaster;Integrated Security=True";
        static string IntegrationDB_SMA = "Data Source=ERP-SRVR\\ASMSDEV;Initial Catalog=ASMSSAGEINTEGRATION;Integrated Security=True";

        SqlTableDependency<SqlNotify_Pay> tableDependPay;
        SqlTableDependency<SqlNotifyCancellation> tableDependCancellation;
        SqlTableDependency<SqlNotify_DocumentInfo> tableDependInfo;

        public static SqlConnection connGeneric;
        public static SqlConnection connIntegration;
        public static SqlConnection connMsgQueue;

        //Session session;
        //DBLink mDBLinkCmpRW;

        AccpacSession mAccpacSession;
        AccpacDBLink mAccpacDBLink;

        int prevInvoice = -100;
        DateTime prevTime;
        DateTime currentTime;
        string dbConn;

        Integration intLink;
        int currentInvoice = -1;
        bool closed = false;

        AccpacView CBBTCH1batch;
        AccpacView CBBTCH1header;
        AccpacView CBBTCH1detail1;
        AccpacView CBBTCH1detail2;
        AccpacView CBBTCH1detail3;
        AccpacView CBBTCH1detail4;
        AccpacView CBBTCH1detail5;
        AccpacView CBBTCH1detail6;
        AccpacView CBBTCH1detail7;
        AccpacView CBBTCH1detail8;

        AccpacView b1_arInvoiceBatch;
        AccpacView b1_arInvoiceHeader;
        AccpacView b1_arInvoiceDetail;
        AccpacView b1_arInvoicePaymentSchedules;
        AccpacView b1_arInvoiceHeaderOptFields;
        AccpacView b1_arInvoiceDetailOptFields;

        AccpacView arRecptBatch;
        AccpacView arRecptHeader;
        AccpacView arRecptDetail1;
        AccpacView arRecptDetail2;
        AccpacView arRecptDetail3;
        AccpacView arRecptDetail4;
        AccpacView arRecptDetail5;
        AccpacView arRecptDetail6;

        AccpacView csRateHeader;
        AccpacView csRateDetail;

        public Monitor_()
        {
            InitializeComponent();
            btnStop.Enabled = false;
            dbConn = smaDbserv;

            connGeneric = new SqlConnection(smaDbserv);
            connIntegration = new SqlConnection(IntegrationDB_SMA);
            connMsgQueue = new SqlConnection(IntegrationDB_SMA);

            prevTime = DateTime.Now.AddDays(2);
            currentTime = DateTime.Now;
            string filename = @"\middleware_log.txt";
            string path = Environment.SpecialFolder.MyDocuments + filename;
            File.AppendAllText("test", Environment.SpecialFolder.MyDocuments + filename);
            mAccpacSession = new AccpacSession();

            using (tableDependPay = new SqlTableDependency<SqlNotify_Pay>(dbConn, "tblARPayments"))
            {
                tableDependPay.OnChanged += TableDependPay_OnChanged;
                tableDependPay.OnError += TableDependPay_OnError;
            }

            using (tableDependCancellation = new SqlTableDependency<SqlNotifyCancellation>(dbConn, "tblARInvoices"))
            {
                tableDependCancellation.OnChanged += TableDependCancellation_OnChanged;
                tableDependCancellation.OnError += TableDependCancellation_OnError;
            }

            using (tableDependInfo = new SqlTableDependency<SqlNotify_DocumentInfo>(dbConn, "tblGLDocuments"))
            {
                tableDependInfo.OnChanged += TableDependInfo_OnChanged;
                tableDependInfo.OnError += TableDependInfo_OnError;
            }

            Text += " | " + DateTime.Now.Date.ToLongDateString();
            intLink = new Integration(connGeneric, connIntegration, connMsgQueue);
            LogOperation("Program Started", 0);

            StatusUpdate.Start();
            StartService();
        }

        private void btn_Start(object sender, EventArgs e)
        {
            StartService();
        }

        void StartService()
        {
            int es = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - DateTime.Now.Day;
            es++;
            tableDependPay.Dispose();
            tableDependCancellation.Dispose();
            tableDependInfo.Dispose();

            DateTime dt1 = DateTime.Now.AddDays(es);

            try
            {
                LogOperation("Initialize Session", 2);

                mAccpacSession.Init("", "XY", "XY1000", "63A");
                mAccpacSession.Open("ADMIN", "SPECTRUM9", SAGE_COMPANY, DateTime.Today, 0, "");
                mAccpacDBLink = mAccpacSession.OpenDBLink(tagDBLinkTypeEnum.DBLINK_COMPANY, tagDBLinkFlagsEnum.DBLINK_FLG_READWRITE);

                if (!StatusUpdate.Enabled)
                {
                    Code = 31;
                    StatusUpdate.Start();
                }
                else
                {
                    Code = 31;
                }

                btnStart.Enabled = false;
                btnStop.Enabled = true;

                tableDependPay.Start();
                tableDependCancellation.Start();
                tableDependInfo.Start();
                resetCounterTimer.Start();
                deferredTimer.Start();

                btnStart.Text = "Running";
                LogOperation("Service Started", 1);
            }

            catch (Exception ex)
            {
                var msg = ex.Message;
                LogOperation("Cannot Initialize Session", 1);
                MessageBox.Show("Failed to initialize session", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Code = 21;
            }
            monitorRunning = true;
            LogOperation("Session status: " + mAccpacSession.IsOpened.ToString(), 1);
        }

        void LogOperation(string message, int breaks)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string, int>(LogOperation), new object[] { message, breaks });
            }
            else
            {
                int i = 0;
                if (breaks <= 0)
                {
                    log.AppendText(" " + DateTime.Now.ToString("HH:mm:ss") + ": " + message);
                    intLink.Log(message);
                    FileLog.Write(message);
                }
                else
                {
                    for (i = 0; i < breaks; i++)
                    {
                        log.AppendText("\n");
                    }
                    log.AppendText(" " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToString("HH:mm:ss") + ": " + message);
                    log.SelectionStart = log.Text.Length;
                    log.ScrollToCaret();
                    intLink.Log(message);
                    FileLog.Write(message);
                }
            }
        }

        public int getEntryNumber(int docNumber)
        {
            int entry = -1;
            string docNum = docNumber.ToString();
            mAccpacDBLink.OpenView("AR0032", out b1_arInvoiceHeader);

            string searchFilter = "IDINVC = " + docNum + "";
            b1_arInvoiceHeader.Browse(searchFilter, true);

            bool gotIt = b1_arInvoiceHeader.GoBottom();

            if (gotIt)
            {
                entry = Convert.ToInt32(b1_arInvoiceHeader.Fields.FieldByName["CNTITEM"].get_Value());
            }
            else
            {
                LogOperation("Invoice not found", 1);
            }
            return entry;
        }

        public bool receiptBatchAvail(string bankcode)
        {
            if (connGeneric.State != ConnectionState.Open)
            {
                connGeneric.Open();
            }

            if (connIntegration.State != ConnectionState.Open)
            {
                connIntegration.Open();
            }

            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            bool truth = false;
            int receiptBatch = -1;
            DateTime expiryDate = DateTime.Now;

            cmd.CommandText = "exec sp_rBatchAvail @bankcode";
            cmd.Parameters.AddWithValue("@bankcode", bankcode);
            cmd.Connection = connIntegration;

            reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                receiptBatch = Convert.ToInt32(reader[0]);
                expiryDate = Convert.ToDateTime(reader["ExpiryDate"]);
                reader.Close();

                if (DateTime.Now < expiryDate)
                {
                    if (!checkAccpacRBatchPosted(receiptBatch))
                    {
                        truth = true;
                    }
                    else
                    {
                        intLink.closeReceiptBatch(receiptBatch);
                    }
                }
                else
                {
                    intLink.closeReceiptBatch(receiptBatch);
                }

                return truth;
            }
            else
            {
                reader.Close();
                return truth;
            }
        }

        public int getIbatchNumber(int docNumber)
        {
            int batchNum = -1;
            AccpacView cssql = null;
            mAccpacDBLink.OpenView("CS0120", out cssql);

            cssql.Browse("SELECT CNTBTCH FROM ARIBH WHERE IDINVC = '" + docNumber.ToString() + "'", true);
            cssql.InternalSet(256);

            if (cssql.GoNext())
            {
                batchNum = Convert.ToInt32(cssql.Fields.FieldByName["CNTBTCH"].get_Value());
            }
            return batchNum;
        }

        public int getRBatchNumber(string referenceNumber)
        {
            int batchNum = -1;
            AccpacView cssql = null;
            mAccpacDBLink.OpenView("CS0120", out cssql);
            cssql.Browse("SELECT CNTBTCH FROM ARTCR WHERE IDRMIT = '" + referenceNumber + "'", true);
            cssql.InternalSet(256);

            if (cssql.GoNext())
            {
                batchNum = Convert.ToInt32(cssql.Fields.FieldByName["CNTBTCH"].get_Value());
            }
            return batchNum;
        }

        public string getDocNumber(string referenceNumber)
        {
            string docNum = "";
            AccpacView cssql = null;
            mAccpacDBLink.OpenView("CS0120", out cssql);
            cssql.Browse("SELECT DOCNBR FROM ARTCR WHERE IDRMIT = '" + referenceNumber + "'", true);
            cssql.InternalSet(256);

            if (cssql.GoNext())
            {
                docNum = Convert.ToString(cssql.Fields.FieldByName["DOCNBR"].get_Value());
            }
            return docNum;
        }


        public bool isPeriodCreated(int finyear)
        {
            string fiscYear = "";
            AccpacView cssql = null;
            mAccpacDBLink.OpenView("CS0120", out cssql);
            cssql.Browse("SELECT FSCYEAR FROM CSFSC WHERE FSCYEAR = '"+ finyear.ToString()+"'", true);
            cssql.InternalSet(256);

            if (cssql.GoNext())
            {
                fiscYear = Convert.ToString(cssql.Fields.FieldByName["FSCYEAR"].get_Value());
            }

            if (fiscYear != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void GetViewInfo(ACCPAC.Advantage.View ax, string filename)
        {
            LogOperation("Load View Info .. " + filename, 1);
            string mydocpath =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            StreamWriter output = new StreamWriter(mydocpath + @"\" + filename + ".txt");

            int count = ax.Fields.Count;
            output.WriteLine(count.ToString() + " fields found - " + ax.Description);
            output.WriteLine(" ");
            output.WriteLine("------------------");
            output.WriteLine(" ");

            string name, desc;
            for (int i = 1; i <= count; i++)
            {
                var x = ax.Fields.FieldByID(i);
                name = x.Name;
                desc = x.Description;

                output.WriteLine(i.ToString() + ". " + name + " ----------- " + desc);
            }
            output.Close();
            LogOperation("Finished." + filename, 1);
        }
        int countBatchPaymentEntries(string batchId)
        {
            int count = 0;
            mAccpacDBLink.OpenView("CB0009", out CBBTCH1batch);
            mAccpacDBLink.OpenView("CB0010", out CBBTCH1header);
            mAccpacDBLink.OpenView("CB0011", out CBBTCH1detail1);
            mAccpacDBLink.OpenView("CB0012", out CBBTCH1detail2);
            mAccpacDBLink.OpenView("CB0013", out CBBTCH1detail3);
            mAccpacDBLink.OpenView("CB0014", out CBBTCH1detail4);
            mAccpacDBLink.OpenView("CB0015", out CBBTCH1detail5);
            mAccpacDBLink.OpenView("CB0016", out CBBTCH1detail6);
            mAccpacDBLink.OpenView("CB0403", out CBBTCH1detail7);
            mAccpacDBLink.OpenView("CB0404", out CBBTCH1detail8);

            CBBTCH1batch.Compose(new AccpacView[] { CBBTCH1header });
            CBBTCH1header.Compose(new AccpacView[] { CBBTCH1batch, CBBTCH1detail1, CBBTCH1detail4, CBBTCH1detail8 });
            CBBTCH1detail1.Compose(new AccpacView[] { CBBTCH1header, CBBTCH1detail2, CBBTCH1detail5, CBBTCH1detail7 });
            CBBTCH1detail2.Compose(new AccpacView[] { CBBTCH1detail1, CBBTCH1detail3, CBBTCH1detail6 });
            CBBTCH1detail3.Compose(new AccpacView[] { CBBTCH1detail2 });
            CBBTCH1detail4.Compose(new AccpacView[] { CBBTCH1header });
            CBBTCH1detail5.Compose(new AccpacView[] { CBBTCH1detail1 });
            CBBTCH1detail6.Compose(new AccpacView[] { CBBTCH1detail2 });
            CBBTCH1detail7.Compose(new AccpacView[] { CBBTCH1detail1 });
            CBBTCH1detail8.Compose(new AccpacView[] { CBBTCH1header });

            CBBTCH1header.Init();
            CBBTCH1batch.Fields.FieldByName["BATCHID"].set_Value(batchId);
            CBBTCH1batch.Read();
            CBBTCH1header.Read();

            bool gotIt = CBBTCH1header.GoTop();
            string refNumber = "";

            while (gotIt)
            {
                refNumber = Convert.ToInt32(CBBTCH1header.Fields.FieldByName["REFERENCE"].get_Value());
                gotIt = CBBTCH1header.GoNext();
                count++;
            }
            return count;
        }

        public bool checkAccpacInvoiceAvail(int invoiceId)
        {
            AccpacView cssql = null;
            mAccpacDBLink.OpenView("CS0120", out cssql);
            cssql.Browse("SELECT IDINVC FROM ARIBH WHERE IDINVC = '" + invoiceId.ToString() + "'", true);
            cssql.InternalSet(256);

            if (cssql.GoNext())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool checkAccpacRBatchPosted(int batchNumber)
        {
            AccpacView cssql = null;
            mAccpacDBLink.OpenView("CS0120", out cssql);

            cssql.Browse("SELECT CNTBTCH, BATCHSTAT FROM ARBTA WHERE CNTBTCH = '" + batchNumber.ToString() + "'", true);
            cssql.InternalSet(256);

            if (cssql.GoNext())
            {
                string val = Convert.ToString(cssql.Fields.FieldByName["BATCHSTAT"].get_Value());

                if (val == "1")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        public bool checkAccpacIBatchPosted(int batchNumber)
        {
            AccpacView cssql = null;
            mAccpacDBLink.OpenView("CS0120", out cssql);

            cssql.Browse("SELECT BTCHSTTS FROM ARIBC WHERE CNTBTCH = '" + batchNumber.ToString() + "'", true);
            cssql.InternalSet(256);

            if (cssql.GoNext())
            {
                string val = Convert.ToString(cssql.Fields.FieldByName["BTCHSTTS"].get_Value());

                if (val == "1")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        bool PayBatchInsert(string entryType, string idCust, string desc, string acct, string debitAmt, string cName, string batchId, string referenceNumber, string invnum, DateTime paymentDate, string findesc, string cid, DateTime valstart, DateTime valend)
        {
            string notes = intLink.isAnnualFee(Convert.ToInt32(invnum));

            string receiptDesc = " ";
            if (notes == "Annual Fee")
            {
                receiptDesc = receiptDesc = findesc + " for Licence " + cid;
            }
            else if (findesc == "Processing Fee" && idCust[6].ToString() == "T")
            {
                receiptDesc = findesc + " for Type Approval Certification";
            }

            else if (invnum == "0")
            {
                receiptDesc = findesc + " for Licence " + cid + " for Period " + valstart.Date.ToString("dd/MM/yy") + " to " + valend.Date.ToString("dd/MM/yy");
            }

            else
            {
                receiptDesc = findesc + " for Licence " + cid + " for Period " + valstart.Date.ToString("dd/MM/yy") + " to " + valend.Date.ToString("dd/MM/yy");
            }

            if (!CustomerExists(idCust))
            {
                Ignore(idCust, DateTime.Now.ToString(), "Receipt");
                LogOperation("Customer " + idCust + " does not exist", 1);
                LogOperation("Transfer Failed", 1);
                return false;
            }

            else
            {
                if (invnum == "0")
                {
                    string account = "";
                    if (idCust[5].ToString() + idCust[6].ToString() == "-L")
                    {
                        account = "30010-100-SFRCPT";
                    }
                    else if (idCust[5].ToString() + idCust[6].ToString() == "-R")
                    {
                        account = "30040-100";
                    }

                    entryType = "0";
                    mAccpacDBLink.OpenView("CB0009", out CBBTCH1batch);
                    mAccpacDBLink.OpenView("CB0010", out CBBTCH1header);
                    mAccpacDBLink.OpenView("CB0011", out CBBTCH1detail1);
                    mAccpacDBLink.OpenView("CB0012", out CBBTCH1detail2);
                    mAccpacDBLink.OpenView("CB0013", out CBBTCH1detail3);
                    mAccpacDBLink.OpenView("CB0014", out CBBTCH1detail4);
                    mAccpacDBLink.OpenView("CB0015", out CBBTCH1detail5);
                    mAccpacDBLink.OpenView("CB0016", out CBBTCH1detail6);
                    mAccpacDBLink.OpenView("CB0403", out CBBTCH1detail7);
                    mAccpacDBLink.OpenView("CB0404", out CBBTCH1detail8);

                    CBBTCH1batch.Compose(new AccpacView[] { CBBTCH1header });
                    CBBTCH1header.Compose(new AccpacView[] { CBBTCH1batch, CBBTCH1detail1, CBBTCH1detail4, CBBTCH1detail8 });
                    CBBTCH1detail1.Compose(new AccpacView[] { CBBTCH1header, CBBTCH1detail2, CBBTCH1detail5, CBBTCH1detail7 });
                    CBBTCH1detail2.Compose(new AccpacView[] { CBBTCH1detail1, CBBTCH1detail3, CBBTCH1detail6 });
                    CBBTCH1detail3.Compose(new AccpacView[] { CBBTCH1detail2 });
                    CBBTCH1detail4.Compose(new AccpacView[] { CBBTCH1header });
                    CBBTCH1detail5.Compose(new AccpacView[] { CBBTCH1detail1 });
                    CBBTCH1detail6.Compose(new AccpacView[] { CBBTCH1detail2 });
                    CBBTCH1detail7.Compose(new AccpacView[] { CBBTCH1detail1 });
                    CBBTCH1detail8.Compose(new AccpacView[] { CBBTCH1header });

                    CBBTCH1batch.Fields.FieldByName["BATCHID"].set_Value(batchId);
                    CBBTCH1batch.Read();
                    CBBTCH1detail4.Fields.FieldByName["NAME"].set_Value(cName);
                    CBBTCH1header.Init();

                    CBBTCH1header.Fields.FieldByName["ENTRYTYPE"].set_Value(entryType);
                    CBBTCH1header.Fields.FieldByName["MISCCODE"].set_Value(idCust);
                    CBBTCH1header.Fields.FieldByName["TEXTDESC"].set_Value(desc);
                    CBBTCH1header.Fields.FieldByName["REFERENCE"].set_Value(referenceNumber);
                    CBBTCH1header.Fields.FieldByName["DATE"].set_Value(paymentDate);

                    CBBTCH1detail1.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_NOINSERT);
                    CBBTCH1detail1.Fields.FieldByName["SRCECODE"].set_Value(acct);
                    CBBTCH1detail1.Fields.FieldByName["ACCTID"].set_Value(account);
                    CBBTCH1detail1.Insert();

                    CBBTCH1detail1.Read();
                    CBBTCH1detail1.Fields.FieldByName["DEBITAMT"].set_Value(debitAmt);
                    CBBTCH1detail1.Update();



                    CBBTCH1detail1.Read();
                    CBBTCH1detail1.Fields.FieldByName["TEXTDESC"].set_Value(cName);
                    CBBTCH1detail1.Update();

                    CBBTCH1detail1.Read();
                    CBBTCH1detail1.Update();
                    CBBTCH1header.Insert();

                    CBBTCH1batch.Close();
                    CBBTCH1header.Close();
                    CBBTCH1detail1.Close();
                    CBBTCH1detail2.Close();
                    CBBTCH1detail3.Close();
                    CBBTCH1detail4.Close();
                    CBBTCH1detail5.Close();
                    CBBTCH1detail6.Close();
                    CBBTCH1detail7.Close();
                    CBBTCH1detail8.Close();

                }
                else
                {
                    mAccpacDBLink.OpenView("CB0009", out CBBTCH1batch);
                    mAccpacDBLink.OpenView("CB0010", out CBBTCH1header);
                    mAccpacDBLink.OpenView("CB0011", out CBBTCH1detail1);
                    mAccpacDBLink.OpenView("CB0012", out CBBTCH1detail2);
                    mAccpacDBLink.OpenView("CB0013", out CBBTCH1detail3);
                    mAccpacDBLink.OpenView("CB0014", out CBBTCH1detail4);
                    mAccpacDBLink.OpenView("CB0015", out CBBTCH1detail5);
                    mAccpacDBLink.OpenView("CB0016", out CBBTCH1detail6);
                    mAccpacDBLink.OpenView("CB0403", out CBBTCH1detail7);
                    mAccpacDBLink.OpenView("CB0404", out CBBTCH1detail8);

                    CBBTCH1batch.Compose(new AccpacView[] { CBBTCH1header });
                    CBBTCH1header.Compose(new AccpacView[] { CBBTCH1batch, CBBTCH1detail1, CBBTCH1detail4, CBBTCH1detail8 });
                    CBBTCH1detail1.Compose(new AccpacView[] { CBBTCH1header, CBBTCH1detail2, CBBTCH1detail5, CBBTCH1detail7 });
                    CBBTCH1detail2.Compose(new AccpacView[] { CBBTCH1detail1, CBBTCH1detail3, CBBTCH1detail6 });
                    CBBTCH1detail3.Compose(new AccpacView[] { CBBTCH1detail2 });
                    CBBTCH1detail4.Compose(new AccpacView[] { CBBTCH1header });
                    CBBTCH1detail5.Compose(new AccpacView[] { CBBTCH1detail1 });
                    CBBTCH1detail6.Compose(new AccpacView[] { CBBTCH1detail2 });
                    CBBTCH1detail7.Compose(new AccpacView[] { CBBTCH1detail1 });
                    CBBTCH1detail8.Compose(new AccpacView[] { CBBTCH1header });

                    CBBTCH1batch.Fields.FieldByName["BATCHID"].set_Value(batchId);
                    CBBTCH1batch.Read();
                    CBBTCH1detail4.Fields.FieldByName["NAME"].set_Value(cName);

                    CBBTCH1header.Init();

                    CBBTCH1header.Fields.FieldByName["ENTRYTYPE"].set_Value(entryType);
                    CBBTCH1header.Fields.FieldByName["MISCCODE"].set_Value(idCust);
                    CBBTCH1header.Fields.FieldByName["TEXTDESC"].set_Value(desc);
                    CBBTCH1header.Fields.FieldByName["REFERENCE"].set_Value(referenceNumber);
                    CBBTCH1header.Fields.FieldByName["DATE"].set_Value(paymentDate);

                    CBBTCH1detail1.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_NOINSERT);
                    CBBTCH1detail1.Fields.FieldByName["SRCECODE"].set_Value("REC1");
                    //  CBBTCH1detail1.Fields.FieldByName("ACCTID").SetValue("10200-100", false);
                    CBBTCH1detail1.Insert();


                    CBBTCH1detail1.Read();
                    CBBTCH1detail1.Fields.FieldByName["DEBITAMT"].set_Value(debitAmt);
                    CBBTCH1detail1.Update();
                    //CBBTCH1detail1.Read(false);


                    CBBTCH1detail1.Fields.FieldByName["TEXTDESC"].set_Value(receiptDesc);
                    CBBTCH1detail1.Update();

                    CBBTCH1detail1.Read();
                    CBBTCH1detail1.Update();
                    CBBTCH1header.Insert();

                    CBBTCH1batch.Close();
                    CBBTCH1header.Close();
                    CBBTCH1detail1.Close();
                    CBBTCH1detail2.Close();
                    CBBTCH1detail3.Close();
                    CBBTCH1detail4.Close();
                    CBBTCH1detail5.Close();
                    CBBTCH1detail6.Close();
                    CBBTCH1detail7.Close();
                    CBBTCH1detail8.Close();

                }
                UpdateList(idCust, "Transferred", "Receipt");
                LogOperation("Receipt Transferred", 1);
                return true;
            }
        }

        void CreateReceiptBatch(string bankcode)
        {
            mAccpacDBLink.OpenView("CB0009", out CBBTCH1batch);
            mAccpacDBLink.OpenView("CB0010", out CBBTCH1header);
            mAccpacDBLink.OpenView("CB0011", out CBBTCH1detail1);
            mAccpacDBLink.OpenView("CB0012", out CBBTCH1detail2);
            mAccpacDBLink.OpenView("CB0013", out CBBTCH1detail3);
            mAccpacDBLink.OpenView("CB0014", out CBBTCH1detail4);
            mAccpacDBLink.OpenView("CB0015", out CBBTCH1detail5);
            mAccpacDBLink.OpenView("CB0016", out CBBTCH1detail6);
            mAccpacDBLink.OpenView("CB0403", out CBBTCH1detail7);
            mAccpacDBLink.OpenView("CB0404", out CBBTCH1detail8);

            CBBTCH1batch.Compose(new AccpacView[] { CBBTCH1header });
            CBBTCH1header.Compose(new AccpacView[] { CBBTCH1batch, CBBTCH1detail1, CBBTCH1detail4, CBBTCH1detail8 });
            CBBTCH1detail1.Compose(new AccpacView[] { CBBTCH1header, CBBTCH1detail2, CBBTCH1detail5, CBBTCH1detail7 });
            CBBTCH1detail2.Compose(new AccpacView[] { CBBTCH1detail1, CBBTCH1detail3, CBBTCH1detail6 });
            CBBTCH1detail3.Compose(new AccpacView[] { CBBTCH1detail2 });
            CBBTCH1detail4.Compose(new AccpacView[] { CBBTCH1header });
            CBBTCH1detail5.Compose(new AccpacView[] { CBBTCH1detail1 });
            CBBTCH1detail6.Compose(new AccpacView[] { CBBTCH1detail2 });
            CBBTCH1detail7.Compose(new AccpacView[] { CBBTCH1detail1 });
            CBBTCH1detail8.Compose(new AccpacView[] { CBBTCH1header });


            CBBTCH1batch.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_INSERT);
            CBBTCH1batch.Read();
            CBBTCH1batch.Fields.FieldByName["BANKCODE"].set_Value(bankcode);
            CBBTCH1batch.Fields.FieldByName["DATECREATE"].set_Value(DateTime.Now.Date.ToString());
            CBBTCH1batch.Update();
            CBBTCH1header.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
            CBBTCH1detail1.Cancel();
        }

        void CreateReceiptBatchEx(string bankcode, string batchDesc)
        {
            mAccpacDBLink.OpenView("AR0041", out CBBTCH1batch);
            mAccpacDBLink.OpenView("AR0042", out CBBTCH1header);
            mAccpacDBLink.OpenView("AR0044", out CBBTCH1detail1);
            mAccpacDBLink.OpenView("AR0045", out CBBTCH1detail2);
            mAccpacDBLink.OpenView("AR0043", out CBBTCH1detail3);
            mAccpacDBLink.OpenView("AR0061", out CBBTCH1detail4);
            mAccpacDBLink.OpenView("AR0406", out CBBTCH1detail5);
            mAccpacDBLink.OpenView("AR0170", out CBBTCH1detail6);

            CBBTCH1batch.Compose(new AccpacView[] { CBBTCH1header });
            CBBTCH1header.Compose(new AccpacView[] { CBBTCH1batch, CBBTCH1detail3, CBBTCH1detail1, CBBTCH1detail5, CBBTCH1detail6 });
            CBBTCH1detail1.Compose(new AccpacView[] { CBBTCH1header, CBBTCH1detail2, CBBTCH1detail4 });
            CBBTCH1detail2.Compose(new AccpacView[] { CBBTCH1detail1 });
            CBBTCH1detail3.Compose(new AccpacView[] { CBBTCH1header });
            CBBTCH1detail4.Compose(new AccpacView[] { CBBTCH1batch, CBBTCH1header, CBBTCH1header, CBBTCH1detail3, CBBTCH1detail1, CBBTCH1detail2 });
            CBBTCH1detail5.Compose(new AccpacView[] { CBBTCH1header });
            CBBTCH1detail6.Compose(new AccpacView[] { CBBTCH1header });

            CBBTCH1batch.Fields.FieldByName["CODEPYMTYP"].set_Value("CA");
            CBBTCH1header.Fields.FieldByName["CODEPYMTYP"].set_Value("CA");
            CBBTCH1detail3.Fields.FieldByName["CODEPAYM"].set_Value("CA");
            CBBTCH1detail1.Fields.FieldByName["CODEPAYM"].set_Value("CA");
            CBBTCH1detail2.Fields.FieldByName["CODEPAYM"].set_Value("CA");
            CBBTCH1detail4.Fields.FieldByName["PAYMTYPE"].set_Value("CA");
            CBBTCH1batch.Fields.FieldByName["CODEPYMTYP"].set_Value("CA");

            CBBTCH1batch.RecordClear();
            CBBTCH1batch.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_INSERT);
            CBBTCH1header.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
            CBBTCH1batch.Fields.FieldByName["BATCHDESC"].set_Value(batchDesc);
            CBBTCH1batch.Fields.FieldByName["DATEBTCH"].set_Value(DateTime.Now.ToString());
            CBBTCH1batch.Update();
            CBBTCH1batch.Fields.FieldByName["IDBANK"].set_Value(bankcode);
            CBBTCH1batch.Update();
            CBBTCH1header.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
        }

        public void comApiRateInsert()
        {
            try
            {
                mAccpacDBLink.OpenView("CS0005", out csRateHeader);
                mAccpacDBLink.OpenView("CS0006", out csRateDetail);

                csRateHeader.Compose(new AccpacView[] { csRateDetail });
                csRateDetail.Compose(new AccpacView[] { csRateHeader });

                csRateHeader.Fields.FieldByName["PRGTNOW"].set_Value("1");
                csRateHeader.Fields.FieldByName["HOMECUR"].set_Value("JAD");
                csRateHeader.Fields.FieldByName["RATETYPE"].set_Value("BB");
                csRateHeader.Read();


                csRateDetail.Read();
                csRateDetail.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_NOINSERT);
                csRateDetail.Fields.FieldByName["SOURCECUR"].set_Value("USD");
                csRateDetail.Fields.FieldByName["RATEDATE"].set_Value("19/06/2017");
                csRateDetail.Fields.FieldByName["RATE"].set_Value("170.9700000");

                csRateDetail.Insert();
                csRateDetail.Fields.FieldByName["SOURCECUR"].set_Value("USD");
                csRateDetail.Read();

                csRateHeader.Update();
            }
            catch (Exception e)
            {
                var err = mAccpacSession.Errors;
                string res = err.GenerateErrorFile();
                LogOperation("Error file: " + res, 1);
            }
        }

        public void comApiReceiptInsert(string batchNumber, string customerId, string amount, string date, string description, string referenceNumber, string invoiceId, bool allocate)
        {
            try
            {
                bool flagInsert = false;

                mAccpacDBLink.OpenView("AR0041", out arRecptBatch);
                mAccpacDBLink.OpenView("AR0042", out arRecptHeader);
                mAccpacDBLink.OpenView("AR0044", out arRecptDetail1);
                mAccpacDBLink.OpenView("AR0045", out arRecptDetail2);
                mAccpacDBLink.OpenView("AR0043", out arRecptDetail3);
                mAccpacDBLink.OpenView("AR0061", out arRecptDetail4);
                mAccpacDBLink.OpenView("AR0406", out arRecptDetail5);
                mAccpacDBLink.OpenView("AR0170", out arRecptDetail6);

                arRecptBatch.Compose(new AccpacView[] { arRecptHeader });
                arRecptHeader.Compose(new AccpacView[] { arRecptBatch, arRecptDetail3, arRecptDetail1, arRecptDetail5, arRecptDetail6 });
                arRecptDetail1.Compose(new AccpacView[] { arRecptHeader, arRecptDetail2, arRecptDetail4 });
                arRecptDetail2.Compose(new AccpacView[] { arRecptDetail1 });
                arRecptDetail3.Compose(new AccpacView[] { arRecptHeader });
                arRecptDetail4.Compose(new AccpacView[] { arRecptBatch, arRecptHeader, arRecptDetail3, arRecptDetail1, arRecptDetail2 });
                arRecptDetail5.Compose(new AccpacView[] { arRecptHeader });
                arRecptDetail6.Compose(new AccpacView[] { arRecptHeader });

                arRecptBatch.RecordClear();

                arRecptBatch.Fields.FieldByName["CODEPYMTYP"].PutWithoutVerification("CA");
                arRecptHeader.Fields.FieldByName["CODEPYMTYP"].PutWithoutVerification("CA");
                arRecptDetail3.Fields.FieldByName["CODEPAYM"].PutWithoutVerification("CA");
                arRecptDetail1.Fields.FieldByName["CODEPAYM"].PutWithoutVerification("CA");
                arRecptDetail2.Fields.FieldByName["CODEPAYM"].PutWithoutVerification("CA");
                arRecptDetail4.Fields.FieldByName["PAYMTYPE"].PutWithoutVerification("CA");
                arRecptBatch.Fields.FieldByName["CNTBTCH"].PutWithoutVerification(batchNumber);
                arRecptBatch.Read();

                arRecptDetail4.Fields.FieldByName["PAYMTYPE"].set_Value("CA");
                arRecptDetail4.Fields.FieldByName["CNTBTCH"].set_Value(batchNumber);
                arRecptDetail4.Fields.FieldByName["CNTITEM"].set_Value("1");
                arRecptDetail4.Fields.FieldByName["IDCUST"].set_Value(customerId);
                arRecptDetail4.Fields.FieldByName["AMTRMIT"].set_Value(amount);
                arRecptDetail4.Fields.FieldByName["STDOCDTE"].PutWithoutVerification(date);

                arRecptHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);

                arRecptHeader.Fields.FieldByName["TEXTRMIT"].set_Value(description);
                arRecptHeader.Fields.FieldByName["IDCUST"].set_Value(customerId);
                arRecptHeader.Fields.FieldByName["CODEPAYM"].set_Value("CASH");
                arRecptHeader.Fields.FieldByName["DATEBUS"].set_Value(date);
                arRecptHeader.Fields.FieldByName["IDRMIT"].set_Value(referenceNumber);
                arRecptHeader.Fields.FieldByName["AMTRMIT"].set_Value(amount);

                if (allocate)
                {
                    LogOperation("Applying receipt to invoice: " + invoiceId, 1);
                    arRecptDetail4.Fields.FieldByName["SHOWTYPE"].set_Value("2");
                    arRecptDetail4.Fields.FieldByName["STDOCSTR"].set_Value(invoiceId);

                    arRecptDetail4.Process();
                    arRecptDetail4.Fields.FieldByName["CNTKEY"].PutWithoutVerification("-1");
                    arRecptDetail4.Read();

                    var netAmount = arRecptDetail4.Fields.FieldByName["AMTNET"].get_Value();

                    if (netAmount == 0)
                    {
                        flagInsert = true;
                        LogOperation("The net balance is zero for this invoice. Cannot apply an additional amount", 2);
                    }
                    else
                    {
                        flagInsert = true;
                        arRecptDetail4.Fields.FieldByName["APPLY"].set_Value("Y");
                        arRecptDetail4.Update();
                    }
                }
                else
                {
                    LogOperation("Cannot apply to invoice: " + invoiceId + ". Check if invoice exist and the batch is posted.", 2);
                    flagInsert = true;
                }

                if (flagInsert)
                {
                    arRecptHeader.Insert();
                    arRecptHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);

                    UpdateList(customerId, "Transferred", "Receipt");
                    LogOperation("Receipt Transferred", 1);
                }

                arRecptBatch.Close();
                arRecptHeader.Close();
                arRecptDetail1.Close();
                arRecptDetail2.Close();
                arRecptDetail3.Close();
                arRecptDetail4.Close();
                arRecptDetail5.Close();
                arRecptDetail6.Close();
            }

            catch (Exception ex)
            {
                string location = mAccpacSession.Errors.GenerateErrorFile();
                LogOperation("Error file: " + location, 1);
            }

        }

        public bool ReceiptTransfer(string batchNumber, string customerId, string amount, string receiptDescription, string referenceNumber, string invnum, DateTime paymentDate, string findesc, string cid, DateTime valstart, DateTime valend)
        {
            string notes = intLink.isAnnualFee(Convert.ToInt32(invnum));
            string receiptDescriptionEx = "";

            if (notes == "Annual Fee")
            {
                receiptDescriptionEx = findesc + " for Licence " + cid;
            }
            else if (findesc == "Processing Fee" && customerId[6].ToString() == "T")
            {
                receiptDescriptionEx = findesc + " for Type Approval Certification";
            }
            else if (invnum == "0")
            {
                receiptDescriptionEx = findesc + " for Lic# " + cid + " for Period " + valstart.Date.ToString("dd/MM/yy") + " to " + valend.Date.ToString("dd/MM/yy");
            }
            else
            {
                receiptDescriptionEx = findesc + " for Lic# " + cid + " for Period " + valstart.Date.ToString("dd/MM/yy") + " to " + valend.Date.ToString("dd/MM/yy");
            }

            if (!CustomerExists(customerId))
            {
                Ignore(customerId, DateTime.Now.ToString(), "Receipt");
                LogOperation("Customer " + customerId + " does not exist", 1);
                LogOperation("Transfer Failed", 1);
                return false;
            }
            else
            {
                if (invnum == "0") //Prepayment Transfer
                {
                    mAccpacDBLink.OpenView("AR0041", out CBBTCH1batch);
                    mAccpacDBLink.OpenView("AR0042", out CBBTCH1header);
                    mAccpacDBLink.OpenView("AR0044", out CBBTCH1detail1);
                    mAccpacDBLink.OpenView("AR0045", out CBBTCH1detail2);
                    mAccpacDBLink.OpenView("AR0043", out CBBTCH1detail3);
                    mAccpacDBLink.OpenView("AR0061", out CBBTCH1detail4);
                    mAccpacDBLink.OpenView("AR0406", out CBBTCH1detail5);
                    mAccpacDBLink.OpenView("AR0170", out CBBTCH1detail6);

                    CBBTCH1batch.Compose(new AccpacView[] { CBBTCH1header });
                    CBBTCH1header.Compose(new AccpacView[] { CBBTCH1batch, CBBTCH1detail3, CBBTCH1detail1, CBBTCH1detail5, CBBTCH1detail6 });
                    CBBTCH1detail1.Compose(new AccpacView[] { CBBTCH1header, CBBTCH1detail2, CBBTCH1detail4 });
                    CBBTCH1detail2.Compose(new AccpacView[] { CBBTCH1detail1 });
                    CBBTCH1detail3.Compose(new AccpacView[] { CBBTCH1header });
                    CBBTCH1detail4.Compose(new AccpacView[] { CBBTCH1batch, CBBTCH1header, CBBTCH1header, CBBTCH1detail3, CBBTCH1detail1, CBBTCH1detail2 });
                    CBBTCH1detail5.Compose(new AccpacView[] { CBBTCH1header });
                    CBBTCH1detail6.Compose(new AccpacView[] { CBBTCH1header });

                    CBBTCH1batch.RecordClear();
                    CBBTCH1batch.Fields.FieldByName["CODEPYMTYP"].set_Value("CA");
                    CBBTCH1header.Fields.FieldByName["CODEPYMTYP"].set_Value("CA");
                    CBBTCH1detail3.Fields.FieldByName["CODEPAYM"].set_Value("CA");
                    CBBTCH1detail1.Fields.FieldByName["CODEPAYM"].set_Value("CA");
                    CBBTCH1detail2.Fields.FieldByName["CODEPAYM"].set_Value("CA");
                    CBBTCH1detail4.Fields.FieldByName["PAYMTYPE"].set_Value("CA");
                    CBBTCH1batch.Fields.FieldByName["CNTBTCH"].set_Value(batchNumber);
                    CBBTCH1batch.Read();

                    CBBTCH1header.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
                    CBBTCH1header.Fields.FieldByName["RMITTYPE"].set_Value("2");
                    CBBTCH1detail1.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_NOINSERT);
                    CBBTCH1header.Fields.FieldByName["IDCUST"].set_Value(customerId);

                    CBBTCH1detail1.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_NOINSERT);
                    CBBTCH1header.Fields.FieldByName["TEXTRMIT"].set_Value(receiptDescription);
                    CBBTCH1header.Fields.FieldByName["CODEPAYM"].set_Value("CASH");
                    CBBTCH1header.Fields.FieldByName["IDRMIT"].set_Value(referenceNumber);
                    CBBTCH1header.Fields.FieldByName["AMTRMIT"].set_Value(amount);
                    CBBTCH1header.Fields.FieldByName["DATERMIT"].set_Value(paymentDate);
                    CBBTCH1detail1.Fields.FieldByName["AMTPAYM"].set_Value(amount);

                    CBBTCH1detail1.Insert();
                    CBBTCH1header.Insert();
                    CBBTCH1header.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);

                    UpdateList(customerId, "Transferred", "Prepayment");
                    LogOperation("Prepayment Transferred", 1);
                    return true;
                }
                else //Receipt Transfer
                {
                    receiptDescription = receiptDescriptionEx;
                    int batchNum = getIbatchNumber(Convert.ToInt32(invnum));
                    bool shouldAllocate = false;

                    if (checkAccpacInvoiceAvail(Convert.ToInt32(invnum)))
                    {
                        if (checkAccpacIBatchPosted(batchNum))
                        {
                            shouldAllocate = true;
                        }
                    }

                    comApiReceiptInsert(batchNumber, customerId, amount, paymentDate.ToString(), receiptDescription, referenceNumber, invnum, shouldAllocate);


                    return true;
                }
            }
        }

        bool IsEmpty(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
                if (table.Rows.Count != 0) return false;

            return true;
        }

        Data Translate(string cNum, string feeType, string companyName, string debit, string notes, string _fcode, string FreqUsage)
        {
            string temp = "";
            string iv_customerId = "";
            Data dt = new Data();

            if (_fcode == "PAYMENT")
            {
                dt.fcode = "";
            }
            else
            {
                dt.fcode = _fcode;
            }

            if (debit != "0.0000" && debit != "")
            {
                dt.debit = debit;

                for (int i = 0; i < cNum.Length; i++)
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

                if (feeType == "SLF")
                {
                    iv_customerId = cNum + "-L";
                    dt.customerId = iv_customerId;
                    dt.feeType = "SLF";
                    dt.companyName = companyName + " - Spec Fee";
                    dt.desc = "Spec Fee";
                }

                else if (notes == "Radio Operator")
                {
                    iv_customerId = cNum + "-L";
                    dt.customerId = iv_customerId;
                    dt.feeType = "SLF";
                    dt.companyName = companyName + " - Spec Fee";
                    dt.desc = "Spec Fee";
                }

                else if (notes == "Type Approval" || FreqUsage == "TA-ProAmend")
                {
                    iv_customerId = cNum + "-T";
                    dt.customerId = iv_customerId;
                    dt.feeType = "RF";
                    dt.companyName = companyName + " - Type Approval";
                    dt.desc = "Processing Fee";
                }

                else if (notes != "Type Approval" && feeType == "APF")
                {
                    iv_customerId = cNum + "-R";
                    dt.customerId = iv_customerId;
                    dt.feeType = "RF";
                    dt.companyName = companyName + " - Processing Fee";
                    dt.desc = "Processing Fee";
                }
                else
                {
                    iv_customerId = cNum + "-R";
                    dt.customerId = iv_customerId;
                    dt.companyName = companyName + " - Reg Fee";
                    dt.feeType = "RF";
                    dt.desc = "Reg Fee";
                }

                dt.success = true;
                return dt;
            }
            else if (debit == "")
            {
                if (companyName == "")
                {
                    companyName = " ";
                    dt.companyName = companyName;
                }
                else
                {
                    dt.companyName = companyName;
                }

                for (int i = 0; i < cNum.Length; i++)
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

                if (feeType == "SLF")
                {
                    iv_customerId = cNum + "-L";
                    dt.customerId = iv_customerId;
                    dt.feeType = "SLF";
                    dt.companyName = companyName + " - Spec Fee";
                    dt.companyName_NewCust = companyName;
                    dt.desc = "Spec Fee";
                }

                else if (feeType == "Reg")
                {
                    iv_customerId = cNum + "-R";
                    dt.customerId = iv_customerId;
                    dt.feeType = "RF";
                    dt.companyName = companyName + " - Reg Fee";
                    dt.companyName_NewCust = companyName;
                    dt.desc = "Reg Fee";
                }
                else if (notes == "Radio Operator")
                {
                    iv_customerId = cNum + "-L";
                    dt.customerId = iv_customerId;
                    dt.feeType = "SLF";
                    dt.companyName = companyName + " - Spec Fee";
                    dt.companyName_NewCust = companyName;
                    dt.desc = "Spec Fee";
                }

                else if (notes == "Type Approval" || FreqUsage == "TA-ProAmend")
                {
                    iv_customerId = cNum + "-T";
                    dt.customerId = iv_customerId;
                    dt.feeType = "RF";
                    dt.companyName = companyName + " - Type Approval";
                    dt.companyName_NewCust = companyName;
                    dt.desc = "Processing Fee";
                }

                else if (notes != "Type Approval" && feeType == "APF")
                {
                    iv_customerId = cNum + "-R";
                    dt.customerId = iv_customerId;
                    dt.feeType = "RF";
                    dt.companyName = companyName + " - Processing Fee";
                    dt.companyName_NewCust = companyName;
                    dt.desc = "Processing Fee";
                }
                else
                {
                    iv_customerId = cNum + "-R";
                    dt.customerId = iv_customerId;
                    dt.feeType = "RF";
                    dt.companyName = companyName + " - Reg Fee";
                    dt.companyName_NewCust = companyName;
                    dt.desc = "Reg Fee";
                }

                dt.success = true;

                return dt;
            }
            else
            {
                dt.success = false;
                return dt;
            }
        }

        void CreateCustomer(string idCust, string nameCust)
        {
            try
            {
                LogOperation("Creating Customer " + idCust, 1);
                string groupCode = "";
                AccpacView ARCUSTOMER1header = null;
                mAccpacDBLink.OpenView("AR0024", out ARCUSTOMER1header);
                AccpacView ARCUSTOMER1detail = null;
                mAccpacDBLink.OpenView("AR0400", out ARCUSTOMER1detail);
                AccpacView ARCUSTSTAT2 = null;
                mAccpacDBLink.OpenView("AR0022", out ARCUSTSTAT2);
                AccpacView ARCUSTCMT3 = null;
                mAccpacDBLink.OpenView("AR0021", out ARCUSTCMT3);

                ARCUSTOMER1header.Compose(new AccpacView[] { ARCUSTOMER1detail });
                ARCUSTOMER1detail.Compose(new AccpacView[] { ARCUSTOMER1header });

                if (idCust[5].ToString() + idCust[6].ToString() == "-L")
                {
                    groupCode = "LICCOM";
                }
                else if (idCust[5].ToString() + idCust[6].ToString() == "-R")
                {
                    groupCode = "REGCOM";
                }

                else if (idCust[5].ToString() + idCust[6].ToString() == "-T")
                {
                    groupCode = "TYPEUS";
                }

                ARCUSTOMER1header.Fields.FieldByName["IDCUST"].set_Value(idCust);
                ARCUSTOMER1header.Process();
                ARCUSTOMER1header.Fields.FieldByName["NAMECUST"].set_Value(nameCust);
                ARCUSTOMER1header.Fields.FieldByName["IDGRP"].set_Value(groupCode);
                ARCUSTOMER1header.Process();
                ARCUSTOMER1header.Fields.FieldByName["CODETAXGRP"].set_Value("JATAX");
                ARCUSTOMER1header.Insert();
                UpdateList(idCust, "Customer Created", " ");
                intLink.UpdateCustomerCount();
                intLink.StoreCustomer(idCust, nameCust);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        void Ignore(string invoiceNum, string date, string documentType)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, string, string>(Ignore), new object[] { invoiceNum, date, documentType });
            }
            else
            {
                ListViewItem item = new ListViewItem(invoiceNum);
                item.SubItems.Add(date);
                item.SubItems.Add("Ignored");
                item.SubItems.Add(documentType);
                listView1.Items.Add(item);
                listView1.EnsureVisible(listView1.Items.Count - 1);
            }
        }

        void UpdateList(string invoiceNum, string message, string documentT)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, string, string>(UpdateList), new object[] { invoiceNum, message, documentT });
            }
            else
            {
                ListViewItem item = new ListViewItem(invoiceNum);
                item.SubItems.Add(DateTime.Now.ToString());
                item.SubItems.Add(message);
                item.SubItems.Add(documentT);
                listView1.Items.Add(item);
                listView1.EnsureVisible(listView1.Items.Count - 1);
            }
        }

        public string createBatchDesc(string batchType)
        {
            string result = "";
            switch (batchType)
            {
                case TYPE_APPROVAL:
                    result = "New Applications - Type Approvals - " + DateTime.Now.ToString("dd/MM/yyyy");
                    break;
                case MAJ:
                    result = "New Applications - MAJ Licences - " + DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Year.ToString();
                    break;
                case NON_MAJ:
                    result = "New Applications - Non-MAJ Licences - " + DateTime.Now.ToString("dd/MM/yyyy");
                    break;
                case CREDIT_NOTE:
                    result = "Credit Notes for " + DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Year.ToString();
                    break;
            }
            return result;
        }

        public int getBatch(string batchType, string invoiceid)
        {
            DateTime val = intLink.GetValidity(Convert.ToInt32(invoiceid));
            string renspec = RENEWAL_SPEC + val.ToString("MMMM") + " " + val.Year.ToString();
            string renreg = RENEWAL_REG + val.ToString("MMMM") + " " + val.Year.ToString();

            if (intLink.batchAvail(batchType))
            {
                int batch = intLink.getAvailBatch(batchType);
                if (!intLink.isBatchExpired(batch))
                {
                    if (!checkAccpacIBatchPosted(batch))
                    {
                        return batch;
                    }
                    else
                    {
                        LogOperation("Batch " + batch.ToString() + " posted", 2);
                        intLink.closeInvoiceBatch(batch);

                        int newbatch = GetLastInvoiceBatch() + 1;
                        LogOperation("Creating new batch: " + newbatch.ToString(), 1);
                        LogOperation("Batch type: " + batchType, 1);

                        if (batchType == renreg)
                        {
                            intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "Regulatory");
                        }

                        else if (batchType == renspec)
                        {
                            intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "Spectrum");
                        }

                        else
                        {
                            intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "");
                        }


                        if (batchType == renreg || batchType == renspec)
                        {
                            CreateInvoiceBatch(batchType);
                        }
                        else
                            CreateInvoiceBatch(createBatchDesc(batchType));

                        return newbatch;
                    }
                }
                else
                {
                    if (batchType == "" || batchType == "")
                    {
                        intLink.resetInvoiceTotal();
                    }

                    LogOperation("Batch " + batch.ToString() + " expired", 2);
                    intLink.closeInvoiceBatch(batch);
                    int newbatch = GetLastInvoiceBatch() + 1;

                    if (batchType == renreg)
                    {
                        intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "Regulatory");
                    }
                    else if (batchType == renspec)
                    {
                        intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "Spectrum");
                    }
                    else
                    {
                        intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "");
                    }

                    if (batchType == renreg || batchType == renspec)
                    {
                        CreateInvoiceBatch(batchType);
                    }
                    else
                    {
                        CreateInvoiceBatch(createBatchDesc(batchType));
                    }

                    LogOperation("Batch: " + newbatch.ToString() + " created", 1);
                    return newbatch;
                }

            }
            else
            {
                int newbatch = GetLastInvoiceBatch() + 1;
                if (batchType == renreg)
                {
                    intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "Regulatory");
                }

                else if (batchType == renspec)
                {
                    intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "Spectrum");
                }

                else
                {
                    intLink.createInvoiceBatch(generateDaysExpire(batchType), newbatch, batchType, "");
                }


                if (batchType == renreg || batchType == renspec)
                {
                    CreateInvoiceBatch(batchType);
                }
                else
                    CreateInvoiceBatch(createBatchDesc(batchType));


                return newbatch;
            }
        }

        public int generateDaysExpire(string batchType)
        {
            if (batchType != NON_MAJ && batchType != TYPE_APPROVAL && batchType != ONE_DAY)
            {
                int expiry = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - DateTime.Now.Day;
                expiry++;

                return expiry;
            }
            else return 1;
        }

        private void TableDepend__OnError(object sender, TableDependency.EventArgs.ErrorEventArgs e)
        {
            try
            {
                var x = e.Message;
                LogOperation(x, 2);
                tableDependPay.Stop();
                tableDependCancellation.Stop();

                LogOperation("Reconfiguring table dependecies", 1);
                tableDependPay.Start();
                tableDependCancellation.Start();

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                tableDependPay.Start();
                tableDependCancellation.Start();

            }
        }

        private void TableDepend__InvoicesOnError(object sender, TableDependency.EventArgs.ErrorEventArgs e)
        {
            try
            {
                var x = e.Message;
                LogOperation(x, 2);
                tableDependPay.Stop();
                tableDependCancellation.Stop();

                LogOperation("Reconfiguring table dependecies", 1);
                tableDependPay.Start();
                tableDependCancellation.Start();

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                tableDependPay.Start();
                tableDependCancellation.Start();
            }
        }

        private void TableDependPay_OnError(object sender, TableDependency.EventArgs.ErrorEventArgs e)
        {
            try
            {
                var x = e.Message;
                LogOperation(x, 2);

                tableDependPay.Stop();
                tableDependCancellation.Stop();

                LogOperation("Reconfiguring table dependecies", 1);
                tableDependPay.Start();
                tableDependCancellation.Start();

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                tableDependPay.Start();
                tableDependCancellation.Start();
            }
        }

        private void TableDependCancellation_OnError(object sender, TableDependency.EventArgs.ErrorEventArgs e)
        {
            string msg = e.Message;
        }

        private void TableDependInfo_OnError(object sender, TableDependency.EventArgs.ErrorEventArgs e)
        {
            string msg = e.Message;
        }

        private void TableDependInfo_OnChanged(object sender, RecordChangedEventArgs<SqlNotify_DocumentInfo> e)
        {
            var docInfo = e.Entity;

            try
            {
                if (e.ChangeType == ChangeType.Insert)
                {
                    FileLog.Write("Event Type: INSERT");
                    if (docInfo.DocumentType == INVOICE)
                    {
                        LogOperation("Incoming Invoice", 2);
                        InvoiceInfo invoiceInfo = new InvoiceInfo();
                        FileLog.Write("Invoice Info Object: " + invoiceInfo.ToString());

                        while (invoiceInfo.amount == 0)
                        {
                            FileLog.Write("Waiting for invoice amount to update, current value: " + invoiceInfo.amount.ToString());
                            invoiceInfo = intLink.getInvoiceDetails(docInfo.OriginalDocumentID);
                            Thread.Sleep(1000);
                        }
                        FileLog.Write("Invoice amount: " + invoiceInfo.amount.ToString());

                        List<string> clientInfo = intLink.getClientInfo_inv(invoiceInfo.CustomerId.ToString());
                        FileLog.Write("Client Info: " + clientInfo.ToString());

                        string companyName = clientInfo[0].ToString();
                        string cNum = clientInfo[1].ToString();
                        string fname = clientInfo[2].ToString();
                        string lname = clientInfo[3].ToString();
                        Maj m = new Maj();
                        InsertionReturn stat = new InsertionReturn();

                        if (companyName == "" || companyName == " " || companyName == null)
                        {
                            companyName = fname + " " + lname;
                        }

                        LogOperation("Amount: " + invoiceInfo.amount.ToString(), 1);
                        LogOperation("Client Name(first | last): " + fname + " " + lname, 1);
                        LogOperation("Company Name: " + companyName, 1);

                        FileLog.Write("Translation started...");
                        Data dt = Translate(cNum, invoiceInfo.FeeType, companyName, "", invoiceInfo.notes, intLink.GetAccountNumber(invoiceInfo.Glid), invoiceInfo.FreqUsage); // application stops here...
                        FileLog.Write("Object result: " + dt.ToString());

                        DateTime invoiceValidity = intLink.GetValidity(docInfo.OriginalDocumentID); // or here...
                        FileLog.Write("Invoice Validity: " + invoiceValidity);

                        int financialyear = 0;
                        if (invoiceValidity.Month > 3)
                        {
                            financialyear = invoiceValidity.Year + 1;
                        }
                        else
                        {
                            financialyear = invoiceValidity.Year;
                        }
                        FileLog.Write("Financial Year: " + financialyear);

                        List<string> data = intLink.checkInvoiceAvail(docInfo.OriginalDocumentID.ToString());
                        int r = intLink.getInvoiceReference(docInfo.OriginalDocumentID);
                        FileLog.Write("Invoice Reference number: " + r);

                        if (r != -1)
                        {
                            FileLog.Write("Getting Maj Details...");
                            m = intLink.getMajDetail(r);
                            FileLog.Write(m.ToString());
                        }

                        if (isPeriodCreated(financialyear))
                        {
                            if (invoiceInfo.Glid < 5000 || data != null)
                            {
                                LogOperation("CreditGL: " + invoiceInfo.Glid, 1);
                                if (data != null)
                                {
                                    if (data[1].ToString() == "NT")
                                    {
                                        if (dt.feeType == "SLF" && invoiceInfo.notes == "Renewal")
                                        {

                                            stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()).ToString());

                                            if (stat.status == "Not Exist")
                                            {
                                                LogOperation("Customer " + dt.customerId + " does not exist", 1);
                                                CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                                InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()).ToString());

                                                intLink.UpdateBatchCount(RENEWAL_SPEC + "For " + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString());
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                            else
                                            {
                                                intLink.UpdateBatchCount(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString());
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString() + " For " + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                        }
                                        else if (dt.feeType == "RF" && invoiceInfo.notes == "Renewal")
                                        {
                                            stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()).ToString());

                                            if (stat.status == "Not Exist")
                                            {
                                                CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                                InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()).ToString());

                                                intLink.UpdateBatchCount(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString());
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                            else
                                            {
                                                intLink.UpdateBatchCount(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString());
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);

                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                        }
                                        else if ((invoiceInfo.notes == "Annual Fee" && m.stationType == "SSL" && m.certificateType == 0 && m.proj == "JMC") || (invoiceInfo.FreqUsage == "PRS55"))
                                        {
                                            stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(MAJ, docInfo.OriginalDocumentID.ToString()).ToString());

                                            if (stat.status == "Not Exist")
                                            {
                                                CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                                InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(MAJ, docInfo.OriginalDocumentID.ToString()).ToString());

                                                intLink.UpdateBatchCount(MAJ);
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);

                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                            else
                                            {
                                                intLink.UpdateBatchCount(MAJ);
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                        }
                                        else if (invoiceInfo.notes == "Type Approval" || invoiceInfo.FreqUsage == "TA-ProAmend")
                                        {
                                            stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, changetous(invoiceInfo.amount).ToString(), getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()).ToString());

                                            if (stat.status == "Not Exist")
                                            {
                                                CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                                InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, changetous(invoiceInfo.amount).ToString(), getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()).ToString());

                                                intLink.UpdateBatchCount(TYPE_APPROVAL);
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", intLink.GetRate(), changetous(invoiceInfo.amount), invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                            else
                                            {
                                                intLink.UpdateBatchCount(TYPE_APPROVAL);
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", intLink.GetRate(), changetous(invoiceInfo.amount), invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                        }

                                        else if (invoiceInfo.notes == "Annual Fee" || invoiceInfo.notes == "Modification" || invoiceInfo.notes == "Radio Operator")
                                        {
                                            stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()).ToString());

                                            if (stat.status == "Not Exist")
                                            {
                                                CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                                InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()).ToString());

                                                intLink.UpdateBatchCount(NON_MAJ);
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);

                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                            else
                                            {
                                                intLink.UpdateBatchCount(NON_MAJ);
                                                intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                                prevInvoice = currentInvoice;
                                                LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                                intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                                intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                                prevInvoice = currentInvoice;
                                            }
                                        }
                                    }
                                    else if (data[1].ToString() == "T")
                                    {
                                        List<string> detail = new List<string>(3);
                                        detail = intLink.GetInvoiceDetails(docInfo.OriginalDocumentID);
                                        int batchNumber = getIbatchNumber(docInfo.OriginalDocumentID);


                                        if (!checkAccpacIBatchPosted(batchNumber))
                                        {
                                            if (invoiceInfo.Glid != Convert.ToInt32(detail[2].ToString()) || invoiceInfo.amount.ToString() != detail[3].ToString())
                                            {
                                                if (invoiceInfo.notes == "Type Approval" || invoiceInfo.FreqUsage == "TA-ProAmend")
                                                {
                                                    string usamt = "";
                                                    usamt = changetousupdated(invoiceInfo.amount, docInfo.OriginalDocumentID).ToString();
                                                    UpdateInvoice(dt.fcode, Math.Round(changetousupdated(invoiceInfo.amount, docInfo.OriginalDocumentID), 2).ToString(), batchNumber.ToString().ToString(), getEntryNumber(docInfo.OriginalDocumentID).ToString());
                                                    intLink.storeInvoice(docInfo.OriginalDocumentID, batchNumber, invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "updated", 1, Convert.ToDecimal(usamt), invoiceInfo.isvoided, 0, 0);
                                                }
                                                else
                                                {
                                                    UpdateInvoice(dt.fcode, invoiceInfo.amount.ToString(), batchNumber.ToString().ToString(), getEntryNumber(docInfo.OriginalDocumentID).ToString());
                                                    intLink.storeInvoice(docInfo.OriginalDocumentID, batchNumber, invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "updated", 1, 0, invoiceInfo.isvoided, 0, 0);

                                                }
                                                intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);
                                                LogOperation("Updated Invoice: " + docInfo.OriginalDocumentID.ToString(), 2);
                                                prevInvoice = currentInvoice;
                                            }
                                            else
                                            {
                                                prevInvoice = currentInvoice;
                                                LogOperation("Update is not needed." + docInfo.OriginalDocumentID.ToString(), 2);
                                            }
                                        }
                                        else
                                        {
                                            LogOperation("Batch: " + batchNumber.ToString() + " is already posted", 2);
                                        }
                                    }
                                }
                                else
                                {
                                    if (dt.feeType == "SLF" && invoiceInfo.notes == "Renewal")
                                    {
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Stored", 1);
                                        prevInvoice = currentInvoice;
                                        prevTime = DateTime.Now;
                                    }
                                    else if (dt.feeType == "RF" && invoiceInfo.notes == "Renewal")
                                    {
                                        DataSet df = new DataSet();
                                        df = intLink.GetRenewalInvoiceValidity(docInfo.OriginalDocumentID);
                                        DateTime val = DateTime.Now;
                                        if (!IsEmpty(df))
                                        {

                                            DataRow dr = df.Tables[0].Rows[0];
                                            string date = dr.ItemArray.GetValue(0).ToString();
                                            val = Convert.ToDateTime(date);

                                        }

                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Stored", 1);
                                        prevInvoice = currentInvoice;
                                        prevTime = DateTime.Now;
                                    }

                                    else if ((invoiceInfo.notes == "Annual Fee" && m.stationType == "SSL" && m.certificateType == 0 && m.proj == "JMC") || (invoiceInfo.FreqUsage == "PRS55"))
                                    {
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Stored", 1);
                                        prevInvoice = currentInvoice;
                                        prevTime = DateTime.Now;
                                    }
                                    else if (invoiceInfo.notes == "Type Approval" || invoiceInfo.FreqUsage == "TA-ProAmend")
                                    {
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", intLink.GetRate(), changetous(invoiceInfo.amount), invoiceInfo.isvoided, 0, 0);
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Stored", 1);
                                        prevInvoice = currentInvoice;
                                        prevTime = DateTime.Now;
                                    }
                                    else if (invoiceInfo.notes == "Annual Fee" || invoiceInfo.notes == "Modification" || invoiceInfo.notes == "Radio Operator")
                                    {
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Stored", 1);
                                        prevInvoice = currentInvoice;
                                        prevTime = DateTime.Now;
                                    }
                                }
                            }
                            else
                            {
                                if (dt.feeType == "SLF" && invoiceInfo.notes == "Renewal")
                                {
                                    stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()).ToString());

                                    if (stat.status == "Not Exist")
                                    {
                                        LogOperation("Customer " + dt.customerId + " does not exist", 1);
                                        CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                        InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()).ToString());

                                        intLink.UpdateBatchCount(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString());
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), invoiceInfo.amount);
                                    }
                                    else
                                    {
                                        intLink.UpdateBatchCount(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString());
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);

                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);
                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(RENEWAL_SPEC + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), invoiceInfo.amount);
                                    }
                                }
                                else if (dt.feeType == "RF" && invoiceInfo.notes == "Renewal")
                                {
                                    stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()).ToString());

                                    if (stat.status == "Not Exist")
                                    {
                                        CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                        InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()).ToString());

                                        intLink.UpdateBatchCount(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString());
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);

                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), invoiceInfo.amount);
                                    }
                                    else
                                    {
                                        intLink.UpdateBatchCount(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString());
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);

                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(RENEWAL_REG + invoiceValidity.ToString("MMMM") + " " + invoiceValidity.Year.ToString(), invoiceInfo.amount);
                                    }
                                }

                                else if ((invoiceInfo.notes == "Annual Fee" && m.stationType == "SSL" && m.certificateType == 0 && m.proj == "JMC") || (invoiceInfo.FreqUsage == "PRS55"))
                                {
                                    stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(MAJ, docInfo.OriginalDocumentID.ToString()).ToString());

                                    if (stat.status == "Not Exist")
                                    {
                                        CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                        InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(MAJ, docInfo.OriginalDocumentID.ToString()).ToString());

                                        intLink.UpdateBatchCount(MAJ);
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);

                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(MAJ, invoiceInfo.amount);
                                    }
                                    else
                                    {
                                        intLink.UpdateBatchCount(MAJ);
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);

                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(MAJ, invoiceInfo.amount);
                                    }
                                }
                                else if (invoiceInfo.notes == "Type Approval" || invoiceInfo.FreqUsage == "TA-ProAmend")
                                {
                                    stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, changetous(invoiceInfo.amount).ToString(), getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()).ToString());

                                    if (stat.status == "Not Exist")
                                    {
                                        CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                        InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, changetous(invoiceInfo.amount).ToString(), getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()).ToString());

                                        intLink.UpdateBatchCount(TYPE_APPROVAL);
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", intLink.GetRate(), changetous(invoiceInfo.amount), invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(TYPE_APPROVAL, invoiceInfo.amount);
                                    }
                                    else
                                    {
                                        intLink.UpdateBatchCount(TYPE_APPROVAL);
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(TYPE_APPROVAL, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", intLink.GetRate(), changetous(invoiceInfo.amount), invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(TYPE_APPROVAL, invoiceInfo.amount);
                                    }
                                }

                                else if (invoiceInfo.notes == "Annual Fee" || invoiceInfo.notes == "Modification" || invoiceInfo.notes == "Radio Operator")
                                {
                                    stat = InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()).ToString());

                                    if (stat.status == "Not Exist")
                                    {
                                        CreateCustomer(dt.customerId, dt.companyName_NewCust);
                                        InvBatchInsert(dt.customerId, docInfo.OriginalDocumentID.ToString(), dt.companyName, dt.fcode, invoiceInfo.amount.ToString(), getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()).ToString());

                                        intLink.UpdateBatchCount(NON_MAJ);
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(NON_MAJ, invoiceInfo.amount);
                                    }
                                    else
                                    {
                                        intLink.UpdateBatchCount(NON_MAJ);
                                        intLink.UpdateEntryNumber(docInfo.OriginalDocumentID);
                                        intLink.UpdateCreditGl(docInfo.OriginalDocumentID, invoiceInfo.Glid);

                                        prevInvoice = currentInvoice;
                                        LogOperation("Invoice Id: " + docInfo.OriginalDocumentID.ToString() + " Transferred", 1);
                                        intLink.storeInvoice(docInfo.OriginalDocumentID, getBatch(NON_MAJ, docInfo.OriginalDocumentID.ToString()), invoiceInfo.Glid, companyName, dt.customerId, DateTime.Now, invoiceInfo.Author, invoiceInfo.amount, "no modification", 1, 0, invoiceInfo.isvoided, 0, 0);
                                        intLink.MarkAsTransferred(docInfo.OriginalDocumentID);
                                        prevInvoice = currentInvoice;
                                        intLink.updateBatchAmount(NON_MAJ, invoiceInfo.amount);
                                    }
                                }
                            }

                            prevInvoice = currentInvoice;
                            prevTime = DateTime.Now;
                        }

                        else
                        {
                            LogOperation("Invoice not Transferred as fiscal year " + financialyear + " not yet Created in Sage. ", 2);
                        }
                    }
                    else if (docInfo.DocumentType == RECEIPT && docInfo.PaymentMethod != 99)
                    {
                        LogOperation("Incoming Receipt", 2);
                        Data dt = new Data();
                        PaymentInfo pinfo = new PaymentInfo();

                        List<string> paymentData = new List<string>(3);
                        List<string> clientData = new List<string>(3);
                        List<string> feeData = new List<string>(3);

                        while (pinfo.ReceiptNumber == 0)
                        {
                            pinfo = intLink.getPaymentInfo(docInfo.OriginalDocumentID);
                            Thread.Sleep(1000);
                        }

                        var receipt = pinfo.ReceiptNumber;
                        var transid = pinfo.GLTransactionID;
                        var id = pinfo.CustomerID.ToString();

                        paymentData = intLink.GetPaymentInfo(transid);
                        var debit = pinfo.Debit.ToString();
                        var glid = pinfo.GLID.ToString();
                        var invoiceId = pinfo.InvoiceID.ToString();

                        DateTime paymentDate = pinfo.Date1;
                        string prepstat = " ";
                        DateTime valstart = DateTime.Now.Date;
                        DateTime valend = DateTime.Now.Date;

                        clientData = intLink.getClientInfo_inv(id);
                        var companyName = clientData[0].ToString();
                        var customerId = clientData[1].ToString();
                        var fname = clientData[2].ToString();
                        var lname = clientData[3].ToString();

                        if (companyName == "" || companyName == " " || companyName == null)
                        {
                            companyName = fname + " " + lname;
                        }
                        var ftype = " ";
                        var notes = " ";

                        if (Convert.ToInt32(invoiceId) > 0)
                        {
                            feeData = intLink.GetFeeInfo(Convert.ToInt32(invoiceId));
                            ftype = feeData[0].ToString();
                            notes = feeData[1].ToString();
                            LogOperation("Invoice Id: " + invoiceId.ToString(), 1);
                            LogOperation("Customer Id: " + customerId, 1);
                            prepstat = "No";
                            valstart = intLink.GetValidity(Convert.ToInt32(invoiceId));
                            valend = intLink.GetValidityEnd(Convert.ToInt32(invoiceId));
                        }
                        else
                        {
                            prepstat = "Yes";
                            LogOperation("Prepayment", 1);
                            LogOperation("Customer Id: " + customerId, 1);
                            var gl = intLink.GetCreditGlID((transid + 1).ToString());

                            if (gl == 5321)
                            {
                                ftype = "SLF";
                            }
                            else if (gl == 5149)
                            {
                                ftype = "RF";
                            }
                        }

                        dt = Translate(customerId, ftype, companyName, debit, notes, "", intLink.getFreqUsage(Convert.ToInt32(invoiceId)));

                        bool cusexists;
                        cusexists = CustomerExists(dt.customerId);
                        if (Convert.ToInt32(invoiceId) == 0)
                        {
                            if (!cusexists)
                            {
                                CreateCustomer(dt.customerId, companyName);
                                intLink.StoreCustomer(dt.customerId, companyName);
                            }
                        }

                        if (cusexists || Convert.ToInt32(invoiceId) == 0 && Convert.ToInt32(glid) > 0)
                        {
                            if (glid == "5146")
                            {
                                LogOperation("Bank: FGB JA$ CURRENT A/C", 1);
                                if (dt.success)
                                {
                                    if (receiptBatchAvail("FGBJMREC"))
                                    {
                                        string reference = intLink.GetCurrentRef("FGBJMREC");
                                        LogOperation("Target Batch: " + intLink.getRecieptBatch("FGBJMREC"), 1);
                                        LogOperation("Transferring Receipt", 1);

                                        ReceiptTransfer(intLink.getRecieptBatch("FGBJMREC"), dt.customerId, dt.debit, dt.companyName, reference, invoiceId, paymentDate, dt.desc, customerId, valstart, valend);
                                        intLink.UpdateBatchCountPayment(intLink.getRecieptBatch("FGBJMREC"));
                                        intLink.UpdateReceiptNumber(receipt, intLink.GetCurrentRef("FGBJMREC"));
                                        intLink.IncrementReferenceNumber(intLink.getBankCodeId("FGBJMREC"), Convert.ToDecimal(dt.debit));
                                        intLink.storePayment(dt.customerId, companyName, DateTime.Now, invoiceId, Convert.ToDecimal(dt.debit), 0, prepstat, Convert.ToInt32(reference), Convert.ToInt32(glid), "No", 0);
                                    }
                                    else
                                    {
                                        string reference = intLink.GetCurrentRef("FGBJMREC");
                                        CreateReceiptBatchEx("FGBJMREC", "Middleware Generated Batch for FGBJMREC");
                                        intLink.openNewReceiptBatch(1, GetLastPaymentBatch(), "FGBJMREC");

                                        LogOperation("Target Batch: " + intLink.getRecieptBatch("FGBJMREC"), 1);
                                        LogOperation("Transferring Receipt", 1);

                                        ReceiptTransfer(intLink.getRecieptBatch("FGBJMREC"), dt.customerId, dt.debit, dt.companyName, reference, invoiceId, paymentDate, dt.desc, customerId, valstart, valend);
                                        intLink.UpdateBatchCountPayment(intLink.getRecieptBatch("FGBJMREC"));
                                        intLink.UpdateReceiptNumber(receipt, intLink.GetCurrentRef("FGBJMREC"));
                                        intLink.IncrementReferenceNumber(intLink.getBankCodeId("FGBJMREC"), Convert.ToDecimal(dt.debit));
                                        intLink.storePayment(dt.customerId, companyName, DateTime.Now, invoiceId, Convert.ToDecimal(dt.debit), 0, prepstat, Convert.ToInt32(reference), Convert.ToInt32(glid), "No", 0);
                                    }
                                }
                            }
                            else if (glid == "5147")
                            {
                                LogOperation("Bank: FGB US$ SAVINGS A/C", 1);
                                decimal usamount = 0;
                                decimal transferedAmt = Convert.ToDecimal(dt.debit) / intLink.GetUsRateByInvoice(Convert.ToInt32(invoiceId));
                                string clientIdPrefix = "";
                                decimal currentRate = 1;

                                for (int i = 0; i < dt.customerId.Length; i++)
                                {
                                    if (dt.customerId[i] != '-')
                                    {
                                        clientIdPrefix += dt.customerId[i];
                                    }
                                    else
                                    {
                                        i = dt.customerId.Length;
                                    }
                                }

                                if (prepstat == "Yes" && clientIdPrefix == intLink.getClientIdZRecord())
                                {
                                    dt.customerId = clientIdPrefix + "-T";
                                    currentRate = intLink.GetRate();
                                }

                                if (dt.customerId[6] == 'T')
                                {
                                    usamount = Convert.ToDecimal(dt.debit) / intLink.GetUsRateByInvoice(Convert.ToInt32(invoiceId));
                                    intLink.modifyInvoiceList(0, intLink.GetUsRateByInvoice(Convert.ToInt32(invoiceId)), dt.customerId);
                                    currentRate = intLink.GetRate();
                                }

                                if (dt.success)
                                {
                                    if (receiptBatchAvail("FGBUSMRC"))
                                    {
                                        string reference = intLink.GetCurrentRef("FGBUSMRC");
                                        LogOperation("Target Batch: " + intLink.getRecieptBatch("FGBUSMRC"), 1);
                                        LogOperation("Transferring Receipt", 1);

                                        ReceiptTransfer(intLink.getRecieptBatch("FGBUSMRC"), dt.customerId, Math.Round(transferedAmt, 2).ToString(), dt.companyName, reference, invoiceId, paymentDate, dt.desc, customerId, valstart, valend);
                                        intLink.UpdateBatchCountPayment(intLink.getRecieptBatch("FGBUSMRC"));
                                        intLink.UpdateReceiptNumber(receipt, intLink.GetCurrentRef("FGBUSMRC"));
                                        intLink.IncrementReferenceNumber(intLink.getBankCodeId("FGBUSMRC"), Convert.ToDecimal(dt.debit));
                                        intLink.storePayment(dt.customerId, companyName, DateTime.Now, invoiceId, Convert.ToDecimal(dt.debit), usamount, prepstat, Convert.ToInt32(reference), Convert.ToInt32(glid), "No", currentRate);
                                    }
                                    else
                                    {
                                        string reference = intLink.GetCurrentRef("FGBUSMRC");
                                        CreateReceiptBatchEx("FGBUSMRC", "Middleware Generated Batch for FGBUSMRC");
                                        intLink.openNewReceiptBatch(1, GetLastPaymentBatch(), "FGBUSMRC");

                                        LogOperation("Target Batch: " + intLink.getRecieptBatch("FGBUSMRC"), 1);
                                        LogOperation("Transferring Receipt", 1);

                                        ReceiptTransfer(intLink.getRecieptBatch("FGBUSMRC"), dt.customerId, Math.Round(transferedAmt, 2).ToString(), dt.companyName, reference, invoiceId, paymentDate, dt.desc, customerId, valstart, valend);
                                        intLink.UpdateBatchCountPayment(intLink.getRecieptBatch("FGBUSMRC"));
                                        intLink.UpdateReceiptNumber(receipt, intLink.GetCurrentRef("FGBUSMRC"));
                                        intLink.IncrementReferenceNumber(intLink.getBankCodeId("FGBUSMRC"), Convert.ToDecimal(dt.debit));
                                        intLink.storePayment(dt.customerId, companyName, DateTime.Now, invoiceId, Convert.ToDecimal(dt.debit), usamount, prepstat, Convert.ToInt32(reference), Convert.ToInt32(glid), "No", currentRate);
                                    }
                                }
                            }
                            else if (glid == "5148")
                            {
                                LogOperation("NCB JA$ SAVINGS A/C", 1);
                                if (dt.success)
                                {
                                    if (receiptBatchAvail("NCBJMREC"))
                                    {
                                        string reference = intLink.GetCurrentRef("NCBJMREC");
                                        LogOperation("Target Batch: " + intLink.getRecieptBatch("NCBJMREC"), 1);
                                        LogOperation("Transferring Receipt", 1);

                                        ReceiptTransfer(intLink.getRecieptBatch("NCBJMREC"), dt.customerId, dt.debit, dt.companyName, reference, invoiceId, paymentDate, dt.desc, customerId, valstart, valend);
                                        intLink.UpdateBatchCountPayment(intLink.getRecieptBatch("NCBJMREC"));
                                        intLink.UpdateReceiptNumber(receipt, intLink.GetCurrentRef("NCBJMREC"));
                                        intLink.IncrementReferenceNumber(intLink.getBankCodeId("NCBJMREC"), Convert.ToDecimal(dt.debit));
                                        intLink.storePayment(dt.customerId, companyName, DateTime.Now, invoiceId, Convert.ToDecimal(dt.debit), 0, prepstat, Convert.ToInt32(reference), Convert.ToInt32(glid), "No", 1);
                                    }
                                    else
                                    {
                                        string reference = intLink.GetCurrentRef("NCBJMREC");
                                        CreateReceiptBatchEx("NCBJMREC", "Middleware Generated Batch for NCBJMREC");
                                        intLink.openNewReceiptBatch(1, GetLastPaymentBatch(), "NCBJMREC");

                                        LogOperation("Target Batch: " + intLink.getRecieptBatch("NCBJMREC"), 1);
                                        LogOperation("Transferring Receipt", 1);

                                        ReceiptTransfer(intLink.getRecieptBatch("NCBJMREC"), dt.customerId, dt.debit, dt.companyName, reference, invoiceId, paymentDate, dt.desc, customerId, valstart, valend);
                                        intLink.UpdateBatchCountPayment(intLink.getRecieptBatch("NCBJMREC"));
                                        intLink.UpdateReceiptNumber(receipt, intLink.GetCurrentRef("NCBJMREC"));
                                        intLink.IncrementReferenceNumber(intLink.getBankCodeId("NCBJMREC"), Convert.ToDecimal(dt.debit));
                                        intLink.storePayment(dt.customerId, companyName, DateTime.Now, invoiceId, Convert.ToDecimal(dt.debit), 0, prepstat, Convert.ToInt32(reference), Convert.ToInt32(glid), "No", 1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            LogOperation("The customer was not found in ACCPAC", 1);
                            LogOperation("Transaction cancelled.", 1);
                        }
                    }
                    else if (docInfo.DocumentType == CREDIT_MEMO)
                    {
                        LogOperation("New Credit Memo...", 2);
                        CreditNoteInfo creditNote = new CreditNoteInfo();

                        while (creditNote.amount == 0)
                        {
                            creditNote = intLink.getCreditNoteInfo(docInfo.OriginalDocumentID, docInfo.DocumentID);
                            Thread.Sleep(1000);
                        }

                        List<string> clientInfo = new List<string>(4);
                        clientInfo = intLink.getClientInfo_inv(creditNote.CustomerID.ToString());
                        var accountNum = intLink.GetAccountNumber(creditNote.CreditGL);
                        DateTime invoiceValidity = intLink.GetValidity(creditNote.ARInvoiceID);

                        string companyName = clientInfo[0].ToString();
                        string cNum = clientInfo[1].ToString();
                        string fname = clientInfo[2].ToString();
                        string lname = clientInfo[3].ToString();
                        string creditNoteDesc = creditNote.remarks;
                        int cred_docNum = 0;

                        if (creditNoteDesc == "" || creditNoteDesc == null)
                        {
                            creditNoteDesc = companyName + " - Credit Note";
                        }

                        if (companyName == "" || companyName == " " || companyName == null)
                        {
                            companyName = fname + " " + lname;
                        }

                        Data dt = Translate(cNum, creditNote.FeeType, companyName, creditNote.amount.ToString(), creditNote.notes, accountNum, intLink.getFreqUsage(creditNote.ARInvoiceID));

                        if (checkAccpacInvoiceAvail(creditNote.ARInvoiceID))
                        {
                            cred_docNum = intLink.getCreditMemoNumber();
                            LogOperation("Creating credit memo", 1);
                            int batchNumber = getBatch(CREDIT_NOTE, creditNote.ARInvoiceID.ToString());

                            intLink.storeInvoice(creditNote.ARInvoiceID, batchNumber, creditNote.CreditGL, companyName, dt.customerId, DateTime.Now, "", creditNote.amount, "no modification", 1, 0, 0, 1, cred_docNum);
                            creditNoteInsert(batchNumber.ToString(), dt.customerId, accountNum, creditNote.amount.ToString(), creditNote.ARInvoiceID.ToString(), cred_docNum.ToString(), creditNoteDesc);
                            intLink.updateAsmsCreditMemoNumber(docInfo.DocumentID, cred_docNum);
                        }
                        else
                        {
                            LogOperation("The Credit Memo was not created. The Invoice does not exist.", 1);
                            cred_docNum = intLink.getCreditMemoNumber();
                            intLink.updateAsmsCreditMemoNumber(docInfo.DocumentID, cred_docNum);
                            LogOperation("The Credit Memo number in ASMS updated.", 1);
                        }
                    }
                    else if (docInfo.DocumentType == RECEIPT && docInfo.PaymentMethod == 99)
                    {
                        LogOperation("Payment By Credit", 2);

                        PaymentInfo pinfo = intLink.getPaymentInfo(docInfo.OriginalDocumentID);
                        List<string> clientData = intLink.getClientInfo_inv(pinfo.CustomerID.ToString());
                        List<string> feeData = new List<string>(3);

                        var companyName = clientData[0].ToString();
                        var customerId = clientData[1].ToString();
                        var fname = clientData[2].ToString();
                        var lname = clientData[3].ToString();

                        if (companyName == "" || companyName == " " || companyName == null)
                        {
                            companyName = fname + " " + lname;
                        }

                        feeData = intLink.GetFeeInfo(pinfo.InvoiceID);
                        var ftype = feeData[0].ToString();
                        var notes = feeData[1].ToString();

                        Data dt = Translate(customerId, ftype, companyName, pinfo.Debit.ToString(), notes, "", intLink.getFreqUsage(pinfo.InvoiceID).ToString());
                        PrepaymentData pData = intLink.checkPrepaymentAvail(dt.customerId);
                        int invoiceBatch = getIbatchNumber(pinfo.InvoiceID);            //This value represents the batch that the invoice belongs to.
                        int receiptBatch = getRBatchNumber(pData.referenceNumber);      //This value represents the batch that the receipt belongs to.

                        if (pData.dataAvail)
                        {
                            if (checkAccpacIBatchPosted(invoiceBatch) && checkAccpacRBatchPosted(receiptBatch))
                            {
                                var glid = pData.destinationBank.ToString();

                                if (glid == "5146")
                                {
                                    LogOperation("Bank: FGB JA$ CURRENT A/C", 1);

                                    if (pData.totalPrepaymentRemainder >= pinfo.Debit)
                                    {
                                        LogOperation("Carrying out Payment By Credit Transaction", 1);

                                        decimal reducingAmt = pinfo.Debit;

                                        if (receiptBatchAvail("FGBJMREC"))
                                        {
                                            while (reducingAmt > 0)
                                            {
                                                pData = intLink.checkPrepaymentAvail(dt.customerId);
                                                comApiPayByCredit(dt.customerId, pinfo.InvoiceID.ToString(), intLink.getRecieptBatch("FGBJMREC"), getDocNumber(pData.referenceNumber));
                                                if (reducingAmt > pData.remainder) intLink.adjustPrepaymentRemainder(pData.remainder, pData.sequenceNumber);
                                                else intLink.adjustPrepaymentRemainder(reducingAmt, pData.sequenceNumber);
                                                reducingAmt = reducingAmt - pData.remainder;
                                            }
                                            intLink.storePayment(dt.customerId, companyName, DateTime.Now, pinfo.InvoiceID.ToString(), pinfo.Debit, 0, "No", 0, Convert.ToInt32(glid), "Yes", 1);
                                            LogOperation("Payment by credit transaction complete", 1);
                                        }
                                        else
                                        {
                                            CreateReceiptBatchEx("FGBJMREC", "Middleware Generated Batch for FGBJMREC");
                                            intLink.openNewReceiptBatch(1, GetLastPaymentBatch(), "FGBJMREC");
                                            LogOperation("Target Batch: " + intLink.getRecieptBatch("FGBJMREC"), 1);
                                            while (reducingAmt > 0)
                                            {
                                                pData = intLink.checkPrepaymentAvail(dt.customerId);
                                                comApiPayByCredit(dt.customerId, pinfo.InvoiceID.ToString(), intLink.getRecieptBatch("FGBJMREC"), getDocNumber(pData.referenceNumber));
                                                if (reducingAmt > pData.remainder) intLink.adjustPrepaymentRemainder(pData.remainder, pData.sequenceNumber);
                                                else intLink.adjustPrepaymentRemainder(reducingAmt, pData.sequenceNumber);
                                                reducingAmt = reducingAmt - pData.remainder;
                                            }
                                            intLink.storePayment(dt.customerId, companyName, DateTime.Now, pinfo.InvoiceID.ToString(), pinfo.Debit, 0, "No", 0, Convert.ToInt32(glid), "Yes", 1);
                                            LogOperation("Payment by credit transaction complete", 1);
                                        }
                                    }
                                    else LogOperation("Prepayment balance not enough to carry out Transaction", 1);
                                }

                                if (glid == "5147")
                                {
                                    LogOperation("Bank: FGB US$ SAVINGS A/C", 1);

                                    decimal usRate = intLink.GetUsRateByInvoice(pinfo.InvoiceID); //using the US rate from the Invoice at the time it was created.
                                    decimal usAmount = Convert.ToDecimal(dt.debit) / usRate;
                                    if (pData.totalPrepaymentRemainder >= usAmount)
                                    {
                                        LogOperation("Carrying out Payment By Credit Transaction", 1);

                                        decimal reducingAmt = usAmount;

                                        if (receiptBatchAvail("FGBUSMRC"))
                                        {
                                            while (reducingAmt > 0)
                                            {
                                                pData = intLink.checkPrepaymentAvail(dt.customerId);
                                                comApiPayByCredit(dt.customerId, pinfo.InvoiceID.ToString(), intLink.getRecieptBatch("FGBUSMRC"), getDocNumber(pData.referenceNumber));
                                                if (reducingAmt > pData.remainder) intLink.adjustPrepaymentRemainder(pData.remainder, pData.sequenceNumber);
                                                else intLink.adjustPrepaymentRemainder(reducingAmt, pData.sequenceNumber);
                                                reducingAmt = reducingAmt - pData.remainder;
                                            }
                                            if (usRate == 1) intLink.storePayment(dt.customerId, companyName, DateTime.Now, pinfo.InvoiceID.ToString(), pinfo.Debit, 0, "No", 0, Convert.ToInt32(glid), "Yes", 1);
                                            else intLink.storePayment(dt.customerId, companyName, DateTime.Now, pinfo.InvoiceID.ToString(), pinfo.Debit, usAmount, "No", 0, Convert.ToInt32(glid), "Yes", 1);
                                            LogOperation("Payment by credit transaction complete", 1);
                                        }
                                        else
                                        {
                                            CreateReceiptBatchEx("FGBUSMRC", "Middleware Generated Batch for FGBUSMRC");
                                            intLink.openNewReceiptBatch(1, GetLastPaymentBatch(), "FGBUSMRC");
                                            LogOperation("Target Batch: " + intLink.getRecieptBatch("FGBUSMRC"), 1);
                                            while (reducingAmt > 0)
                                            {
                                                pData = intLink.checkPrepaymentAvail(dt.customerId);
                                                comApiPayByCredit(dt.customerId, pinfo.InvoiceID.ToString(), intLink.getRecieptBatch("FGBUSMRC"), getDocNumber(pData.referenceNumber));
                                                if (reducingAmt > pData.remainder) intLink.adjustPrepaymentRemainder(pData.remainder, pData.sequenceNumber);
                                                else intLink.adjustPrepaymentRemainder(reducingAmt, pData.sequenceNumber);
                                                reducingAmt = reducingAmt - pData.remainder;
                                            }
                                            if (usRate == 1) intLink.storePayment(dt.customerId, companyName, DateTime.Now, pinfo.InvoiceID.ToString(), pinfo.Debit, 0, "No", 0, Convert.ToInt32(glid), "Yes", 1);
                                            else intLink.storePayment(dt.customerId, companyName, DateTime.Now, pinfo.InvoiceID.ToString(), pinfo.Debit, usAmount, "No", 0, Convert.ToInt32(glid), "Yes", 1);
                                            LogOperation("Payment by credit transaction complete", 1);
                                        }
                                    }
                                    else LogOperation("Prepayment balance not enough to carry out Transaction", 1);
                                }

                                if (glid == "5148")
                                {
                                    LogOperation("Bank: NCB JA$ SAVINGS A/C", 1);

                                    if (pData.totalPrepaymentRemainder >= pinfo.Debit)
                                    {
                                        LogOperation("Carrying out Payment By Credit Transaction", 1);

                                        decimal reducingAmt = pinfo.Debit;

                                        if (receiptBatchAvail("NCBJMREC"))
                                        {
                                            while (reducingAmt > 0)
                                            {
                                                pData = intLink.checkPrepaymentAvail(dt.customerId);
                                                comApiPayByCredit(dt.customerId, pinfo.InvoiceID.ToString(), intLink.getRecieptBatch("NCBJMREC"), getDocNumber(pData.referenceNumber));
                                                if (reducingAmt > pData.remainder) intLink.adjustPrepaymentRemainder(pData.remainder, pData.sequenceNumber);
                                                else intLink.adjustPrepaymentRemainder(reducingAmt, pData.sequenceNumber);
                                                reducingAmt = reducingAmt - pData.remainder;
                                            }
                                            intLink.storePayment(dt.customerId, companyName, DateTime.Now, pinfo.InvoiceID.ToString(), pinfo.Debit, 0, "No", 0, Convert.ToInt32(glid), "Yes", 1);
                                            LogOperation("Payment by credit transaction complete", 1);
                                        }
                                        else
                                        {
                                            CreateReceiptBatchEx("NCBJMREC", "Middleware Generated Batch for NCBJMREC");
                                            intLink.openNewReceiptBatch(1, GetLastPaymentBatch(), "NCBJMREC");
                                            LogOperation("Target Batch: " + intLink.getRecieptBatch("NCBJMREC"), 1);
                                            while (reducingAmt > 0)
                                            {
                                                pData = intLink.checkPrepaymentAvail(dt.customerId);
                                                comApiPayByCredit(dt.customerId, pinfo.InvoiceID.ToString(), intLink.getRecieptBatch("NCBJMREC"), getDocNumber(pData.referenceNumber));
                                                if (reducingAmt > pData.remainder) intLink.adjustPrepaymentRemainder(pData.remainder, pData.sequenceNumber);
                                                else intLink.adjustPrepaymentRemainder(reducingAmt, pData.sequenceNumber);
                                                reducingAmt = reducingAmt - pData.remainder;
                                            }
                                            intLink.storePayment(dt.customerId, companyName, DateTime.Now, pinfo.InvoiceID.ToString(), pinfo.Debit, 0, "No", 0, Convert.ToInt32(glid), "Yes", 1);
                                            LogOperation("Payment by credit transaction complete", 1);
                                        }
                                    }
                                    else LogOperation("Prepayment balance not enough to carry out Transaction", 1);
                                }

                                if (glid != "5146" && glid != "5147" && glid != "5148") LogOperation("Bank Selected Not found, Cannot complete Transaction", 1);
                            }
                            else
                            {
                                LogOperation("Both invoice and receipt must be posted before attempting this transaction", 1);
                            }
                        }
                        else
                        {
                            LogOperation("No prepayment record found for customer: " + dt.customerId, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string e_path = mAccpacSession.Errors.GenerateErrorFile().Replace("\\", "");
                LogOperation("Error file: " + e_path, 1);
                AccpacErrors errors = mAccpacSession.Errors;
            }
        }

        private void TableDependPay_OnChanged(object sender, RecordChangedEventArgs<SqlNotify_Pay> e)

        {
            var entity = e.Entity;
            if (e.ChangeType == ChangeType.Insert)
            {

            }
        }

        private void TableDependCancellation_OnChanged(object sender, RecordChangedEventArgs<SqlNotifyCancellation> e)
        {
            Thread.Sleep(2000);

            var values = e.Entity;
            var customerId = values.CustomerID;
            var invoiceId = values.ARInvoiceID;
            var amount = values.Amount;
            var feeType = values.FeeType;
            var notes = values.notes;
            var cancelledBy = values.canceledBy;


            if (cancelledBy != null)
            {
                string freqUsage = intLink.getFreqUsage(invoiceId);
                DateTime invoiceValidity = intLink.GetValidity(invoiceId);
                var creditGl = intLink.GetCreditGl(invoiceId.ToString());
                var accountNum = intLink.GetAccountNumber(creditGl);
                List<string> clientInfo = new List<string>(4);
                clientInfo = intLink.getClientInfo_inv(customerId.ToString());
                intLink.getClientInfo_inv(customerId.ToString());

                string companyName = clientInfo[0].ToString();
                string cNum = clientInfo[1].ToString();
                string fname = clientInfo[2].ToString();
                string lname = clientInfo[3].ToString();


                if (companyName == "" || companyName == " " || companyName == null)
                {
                    companyName = fname + " " + lname;
                }

                string creditNoteDesc = companyName + " - Credit Note";
                Data dt = Translate(cNum, feeType, companyName, "", notes, accountNum, freqUsage);

                if (values.isVoided == 1 && e.ChangeType == ChangeType.Update)
                {
                    LogOperation("Cancellation found", 2);

                    if (checkAccpacInvoiceAvail(invoiceId))
                    {
                        int postedBatch = getIbatchNumber(invoiceId);
                        if (!invoiceDelete(invoiceId))
                        {
                            int cred_docNum = intLink.getCreditMemoNumber();
                            LogOperation("Creating a credit memo", 1);
                            int batchNumber = getBatch(CREDIT_NOTE, invoiceId.ToString());

                            intLink.storeInvoice(invoiceId, batchNumber, creditGl, companyName, dt.customerId, invoiceValidity, cancelledBy, amount, "no modification", 1, 0, 0, 1, cred_docNum);
                            creditNoteInsert(batchNumber.ToString(), dt.customerId, accountNum, amount.ToString(), invoiceId.ToString(), cred_docNum.ToString(), creditNoteDesc);
                        }
                        else
                        {
                            Maj m = new Maj();
                            int r = intLink.getInvoiceReference(invoiceId);

                            if (r != -1)
                            {
                                m = intLink.getMajDetail(r);
                            }

                            if (dt.feeType == "SLF" && notes == "Renewal")
                            {
                                intLink.storeInvoice(Convert.ToInt32(invoiceId), postedBatch, creditGl, companyName, dt.customerId, DateTime.Now, cancelledBy, Convert.ToDecimal(amount), "no modification", 1, 0, 1, 0, 0);
                            }
                            else if (dt.feeType == "RF" && notes == "Renewal")
                            {
                                intLink.storeInvoice(Convert.ToInt32(invoiceId), postedBatch, creditGl, companyName, dt.customerId, DateTime.Now, cancelledBy, Convert.ToDecimal(amount), "no modification", 1, 0, 1, 0, 0);
                            }
                            else if ((notes == "Annual Fee" && m.stationType == "SSL" && m.certificateType == 0 && m.proj == "JMC") || (freqUsage == "PRS55"))
                            {
                                intLink.storeInvoice(Convert.ToInt32(invoiceId), postedBatch, creditGl, companyName, dt.customerId, DateTime.Now, cancelledBy, Convert.ToDecimal(amount), "no modification", 1, 0, 1, 0, 0);
                            }
                            else if (notes == "Type Approval" || freqUsage == "TA-ProAmend")
                            {
                                intLink.storeInvoice(Convert.ToInt32(invoiceId), postedBatch, creditGl, companyName, dt.customerId, DateTime.Now, cancelledBy, Convert.ToDecimal(amount), "no modification", intLink.GetRate(), changetous(Convert.ToDecimal(amount)), 1, 0, 0);
                            }
                            else if (notes == "Annual Fee" || notes == "Modification" || notes == "Radio Operator")
                            {
                                intLink.storeInvoice(Convert.ToInt32(invoiceId), postedBatch, creditGl, companyName, dt.customerId, DateTime.Now, cancelledBy, Convert.ToDecimal(amount), "no modification", 1, 0, 1, 0, 0);
                            }
                        }
                    }
                    else
                    {
                        LogOperation("Invoice: " + invoiceId.ToString() + " was not found in Sage. Cannot delete.", 2);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Code = 2;

            LogOperation("Stopping Service ", 2);

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnStart.Text = "Start Service";
            mAccpacSession.Close();
            LogOperation("Service Stopped", 1);
            monitorRunning = false;
            tableDependInfo.Stop();
        }

        bool getInvoiceNumber(string DocNum)
        {
            mAccpacDBLink.OpenView("AR0031", out b1_arInvoiceBatch);
            mAccpacDBLink.OpenView("AR0032", out b1_arInvoiceHeader);
            mAccpacDBLink.OpenView("AR0033", out b1_arInvoiceDetail);
            mAccpacDBLink.OpenView("AR0034", out b1_arInvoicePaymentSchedules);
            mAccpacDBLink.OpenView("AR0402", out b1_arInvoiceHeaderOptFields);
            mAccpacDBLink.OpenView("AR0401", out b1_arInvoiceDetailOptFields);

            bool truth;
            string doc = "";

            string mydocpath =
               Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            StreamWriter output = new StreamWriter(mydocpath + @"\" + "outputInv" + ".txt");
            truth = b1_arInvoiceHeader.GoTop();

            while (truth)
            {
                doc = Convert.ToString(b1_arInvoiceHeader.Fields.FieldByName["IDINVC"].get_Value());
                output.WriteLine(doc);
                truth = b1_arInvoiceHeader.GoNext();
            }
            return truth;
        }

        bool CustomerExists(string idCust)
        {
            bool exist = false;

            AccpacView cssql = null;
            mAccpacDBLink.OpenView("CS0120", out cssql);

            cssql.Browse("SELECT IDCUST FROM ARCUS WHERE IDCUST = '" + idCust + "'", true);
            cssql.InternalSet(256);

            if (cssql.GoNext())
            {
                exist = true;
            }

            return exist;
        }

        void UpdateInvoice(string accountNumber, string amt, string BatchId, string entryNumber)
        {
            mAccpacDBLink.OpenView("AR0031", out b1_arInvoiceBatch);
            mAccpacDBLink.OpenView("AR0032", out b1_arInvoiceHeader);
            mAccpacDBLink.OpenView("AR0033", out b1_arInvoiceDetail);
            mAccpacDBLink.OpenView("AR0034", out b1_arInvoicePaymentSchedules);
            mAccpacDBLink.OpenView("AR0402", out b1_arInvoiceHeaderOptFields);
            mAccpacDBLink.OpenView("AR0401", out b1_arInvoiceDetailOptFields);

            b1_arInvoiceBatch.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceHeader.Compose(new AccpacView[] { b1_arInvoiceBatch, b1_arInvoiceDetail, b1_arInvoicePaymentSchedules, b1_arInvoiceHeaderOptFields });
            b1_arInvoiceDetail.Compose(new AccpacView[] { b1_arInvoiceHeader, b1_arInvoiceBatch, b1_arInvoiceDetailOptFields });
            b1_arInvoicePaymentSchedules.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceHeaderOptFields.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceDetailOptFields.Compose(new AccpacView[] { b1_arInvoiceDetail });

            b1_arInvoiceBatch.Fields.FieldByName["CNTBTCH"].set_Value(BatchId);
            b1_arInvoiceBatch.Browse("((BTCHSTTS = 1) OR (BTCHSTTS = 7))", false);
            b1_arInvoiceBatch.Fetch();

            b1_arInvoiceBatch.Process();
            b1_arInvoiceHeader.Fields.FieldByName["CNTITEM"].set_Value(entryNumber);
            b1_arInvoiceHeader.Browse("", true);

            b1_arInvoiceHeader.Fetch();
            b1_arInvoiceDetail.Read();
            b1_arInvoiceDetail.Read();
            b1_arInvoiceDetail.Fields.FieldByName["CNTLINE"].set_Value("20");

            b1_arInvoiceDetail.Read();
            b1_arInvoiceDetail.Fields.FieldByName["IDACCTREV"].set_Value(accountNumber);
            b1_arInvoiceDetail.Fields.FieldByName["AMTEXTN"].set_Value(amt);
            b1_arInvoiceDetail.Update();
            b1_arInvoiceDetail.Fields.FieldByName["CNTLINE"].set_Value("20");

            b1_arInvoiceDetail.Read();
            b1_arInvoiceHeader.Update();
            b1_arInvoiceDetail.Read();
            b1_arInvoiceDetail.Read();
        }

        InsertionReturn InvBatchInsert(string idCust, string docNum, string desc, string feeCode, string amt, string batchId)
        {
            InsertionReturn success = new InsertionReturn();
            LogOperation("Transferring Invoice", 1);
            DateTime postDate = intLink.GetValidity(Convert.ToInt32(docNum));

            if (postDate < DateTime.Now) postDate = DateTime.Now;

            DateTime docDate = intLink.getDocDate(Convert.ToInt32(docNum));
            DateTime now = DateTime.Now;
            bool gotOne;

            mAccpacDBLink.OpenView("AR0031", out b1_arInvoiceBatch);
            mAccpacDBLink.OpenView("AR0032", out b1_arInvoiceHeader);
            mAccpacDBLink.OpenView("AR0033", out b1_arInvoiceDetail);
            mAccpacDBLink.OpenView("AR0034", out b1_arInvoicePaymentSchedules);
            mAccpacDBLink.OpenView("AR0402", out b1_arInvoiceHeaderOptFields);
            mAccpacDBLink.OpenView("AR0401", out b1_arInvoiceDetailOptFields);

            b1_arInvoiceBatch.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceHeader.Compose(new AccpacView[] { b1_arInvoiceBatch, b1_arInvoiceDetail, b1_arInvoicePaymentSchedules, b1_arInvoiceHeaderOptFields });
            b1_arInvoiceDetail.Compose(new AccpacView[] { b1_arInvoiceHeader, b1_arInvoiceBatch, b1_arInvoiceDetailOptFields });
            b1_arInvoicePaymentSchedules.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceHeaderOptFields.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceDetailOptFields.Compose(new AccpacView[] { b1_arInvoiceDetail });

            gotOne = CustomerExists(idCust);

            try
            {
                if (gotOne)
                {
                    b1_arInvoiceBatch.Process();
                    b1_arInvoiceBatch.Fields.FieldByName["CNTBTCH"].set_Value(batchId);
                    b1_arInvoiceBatch.Read();
                    b1_arInvoiceHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
                    b1_arInvoiceDetail.Cancel();
                    b1_arInvoiceHeader.Fields.FieldByName["DATEBUS"].set_Value(postDate.ToString());
                    b1_arInvoiceHeader.Fields.FieldByName["IDCUST"].set_Value(idCust);
                    LogOperation("Target Batch: " + batchId, 1);

                    b1_arInvoiceHeader.Process();
                    b1_arInvoiceHeader.Fields.FieldByName["IDINVC"].set_Value(docNum);

                    var temp = b1_arInvoiceDetail.Exists;
                    b1_arInvoiceDetail.RecordClear();
                    temp = b1_arInvoiceDetail.Exists;
                    b1_arInvoiceDetail.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_NOINSERT);

                    b1_arInvoiceDetail.Process();
                    b1_arInvoiceDetail.Fields.FieldByName["TEXTDESC"].set_Value(desc);
                    b1_arInvoiceDetail.Fields.FieldByName["IDACCTREV"].set_Value(feeCode);
                    b1_arInvoiceDetail.Fields.FieldByName["AMTEXTN"].set_Value(amt);
                    b1_arInvoiceDetail.Insert();

                    b1_arInvoiceDetail.Read();
                    b1_arInvoiceHeader.Insert();
                    b1_arInvoiceDetail.Read();
                    b1_arInvoiceDetail.Read();
                    b1_arInvoiceBatch.Read();
                    b1_arInvoiceHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
                    b1_arInvoiceDetail.Cancel();

                    UpdateList(idCust, "Transferred", "Invoice");
                    LogOperation("Invoice Transferred", 1);

                    b1_arInvoiceBatch.Close();
                    b1_arInvoiceDetail.Close();
                    b1_arInvoiceDetailOptFields.Close();
                    b1_arInvoiceHeader.Close();
                    b1_arInvoiceHeaderOptFields.Close();
                    b1_arInvoicePaymentSchedules.Close();
                }

                else
                {
                    success.status = "Not Exist";
                    Ignore(idCust, "Ignored", "Invoice");
                }
                return success;
            }
            catch (Exception ex)
            {

                var a = b1_arInvoiceBatch.Session.Errors;
                var b = b1_arInvoiceHeader.Session.Errors;
                var c = b1_arInvoiceDetail.Session.Errors;
                var d = b1_arInvoicePaymentSchedules.Session.Errors;
                var e = b1_arInvoiceHeaderOptFields.Session.Errors;
                var f = b1_arInvoiceDetailOptFields.Session.Errors;
                var s = mAccpacSession.Errors.Count;
                return null;

            }
        }
        void CreateInvoiceBatchSet()
        {
            mAccpacDBLink.OpenView("AR0031", out b1_arInvoiceBatch);
            mAccpacDBLink.OpenView("AR0032", out b1_arInvoiceHeader);
            mAccpacDBLink.OpenView("AR0033", out b1_arInvoiceDetail);
            mAccpacDBLink.OpenView("AR0034", out b1_arInvoicePaymentSchedules);
            mAccpacDBLink.OpenView("AR0402", out b1_arInvoiceHeaderOptFields);
            mAccpacDBLink.OpenView("AR0401", out b1_arInvoiceDetailOptFields);

            b1_arInvoiceBatch.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceHeader.Compose(new AccpacView[] { b1_arInvoiceBatch, b1_arInvoiceDetail, b1_arInvoicePaymentSchedules, b1_arInvoiceHeaderOptFields });
            b1_arInvoiceDetail.Compose(new AccpacView[] { b1_arInvoiceHeader, b1_arInvoiceBatch, b1_arInvoiceDetailOptFields });
            b1_arInvoicePaymentSchedules.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceHeaderOptFields.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceDetailOptFields.Compose(new AccpacView[] { b1_arInvoiceDetail });

            b1_arInvoiceBatch.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_INSERT);
            b1_arInvoiceBatch.Read();
            b1_arInvoiceHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
            b1_arInvoiceDetail.Cancel();

            b1_arInvoiceBatch.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_INSERT);
            b1_arInvoiceBatch.Read();
            b1_arInvoiceHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
            b1_arInvoiceDetail.Cancel();

            b1_arInvoiceBatch.Close();
            b1_arInvoiceDetail.Close();
            b1_arInvoiceDetailOptFields.Close();
            b1_arInvoiceHeader.Close();
            b1_arInvoiceHeaderOptFields.Close();
            b1_arInvoicePaymentSchedules.Close();
        }

        void CreateInvoiceBatch(string description)
        {
            mAccpacDBLink.OpenView("AR0031", out b1_arInvoiceBatch);
            mAccpacDBLink.OpenView("AR0032", out b1_arInvoiceHeader);
            mAccpacDBLink.OpenView("AR0033", out b1_arInvoiceDetail);
            mAccpacDBLink.OpenView("AR0034", out b1_arInvoicePaymentSchedules);
            mAccpacDBLink.OpenView("AR0402", out b1_arInvoiceHeaderOptFields);
            mAccpacDBLink.OpenView("AR0401", out b1_arInvoiceDetailOptFields);

            b1_arInvoiceBatch.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceHeader.Compose(new AccpacView[] { b1_arInvoiceBatch, b1_arInvoiceDetail, b1_arInvoicePaymentSchedules, b1_arInvoiceHeaderOptFields });
            b1_arInvoiceDetail.Compose(new AccpacView[] { b1_arInvoiceHeader, b1_arInvoiceBatch, b1_arInvoiceDetailOptFields });
            b1_arInvoicePaymentSchedules.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceHeaderOptFields.Compose(new AccpacView[] { b1_arInvoiceHeader });
            b1_arInvoiceDetailOptFields.Compose(new AccpacView[] { b1_arInvoiceDetail });

            b1_arInvoiceBatch.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_INSERT);
            b1_arInvoiceBatch.Read();

            b1_arInvoiceHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
            b1_arInvoiceDetail.Cancel();
            b1_arInvoiceBatch.Fields.FieldByName["BTCHDESC"].set_Value(description);
            b1_arInvoiceBatch.Fields.FieldByName["DATEBTCH"].set_Value(DateTime.Now.Date.ToString());
            b1_arInvoiceBatch.Update();

            b1_arInvoiceBatch.Close();
            b1_arInvoiceDetail.Close();
            b1_arInvoiceDetailOptFields.Close();
            b1_arInvoiceHeader.Close();
            b1_arInvoiceHeaderOptFields.Close();
            b1_arInvoicePaymentSchedules.Close();
        }


        void BroadcastStatus(int code)
        {
            try
            {
                JavaScriptSerializer serialize = new JavaScriptSerializer();
                var stat = new { status = code };
                var json = serialize.Serialize(stat);
                var client = new WebClient();
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.UploadString("http://localhost:8080/IntegrationService.asmx/SetMonStat", "POST", json);
            }
            catch (Exception ex)
            {
                LogOperation(ex.InnerException.Message, 2);
                StatusUpdate.Stop();
            }
        }

        void GenRptRequest(string ReportType)
        {
            try
            {
                int m = 0; 
                int y = 0; 

                if (ReportType == "Monthly")
                {
                    m = DateTime.Now.Month - 1;
                    y = DateTime.Now.Year;

                    if (m == 0)
                    {
                        m = 12;
                        y = y - 1;
                    }
                }
                
                if (ReportType == "Annual")
                {
                    m = 4;
                    y = DateTime.Now.Year - 1;
                }
                
                JavaScriptSerializer serialize = new JavaScriptSerializer();
                var param = new { ReportType = ReportType, month = m, year = y };
                var json = serialize.Serialize(param);
                var client = new WebClient();
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string id = client.UploadString("http://localhost:8080/IntegrationService.asmx/Generate_SaveDeferredRpt", "POST", json);
            }
            catch (Exception ex)
            {
                LogOperation(ex.InnerException.Message, 2);
                StatusUpdate.Stop();
            }
        }

        void SetStatus(int code)
        {
            Code = code;
        }

        int GetLastPaymentBatch()
        {
            int BatchId = 0;
            bool gotIt;
            mAccpacDBLink.OpenView("AR0041", out CBBTCH1batch);
            gotIt = CBBTCH1batch.GoBottom();

            if (gotIt)
            {
                BatchId = Convert.ToInt32(CBBTCH1batch.Fields.FieldByName["CNTBTCH"].get_Value());
            }
            return BatchId;
        }

        int GetLastInvoiceBatch()
        {
            int BatchId = 0;
            mAccpacDBLink.OpenView("AR0031", out b1_arInvoiceBatch);
            b1_arInvoiceBatch.GoBottom();
            BatchId = Convert.ToInt32(b1_arInvoiceBatch.Fields.FieldByName["CNTBTCH"].get_Value());

            b1_arInvoiceBatch.Close();
            return BatchId;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(this, "Are you sure you want to quit?", "Message", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Code = 2;
                StatusUpdate.Stop();

                if (tableDependInfo.Status.ToString() != "StoppedDueToCancellation" && (tableDependInfo.Status.ToString() != "WaitingForStart"))
                {
                    try
                    {
                        LogOperation("Stopping Service: ", 2);
                        mAccpacSession.Close();
                        tableDependInfo.Stop();
                        tableDependCancellation.Stop();
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                    }
                }


                if (tableDependPay != null)
                {
                    try
                    {
                        mAccpacSession.Close();
                        tableDependPay.Stop();
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                    }
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime nowTime = DateTime.Now;
            int day = nowTime.Day;
            int month = nowTime.Month;
            int year = nowTime.Year;

            DateTime openTime = new DateTime(year, month, day, 8, 45, 0);
            DateTime closeTime = new DateTime(year, month, day, 18, 30, 0);

            if (DateTime.Now > closeTime)
            {
                if (!closed)
                {
                    tableDependPay.Stop();
                    tableDependCancellation.Stop();
                    closed = true;
                    btnStart.Enabled = false;
                    btnStop.Enabled = false;
                    btnStart.Text = "Suspended";
                    UpdateList(" ", "Suspended", " ");
                }
            }

            else if (DateTime.Now > openTime)
            {
                if (closed)
                {
                    tableDependPay.Start();
                    btnStart.Enabled = false;
                    btnStart.Text = "Running";
                    btnStop.Enabled = true;
                    UpdateList(" ", "Restarted", " ");
                    closed = false;
                }
            }
        }

        public bool invoiceDelete(int invoiceId)
        {
            int entryNumber = -1;
            int batchNumber = -1;

            entryNumber = getEntryNumber(invoiceId);
            batchNumber = getIbatchNumber(invoiceId);


            if (!checkAccpacIBatchPosted(batchNumber))
            {
                mAccpacDBLink.OpenView("AR0031", out b1_arInvoiceBatch);
                mAccpacDBLink.OpenView("AR0032", out b1_arInvoiceHeader);
                mAccpacDBLink.OpenView("AR0033", out b1_arInvoiceDetail);
                mAccpacDBLink.OpenView("AR0034", out b1_arInvoicePaymentSchedules);
                mAccpacDBLink.OpenView("AR0402", out b1_arInvoiceHeaderOptFields);
                mAccpacDBLink.OpenView("AR0401", out b1_arInvoiceDetailOptFields);

                b1_arInvoiceBatch.Compose(new AccpacView[] { b1_arInvoiceHeader });
                b1_arInvoiceHeader.Compose(new AccpacView[] { b1_arInvoiceBatch, b1_arInvoiceDetail, b1_arInvoicePaymentSchedules, null });
                b1_arInvoiceDetail.Compose(new AccpacView[] { b1_arInvoiceHeader, b1_arInvoiceBatch, b1_arInvoiceDetailOptFields });
                b1_arInvoicePaymentSchedules.Compose(new AccpacView[] { b1_arInvoiceHeader });
                b1_arInvoiceHeaderOptFields.Compose(new AccpacView[] { b1_arInvoiceHeader });
                b1_arInvoiceDetailOptFields.Compose(new AccpacView[] { b1_arInvoiceDetail });

                b1_arInvoiceBatch.Fields.FieldByName["CNTBTCH"].set_Value(batchNumber.ToString());

                string searchFilter = "CNTITEM = " + entryNumber;
                b1_arInvoiceHeader.Browse(searchFilter, true);

                b1_arInvoiceHeader.Fetch();
                b1_arInvoiceHeader.Delete();

                LogOperation("Invoice: " + invoiceId.ToString() + " was deleted from the batch (" + batchNumber + ")", 1);
                return true;
            }
            else
            {
                LogOperation("The batch is already posted, cannot delete invoice", 2);
                return false;
            }
        }

        public void creditNoteInsert(string batchNumber, string customerId, string acctNumber, string amount, string invoiceToApply, string docNumber, string description)
        {
            try
            {
                mAccpacDBLink.OpenView("AR0031", out b1_arInvoiceBatch);
                mAccpacDBLink.OpenView("AR0032", out b1_arInvoiceHeader);
                mAccpacDBLink.OpenView("AR0033", out b1_arInvoiceDetail);
                mAccpacDBLink.OpenView("AR0034", out b1_arInvoicePaymentSchedules);
                mAccpacDBLink.OpenView("AR0402", out b1_arInvoiceHeaderOptFields);
                mAccpacDBLink.OpenView("AR0401", out b1_arInvoiceDetailOptFields);

                b1_arInvoiceBatch.Compose(new AccpacView[] { b1_arInvoiceHeader });
                b1_arInvoiceHeader.Compose(new AccpacView[] { b1_arInvoiceBatch, b1_arInvoiceDetail, b1_arInvoicePaymentSchedules, b1_arInvoiceHeaderOptFields });
                b1_arInvoiceDetail.Compose(new AccpacView[] { b1_arInvoiceHeader, b1_arInvoiceBatch, b1_arInvoiceDetailOptFields });
                b1_arInvoicePaymentSchedules.Compose(new AccpacView[] { b1_arInvoiceHeader });
                b1_arInvoiceHeaderOptFields.Compose(new AccpacView[] { b1_arInvoiceHeader });
                b1_arInvoiceDetailOptFields.Compose(new AccpacView[] { b1_arInvoiceDetail });

                b1_arInvoiceBatch.Fields.FieldByName["CNTBTCH"].set_Value(batchNumber);
                b1_arInvoiceHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
                b1_arInvoiceHeader.Fields.FieldByName["IDCUST"].set_Value(customerId);
                b1_arInvoiceHeader.Fields.FieldByName["TEXTTRX"].set_Value("3");
                b1_arInvoiceDetail.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_NOINSERT);

                b1_arInvoiceDetail.Fields.FieldByName["IDACCTREV"].set_Value(acctNumber);
                b1_arInvoiceDetail.Fields.FieldByName["AMTEXTN"].set_Value(amount);
                b1_arInvoiceDetail.Fields.FieldByName["TEXTDESC"].set_Value(description);
                b1_arInvoiceDetail.Insert();

                b1_arInvoiceHeader.Fields.FieldByName["INVCAPPLTO"].set_Value(invoiceToApply);
                b1_arInvoiceHeader.Fields.FieldByName["IDINVC"].set_Value(docNumber);
                b1_arInvoiceHeader.Insert();
                b1_arInvoiceHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);

                UpdateList(customerId, "Transferred", "Credit Note");
                LogOperation("Credit Memo transferred", 1);
            }
            catch (Exception ex)
            {
                string location = mAccpacSession.Errors.GenerateErrorFile();
                LogOperation("Error file: " + location, 1);
            }
        }

        public void comApiPayByCredit(string customerId, string invoiceId, string batchNumber, string documentNumber)
        {
            mAccpacDBLink.OpenView("AR0041", out arRecptBatch);
            mAccpacDBLink.OpenView("AR0042", out arRecptHeader);
            mAccpacDBLink.OpenView("AR0044", out arRecptDetail1);
            mAccpacDBLink.OpenView("AR0045", out arRecptDetail2);
            mAccpacDBLink.OpenView("AR0043", out arRecptDetail3);
            mAccpacDBLink.OpenView("AR0061", out arRecptDetail4);
            mAccpacDBLink.OpenView("AR0406", out arRecptDetail5);
            mAccpacDBLink.OpenView("AR0170", out arRecptDetail6);

            arRecptBatch.Compose(new AccpacView[] { arRecptHeader });
            arRecptHeader.Compose(new AccpacView[] { arRecptBatch, arRecptDetail3, arRecptDetail1, arRecptDetail5, arRecptDetail6 });
            arRecptDetail1.Compose(new AccpacView[] { arRecptHeader, arRecptDetail2, arRecptDetail4 });
            arRecptDetail2.Compose(new AccpacView[] { arRecptDetail1 });
            arRecptDetail3.Compose(new AccpacView[] { arRecptHeader });
            arRecptDetail4.Compose(new AccpacView[] { arRecptBatch, arRecptHeader, arRecptDetail3, arRecptDetail1, arRecptDetail2 });
            arRecptDetail5.Compose(new AccpacView[] { arRecptHeader });
            arRecptDetail6.Compose(new AccpacView[] { arRecptHeader });

            arRecptBatch.RecordClear();
            arRecptBatch.Fields.FieldByName["CODEPYMTYP"].set_Value("CA");
            arRecptHeader.Fields.FieldByName["CODEPYMTYP"].set_Value("CA");
            arRecptDetail3.Fields.FieldByName["CODEPAYM"].set_Value("CA");
            arRecptDetail1.Fields.FieldByName["CODEPAYM"].set_Value("CA");
            arRecptDetail2.Fields.FieldByName["CODEPAYM"].set_Value("CA");
            arRecptDetail4.Fields.FieldByName["PAYMTYPE"].set_Value("CA");
            arRecptBatch.Fields.FieldByName["CNTBTCH"].set_Value(batchNumber);
            arRecptBatch.Read();

            arRecptDetail4.Cancel();
            arRecptDetail4.Fields.FieldByName["PAYMTYPE"].set_Value("CA");
            arRecptDetail4.Fields.FieldByName["CNTBTCH"].set_Value(batchNumber);
            arRecptDetail4.Fields.FieldByName["CNTITEM"].set_Value("1");
            arRecptDetail4.Fields.FieldByName["IDCUST"].set_Value(customerId);
            arRecptDetail4.Fields.FieldByName["AMTRMIT"].set_Value("0.000");
            arRecptDetail4.Fields.FieldByName["STDOCDTE"].set_Value(DateTime.Now.ToShortDateString());


            arRecptHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);
            arRecptHeader.Fields.FieldByName["RMITTYPE"].set_Value("4");
            arRecptHeader.Fields.FieldByName["IDCUST"].set_Value(customerId);

            arRecptDetail4.Cancel();
            arRecptHeader.Fields.FieldByName["DOCNBR"].set_Value(documentNumber);
            arRecptDetail4.Fields.FieldByName["STDOCSTR"].set_Value(invoiceId);

            arRecptDetail4.Fields.FieldByName["PAYMTYPE"].set_Value("CA");
            arRecptDetail4.Fields.FieldByName["CNTBTCH"].set_Value(batchNumber);
            arRecptDetail4.Fields.FieldByName["CNTITEM"].set_Value("0");
            arRecptDetail4.Fields.FieldByName["IDCUST"].set_Value(customerId);
            arRecptDetail4.Fields.FieldByName["AMTRMIT"].set_Value("0.000");

            arRecptDetail4.Process();

            arRecptDetail4.Fields.FieldByName["CNTITEM"].set_Value("0");
            arRecptDetail4.Fields.FieldByName["CNTKEY"].set_Value("-1");
            arRecptDetail4.Read();

            arRecptDetail4.Fields.FieldByName["APPLY"].set_Value("Y");
            arRecptDetail4.Update();

            arRecptDetail4.Read();
            arRecptHeader.Insert();

            arRecptBatch.Read();
            arRecptHeader.RecordCreate(tagViewRecordCreateEnum.VIEW_RECORD_CREATE_DELAYKEY);

            UpdateList(customerId, "Transferred", "Payment By Credit");
        }

        public void MessageHandler()
        {
            try
            {
                List<Queue> msg = intLink.ReadMessageQueue();
                if (msg.Count > 0)
                {
                    for (int i = 0; i < msg.Count; i++)
                    {
                        switch (msg[i].msg)
                        {
                            case "0x68":
                                Hide();
                                hidden = true;

                                if (hidden)
                                {
                                    if (Code == 2)
                                    {
                                        Code = 21;
                                    }
                                    else if (Code == 3)
                                    {
                                        Code = 31;
                                    }
                                }
                                break;

                            case "0x65":
                                Show();
                                hidden = false;

                                if (!hidden)
                                {
                                    if (Code == 21)
                                    {
                                        Code = 2;
                                    }
                                    else if (Code == 31)
                                    {
                                        Code = 3;
                                    }
                                }
                                break;

                            case "0x69":
                                if (hidden)
                                {
                                    Code = 21;
                                }
                                else
                                {
                                    Code = 2;
                                }


                                if (monitorRunning)
                                {
                                    LogOperation("Stopping Service ", 2);

                                    tableDependPay.Stop();
                                    tableDependCancellation.Stop();

                                    btnStart.Enabled = true;
                                    btnStop.Enabled = false;
                                    btnStart.Text = "Start Service";
                                    mAccpacSession.Close();
                                    LogOperation("Service Stopped", 1);
                                    monitorRunning = false;
                                }
                                break;

                            case "0x63":
                                if (!monitorRunning)
                                {
                                    LogOperation("Initialize Session", 2);

                                    mAccpacSession.Init("", "XY", "XY1000", "62A");
                                    mAccpacSession.Open("ADMIN", "SPECTRUM9", SAGE_COMPANY, DateTime.Today, 0, "");
                                    mAccpacDBLink = mAccpacSession.OpenDBLink(tagDBLinkTypeEnum.DBLINK_COMPANY, tagDBLinkFlagsEnum.DBLINK_FLG_READWRITE);

                                    if (!StatusUpdate.Enabled)
                                    {
                                        Code = 3;
                                        if (hidden)
                                        {
                                            Code = 31;
                                        }
                                        else
                                        {
                                            Code = 3;
                                        }

                                        StatusUpdate.Start();
                                    }
                                    else
                                    {
                                        Code = 3;
                                        if (hidden)
                                        {
                                            Code = 31;
                                        }
                                        else
                                        {
                                            Code = 3;
                                        }
                                    }

                                    btnStart.Enabled = false;
                                    btnStop.Enabled = true;
                                    tableDependPay.Start();
                                    tableDependCancellation.Start();
                                    btnStart.Text = "Running";

                                    if (!intLink.isInitialized())
                                    {
                                        LogOperation("Creating Invoice Batch Set", 1);
                                        int LastBatchId = GetLastInvoiceBatch();
                                        int expiry5 = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - DateTime.Now.Day;
                                        expiry5++;

                                        intLink.Init(LastBatchId, expiry5);
                                        CreateInvoiceBatchSet();
                                    }


                                    LogOperation("Service Started", 1);
                                    monitorRunning = true;
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogOperation(ex.Message, 2);
            }
        }

        public decimal changetous(decimal regamt)
        {
            decimal rate = intLink.GetRate();
            decimal usamt = regamt / rate;
            return Math.Round(usamt,2);
        }
        public decimal changetousupdated(decimal regamt, int invnum)
        {
            decimal rate = intLink.GetUsRateByInvoice(invnum);
            decimal usamt = regamt / rate;
            return Math.Round(usamt,2);
        }
        private void StatusUpdate_Tick(object sender, EventArgs e)
        {
            BroadcastStatus(Code);
            MessageHandler();
        }

        private void Monitor__Load(object sender, EventArgs e)
        {

        }

        private void Monitor__Shown(object sender, EventArgs e)
        {
            //Hide();
        }

        private void resetCounterTimer_Tick(object sender, EventArgs e)
        {
            if (resetCounterTimer.Interval == 1000)
            {
                resetCounterTimer.Interval = RESET_FREQUENCY;
            }
            intLink.checkResetCounters(generateDaysExpire(""), generateDaysExpire(ONE_DAY));
        }

        private void deferredTimer_Tick(object sender, EventArgs e)
        {
            DateTime MonthlyRptDate = intLink.GetNextGenDate("Monthly");
            DateTime AnnualRptDate = intLink.GetNextGenDate("Annual");

            if (DateTime.Now.Year == MonthlyRptDate.Year && DateTime.Now.Month == MonthlyRptDate.Month && DateTime.Now.Day == MonthlyRptDate.Day)
            {
                if (DateTime.Now.Hour == MonthlyRptDate.Hour)
                {
                    LogOperation("Generating Monthly Deferred Income Report...", 2);
                    GenRptRequest("Monthly");
                    //here we set the next Report Generation Date
                    int es = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - DateTime.Now.Day;
                    es++;
                    DateTime nextMonth = DateTime.Now.AddDays(es);
                    DateTime nextGenDate = new DateTime(nextMonth.Year, nextMonth.Month, 2);
                    nextGenDate = nextGenDate.AddHours(2);
                    intLink.SetNextGenDate("Monthly", nextGenDate);
                    LogOperation("Monthly Deferred Report Generated.", 1);
                }
            }

            if (DateTime.Now.Year == AnnualRptDate.Year && DateTime.Now.Month == AnnualRptDate.Month && DateTime.Now.Day == AnnualRptDate.Day)
            {
                if (DateTime.Now.Hour == AnnualRptDate.Hour)
                {
                    LogOperation("Generating Annual Deferred Income Report...", 2);
                    GenRptRequest("Annual");
                    //here we set the next Report Generation Date
                    DateTime nextGenDate = new DateTime(DateTime.Now.Year + 1, 4, 2);
                    nextGenDate = nextGenDate.AddHours(3);
                    intLink.SetNextGenDate("Annual", nextGenDate);
                    LogOperation("Annual Deferred Report Generated.", 1);
                }
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public string GenerateErrorFile()
        {
            throw new NotImplementedException();
        }

        public string Item(int Index)
        {
            throw new NotImplementedException();
        }

        public void Put(string Msg, tagErrorPriority Priority, ref object Params, string Source = "", string ErrCode = "", string HelpFile = "", int HelpContextID = -1)
        {
            throw new NotImplementedException();
        }

        public void Get(int Index, out string pMsg, out tagErrorPriority pPriority, out string pSource, out string pErrCode, out string pHelpFile, out int pHelpID)
        {
            throw new NotImplementedException();
        }

        public void PutRscMsg(string AppID, int rscID, tagErrorPriority Priority, ref object Params, string Source = "", string ErrCode = "", string HelpFile = "", int HelpContextID = -1)
        {
            throw new NotImplementedException();
        }

        public void Get2(int Index, out object pMsg, out object pPriority, out object pSource, out object pErrCode, out object pHelpFile, out object pHelpID)
        {
            throw new NotImplementedException();
        }

        public tagErrorPriority GetPriority(int Index)
        {
            throw new NotImplementedException();
        }

        public int Count => throw new NotImplementedException();
    }
}
