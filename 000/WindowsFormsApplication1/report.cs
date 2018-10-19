using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace SyncMon
{
    public class report
    {
        public void gen_rpt(SyncMon.Integration intlink)
        {
            SqlConnection connection1 = new SqlConnection("Data Source=SMA-DBSRV\\ASMSDEV;Initial Catalog=ASMSGenericMaster;Integrated Security=True");
            SqlConnection connection = new SqlConnection("Data Source=SMA-DBSRV\\ASMSDEV;Initial Catalog=ASMSGenericMaster;Integrated Security=True");
            SqlConnection connection2 = new SqlConnection("Data Source=SMA-DBSRV\\ASMSDEV;Initial Catalog=ASMSSAGEINTEGRATION; MultipleActiveResultSets=True; Integrated Security=True");
            
            decimal budgettotal = 0;
            decimal invoiceTotal = 0;
            decimal fromRev = 0;
            decimal toRev = 0;
            decimal closeBal = 0;
            decimal balancebf = 0;
            decimal invoiceTotalForYes = 0;

            decimal invoicetotcell = 0;
            decimal fromremcell = 0;
            decimal torevcell = 0;
            decimal closingtotcell = 0;
            decimal balancebfcell = 0;
            decimal budgettotcell = 0;

            decimal invoicetotbb = 0;
            decimal fromrembb = 0;
            decimal torevbb = 0;
            decimal closingtotbb = 0;
            decimal balancebfbb = 0;
            decimal budgettotbb = 0;

            decimal invoicetotmicro = 0;
            decimal fromrevmicro = 0;
            decimal torevmicro = 0;
            decimal closingtotmicro = 0;
            decimal balancebfmicro = 0;
            decimal budgettotmic = 0;

            decimal invoicetottrunk = 0;
            decimal fromrevtrunk = 0;
            decimal torevtrunk = 0;
            decimal closingtottrunk = 0;
            decimal balancebftrunk = 0;
            decimal budgettottrunk = 0;

            decimal invoicetotaero = 0;
            decimal fromrevaero = 0;
            decimal torevaero = 0;
            decimal closingtotaero = 0;
            decimal balancebfaero = 0;
            decimal budgettotaero = 0;

            decimal invoicetotds = 0;
            decimal fromrevds = 0;
            decimal torevds = 0;
            decimal closingtotds = 0;
            decimal balancebfds = 0;
            decimal budgettotds = 0;

            decimal invoicetotmar = 0;
            decimal fromrevmar = 0;
            decimal torevmar = 0;
            decimal closingtotmar = 0;
            decimal balancebfmar = 0;
            decimal budgettotmar = 0;

            decimal invoicetotother = 0;
            decimal fromrevother = 0;
            decimal torevother = 0;
            decimal closingtotother = 0;
            decimal balancebfother = 0;
            decimal budgettotother = 0;

            decimal invoicetotvsat = 0;
            decimal fromrevvsat = 0;
            decimal torevvsat = 0;
            decimal closingtotvsat = 0;
            decimal balancebfvsat = 0;
            decimal budgettotvsat = 0;

            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            int mmonth = 0, yyear = 0;

            DateTime defered = new DateTime(year, month, 1);

            if (month == 12)
            {
                mmonth = 1;
                yyear = year + 1;
            }
            else
            {
                mmonth = month + 1;
                yyear = year;
            }

            Int32 id = 0;
            Decimal invoiceamount = 0;
            DateTime ValidityS;
            DateTime ValidityF;
            int invoiceid = 0;
            string ccnum = " ";
            string Company = " ";
            string fname = " ";
            string lname = " ";
            int glid = 0; string description = " ";

            SqlCommand newCmd = new SqlCommand("sp_report", connection1);
            newCmd.CommandType = System.Data.CommandType.StoredProcedure;
            newCmd.Parameters.AddWithValue("@date", defered);

            SqlCommand newCmdd = new SqlCommand("sp_OpeningBalCleanUp", connection2);
            newCmdd.CommandType = System.Data.CommandType.StoredProcedure;
            newCmdd.Parameters.AddWithValue("@date", defered);
            connection2.Open();
            newCmdd.ExecuteNonQuery();
            connection2.Close();

            SqlCommand newCmddd = new SqlCommand("sp_getCancellationsAndAdjustments", connection2);
            newCmddd.CommandType = System.Data.CommandType.StoredProcedure;
            newCmddd.Parameters.AddWithValue("@month", month);
            newCmddd.Parameters.AddWithValue("@year", year);

            SqlCommand newCmdddd = new SqlCommand("sp_getCreditMemos", connection2);
            newCmdddd.CommandType = System.Data.CommandType.StoredProcedure;
            newCmdddd.Parameters.AddWithValue("@month", month);
            newCmdddd.Parameters.AddWithValue("@year", year);

            SqlCommand newCmdCredFwd = new SqlCommand("sp_getCreditMemos", connection2);
            newCmdCredFwd.CommandType = System.Data.CommandType.StoredProcedure;
            newCmdCredFwd.Parameters.AddWithValue("@month", mmonth);
            newCmdCredFwd.Parameters.AddWithValue("@year", yyear);

            //Instantiate objects for UI output here
            List<DataWrapper> tables = new List<DataWrapper>();
            DataWrapper cell_table = new DataWrapper("Cellular");
            DataWrapper micro_table = new DataWrapper("Microwave");
            DataWrapper bbrand_table = new DataWrapper("Broadband");
            DataWrapper vsat_table = new DataWrapper("Vsat");
            DataWrapper other_table = new DataWrapper("Other");
            DataWrapper trunking_table = new DataWrapper("Trunking");
            DataWrapper aero_table = new DataWrapper("Aeronautical");
            DataWrapper marine_table = new DataWrapper("Marine");
            DataWrapper dservices_table = new DataWrapper("Data & Services");
            DataWrapper table_table = new DataWrapper("Table");

            UIData row_cell = null;
            UIData row_micro = null;
            UIData row_bbrand = null;
            UIData row_vsat = null;
            UIData row_other = null;
            UIData row_trunking = null;
            UIData row_aero = null;
            UIData row_marine = null;
            UIData row_dservices = null;
            UIData row_table = null;

            try
            {
                connection1.Open();
                connection2.Open();
                SqlDataReader rdr = newCmd.ExecuteReader();
                SqlDataReader rdr2 = newCmddd.ExecuteReader();
                SqlDataReader rdr3 = newCmdddd.ExecuteReader();
                SqlDataReader rdr4 = newCmdCredFwd.ExecuteReader();

                while (rdr.Read())
                {
                    id = rdr.GetInt32(0);
                    ccnum = rdr.GetString(1);
                    Company = rdr.GetString(2);
                    fname = rdr.GetString(3);
                    lname = rdr.GetString(4);
                    invoiceamount = rdr.GetDecimal(5);
                    ValidityS = rdr.GetDateTime(6);
                    ValidityF = rdr.GetDateTime(7);
                    glid = rdr.GetInt32(8);
                    description = rdr.GetString(9);
                    invoiceid = rdr.GetInt32(10);
                    DateTime createdate = rdr.GetDateTime(11);

                    int Differencee = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                    decimal budget = 0;
                    int validity = 0;
                    int Difference = 0;

                    var ValiditySS = DateTime.Now.ToString("dd/MM/yyyy");
                    if (description == "Modification")
                    {
                        int totmonths = CheckMonths(ValidityS, ValidityF);
                        int monthsgone = CheckMonthsModification(ValidityS, createdate);
                        validity = totmonths - monthsgone;
                        int Di = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                        Difference = Di - monthsgone;
                        ValiditySS = createdate.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        validity = CheckMonths(ValidityS, ValidityF);
                        if (validity == 2) validity = 3;
                        Difference = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                        ValiditySS = ValidityS.ToString("dd/MM/yyyy");
                    }

                    int trial33 = validity - Differencee;
                    Differencee = Differencee + 1;
                    DataSet df = new DataSet();

                    df = brian_businessClass.GetBudget(ccnum, invoiceid);
                    if (!IsEmpty(df) && Differencee > 1)
                    {
                        DataRow dr = df.Tables[0].Rows[0];
                        budget = Convert.ToDecimal(dr.ItemArray.GetValue(1));
                    }

                    if (!IsEmpty(df) && Differencee == 1)
                    {
                        DataRow dr = df.Tables[0].Rows[0];
                        DateTime ne = Convert.ToDateTime(dr.ItemArray.GetValue(2));
                        if (ne.Month >= ValidityS.Month /*&& ne.Year >= ValidityS.Year*/)
                        {
                            budget = Convert.ToDecimal(dr.ItemArray.GetValue(1));
                        }
                    }

                    DataSet budg = brian_businessClass.GetCustBudget(ccnum, invoiceid);
                    if (IsEmpty(budg))
                    {
                        brian_businessClass.InsertBudgetInfo(ccnum, budget, invoiceid);
                    }

                    var ValidityFF = ValidityF.ToString("dd/MM/yyyy");

                    string invoicestat = " ";
                    string clientCompany = " ";
                    decimal openingbalance = 0;
                    decimal closingbalance = 0;
                    decimal toRevenue = 0;
                    decimal fromRevenue = 0;

                    if (Company == null || Company == "")
                    {
                        clientCompany = fname + " " + lname;

                    }
                    else
                    {
                        clientCompany = Company;
                        clientCompany = System.Net.WebUtility.HtmlDecode(clientCompany);
                    }

                    int trial = ((defered.Year - ValidityS.Year) * 12) + validity - Difference;
                    int trial2 = validity - Difference;

                    decimal opp = 0;
                    string op = " ";
                    DataSet opset = new DataSet();

                    DataSet IfIsCredMemo = brian_businessClass.GetIsInvoiceCreditMemo(invoiceid);
                    {
                        if (Difference > 0 && ValidityS.Month != defered.Month || ValidityS.Month == defered.Month && ValidityF.Year != defered.Year && ValidityS.Year != defered.Year || ValidityF.Month == defered.Month)
                        {
                            if (defered.Month - 1 == 0)
                            {
                                opset = brian_businessClass.GetOpeningBalanceForMonth(ccnum, 12, (defered.Year - 1), invoiceid);
                            }

                            else
                                opset = brian_businessClass.GetOpeningBalanceForMonth(ccnum, (defered.Month - 1), defered.Year, invoiceid);
                            if (!IsEmpty(opset))
                            {
                                DataRow dr = opset.Tables[0].Rows[0];
                                decimal.TryParse(dr["openingbalance"].ToString(), out opp);
                            }
                            else
                                opp = 0;
                        }

                        if (opp == 0)
                        {
                            opp = 0;
                        }
                        else
                        {
                            opp = opp;
                        }
                    }

                    if (invoiceid == 16471)
                    {
                        int a = 0;
                    }

                    if (ValidityS.Month == defered.Month && ValidityS.Year == defered.Year && IsEmpty(IfIsCredMemo))
                    {
                        if (ValidityS.Day <= 15)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                Difference = Difference + 1;
                                trial2 = validity - Difference;
                                openingbalance = 0;
                                toRevenue = invoiceamount * 1 / validity;
                                closingbalance = invoiceamount - toRevenue;

                                fromRevenue = invoiceamount;
                                invoicestat = "Yes";
                            }

                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += openingbalance;
                                    budgettotcell += budget;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }

                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }

                            else if (glid == 5160)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);

                                }
                            }

                            else if (glid == 5161)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }


                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    budgettotal += budget;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);

                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }

                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += openingbalance;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }
                        }

                        if (ValidityS.Day > 15)
                        {

                            if (Difference >= 0 && trial2 >= 0)
                            {
                                openingbalance = 0;
                                toRevenue = 0;
                                fromRevenue = invoiceamount;
                                closingbalance = invoiceamount;
                                invoicestat = "Yes";


                                //if (!IsEmpty(opset))
                                //{
                                //    opp = opp;
                                //}
                                //else
                                //    opp = 0;

                                opp = 0;

                                if (Difference > 1 && IsEmpty(opset))
                                {
                                    toRevenue = toRevenue * Difference;
                                }
                            }
                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotcell += budget;
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    budgettotal += budget;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }

                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);

                                }
                            }


                            else if (glid == 5160)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }


                            else if (glid == 5161)
                            {

                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }


                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);
                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }


                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                 row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }
                        }
                    }

                    else if (ValidityS.Month == defered.Month && ValidityF.Year != defered.Year && ValidityS.Day <= 15 && ValidityS.Year != defered.Year && IsEmpty(IfIsCredMemo))
                    {

                        if (Difference >= 0 && trial2 >= 0)
                        {

                            Difference = Difference + 1;
                            trial2 = validity - Difference;
                            toRevenue = invoiceamount * 1 / validity;

                            decimal amountalreadypaid = toRevenue * (Difference - 1);
                            openingbalance = invoiceamount - amountalreadypaid;
                            fromRevenue = 0;
                            closingbalance = invoiceamount - (amountalreadypaid + toRevenue);

                            invoicestat = "No";

                            if (!IsEmpty(opset))
                            {
                                opp = opp;
                            }
                            else
                                opp = 0;

                            if (Difference > 1 && IsEmpty(opset))
                            {
                                toRevenue = toRevenue * Difference;
                                fromRevenue = invoiceamount;
                            }
                            if (description == "Modification" && Difference == 0)
                            {
                                toRevenue = 0;
                            }
                            if (description == "Modification" && defered.Month == createdate.Month && defered.Year == createdate.Year)
                            {
                                fromRevenue = invoiceamount;
                                invoicestat = "Yes";
                            }
                        }
                        if (glid == 5156)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotcell += budget;
                                invoicetotcell += invoiceamount;
                                torevcell += toRevenue;
                                fromremcell += fromRevenue;
                                closingtotcell += closingbalance;
                                balancebfcell += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                budgettotal += budget;
                                closeBal += closingbalance;
                                //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_cell = new UIData();
                                row_cell.licenseNumber = ccnum;
                                row_cell.clientCompany = clientCompany;
                                row_cell.invoiceID = invoiceid.ToString();
                                row_cell.budget = formatMoney(Math.Round(budget, 2));
                                row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_cell.thisMonthInv = invoicestat;
                                row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_cell.totalMonths = validity;
                                row_cell.monthUtil = Difference;
                                row_cell.monthRemain = trial2;
                                row_cell.valPStart = ValiditySS;
                                row_cell.valPEnd = ValidityFF;

                                cell_table.records.Add(row_cell);
                            }
                        }

                        else if (glid == 5157)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotbb += budget;
                                invoicetotbb += invoiceamount;
                                torevbb += toRevenue;
                                fromrembb += fromRevenue;
                                closingtotbb += closingbalance;
                                balancebfbb += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                budgettotal += budget;
                                closeBal += closingbalance;
                                //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_bbrand = new UIData();
                                row_bbrand.licenseNumber = ccnum;
                                row_bbrand.clientCompany = clientCompany;
                                row_bbrand.invoiceID = invoiceid.ToString();
                                row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_bbrand.thisMonthInv = invoicestat;
                                row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_bbrand.totalMonths = validity;
                                row_bbrand.monthUtil = Difference;
                                row_bbrand.monthRemain = trial2;
                                row_bbrand.valPStart = ValiditySS;
                                row_bbrand.valPEnd = ValidityFF;

                                bbrand_table.records.Add(row_bbrand);
                            }
                        }
                        else if (glid == 5158)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmic += budget;
                                invoicetotmicro += invoiceamount;
                                torevmicro += toRevenue;
                                fromrevmicro += fromRevenue;
                                closingtotmicro += closingbalance;
                                balancebfmicro += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                budgettotal += budget;
                                closeBal += closingbalance;
                                //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_micro = new UIData();
                                row_micro.licenseNumber = ccnum;
                                row_micro.clientCompany = clientCompany;
                                row_micro.invoiceID = invoiceid.ToString();
                                row_micro.budget = formatMoney(Math.Round(budget, 2));
                                row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_micro.thisMonthInv = invoicestat;
                                row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_micro.totalMonths = validity;
                                row_micro.monthUtil = Difference;
                                row_micro.monthRemain = trial2;
                                row_micro.valPStart = ValiditySS;
                                row_micro.valPEnd = ValidityFF;

                                micro_table.records.Add(row_micro);
                            }
                        }

                        else if (glid == 5159)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotds += budget;
                                invoicetotds += invoiceamount;
                                torevds += toRevenue;
                                fromrevds += fromRevenue;
                                closingtotds += closingbalance;
                                balancebfds += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_dservices = new UIData();
                                row_dservices.licenseNumber = ccnum;
                                row_dservices.clientCompany = clientCompany;
                                row_dservices.invoiceID = invoiceid.ToString();
                                row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_dservices.thisMonthInv = invoicestat;
                                row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_dservices.totalMonths = validity;
                                row_dservices.monthUtil = Difference;
                                row_dservices.monthRemain = trial2;
                                row_dservices.valPStart = ValiditySS;
                                row_dservices.valPEnd = ValidityFF;

                                dservices_table.records.Add(row_dservices);
                            }
                        }

                        else if (glid == 5160)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotvsat += budget;
                                invoicetotvsat += invoiceamount;
                                torevvsat += toRevenue;
                                fromrevvsat += fromRevenue;
                                closingtotvsat += closingbalance;
                                balancebfvsat += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_vsat = new UIData();
                                row_vsat.licenseNumber = ccnum;
                                row_vsat.clientCompany = clientCompany;
                                row_vsat.invoiceID = invoiceid.ToString();
                                row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_vsat.thisMonthInv = invoicestat;
                                row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_vsat.totalMonths = validity;
                                row_vsat.monthUtil = Difference;
                                row_vsat.monthRemain = trial2;
                                row_vsat.valPStart = ValiditySS;
                                row_vsat.valPEnd = ValidityFF;

                                vsat_table.records.Add(row_vsat);
                            }
                        }

                        else if (glid == 5161)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotaero += budget;
                                invoicetotaero += invoiceamount;
                                torevaero += toRevenue;
                                fromrevaero += fromRevenue;
                                closingtotaero += closingbalance;
                                balancebfaero += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_aero = new UIData();
                                row_aero.licenseNumber = ccnum;
                                row_aero.clientCompany = clientCompany;
                                row_aero.invoiceID = invoiceid.ToString();
                                row_aero.budget = formatMoney(Math.Round(budget, 2));
                                row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_aero.thisMonthInv = invoicestat;
                                row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_aero.totalMonths = validity;
                                row_aero.monthUtil = Difference;
                                row_aero.monthRemain = trial2;
                                row_aero.valPStart = ValiditySS;
                                row_aero.valPEnd = ValidityFF;

                                aero_table.records.Add(row_aero);
                            }
                        }

                        else if (glid == 5162)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmar += budget;
                                invoicetotmar += invoiceamount;
                                torevmar += toRevenue;
                                fromrevmar += fromRevenue;
                                closingtotmar += closingbalance;
                                balancebfmar += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_marine = new UIData();
                                row_marine.licenseNumber = ccnum;
                                row_marine.clientCompany = clientCompany;
                                row_marine.invoiceID = invoiceid.ToString();
                                row_marine.budget = formatMoney(Math.Round(budget, 2));
                                row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_marine.thisMonthInv = invoicestat;
                                row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_marine.totalMonths = validity;
                                row_marine.monthUtil = Difference;
                                row_marine.monthRemain = trial2;
                                row_marine.valPStart = ValiditySS;
                                row_marine.valPEnd = ValidityFF;

                                marine_table.records.Add(row_marine);
                            }
                        }

                        else if (glid == 5163)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettottrunk += budget;
                                invoicetottrunk += invoiceamount;
                                torevtrunk += toRevenue;
                                fromrevtrunk += fromRevenue;
                                closingtottrunk += closingbalance;
                                balancebftrunk += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_trunking = new UIData();
                                row_trunking.licenseNumber = ccnum;
                                row_trunking.clientCompany = clientCompany;
                                row_trunking.invoiceID = invoiceid.ToString();
                                row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_trunking.thisMonthInv = invoicestat;
                                row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_trunking.totalMonths = validity;
                                row_trunking.monthUtil = Difference;
                                row_trunking.monthRemain = trial2;
                                row_trunking.valPStart = ValiditySS;
                                row_trunking.valPEnd = ValidityFF;

                                trunking_table.records.Add(row_trunking);
                            }
                        }

                        else if (glid == 5164)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotother += budget;
                                invoicetotother += invoiceamount;
                                torevother += toRevenue;
                                fromrevother += fromRevenue;
                                closingtotother += closingbalance;
                                balancebfother += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_other = new UIData();
                                row_other.licenseNumber = ccnum;
                                row_other.clientCompany = clientCompany;
                                row_other.invoiceID = invoiceid.ToString();
                                row_other.budget = formatMoney(Math.Round(budget, 2));
                                row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_other.thisMonthInv = invoicestat;
                                row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_other.totalMonths = validity;
                                row_other.monthUtil = Difference;
                                row_other.monthRemain = trial2;
                                row_other.valPStart = ValiditySS;
                                row_other.valPEnd = ValidityFF;

                                other_table.records.Add(row_other);
                            }
                        }
                        else
                        {
                            //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                            row_table = new UIData();
                            row_table.licenseNumber = ccnum;
                            row_table.clientCompany = clientCompany;
                            row_table.invoiceID = invoiceid.ToString();
                            row_table.budget = formatMoney(Math.Round(budget, 2));
                            row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_table.thisMonthInv = invoicestat;
                            row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_table.totalMonths = validity;
                            row_table.monthUtil = Difference;
                            row_table.monthRemain = trial2;
                            row_table.valPStart = ValiditySS;
                            row_table.valPEnd = ValidityFF;

                            table_table.records.Add(row_table);
                        }
                    }
                    else if (ValidityS.Month == defered.Month && ValidityF.Year != defered.Year && ValidityS.Day > 15 && ValidityS.Year != defered.Year && IsEmpty(IfIsCredMemo))
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            toRevenue = invoiceamount * 1 / validity;

                            decimal amountalreadypaid = toRevenue * (Difference - 1);
                            openingbalance = invoiceamount - amountalreadypaid;
                            fromRevenue = 0;
                            closingbalance = invoiceamount - (amountalreadypaid + toRevenue);

                            invoicestat = "No";


                            if (!IsEmpty(opset))
                            {
                                opp = opp;

                            }
                            else
                                opp = 0;



                            if (Difference > 1 && IsEmpty(opset))
                            {
                                toRevenue = toRevenue * Difference;
                                fromRevenue = invoiceamount;
                            }

                            if (description == "Modification" && Difference == 0)
                            {
                                toRevenue = 0;
                            }
                            if (description == "Modification" && defered.Month == createdate.Month && defered.Year == createdate.Year)
                            {
                                fromRevenue = invoiceamount;
                                invoicestat = "Yes";
                            }
                        }
                        if (glid == 5156)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotcell += budget;
                                invoicetotcell += invoiceamount;
                                torevcell += toRevenue;
                                fromremcell += fromRevenue;
                                closingtotcell += closingbalance;
                                balancebfcell += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_cell = new UIData();
                                row_cell.licenseNumber = ccnum;
                                row_cell.clientCompany = clientCompany;
                                row_cell.invoiceID = invoiceid.ToString();
                                row_cell.budget = formatMoney(Math.Round(budget, 2));
                                row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_cell.thisMonthInv = invoicestat;
                                row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_cell.totalMonths = validity;
                                row_cell.monthUtil = Difference;
                                row_cell.monthRemain = trial2;
                                row_cell.valPStart = ValiditySS;
                                row_cell.valPEnd = ValidityFF;

                                cell_table.records.Add(row_cell);
                            }
                        }

                        else if (glid == 5157)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotbb += budget;
                                invoicetotbb += invoiceamount;
                                torevbb += toRevenue;
                                fromrembb += fromRevenue;
                                closingtotbb += closingbalance;
                                balancebfbb += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_bbrand = new UIData();
                                row_bbrand.licenseNumber = ccnum;
                                row_bbrand.clientCompany = clientCompany;
                                row_bbrand.invoiceID = invoiceid.ToString();
                                row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_bbrand.thisMonthInv = invoicestat;
                                row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_bbrand.totalMonths = validity;
                                row_bbrand.monthUtil = Difference;
                                row_bbrand.monthRemain = trial2;
                                row_bbrand.valPStart = ValiditySS;
                                row_bbrand.valPEnd = ValidityFF;

                                bbrand_table.records.Add(row_bbrand);
                            }
                        }
                        else if (glid == 5158)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmic += budget;
                                invoicetotmicro += invoiceamount;
                                torevmicro += toRevenue;
                                fromrevmicro += fromRevenue;
                                closingtotmicro += closingbalance;
                                balancebfmicro += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_micro = new UIData();
                                row_micro.licenseNumber = ccnum;
                                row_micro.clientCompany = clientCompany;
                                row_micro.invoiceID = invoiceid.ToString();
                                row_micro.budget = formatMoney(Math.Round(budget, 2));
                                row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_micro.thisMonthInv = invoicestat;
                                row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_micro.totalMonths = validity;
                                row_micro.monthUtil = Difference;
                                row_micro.monthRemain = trial2;
                                row_micro.valPStart = ValiditySS;
                                row_micro.valPEnd = ValidityFF;

                                micro_table.records.Add(row_micro);
                            }
                        }

                        else if (glid == 5159)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotds += budget;
                                invoicetotds += invoiceamount;
                                torevds += toRevenue;
                                fromrevds += fromRevenue;
                                closingtotds += closingbalance;
                                balancebfds += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_dservices = new UIData();
                                row_dservices.licenseNumber = ccnum;
                                row_dservices.clientCompany = clientCompany;
                                row_dservices.invoiceID = invoiceid.ToString();
                                row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_dservices.thisMonthInv = invoicestat;
                                row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_dservices.totalMonths = validity;
                                row_dservices.monthUtil = Difference;
                                row_dservices.monthRemain = trial2;
                                row_dservices.valPStart = ValiditySS;
                                row_dservices.valPEnd = ValidityFF;

                                dservices_table.records.Add(row_dservices);
                            }
                        }

                        else if (glid == 5160)
                        {

                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotvsat += budget;
                                invoicetotvsat += invoiceamount;
                                torevvsat += toRevenue;
                                fromrevvsat += fromRevenue;
                                closingtotvsat += closingbalance;
                                balancebfvsat += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_vsat = new UIData();
                                row_vsat.licenseNumber = ccnum;
                                row_vsat.clientCompany = clientCompany;
                                row_vsat.invoiceID = invoiceid.ToString();
                                row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_vsat.thisMonthInv = invoicestat;
                                row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_vsat.totalMonths = validity;
                                row_vsat.monthUtil = Difference;
                                row_vsat.monthRemain = trial2;
                                row_vsat.valPStart = ValiditySS;
                                row_vsat.valPEnd = ValidityFF;

                                vsat_table.records.Add(row_vsat);
                            }
                        }

                        else if (glid == 5161)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotaero += budget;
                                invoicetotaero += invoiceamount;
                                torevaero += toRevenue;
                                fromrevaero += fromRevenue;
                                closingtotaero += closingbalance;
                                balancebfaero += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;

                                //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_aero = new UIData();
                                row_aero.licenseNumber = ccnum;
                                row_aero.clientCompany = clientCompany;
                                row_aero.invoiceID = invoiceid.ToString();
                                row_aero.budget = formatMoney(Math.Round(budget, 2));
                                row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_aero.thisMonthInv = invoicestat;
                                row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_aero.totalMonths = validity;
                                row_aero.monthUtil = Difference;
                                row_aero.monthRemain = trial2;
                                row_aero.valPStart = ValiditySS;
                                row_aero.valPEnd = ValidityFF;

                                aero_table.records.Add(row_aero);
                            }
                        }

                        else if (glid == 5162)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmar += budget;
                                invoicetotmar += invoiceamount;
                                torevmar += toRevenue;
                                fromrevmar += fromRevenue;
                                closingtotmar += closingbalance;
                                balancebfmar += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_marine = new UIData();
                                row_marine.licenseNumber = ccnum;
                                row_marine.clientCompany = clientCompany;
                                row_marine.invoiceID = invoiceid.ToString();
                                row_marine.budget = formatMoney(Math.Round(budget, 2));
                                row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_marine.thisMonthInv = invoicestat;
                                row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_marine.totalMonths = validity;
                                row_marine.monthUtil = Difference;
                                row_marine.monthRemain = trial2;
                                row_marine.valPStart = ValiditySS;
                                row_marine.valPEnd = ValidityFF;

                                marine_table.records.Add(row_marine);
                            }
                        }

                        else if (glid == 5163)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettottrunk += budget;
                                invoicetottrunk += invoiceamount;
                                torevtrunk += toRevenue;
                                fromrevtrunk += fromRevenue;
                                closingtottrunk += closingbalance;
                                balancebftrunk += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_trunking = new UIData();
                                row_trunking.licenseNumber = ccnum;
                                row_trunking.clientCompany = clientCompany;
                                row_trunking.invoiceID = invoiceid.ToString();
                                row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_trunking.thisMonthInv = invoicestat;
                                row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_trunking.totalMonths = validity;
                                row_trunking.monthUtil = Difference;
                                row_trunking.monthRemain = trial2;
                                row_trunking.valPStart = ValiditySS;
                                row_trunking.valPEnd = ValidityFF;

                                trunking_table.records.Add(row_trunking);
                            }
                        }

                        else if (glid == 5164)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotother += budget;
                                invoicetotother += invoiceamount;
                                torevother += toRevenue;
                                fromrevother += fromRevenue;
                                closingtotother += closingbalance;
                                balancebfother += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_other = new UIData();
                                row_other.licenseNumber = ccnum;
                                row_other.clientCompany = clientCompany;
                                row_other.invoiceID = invoiceid.ToString();
                                row_other.budget = formatMoney(Math.Round(budget, 2));
                                row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_other.thisMonthInv = invoicestat;
                                row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_other.totalMonths = validity;
                                row_other.monthUtil = Difference;
                                row_other.monthRemain = trial2;
                                row_other.valPStart = ValiditySS;
                                row_other.valPEnd = ValidityFF;

                                other_table.records.Add(row_other);
                            }
                        }
                        else
                        {
                            //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), Math.Round(budget, 2), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                            row_table = new UIData();
                            row_table.licenseNumber = ccnum;
                            row_table.clientCompany = clientCompany;
                            row_table.invoiceID = invoiceid.ToString();
                            row_table.budget = formatMoney(Math.Round(budget, 2));
                            row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_table.thisMonthInv = invoicestat;
                            row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_table.totalMonths = validity;
                            row_table.monthUtil = Difference;
                            row_table.monthRemain = trial2;
                            row_table.valPStart = ValiditySS;
                            row_table.valPEnd = ValidityFF;

                            table_table.records.Add(row_table);
                        }
                    }

                    else
                    {

                        if (ValidityS.Month != defered.Month && ValidityS.Day <= 15 && IsEmpty(IfIsCredMemo))
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {

                                Difference = Difference + 1;
                                trial2 = validity - Difference;
                                toRevenue = invoiceamount * 1 / validity;

                                decimal amountalreadypaid = toRevenue * (Difference - 1);
                                openingbalance = invoiceamount - amountalreadypaid;
                                fromRevenue = 0;
                                closingbalance = invoiceamount - (amountalreadypaid + toRevenue);

                                invoicestat = "No";

                                if (!IsEmpty(opset))
                                {
                                    opp = opp;
                                }
                                else
                                    opp = 0;

                                if (Difference > 1 && IsEmpty(opset))
                                {
                                    toRevenue = toRevenue * Difference;
                                    fromRevenue = invoiceamount;
                                }
                                if (description == "Modification" && Difference == 0)
                                {
                                    toRevenue = 0;
                                }
                                if (description == "Modification" && defered.Month == createdate.Month && defered.Year == createdate.Year)
                                {
                                    fromRevenue = invoiceamount;
                                    invoicestat = "Yes";
                                }
                            }
                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotcell += budget;
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }
                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }

                            else if (glid == 5160)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }

                            else if (glid == 5161)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }

                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);
                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }

                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }

                        }

                        else if (ValidityS.Day > 15 && trial2 == 0 && IsEmpty(IfIsCredMemo))
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                toRevenue = invoiceamount * 1 / validity;

                                decimal amountalreadypaid = toRevenue * (Difference - 1);
                                openingbalance = invoiceamount - amountalreadypaid;
                                fromRevenue = 0;
                                closingbalance = invoiceamount - (amountalreadypaid + toRevenue);

                                invoicestat = "No";


                                if (!IsEmpty(opset))
                                {
                                    opp = opp;

                                }
                                else
                                    opp = 0;



                                if (Difference > 1 && IsEmpty(opset))
                                {
                                    toRevenue = toRevenue * Difference;
                                    fromRevenue = invoiceamount;
                                }
                                if (description == "Modification" && Difference == 0)
                                {
                                    toRevenue = 0;
                                }
                                if (description == "Modification" && defered.Month == createdate.Month && defered.Year == createdate.Year)
                                {
                                    fromRevenue = invoiceamount;
                                    invoicestat = "Yes";
                                }
                            }
                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotcell += budget;
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }
                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }

                            else if (glid == 5160)
                            {

                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }

                            else if (glid == 5161)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;

                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }

                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);

                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }

                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }
                        }
                        else if (ValidityS.Month != defered.Month && ValidityS.Day > 15 && IsEmpty(IfIsCredMemo))
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                toRevenue = invoiceamount * 1 / validity;
                                decimal amountalreadypaid = toRevenue * (Difference - 1);
                                openingbalance = invoiceamount - amountalreadypaid;
                                fromRevenue = 0;
                                closingbalance = invoiceamount - (amountalreadypaid + toRevenue);
                                invoicestat = "No";

                                if (!IsEmpty(opset))
                                {
                                    opp = opp;
                                }
                                else
                                    opp = 0;
                                //  balancebf += opp;

                                if (Difference > 1 && IsEmpty(opset))
                                {
                                    toRevenue = toRevenue * Difference;
                                    fromRevenue = invoiceamount;
                                }
                                if (description == "Modification" && Difference == 0)
                                {
                                    toRevenue = 0;
                                }

                                if (description == "Modification" && defered.Month == createdate.Month && defered.Year == createdate.Year)
                                {
                                    fromRevenue = invoiceamount;
                                    invoicestat = "Yes";
                                }
                            }
                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotcell += budget;
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }
                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }

                            else if (glid == 5160)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }

                            else if (glid == 5161)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }

                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += opp;
                                    budgettotal += budget;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);
                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }

                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += opp;
                                    budgettotal += budget;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), Math.Round(invoiceamount, 2), invoicestat, Math.Round(opp, 2), Math.Round(fromRevenue, 2), Math.Round(-toRevenue, 2), Math.Round(closingbalance, 2), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }
                        }
                    }

                    if (invoiceid == 16302)
                    {
                        int a = 0;
                    }

                    if (Difference > 0 || description == "Modification" && Difference == 0 || Difference == 0 && ValidityS.Month == defered.Month && ValidityS.Year == defered.Year && ValidityS.Day > 15)
                    {

                        DataSet opstatus = new DataSet();
                        opstatus = brian_businessClass.GetOpeningBalStat(ccnum, invoiceid, defered);
                        if (IsEmpty(IfIsCredMemo))
                        {
                            if (IsEmpty(opstatus))
                            {
                                brian_businessClass.InsertOpBalNew(ccnum, closingbalance, defered, invoiceid, ValidityS, ValidityF);
                            }
                            else
                            {
                                brian_businessClass.UpdateOpBalNew(ccnum, closingbalance, defered, invoiceid, ValidityS, ValidityF);
                            }
                        }
                    }
                }

                while (rdr2.Read())
                {
                    ccnum = "";
                    invoiceamount = rdr2.GetDecimal(2);
                    string s = "04/01/2017";
                    string e = "03/31/2018";
                    ValidityS = Convert.ToDateTime(s);
                    ValidityF = Convert.ToDateTime(e);
                    glid = 0;

                    invoiceid = rdr2.GetInt32(0);
                    DataSet clientidds = new DataSet();
                    clientidds = brian_businessClass.GetClientId(invoiceid);

                    if (!IsEmpty(clientidds))
                    {

                        DataRow dr = clientidds.Tables[0].Rows[0];
                        id = Convert.ToInt32(dr.ItemArray.GetValue(0));
                        ccnum = id.ToString();
                    }

                    DataSet clientnameds = new DataSet();
                    clientnameds = brian_businessClass.GetClientName(id);
                    if (!IsEmpty(clientnameds))
                    {
                        DataRow dr = clientnameds.Tables[0].Rows[0];
                        Company = dr.ItemArray.GetValue(0).ToString();
                        fname = dr.ItemArray.GetValue(1).ToString();
                        lname = dr.ItemArray.GetValue(2).ToString();
                        ccnum = dr.ItemArray.GetValue(3).ToString();
                    }

                    DataSet validityds = new DataSet();
                    validityds = brian_businessClass.GetValidity(invoiceid, defered.Year, (defered.Month - 1));
                    if (!IsEmpty(validityds))
                    {

                        DataRow dr = validityds.Tables[0].Rows[0];
                        string val = dr.ItemArray.GetValue(0).ToString();

                        if (val != "")
                        {
                            ValidityS = Convert.ToDateTime(dr.ItemArray.GetValue(0).ToString());
                            ValidityF = Convert.ToDateTime(dr.ItemArray.GetValue(1).ToString());
                        }
                    }

                    DataSet glds = new DataSet();
                    glds = brian_businessClass.GetCreditGl(invoiceid);
                    if (!IsEmpty(glds))
                    {

                        DataRow dr = glds.Tables[0].Rows[0];
                        glid = Convert.ToInt32(dr.ItemArray.GetValue(0));
                    }

                    int Differencee = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                    decimal budget = 0;
                    int validity = 0;
                    int Difference = 0;
                    //  int validity = ((ValidityF.Year - ValidityS.Year) * 12) + ValidityF.Month - ValidityS.Month;
                    var ValiditySS = DateTime.Now.ToString("dd/MM/yyyy");

                    validity = CheckMonths(ValidityS, ValidityF);
                    if (validity == 2) validity = 3;
                    Difference = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                    ValiditySS = ValidityS.ToString("dd/MM/yyyy");

                    int trial33 = validity - Differencee;
                    Differencee = Differencee + 1;
                    DataSet df = new DataSet();
                    df = brian_businessClass.GetBudget(ccnum, invoiceid);
                    if (!IsEmpty(df) && Differencee > 1)
                    {
                        DataRow dr = df.Tables[0].Rows[0];
                        budget = Convert.ToDecimal(dr.ItemArray.GetValue(1));
                    }

                    if (!IsEmpty(df) && Differencee == 1)
                    {

                        DataRow dr = df.Tables[0].Rows[0];
                        DateTime ne = Convert.ToDateTime(dr.ItemArray.GetValue(2));
                        if (ne.Month >= ValidityS.Month /*&& ne.Year >= ValidityS.Year*/)
                        {
                            budget = Convert.ToDecimal(dr.ItemArray.GetValue(1));
                        }
                    }

                    DataSet budg = brian_businessClass.GetCustBudget(ccnum, invoiceid);
                    if (IsEmpty(budg))
                    {
                        brian_businessClass.InsertBudgetInfo(ccnum, budget, invoiceid);
                    }

                    var ValidityFF = ValidityF.ToString("dd/MM/yyyy");

                    string invoicestat = " ";

                    string clientCompany = " ";
                    decimal openingbalance = 0;
                    decimal closingbalance = 0;
                    decimal toRevenue = 0;
                    decimal fromRevenue = 0;


                    if (Company == null || Company == "")
                    {
                        clientCompany = fname + " " + lname;

                    }
                    else
                    {
                        clientCompany = Company;
                        clientCompany = System.Net.WebUtility.HtmlDecode(clientCompany);
                    }

                    int trial = ((defered.Year - ValidityS.Year) * 12) + validity - Difference;
                    int trial2 = validity - Difference;


                    decimal opp = 0;
                    string op = " ";
                    DataSet opset = new DataSet();
                    {
                        if (Difference > 0 && ValidityS.Month != defered.Month || ValidityS.Month == defered.Month && ValidityF.Year != defered.Year && ValidityS.Year != defered.Year || ValidityF.Month == defered.Month)
                        {
                            if (defered.Month - 1 == 0)
                            {
                                opset = brian_businessClass.GetOpeningBalanceForMonth(ccnum, 12, (defered.Year - 1), invoiceid);
                            }

                            else
                                opset = brian_businessClass.GetOpeningBalanceForMonth(ccnum, (defered.Month - 1), defered.Year, invoiceid);
                            if (!IsEmpty(opset))
                            {
                                DataRow dr = opset.Tables[0].Rows[0];


                                decimal.TryParse(dr["openingbalance"].ToString(), out opp);


                            }
                            else
                                opp = 0;
                        }

                        if (opp == 0)
                        {
                            opp = 0;
                        }
                        else
                        {
                            opp = opp;
                        }
                    }

                    if (Difference >= 0 && trial2 >= 0 && opp != 0)
                    {
                        if (ValidityS.Day <= 15)
                        {
                            Difference = Difference + 1;
                        }

                        trial2 = validity - Difference;
                        openingbalance = opp;
                        toRevenue = invoiceamount * 1 / validity;
                        closingbalance = 0;
                        decimal amountalreadypaid = toRevenue * (Difference - 1);
                        toRevenue = amountalreadypaid;
                        fromRevenue = invoiceamount;
                        invoicestat = "No";
                    }

                    if (glid == 5156 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            invoicetotcell += invoiceamount;
                            torevcell += -toRevenue;
                            fromremcell += -fromRevenue;
                            closingtotcell += closingbalance;
                            balancebfcell += openingbalance;
                            budgettotcell += budget;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += -toRevenue;
                            budgettotal += budget;
                            closeBal += closingbalance;
                            //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_cell = new UIData();
                            row_cell.licenseNumber = ccnum;
                            row_cell.clientCompany = clientCompany;
                            row_cell.invoiceID = invoiceid.ToString();
                            row_cell.budget = formatMoney(Math.Round(budget, 2));
                            row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_cell.thisMonthInv = invoicestat;
                            row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_cell.totalMonths = validity;
                            row_cell.monthUtil = Difference;
                            row_cell.monthRemain = trial2;
                            row_cell.valPStart = ValiditySS;
                            row_cell.valPEnd = ValidityFF;

                            cell_table.records.Add(row_cell);
                        }
                    }
                    else if (glid == 5157 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotbb += budget;
                            invoicetotbb += invoiceamount;
                            torevbb += -toRevenue;
                            fromrembb += -fromRevenue;
                            closingtotbb += closingbalance;
                            balancebfbb += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += -toRevenue;
                            budgettotal += budget;
                            closeBal += closingbalance;
                            //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_bbrand = new UIData();
                            row_bbrand.licenseNumber = ccnum;
                            row_bbrand.clientCompany = clientCompany;
                            row_bbrand.invoiceID = invoiceid.ToString();
                            row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                            row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_bbrand.thisMonthInv = invoicestat;
                            row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_bbrand.totalMonths = validity;
                            row_bbrand.monthUtil = Difference;
                            row_bbrand.monthRemain = trial2;
                            row_bbrand.valPStart = ValiditySS;
                            row_bbrand.valPEnd = ValidityFF;

                            bbrand_table.records.Add(row_bbrand);
                        }
                    }
                    else if (glid == 5158 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotmic += budget;
                            invoicetotmicro += invoiceamount;
                            torevmicro += -toRevenue;
                            fromrevmicro += -fromRevenue;
                            closingtotmicro += closingbalance;
                            balancebfmicro += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += -toRevenue;
                            budgettotal += budget;
                            closeBal += closingbalance;
                            //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_micro = new UIData();
                            row_micro.licenseNumber = ccnum;
                            row_micro.clientCompany = clientCompany;
                            row_micro.invoiceID = invoiceid.ToString();
                            row_micro.budget = formatMoney(Math.Round(budget, 2));
                            row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_micro.thisMonthInv = invoicestat;
                            row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_micro.totalMonths = validity;
                            row_micro.monthUtil = Difference;
                            row_micro.monthRemain = trial2;
                            row_micro.valPStart = ValiditySS;
                            row_micro.valPEnd = ValidityFF;

                            micro_table.records.Add(row_micro);
                        }
                    }
                    else if (glid == 5159 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotds += budget;
                            invoicetotds += invoiceamount;
                            torevds += -toRevenue;
                            fromrevds += -fromRevenue;
                            closingtotds += closingbalance;
                            balancebfds += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += -toRevenue;
                            budgettotal += budget;
                            closeBal += closingbalance;
                            //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_dservices = new UIData();
                            row_dservices.licenseNumber = ccnum;
                            row_dservices.clientCompany = clientCompany;
                            row_dservices.invoiceID = invoiceid.ToString();
                            row_dservices.budget = formatMoney(Math.Round(budget, 2));
                            row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_dservices.thisMonthInv = invoicestat;
                            row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_dservices.totalMonths = validity;
                            row_dservices.monthUtil = Difference;
                            row_dservices.monthRemain = trial2;
                            row_dservices.valPStart = ValiditySS;
                            row_dservices.valPEnd = ValidityFF;

                            dservices_table.records.Add(row_dservices);
                        }
                    }
                    else if (glid == 5160 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotvsat += budget;
                            invoicetotvsat += invoiceamount;
                            torevvsat += -toRevenue;
                            fromrevvsat += -fromRevenue;
                            closingtotvsat += closingbalance;
                            balancebfvsat += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            budgettotal += budget;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += -toRevenue;
                            closeBal += closingbalance;
                            //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_vsat = new UIData();
                            row_vsat.licenseNumber = ccnum;
                            row_vsat.clientCompany = clientCompany;
                            row_vsat.invoiceID = invoiceid.ToString();
                            row_vsat.budget = formatMoney(Math.Round(budget, 2));
                            row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_vsat.thisMonthInv = invoicestat;
                            row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_vsat.totalMonths = validity;
                            row_vsat.monthUtil = Difference;
                            row_vsat.monthRemain = trial2;
                            row_vsat.valPStart = ValiditySS;
                            row_vsat.valPEnd = ValidityFF;

                            vsat_table.records.Add(row_vsat);
                        }
                    }
                    else if (glid == 5161 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotaero += budget;
                            invoicetotaero += invoiceamount;
                            torevaero += -toRevenue;
                            fromrevaero += -fromRevenue;
                            closingtotaero += closingbalance;
                            balancebfaero += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            budgettotal += budget;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += -toRevenue;
                            closeBal += closingbalance;
                            //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_aero = new UIData();
                            row_aero.licenseNumber = ccnum;
                            row_aero.clientCompany = clientCompany;
                            row_aero.invoiceID = invoiceid.ToString();
                            row_aero.budget = formatMoney(Math.Round(budget, 2));
                            row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_aero.thisMonthInv = invoicestat;
                            row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_aero.totalMonths = validity;
                            row_aero.monthUtil = Difference;
                            row_aero.monthRemain = trial2;
                            row_aero.valPStart = ValiditySS;
                            row_aero.valPEnd = ValidityFF;

                            aero_table.records.Add(row_aero);
                        }
                    }
                    else if (glid == 5162 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotmar += budget;
                            invoicetotmar += invoiceamount;
                            torevmar += -toRevenue;
                            fromrevmar += -fromRevenue;
                            closingtotmar += closingbalance;
                            balancebfmar += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            budgettotal += budget;
                            toRev += -toRevenue;
                            closeBal += closingbalance;
                            //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_marine = new UIData();
                            row_marine.licenseNumber = ccnum;
                            row_marine.clientCompany = clientCompany;
                            row_marine.invoiceID = invoiceid.ToString();
                            row_marine.budget = formatMoney(Math.Round(budget, 2));
                            row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_marine.thisMonthInv = invoicestat;
                            row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_marine.totalMonths = validity;
                            row_marine.monthUtil = Difference;
                            row_marine.monthRemain = trial2;
                            row_marine.valPStart = ValiditySS;
                            row_marine.valPEnd = ValidityFF;

                            marine_table.records.Add(row_marine);
                        }
                    }
                    else if (glid == 5163 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettottrunk += budget;
                            invoicetottrunk += invoiceamount;
                            torevtrunk += -toRevenue;
                            fromrevtrunk += -fromRevenue;
                            closingtottrunk += closingbalance;
                            balancebftrunk += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            budgettotal += budget;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += -toRevenue;
                            closeBal += closingbalance;
                            //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_trunking = new UIData();
                            row_trunking.licenseNumber = ccnum;
                            row_trunking.clientCompany = clientCompany;
                            row_trunking.invoiceID = invoiceid.ToString();
                            row_trunking.budget = formatMoney(Math.Round(budget, 2));
                            row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_trunking.thisMonthInv = invoicestat;
                            row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_trunking.totalMonths = validity;
                            row_trunking.monthUtil = Difference;
                            row_trunking.monthRemain = trial2;
                            row_trunking.valPStart = ValiditySS;
                            row_trunking.valPEnd = ValidityFF;

                            trunking_table.records.Add(row_trunking);
                        }
                    }
                    else if (glid == 5164 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotother += budget;
                            invoicetotother += invoiceamount;
                            torevother += -toRevenue;
                            fromrevother += -fromRevenue;
                            closingtotother += closingbalance;
                            balancebfother += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            budgettotal += budget;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += -toRevenue;
                            closeBal += closingbalance;
                            //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_other = new UIData();
                            row_other.licenseNumber = ccnum;
                            row_other.clientCompany = clientCompany;
                            row_other.invoiceID = invoiceid.ToString();
                            row_other.budget = formatMoney(Math.Round(budget, 2));
                            row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_other.thisMonthInv = invoicestat;
                            row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_other.totalMonths = validity;
                            row_other.monthUtil = Difference;
                            row_other.monthRemain = trial2;
                            row_other.valPStart = ValiditySS;
                            row_other.valPEnd = ValidityFF;

                            other_table.records.Add(row_other);
                        }
                    }
                    else
                    {
                        //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                        row_table = new UIData();
                        row_table.licenseNumber = ccnum;
                        row_table.clientCompany = clientCompany;
                        row_table.invoiceID = invoiceid.ToString();
                        row_table.budget = formatMoney(Math.Round(budget, 2));
                        row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                        row_table.thisMonthInv = invoicestat;
                        row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                        row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                        row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                        row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                        row_table.totalMonths = validity;
                        row_table.monthUtil = Difference;
                        row_table.monthRemain = trial2;
                        row_table.valPStart = ValiditySS;
                        row_table.valPEnd = ValidityFF;

                        table_table.records.Add(row_table);
                    }

                    if (Difference > 0 || description == "Modification" && Difference == 0 && opp != 0)
                    {
                        DataSet opstatus = new DataSet();
                        opstatus = brian_businessClass.GetOpeningBalStat(ccnum, invoiceid, defered);
                        if (IsEmpty(opstatus))
                        {
                            brian_businessClass.InsertOpBal(ccnum, closingbalance, defered, invoiceid);
                        }
                        else
                        {
                            brian_businessClass.UpdateOpBal(ccnum, closingbalance, defered, invoiceid);
                        }
                    }
                }


                while (rdr3.Read())
                {
                    ccnum = "";
                    invoiceamount = rdr3.GetDecimal(2);
                    string s = "04/01/2017";
                    string e = "03/31/2018";
                    ValidityS = Convert.ToDateTime(s);
                    ValidityF = Convert.ToDateTime(e);
                    int creditmemonum = rdr3.GetInt32(3);

                    DataSet creditmemono = new DataSet();
                    creditmemono = brian_businessClass.GetCreditMemoDisplayNo(creditmemonum);

                    if (!IsEmpty(creditmemono))
                    {
                        DataRow dr = creditmemono.Tables[0].Rows[0];
                        creditmemonum = Convert.ToInt32(dr.ItemArray.GetValue(0));
                    }

                    glid = 0;
                    invoiceid = rdr3.GetInt32(0);
                    DataSet clientidds = new DataSet();
                    clientidds = brian_businessClass.GetClientId(invoiceid);
                    if (!IsEmpty(clientidds))
                    {

                        DataRow dr = clientidds.Tables[0].Rows[0];
                        id = Convert.ToInt32(dr.ItemArray.GetValue(0));
                    }

                    DataSet invamountds = new DataSet();
                    invamountds = brian_businessClass.GetInvoiceAmountCredMemo(invoiceid);
                    if (!IsEmpty(invamountds))
                    {
                        DataRow dr = invamountds.Tables[0].Rows[0];
                        invoiceamount = Convert.ToDecimal(dr.ItemArray.GetValue(0));
                    }

                    DataSet clientnameds = new DataSet();
                    clientnameds = brian_businessClass.GetClientName(id);
                    if (!IsEmpty(clientnameds))
                    {
                        DataRow dr = clientnameds.Tables[0].Rows[0];
                        Company = dr.ItemArray.GetValue(0).ToString();
                        fname = dr.ItemArray.GetValue(1).ToString();
                        lname = dr.ItemArray.GetValue(2).ToString();
                        ccnum = dr.ItemArray.GetValue(3).ToString();
                    }

                    DataSet validityds = new DataSet();
                    validityds = brian_businessClass.GetValidityCM(invoiceid);
                    if (!IsEmpty(validityds))
                    {
                        DataRow dr = validityds.Tables[0].Rows[0];
                        ValidityS = Convert.ToDateTime(dr.ItemArray.GetValue(6).ToString());
                        ValidityF = Convert.ToDateTime(dr.ItemArray.GetValue(7).ToString());
                    }

                    DataSet glds = new DataSet();
                    glds = brian_businessClass.GetCreditGl(invoiceid);
                    if (!IsEmpty(glds))
                    {
                        DataRow dr = glds.Tables[0].Rows[0];
                        glid = Convert.ToInt32(dr.ItemArray.GetValue(0));
                    }

                    int Differencee = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                    decimal budget = 0;
                    int validity = 0;
                    int Difference = 0;
                    //  int validity = ((ValidityF.Year - ValidityS.Year) * 12) + ValidityF.Month - ValidityS.Month;
                    var ValiditySS = DateTime.Now.ToString("dd/MM/yyyy");

                    validity = CheckMonths(ValidityS, ValidityF);
                    if (validity == 2) validity = 3;
                    Difference = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                    ValiditySS = ValidityS.ToString("dd/MM/yyyy");

                    int trial33 = validity - Differencee;
                    Differencee = Differencee + 1;
                    DataSet df = new DataSet();
                    df = brian_businessClass.GetBudget(ccnum, invoiceid);
                    if (!IsEmpty(df) && Differencee > 1)
                    {
                        DataRow dr = df.Tables[0].Rows[0];
                        budget = Convert.ToDecimal(dr.ItemArray.GetValue(1));
                    }

                    if (!IsEmpty(df) && Differencee == 1)
                    {

                        DataRow dr = df.Tables[0].Rows[0];
                        DateTime ne = Convert.ToDateTime(dr.ItemArray.GetValue(2));
                        if (ne.Month >= ValidityS.Month /*&& ne.Year >= ValidityS.Year*/)
                        {
                            budget = Convert.ToDecimal(dr.ItemArray.GetValue(1));
                        }
                    }

                    DataSet budg = brian_businessClass.GetCustBudget(ccnum, invoiceid);
                    if (IsEmpty(budg))
                    {
                        brian_businessClass.InsertBudgetInfo(ccnum, budget, invoiceid);
                    }

                    var ValidityFF = ValidityF.ToString("dd/MM/yyyy");
                    string invoicestat = " ";
                    string clientCompany = " ";
                    decimal openingbalance = 0;
                    decimal closingbalance = 0;
                    decimal toRevenue = 0;
                    decimal fromRevenue = 0;

                    if (Company == null || Company == "")
                    {
                        clientCompany = fname + " " + lname;
                    }
                    else
                    {
                        clientCompany = Company;
                        clientCompany = System.Net.WebUtility.HtmlDecode(clientCompany);
                    }

                    int trial = ((defered.Year - ValidityS.Year) * 12) + validity - Difference;
                    int trial2 = validity - Difference;
                    decimal opp = 0;
                    string op = " ";
                    DataSet opset = new DataSet();
                    {
                        if (Difference > 0 && ValidityS.Month != defered.Month || ValidityS.Month == defered.Month && ValidityF.Year != defered.Year && ValidityS.Year != defered.Year || ValidityF.Month == defered.Month)
                        {
                            if (defered.Month - 1 == 0)
                            {
                                opset = brian_businessClass.GetOpeningBalanceForMonth(ccnum, 12, (defered.Year - 1), invoiceid);
                            }

                            else
                                opset = brian_businessClass.GetOpeningBalanceForMonth(ccnum, (defered.Month - 1), defered.Year, invoiceid);
                            if (!IsEmpty(opset))
                            {
                                DataRow dr = opset.Tables[0].Rows[0];


                                decimal.TryParse(dr["openingbalance"].ToString(), out opp);
                            }
                            else
                                opp = 0;
                        }

                        if (opp == 0)
                        {
                            opp = 0;
                        }
                        else
                        {
                            opp = opp;
                        }
                    }

                    if (Difference >= 0 && trial2 >= 0 && opp != 0)
                    {
                        if (ValidityS.Day <= 15)
                        {
                            Difference = Difference + 1;
                        }

                        trial2 = validity - Difference;
                        openingbalance = opp;
                        toRevenue = 0;
                        closingbalance = 0;
                        fromRevenue = opp;
                        invoicestat = "No";
                    }

                    if (glid == 5156 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            invoicetotcell += invoiceamount;
                            torevcell += toRevenue;
                            fromremcell += -fromRevenue;
                            closingtotcell += closingbalance;
                            balancebfcell += openingbalance;
                            budgettotcell += budget;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += toRevenue;
                            budgettotal += budget;
                            closeBal += closingbalance;
                            //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_cell = new UIData();
                            row_cell.licenseNumber = ccnum;
                            row_cell.clientCompany = clientCompany;
                            row_cell.invoiceID = invoiceid.ToString();
                            row_cell.budget = formatMoney(Math.Round(budget, 2));
                            row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_cell.thisMonthInv = invoicestat;
                            row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_cell.totalMonths = validity;
                            row_cell.monthUtil = Difference;
                            row_cell.monthRemain = trial2;
                            row_cell.valPStart = ValiditySS;
                            row_cell.valPEnd = ValidityFF;

                            cell_table.records.Add(row_cell);
                        }
                    }

                    else if (glid == 5157 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotbb += budget;
                            invoicetotbb += invoiceamount;
                            torevbb += toRevenue;
                            fromrembb += -fromRevenue;
                            closingtotbb += closingbalance;
                            balancebfbb += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += toRevenue;
                            budgettotal += budget;
                            closeBal += closingbalance;
                            //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_bbrand = new UIData();
                            row_bbrand.licenseNumber = ccnum;
                            row_bbrand.clientCompany = clientCompany;
                            row_bbrand.invoiceID = invoiceid.ToString();
                            row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                            row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_bbrand.thisMonthInv = invoicestat;
                            row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_bbrand.totalMonths = validity;
                            row_bbrand.monthUtil = Difference;
                            row_bbrand.monthRemain = trial2;
                            row_bbrand.valPStart = ValiditySS;
                            row_bbrand.valPEnd = ValidityFF;

                            bbrand_table.records.Add(row_bbrand);
                        }
                    }

                    else if (glid == 5158 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotmic += budget;
                            invoicetotmicro += invoiceamount;
                            torevmicro += toRevenue;
                            fromrevmicro += -fromRevenue;
                            closingtotmicro += closingbalance;
                            balancebfmicro += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += toRevenue;
                            budgettotal += budget;
                            closeBal += closingbalance;
                            //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_micro = new UIData();
                            row_micro.licenseNumber = ccnum;
                            row_micro.clientCompany = clientCompany;
                            row_micro.invoiceID = invoiceid.ToString();
                            row_micro.budget = formatMoney(Math.Round(budget, 2));
                            row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_micro.thisMonthInv = invoicestat;
                            row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_micro.totalMonths = validity;
                            row_micro.monthUtil = Difference;
                            row_micro.monthRemain = trial2;
                            row_micro.valPStart = ValiditySS;
                            row_micro.valPEnd = ValidityFF;

                            micro_table.records.Add(row_micro);
                        }
                    }

                    else if (glid == 5159 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotds += budget;
                            invoicetotds += invoiceamount;
                            torevds += toRevenue;
                            fromrevds += -fromRevenue;
                            closingtotds += closingbalance;
                            balancebfds += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += toRevenue;
                            budgettotal += budget;
                            closeBal += closingbalance;
                            //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_dservices = new UIData();
                            row_dservices.licenseNumber = ccnum;
                            row_dservices.clientCompany = clientCompany;
                            row_dservices.invoiceID = invoiceid.ToString();
                            row_dservices.budget = formatMoney(Math.Round(budget, 2));
                            row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_dservices.thisMonthInv = invoicestat;
                            row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_dservices.totalMonths = validity;
                            row_dservices.monthUtil = Difference;
                            row_dservices.monthRemain = trial2;
                            row_dservices.valPStart = ValiditySS;
                            row_dservices.valPEnd = ValidityFF;

                            dservices_table.records.Add(row_dservices);
                        }
                    }

                    else if (glid == 5160 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotvsat += budget;
                            invoicetotvsat += invoiceamount;
                            torevvsat += toRevenue;
                            fromrevvsat += -fromRevenue;
                            closingtotvsat += closingbalance;
                            balancebfvsat += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            budgettotal += budget;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += toRevenue;
                            closeBal += closingbalance;
                            //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_vsat = new UIData();
                            row_vsat.licenseNumber = ccnum;
                            row_vsat.clientCompany = clientCompany;
                            row_vsat.invoiceID = invoiceid.ToString();
                            row_vsat.budget = formatMoney(Math.Round(budget, 2));
                            row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_vsat.thisMonthInv = invoicestat;
                            row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_vsat.totalMonths = validity;
                            row_vsat.monthUtil = Difference;
                            row_vsat.monthRemain = trial2;
                            row_vsat.valPStart = ValiditySS;
                            row_vsat.valPEnd = ValidityFF;

                            vsat_table.records.Add(row_vsat);
                        }
                    }

                    else if (glid == 5161 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotaero += budget;
                            invoicetotaero += invoiceamount;
                            torevaero += toRevenue;
                            fromrevaero += -fromRevenue;
                            closingtotaero += closingbalance;
                            balancebfaero += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            budgettotal += budget;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += toRevenue;
                            closeBal += closingbalance;
                            //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_aero = new UIData();
                            row_aero.licenseNumber = ccnum;
                            row_aero.clientCompany = clientCompany;
                            row_aero.invoiceID = invoiceid.ToString();
                            row_aero.budget = formatMoney(Math.Round(budget, 2));
                            row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_aero.thisMonthInv = invoicestat;
                            row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_aero.totalMonths = validity;
                            row_aero.monthUtil = Difference;
                            row_aero.monthRemain = trial2;
                            row_aero.valPStart = ValiditySS;
                            row_aero.valPEnd = ValidityFF;

                            aero_table.records.Add(row_aero);
                        }
                    }


                    else if (glid == 5162 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotmar += budget;
                            invoicetotmar += invoiceamount;
                            torevmar += toRevenue;
                            fromrevmar += -fromRevenue;
                            closingtotmar += closingbalance;
                            balancebfmar += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            budgettotal += budget;
                            toRev += toRevenue;
                            closeBal += closingbalance;
                            //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_marine = new UIData();
                            row_marine.licenseNumber = ccnum;
                            row_marine.clientCompany = clientCompany;
                            row_marine.invoiceID = invoiceid.ToString();
                            row_marine.budget = formatMoney(Math.Round(budget, 2));
                            row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_marine.thisMonthInv = invoicestat;
                            row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_marine.totalMonths = validity;
                            row_marine.monthUtil = Difference;
                            row_marine.monthRemain = trial2;
                            row_marine.valPStart = ValiditySS;
                            row_marine.valPEnd = ValidityFF;

                            marine_table.records.Add(row_marine);
                        }
                    }

                    else if (glid == 5163 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettottrunk += budget;
                            invoicetottrunk += invoiceamount;
                            torevtrunk += toRevenue;
                            fromrevtrunk += -fromRevenue;
                            closingtottrunk += closingbalance;
                            balancebftrunk += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            budgettotal += budget;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += toRevenue;
                            closeBal += closingbalance;
                            //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_trunking = new UIData();
                            row_trunking.licenseNumber = ccnum;
                            row_trunking.clientCompany = clientCompany;
                            row_trunking.invoiceID = invoiceid.ToString();
                            row_trunking.budget = formatMoney(Math.Round(budget, 2));
                            row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_trunking.thisMonthInv = invoicestat;
                            row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_trunking.totalMonths = validity;
                            row_trunking.monthUtil = Difference;
                            row_trunking.monthRemain = trial2;
                            row_trunking.valPStart = ValiditySS;
                            row_trunking.valPEnd = ValidityFF;

                            trunking_table.records.Add(row_trunking);
                        }
                    }

                    else if (glid == 5164 && opp != 0)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            budgettotother += budget;
                            invoicetotother += invoiceamount;
                            torevother += toRevenue;
                            fromrevother += -fromRevenue;
                            closingtotother += closingbalance;
                            balancebfother += openingbalance;
                            fromRev += -fromRevenue;
                            balancebf += opp;
                            budgettotal += budget;
                            invoiceTotalForYes += invoiceamount;
                            invoiceTotal += invoiceamount;
                            toRev += toRevenue;
                            closeBal += closingbalance;
                            //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString() + "/CN" + creditmemonum.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                            row_other = new UIData();
                            row_other.licenseNumber = ccnum;
                            row_other.clientCompany = clientCompany;
                            row_other.invoiceID = invoiceid.ToString();
                            row_other.budget = formatMoney(Math.Round(budget, 2));
                            row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_other.thisMonthInv = invoicestat;
                            row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_other.totalMonths = validity;
                            row_other.monthUtil = Difference;
                            row_other.monthRemain = trial2;
                            row_other.valPStart = ValiditySS;
                            row_other.valPEnd = ValidityFF;

                            other_table.records.Add(row_other);
                        }
                    }
                    else
                    {
                        //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(-fromRevenue, 2)), formatMoney(Math.Round(toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                        row_table = new UIData();
                        row_table.licenseNumber = ccnum;
                        row_table.clientCompany = clientCompany;
                        row_table.invoiceID = invoiceid.ToString();
                        row_table.budget = formatMoney(Math.Round(budget, 2));
                        row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                        row_table.thisMonthInv = invoicestat;
                        row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                        row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                        row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                        row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                        row_table.totalMonths = validity;
                        row_table.monthUtil = Difference;
                        row_table.monthRemain = trial2;
                        row_table.valPStart = ValiditySS;
                        row_table.valPEnd = ValidityFF;

                        table_table.records.Add(row_table);
                    }

                    if (Difference > 0 || description == "Modification" && Difference == 0 && opp != 0)
                    {
                        DataSet opstatus = new DataSet();
                        opstatus = brian_businessClass.GetOpeningBalStat(ccnum, invoiceid, defered);
                        if (IsEmpty(opstatus))
                        {
                            brian_businessClass.InsertOpBal(ccnum, closingbalance, defered, invoiceid);
                        }
                        else
                        {
                            brian_businessClass.UpdateOpBal(ccnum, closingbalance, defered, invoiceid);
                        }
                    }
                }

                while (rdr4.Read())
                {
                    ccnum = "";
                    invoiceamount = rdr4.GetDecimal(2);
                    string s = "04/01/2017";
                    string e = "03/31/2018";
                    ValidityS = Convert.ToDateTime(s);
                    ValidityF = Convert.ToDateTime(e);
                    int creditmemonum = rdr4.GetInt32(3);
                    DateTime creditmemodate = rdr4.GetDateTime(4);
                    DataSet IfIsCredMemo = brian_businessClass.GetIsInvoiceCreditMemo(invoiceid);
                    DataSet creditmemono = new DataSet();
                    creditmemono = brian_businessClass.GetCreditMemoDisplayNo(creditmemonum);

                    if (!IsEmpty(creditmemono))
                    {
                        DataRow dr = creditmemono.Tables[0].Rows[0];
                        creditmemonum = Convert.ToInt32(dr.ItemArray.GetValue(0));
                    }

                    glid = 0;
                    invoiceid = rdr4.GetInt32(0);
                    DataSet clientidds = new DataSet();
                    clientidds = brian_businessClass.GetClientId(invoiceid);

                    if (!IsEmpty(clientidds))
                    {
                        DataRow dr = clientidds.Tables[0].Rows[0];
                        id = Convert.ToInt32(dr.ItemArray.GetValue(0));
                    }

                    DataSet invamountds = new DataSet();
                    invamountds = brian_businessClass.GetInvoiceAmountCredMemo(invoiceid);
                    if (!IsEmpty(invamountds))
                    {
                        DataRow dr = invamountds.Tables[0].Rows[0];
                        invoiceamount = Convert.ToDecimal(dr.ItemArray.GetValue(0));
                    }

                    DataSet clientnameds = new DataSet();
                    clientnameds = brian_businessClass.GetClientName(id);
                    if (!IsEmpty(clientnameds))
                    {
                        DataRow dr = clientnameds.Tables[0].Rows[0];
                        Company = dr.ItemArray.GetValue(0).ToString();
                        fname = dr.ItemArray.GetValue(1).ToString();
                        lname = dr.ItemArray.GetValue(2).ToString();
                        ccnum = dr.ItemArray.GetValue(3).ToString();
                    }

                    DataSet validityds = new DataSet();
                    validityds = brian_businessClass.GetValidityCM(invoiceid);
                    if (!IsEmpty(validityds))
                    {
                        DataRow dr = validityds.Tables[0].Rows[0];
                        ValidityS = Convert.ToDateTime(dr.ItemArray.GetValue(6).ToString());
                        ValidityF = Convert.ToDateTime(dr.ItemArray.GetValue(7).ToString());

                    }

                    DataSet glds = new DataSet();
                    glds = brian_businessClass.GetCreditGl(invoiceid);
                    if (!IsEmpty(glds))
                    {
                        DataRow dr = glds.Tables[0].Rows[0];
                        glid = Convert.ToInt32(dr.ItemArray.GetValue(0));
                    }

                    int Differencee = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                    decimal budget = 0;
                    int validity = 0;
                    int Difference = 0;
                    //  int validity = ((ValidityF.Year - ValidityS.Year) * 12) + ValidityF.Month - ValidityS.Month;
                    var ValiditySS = DateTime.Now.ToString("dd/MM/yyyy");

                    validity = CheckMonths(ValidityS, ValidityF);
                    if (validity == 2) validity = 3;
                    Difference = ((defered.Year - ValidityS.Year) * 12) + defered.Month - ValidityS.Month;
                    ValiditySS = ValidityS.ToString("dd/MM/yyyy");

                    int trial33 = validity - Differencee;
                    Differencee = Differencee + 1;
                    DataSet df = new DataSet();
                    df = brian_businessClass.GetBudget(ccnum, invoiceid);

                    if (!IsEmpty(df) && Differencee > 1)
                    {
                        DataRow dr = df.Tables[0].Rows[0];
                        budget = Convert.ToDecimal(dr.ItemArray.GetValue(1));
                    }

                    if (!IsEmpty(df) && Differencee == 1)
                    {
                        DataRow dr = df.Tables[0].Rows[0];
                        DateTime ne = Convert.ToDateTime(dr.ItemArray.GetValue(2));
                        if (ne.Month >= ValidityS.Month /*&& ne.Year >= ValidityS.Year*/)
                        {
                            budget = Convert.ToDecimal(dr.ItemArray.GetValue(1));
                        }
                    }

                    DataSet budg = brian_businessClass.GetCustBudget(ccnum, invoiceid);
                    if (IsEmpty(budg))
                    {
                        brian_businessClass.InsertBudgetInfo(ccnum, budget, invoiceid);
                    }

                    var ValidityFF = ValidityF.ToString("dd/MM/yyyy");

                    string invoicestat = " ";

                    string clientCompany = " ";
                    decimal openingbalance = 0;
                    decimal closingbalance = 0;
                    decimal toRevenue = 0;
                    decimal fromRevenue = 0;



                    if (Company == null || Company == "")
                    {
                        clientCompany = fname + " " + lname;

                    }
                    else
                    {
                        clientCompany = Company;
                        clientCompany = System.Net.WebUtility.HtmlDecode(clientCompany);
                    }

                    int trial = ((defered.Year - ValidityS.Year) * 12) + validity - Difference;
                    int trial2 = validity - Difference;


                    decimal opp = 0;
                    string op = " ";
                    DataSet opset = new DataSet();


                    {
                        if (Difference > 0 && ValidityS.Month != defered.Month || ValidityS.Month == defered.Month && ValidityF.Year != defered.Year && ValidityS.Year != defered.Year || ValidityF.Month == defered.Month)
                        {
                            if (defered.Month - 1 == 0)
                            {
                                opset = brian_businessClass.GetOpeningBalanceForMonth(ccnum, 12, (defered.Year - 1), invoiceid);
                            }

                            else
                                opset = brian_businessClass.GetOpeningBalanceForMonth(ccnum, (defered.Month - 1), defered.Year, invoiceid);
                            if (!IsEmpty(opset))
                            {
                                DataRow dr = opset.Tables[0].Rows[0];


                                decimal.TryParse(dr["openingbalance"].ToString(), out opp);


                            }
                            else
                                opp = 0;
                        }

                        if (opp == 0)
                        {
                            opp = 0;
                        }
                        else
                        {
                            opp = opp;
                        }
                    }

                    if (ValidityS.Month == defered.Month && ValidityS.Year == defered.Year)
                    {
                        if (ValidityS.Day <= 15)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                Difference = Difference + 1;
                                trial2 = validity - Difference;
                                openingbalance = 0;
                                toRevenue = invoiceamount * 1 / validity;
                                closingbalance = invoiceamount - toRevenue;

                                fromRevenue = invoiceamount;
                                invoicestat = "Yes";
                            }

                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += openingbalance;
                                    budgettotcell += budget;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }

                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }

                            else if (glid == 5160)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }

                            else if (glid == 5161)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }


                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    budgettotal += budget;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);
                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }

                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += openingbalance;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }
                        }

                        if (ValidityS.Day > 15)
                        {

                            if (Difference >= 0 && trial2 >= 0)
                            {
                                openingbalance = 0;
                                toRevenue = 0;
                                fromRevenue = invoiceamount;
                                closingbalance = invoiceamount;
                                invoicestat = "Yes";
                                opp = 0;

                                if (Difference > 1 && IsEmpty(opset))
                                {
                                    toRevenue = toRevenue * Difference;
                                }
                            }
                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotcell += budget;
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    budgettotal += budget;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }

                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }


                            else if (glid == 5160)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }


                            else if (glid == 5161)
                            {

                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }


                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);
                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }


                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(openingbalance, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }
                        }
                    }

                    else if (ValidityS.Month == (defered.Month - 1) && ValidityS.Day > 15 && ValidityS.Year == defered.Year)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            openingbalance = 0;
                            toRevenue = invoiceamount * 1 / validity;
                            //changed
                            fromRevenue = 0;
                            closingbalance = invoiceamount - toRevenue;
                            invoicestat = "No";


                            //if (!IsEmpty(opset))
                            //{
                            //    opp = opp;
                            //}
                            //else
                            //    opp = 0;

                            opp = invoiceamount;

                            if (Difference > 1 && IsEmpty(opset))
                            {
                                toRevenue = toRevenue * Difference;
                            }

                        }
                        if (glid == 5156)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotcell += budget;
                                invoicetotcell += invoiceamount;
                                torevcell += toRevenue;
                                fromremcell += fromRevenue;
                                closingtotcell += closingbalance;
                                balancebfcell += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                budgettotal += budget;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_cell = new UIData();
                                row_cell.licenseNumber = ccnum;
                                row_cell.clientCompany = clientCompany;
                                row_cell.invoiceID = invoiceid.ToString();
                                row_cell.budget = formatMoney(Math.Round(budget, 2));
                                row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_cell.thisMonthInv = invoicestat;
                                row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_cell.totalMonths = validity;
                                row_cell.monthUtil = Difference;
                                row_cell.monthRemain = trial2;
                                row_cell.valPStart = ValiditySS;
                                row_cell.valPEnd = ValidityFF;

                                cell_table.records.Add(row_cell);
                            }
                        }

                        else if (glid == 5157)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotbb += budget;
                                invoicetotbb += invoiceamount;
                                torevbb += toRevenue;
                                fromrembb += fromRevenue;
                                closingtotbb += closingbalance;
                                balancebfbb += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_bbrand = new UIData();
                                row_bbrand.licenseNumber = ccnum;
                                row_bbrand.clientCompany = clientCompany;
                                row_bbrand.invoiceID = invoiceid.ToString();
                                row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_bbrand.thisMonthInv = invoicestat;
                                row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_bbrand.totalMonths = validity;
                                row_bbrand.monthUtil = Difference;
                                row_bbrand.monthRemain = trial2;
                                row_bbrand.valPStart = ValiditySS;
                                row_bbrand.valPEnd = ValidityFF;

                                bbrand_table.records.Add(row_bbrand);
                            }
                        }

                        else if (glid == 5158)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmic += budget;
                                invoicetotmicro += invoiceamount;
                                torevmicro += toRevenue;
                                fromrevmicro += fromRevenue;
                                closingtotmicro += closingbalance;
                                balancebfmicro += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_micro = new UIData();
                                row_micro.licenseNumber = ccnum;
                                row_micro.clientCompany = clientCompany;
                                row_micro.invoiceID = invoiceid.ToString();
                                row_micro.budget = formatMoney(Math.Round(budget, 2));
                                row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_micro.thisMonthInv = invoicestat;
                                row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_micro.totalMonths = validity;
                                row_micro.monthUtil = Difference;
                                row_micro.monthRemain = trial2;
                                row_micro.valPStart = ValiditySS;
                                row_micro.valPEnd = ValidityFF;

                                micro_table.records.Add(row_micro);
                            }
                        }

                        else if (glid == 5159)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotds += budget;
                                invoicetotds += invoiceamount;
                                torevds += toRevenue;
                                fromrevds += fromRevenue;
                                closingtotds += closingbalance;
                                balancebfds += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_dservices = new UIData();
                                row_dservices.licenseNumber = ccnum;
                                row_dservices.clientCompany = clientCompany;
                                row_dservices.invoiceID = invoiceid.ToString();
                                row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_dservices.thisMonthInv = invoicestat;
                                row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_dservices.totalMonths = validity;
                                row_dservices.monthUtil = Difference;
                                row_dservices.monthRemain = trial2;
                                row_dservices.valPStart = ValiditySS;
                                row_dservices.valPEnd = ValidityFF;

                                dservices_table.records.Add(row_dservices);
                            }
                        }


                        else if (glid == 5160)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotvsat += budget;
                                invoicetotvsat += invoiceamount;
                                torevvsat += toRevenue;
                                fromrevvsat += fromRevenue;
                                closingtotvsat += closingbalance;
                                balancebfvsat += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_vsat = new UIData();
                                row_vsat.licenseNumber = ccnum;
                                row_vsat.clientCompany = clientCompany;
                                row_vsat.invoiceID = invoiceid.ToString();
                                row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_vsat.thisMonthInv = invoicestat;
                                row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_vsat.totalMonths = validity;
                                row_vsat.monthUtil = Difference;
                                row_vsat.monthRemain = trial2;
                                row_vsat.valPStart = ValiditySS;
                                row_vsat.valPEnd = ValidityFF;

                                vsat_table.records.Add(row_vsat);
                            }
                        }


                        else if (glid == 5161)
                        {

                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotaero += budget;
                                invoicetotaero += invoiceamount;
                                torevaero += toRevenue;
                                fromrevaero += fromRevenue;
                                closingtotaero += closingbalance;
                                balancebfaero += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                budgettotal += budget;
                                closeBal += closingbalance;
                                //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                            }
                        }


                        else if (glid == 5162)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmar += budget;
                                invoicetotmar += invoiceamount;
                                torevmar += toRevenue;
                                fromrevmar += fromRevenue;
                                closingtotmar += closingbalance;
                                balancebfmar += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_marine = new UIData();
                                row_marine.licenseNumber = ccnum;
                                row_marine.clientCompany = clientCompany;
                                row_marine.invoiceID = invoiceid.ToString();
                                row_marine.budget = formatMoney(Math.Round(budget, 2));
                                row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_marine.thisMonthInv = invoicestat;
                                row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_marine.totalMonths = validity;
                                row_marine.monthUtil = Difference;
                                row_marine.monthRemain = trial2;
                                row_marine.valPStart = ValiditySS;
                                row_marine.valPEnd = ValidityFF;

                                marine_table.records.Add(row_marine);
                            }
                        }

                        else if (glid == 5163)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettottrunk += budget;
                                invoicetottrunk += invoiceamount;
                                torevtrunk += toRevenue;
                                fromrevtrunk += fromRevenue;
                                closingtottrunk += closingbalance;
                                balancebftrunk += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_trunking = new UIData();
                                row_trunking.licenseNumber = ccnum;
                                row_trunking.clientCompany = clientCompany;
                                row_trunking.invoiceID = invoiceid.ToString();
                                row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_trunking.thisMonthInv = invoicestat;
                                row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_trunking.totalMonths = validity;
                                row_trunking.monthUtil = Difference;
                                row_trunking.monthRemain = trial2;
                                row_trunking.valPStart = ValiditySS;
                                row_trunking.valPEnd = ValidityFF;

                                trunking_table.records.Add(row_trunking);
                            }
                        }


                        else if (glid == 5164)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotother += budget;
                                invoicetotother += invoiceamount;
                                torevother += toRevenue;
                                fromrevother += fromRevenue;
                                closingtotother += closingbalance;
                                balancebfother += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_other = new UIData();
                                row_other.licenseNumber = ccnum;
                                row_other.clientCompany = clientCompany;
                                row_other.invoiceID = invoiceid.ToString();
                                row_other.budget = formatMoney(Math.Round(budget, 2));
                                row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_other.thisMonthInv = invoicestat;
                                row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_other.totalMonths = validity;
                                row_other.monthUtil = Difference;
                                row_other.monthRemain = trial2;
                                row_other.valPStart = ValiditySS;
                                row_other.valPEnd = ValidityFF;

                                other_table.records.Add(row_other);
                            }
                        }
                        else
                        {
                            //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                            row_table = new UIData();
                            row_table.licenseNumber = ccnum;
                            row_table.clientCompany = clientCompany;
                            row_table.invoiceID = invoiceid.ToString();
                            row_table.budget = formatMoney(Math.Round(budget, 2));
                            row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_table.thisMonthInv = invoicestat;
                            row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_table.totalMonths = validity;
                            row_table.monthUtil = Difference;
                            row_table.monthRemain = trial2;
                            row_table.valPStart = ValiditySS;
                            row_table.valPEnd = ValidityFF;

                            table_table.records.Add(row_table);
                        }
                    }

                    else
                                if (ValidityS.Month == defered.Month && ValidityF.Year != defered.Year && ValidityS.Day <= 15 && ValidityS.Year != defered.Year)
                    {

                        if (Difference >= 0 && trial2 >= 0)
                        {

                            Difference = Difference + 1;
                            trial2 = validity - Difference;
                            toRevenue = invoiceamount * 1 / validity;

                            decimal amountalreadypaid = toRevenue * (Difference - 1);
                            openingbalance = invoiceamount - amountalreadypaid;
                            fromRevenue = 0;
                            closingbalance = invoiceamount - (amountalreadypaid + toRevenue);

                            invoicestat = "No";

                            if (!IsEmpty(opset))
                            {
                                opp = opp;
                            }
                            else
                                opp = 0;

                            if (Difference > 1 && IsEmpty(opset))
                            {
                                toRevenue = toRevenue * Difference;
                                fromRevenue = invoiceamount;
                            }
                        }
                        if (glid == 5156)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotcell += budget;
                                invoicetotcell += invoiceamount;
                                torevcell += toRevenue;
                                fromremcell += fromRevenue;
                                closingtotcell += closingbalance;
                                balancebfcell += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                budgettotal += budget;
                                closeBal += closingbalance;
                                //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_cell = new UIData();
                                row_cell.licenseNumber = ccnum;
                                row_cell.clientCompany = clientCompany;
                                row_cell.invoiceID = invoiceid.ToString();
                                row_cell.budget = formatMoney(Math.Round(budget, 2));
                                row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_cell.thisMonthInv = invoicestat;
                                row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_cell.totalMonths = validity;
                                row_cell.monthUtil = Difference;
                                row_cell.monthRemain = trial2;
                                row_cell.valPStart = ValiditySS;
                                row_cell.valPEnd = ValidityFF;

                                cell_table.records.Add(row_cell);
                            }
                        }

                        else if (glid == 5157)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotbb += budget;
                                invoicetotbb += invoiceamount;
                                torevbb += toRevenue;
                                fromrembb += fromRevenue;
                                closingtotbb += closingbalance;
                                balancebfbb += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                budgettotal += budget;
                                closeBal += closingbalance;
                                //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_bbrand = new UIData();
                                row_bbrand.licenseNumber = ccnum;
                                row_bbrand.clientCompany = clientCompany;
                                row_bbrand.invoiceID = invoiceid.ToString();
                                row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_bbrand.thisMonthInv = invoicestat;
                                row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_bbrand.totalMonths = validity;
                                row_bbrand.monthUtil = Difference;
                                row_bbrand.monthRemain = trial2;
                                row_bbrand.valPStart = ValiditySS;
                                row_bbrand.valPEnd = ValidityFF;

                                bbrand_table.records.Add(row_bbrand);
                            }
                        }
                        else if (glid == 5158)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmic += budget;
                                invoicetotmicro += invoiceamount;
                                torevmicro += toRevenue;
                                fromrevmicro += fromRevenue;
                                closingtotmicro += closingbalance;
                                balancebfmicro += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                budgettotal += budget;
                                closeBal += closingbalance;
                                //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_micro = new UIData();
                                row_micro.licenseNumber = ccnum;
                                row_micro.clientCompany = clientCompany;
                                row_micro.invoiceID = invoiceid.ToString();
                                row_micro.budget = formatMoney(Math.Round(budget, 2));
                                row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_micro.thisMonthInv = invoicestat;
                                row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_micro.totalMonths = validity;
                                row_micro.monthUtil = Difference;
                                row_micro.monthRemain = trial2;
                                row_micro.valPStart = ValiditySS;
                                row_micro.valPEnd = ValidityFF;

                                micro_table.records.Add(row_micro);
                            }
                        }

                        else if (glid == 5159)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotds += budget;
                                invoicetotds += invoiceamount;
                                torevds += toRevenue;
                                fromrevds += fromRevenue;
                                closingtotds += closingbalance;
                                balancebfds += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_dservices = new UIData();
                                row_dservices.licenseNumber = ccnum;
                                row_dservices.clientCompany = clientCompany;
                                row_dservices.invoiceID = invoiceid.ToString();
                                row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_dservices.thisMonthInv = invoicestat;
                                row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_dservices.totalMonths = validity;
                                row_dservices.monthUtil = Difference;
                                row_dservices.monthRemain = trial2;
                                row_dservices.valPStart = ValiditySS;
                                row_dservices.valPEnd = ValidityFF;

                                dservices_table.records.Add(row_dservices);
                            }
                        }

                        else if (glid == 5160)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotvsat += budget;
                                invoicetotvsat += invoiceamount;
                                torevvsat += toRevenue;
                                fromrevvsat += fromRevenue;
                                closingtotvsat += closingbalance;
                                balancebfvsat += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_vsat = new UIData();
                                row_vsat.licenseNumber = ccnum;
                                row_vsat.clientCompany = clientCompany;
                                row_vsat.invoiceID = invoiceid.ToString();
                                row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_vsat.thisMonthInv = invoicestat;
                                row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_vsat.totalMonths = validity;
                                row_vsat.monthUtil = Difference;
                                row_vsat.monthRemain = trial2;
                                row_vsat.valPStart = ValiditySS;
                                row_vsat.valPEnd = ValidityFF;

                                vsat_table.records.Add(row_vsat);
                            }
                        }

                        else if (glid == 5161)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotaero += budget;
                                invoicetotaero += invoiceamount;
                                torevaero += toRevenue;
                                fromrevaero += fromRevenue;
                                closingtotaero += closingbalance;
                                balancebfaero += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_aero = new UIData();
                                row_aero.licenseNumber = ccnum;
                                row_aero.clientCompany = clientCompany;
                                row_aero.invoiceID = invoiceid.ToString();
                                row_aero.budget = formatMoney(Math.Round(budget, 2));
                                row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_aero.thisMonthInv = invoicestat;
                                row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_aero.totalMonths = validity;
                                row_aero.monthUtil = Difference;
                                row_aero.monthRemain = trial2;
                                row_aero.valPStart = ValiditySS;
                                row_aero.valPEnd = ValidityFF;

                                aero_table.records.Add(row_aero);
                            }
                        }

                        else if (glid == 5162)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmar += budget;
                                invoicetotmar += invoiceamount;
                                torevmar += toRevenue;
                                fromrevmar += fromRevenue;
                                closingtotmar += closingbalance;
                                balancebfmar += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_marine = new UIData();
                                row_marine.licenseNumber = ccnum;
                                row_marine.clientCompany = clientCompany;
                                row_marine.invoiceID = invoiceid.ToString();
                                row_marine.budget = formatMoney(Math.Round(budget, 2));
                                row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_marine.thisMonthInv = invoicestat;
                                row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_marine.totalMonths = validity;
                                row_marine.monthUtil = Difference;
                                row_marine.monthRemain = trial2;
                                row_marine.valPStart = ValiditySS;
                                row_marine.valPEnd = ValidityFF;

                                marine_table.records.Add(row_marine);
                            }
                        }

                        else if (glid == 5163)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettottrunk += budget;
                                invoicetottrunk += invoiceamount;
                                torevtrunk += toRevenue;
                                fromrevtrunk += fromRevenue;
                                closingtottrunk += closingbalance;
                                balancebftrunk += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_trunking = new UIData();
                                row_trunking.licenseNumber = ccnum;
                                row_trunking.clientCompany = clientCompany;
                                row_trunking.invoiceID = invoiceid.ToString();
                                row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_trunking.thisMonthInv = invoicestat;
                                row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_trunking.totalMonths = validity;
                                row_trunking.monthUtil = Difference;
                                row_trunking.monthRemain = trial2;
                                row_trunking.valPStart = ValiditySS;
                                row_trunking.valPEnd = ValidityFF;

                                trunking_table.records.Add(row_trunking);
                            }
                        }

                        else if (glid == 5164)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotother += budget;
                                invoicetotother += invoiceamount;
                                torevother += toRevenue;
                                fromrevother += fromRevenue;
                                closingtotother += closingbalance;
                                balancebfother += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_other = new UIData();
                                row_other.licenseNumber = ccnum;
                                row_other.clientCompany = clientCompany;
                                row_other.invoiceID = invoiceid.ToString();
                                row_other.budget = formatMoney(Math.Round(budget, 2));
                                row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_other.thisMonthInv = invoicestat;
                                row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_other.totalMonths = validity;
                                row_other.monthUtil = Difference;
                                row_other.monthRemain = trial2;
                                row_other.valPStart = ValiditySS;
                                row_other.valPEnd = ValidityFF;

                                other_table.records.Add(row_other);
                            }
                        }
                        else
                        {
                            //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                            row_table = new UIData();
                            row_table.licenseNumber = ccnum;
                            row_table.clientCompany = clientCompany;
                            row_table.invoiceID = invoiceid.ToString();
                            row_table.budget = formatMoney(Math.Round(budget, 2));
                            row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_table.thisMonthInv = invoicestat;
                            row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_table.totalMonths = validity;
                            row_table.monthUtil = Difference;
                            row_table.monthRemain = trial2;
                            row_table.valPStart = ValiditySS;
                            row_table.valPEnd = ValidityFF;

                            table_table.records.Add(row_table);
                        }
                    }

                    else if (ValidityS.Month == defered.Month && ValidityF.Year != defered.Year && ValidityS.Day > 15 && ValidityS.Year != defered.Year)
                    {
                        if (Difference >= 0 && trial2 >= 0)
                        {
                            toRevenue = invoiceamount * 1 / validity;

                            decimal amountalreadypaid = toRevenue * (Difference - 1);
                            openingbalance = invoiceamount - amountalreadypaid;
                            fromRevenue = 0;
                            closingbalance = invoiceamount - (amountalreadypaid + toRevenue);

                            invoicestat = "No";


                            if (!IsEmpty(opset))
                            {
                                opp = opp;

                            }
                            else
                                opp = 0;



                            if (Difference > 1 && IsEmpty(opset))
                            {
                                toRevenue = toRevenue * Difference;
                                fromRevenue = invoiceamount;
                            }
                        }
                        if (glid == 5156)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotcell += budget;
                                invoicetotcell += invoiceamount;
                                torevcell += toRevenue;
                                fromremcell += fromRevenue;
                                closingtotcell += closingbalance;
                                balancebfcell += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_cell = new UIData();
                                row_cell.licenseNumber = ccnum;
                                row_cell.clientCompany = clientCompany;
                                row_cell.invoiceID = invoiceid.ToString();
                                row_cell.budget = formatMoney(Math.Round(budget, 2));
                                row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_cell.thisMonthInv = invoicestat;
                                row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_cell.totalMonths = validity;
                                row_cell.monthUtil = Difference;
                                row_cell.monthRemain = trial2;
                                row_cell.valPStart = ValiditySS;
                                row_cell.valPEnd = ValidityFF;

                                cell_table.records.Add(row_cell);
                            }
                        }

                        else if (glid == 5157)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotbb += budget;
                                invoicetotbb += invoiceamount;
                                torevbb += toRevenue;
                                fromrembb += fromRevenue;
                                closingtotbb += closingbalance;
                                balancebfbb += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_bbrand = new UIData();
                                row_bbrand.licenseNumber = ccnum;
                                row_bbrand.clientCompany = clientCompany;
                                row_bbrand.invoiceID = invoiceid.ToString();
                                row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_bbrand.thisMonthInv = invoicestat;
                                row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_bbrand.totalMonths = validity;
                                row_bbrand.monthUtil = Difference;
                                row_bbrand.monthRemain = trial2;
                                row_bbrand.valPStart = ValiditySS;
                                row_bbrand.valPEnd = ValidityFF;

                                bbrand_table.records.Add(row_bbrand);
                            }
                        }
                        else if (glid == 5158)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmic += budget;
                                invoicetotmicro += invoiceamount;
                                torevmicro += toRevenue;
                                fromrevmicro += fromRevenue;
                                closingtotmicro += closingbalance;
                                balancebfmicro += opp;
                                fromRev += fromRevenue;
                                balancebf += opp;
                                budgettotal += budget;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_micro = new UIData();
                                row_micro.licenseNumber = ccnum;
                                row_micro.clientCompany = clientCompany;
                                row_micro.invoiceID = invoiceid.ToString();
                                row_micro.budget = formatMoney(Math.Round(budget, 2));
                                row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_micro.thisMonthInv = invoicestat;
                                row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_micro.totalMonths = validity;
                                row_micro.monthUtil = Difference;
                                row_micro.monthRemain = trial2;
                                row_micro.valPStart = ValiditySS;
                                row_micro.valPEnd = ValidityFF;

                                micro_table.records.Add(row_micro);
                            }
                        }

                        else if (glid == 5159)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotds += budget;
                                invoicetotds += invoiceamount;
                                torevds += toRevenue;
                                fromrevds += fromRevenue;
                                closingtotds += closingbalance;
                                balancebfds += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_dservices = new UIData();
                                row_dservices.licenseNumber = ccnum;
                                row_dservices.clientCompany = clientCompany;
                                row_dservices.invoiceID = invoiceid.ToString();
                                row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_dservices.thisMonthInv = invoicestat;
                                row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_dservices.totalMonths = validity;
                                row_dservices.monthUtil = Difference;
                                row_dservices.monthRemain = trial2;
                                row_dservices.valPStart = ValiditySS;
                                row_dservices.valPEnd = ValidityFF;

                                dservices_table.records.Add(row_dservices);
                            }
                        }

                        else if (glid == 5160)
                        {

                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotvsat += budget;
                                invoicetotvsat += invoiceamount;
                                torevvsat += toRevenue;
                                fromrevvsat += fromRevenue;
                                closingtotvsat += closingbalance;
                                balancebfvsat += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_vsat = new UIData();
                                row_vsat.licenseNumber = ccnum;
                                row_vsat.clientCompany = clientCompany;
                                row_vsat.invoiceID = invoiceid.ToString();
                                row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_vsat.thisMonthInv = invoicestat;
                                row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_vsat.totalMonths = validity;
                                row_vsat.monthUtil = Difference;
                                row_vsat.monthRemain = trial2;
                                row_vsat.valPStart = ValiditySS;
                                row_vsat.valPEnd = ValidityFF;

                                vsat_table.records.Add(row_vsat);
                            }
                        }

                        else if (glid == 5161)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotaero += budget;
                                invoicetotaero += invoiceamount;
                                torevaero += toRevenue;
                                fromrevaero += fromRevenue;
                                closingtotaero += closingbalance;
                                balancebfaero += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;

                                //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_aero = new UIData();
                                row_aero.licenseNumber = ccnum;
                                row_aero.clientCompany = clientCompany;
                                row_aero.invoiceID = invoiceid.ToString();
                                row_aero.budget = formatMoney(Math.Round(budget, 2));
                                row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_aero.thisMonthInv = invoicestat;
                                row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_aero.totalMonths = validity;
                                row_aero.monthUtil = Difference;
                                row_aero.monthRemain = trial2;
                                row_aero.valPStart = ValiditySS;
                                row_aero.valPEnd = ValidityFF;

                                aero_table.records.Add(row_aero);
                            }
                        }

                        else if (glid == 5162)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotmar += budget;
                                invoicetotmar += invoiceamount;
                                torevmar += toRevenue;
                                fromrevmar += fromRevenue;
                                closingtotmar += closingbalance;
                                balancebfmar += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_marine = new UIData();
                                row_marine.licenseNumber = ccnum;
                                row_marine.clientCompany = clientCompany;
                                row_marine.invoiceID = invoiceid.ToString();
                                row_marine.budget = formatMoney(Math.Round(budget, 2));
                                row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_marine.thisMonthInv = invoicestat;
                                row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_marine.totalMonths = validity;
                                row_marine.monthUtil = Difference;
                                row_marine.monthRemain = trial2;
                                row_marine.valPStart = ValiditySS;
                                row_marine.valPEnd = ValidityFF;

                                marine_table.records.Add(row_marine);
                            }
                        }

                        else if (glid == 5163)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettottrunk += budget;
                                invoicetottrunk += invoiceamount;
                                torevtrunk += toRevenue;
                                fromrevtrunk += fromRevenue;
                                closingtottrunk += closingbalance;
                                balancebftrunk += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_trunking = new UIData();
                                row_trunking.licenseNumber = ccnum;
                                row_trunking.clientCompany = clientCompany;
                                row_trunking.invoiceID = invoiceid.ToString();
                                row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_trunking.thisMonthInv = invoicestat;
                                row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_trunking.totalMonths = validity;
                                row_trunking.monthUtil = Difference;
                                row_trunking.monthRemain = trial2;
                                row_trunking.valPStart = ValiditySS;
                                row_trunking.valPEnd = ValidityFF;

                                trunking_table.records.Add(row_trunking);
                            }
                        }

                        else if (glid == 5164)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                budgettotother += budget;
                                invoicetotother += invoiceamount;
                                torevother += toRevenue;
                                fromrevother += fromRevenue;
                                closingtotother += closingbalance;
                                balancebfother += opp;
                                fromRev += fromRevenue;
                                budgettotal += budget;
                                balancebf += opp;
                                invoiceTotalForYes += invoiceamount;
                                invoiceTotal += invoiceamount;
                                toRev += toRevenue;
                                closeBal += closingbalance;
                                //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                row_other = new UIData();
                                row_other.licenseNumber = ccnum;
                                row_other.clientCompany = clientCompany;
                                row_other.invoiceID = invoiceid.ToString();
                                row_other.budget = formatMoney(Math.Round(budget, 2));
                                row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_other.thisMonthInv = invoicestat;
                                row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_other.totalMonths = validity;
                                row_other.monthUtil = Difference;
                                row_other.monthRemain = trial2;
                                row_other.valPStart = ValiditySS;
                                row_other.valPEnd = ValidityFF;

                                other_table.records.Add(row_other);
                            }
                        }
                        else
                        {
                            //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), Math.Round(budget, 2), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                            row_table = new UIData();
                            row_table.licenseNumber = ccnum;
                            row_table.clientCompany = clientCompany;
                            row_table.invoiceID = invoiceid.ToString();
                            row_table.budget = formatMoney(Math.Round(budget, 2));
                            row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                            row_table.thisMonthInv = invoicestat;
                            row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                            row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                            row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                            row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                            row_table.totalMonths = validity;
                            row_table.monthUtil = Difference;
                            row_table.monthRemain = trial2;
                            row_table.valPStart = ValiditySS;
                            row_table.valPEnd = ValidityFF;

                            table_table.records.Add(row_table);
                        }

                    }

                    else
                    {

                        if (ValidityS.Month != defered.Month && ValidityS.Day <= 15)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {

                                Difference = Difference + 1;
                                trial2 = validity - Difference;
                                toRevenue = invoiceamount * 1 / validity;

                                decimal amountalreadypaid = toRevenue * (Difference - 1);
                                openingbalance = invoiceamount - amountalreadypaid;
                                fromRevenue = 0;
                                closingbalance = invoiceamount - (amountalreadypaid + toRevenue);

                                invoicestat = "No";

                                if (!IsEmpty(opset))
                                {
                                    opp = opp;
                                }
                                else
                                    opp = 0;

                                if (Difference > 1 && IsEmpty(opset))
                                {
                                    toRevenue = toRevenue * Difference;
                                    fromRevenue = invoiceamount;
                                }
                            }
                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotcell += budget;
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }
                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    budgettotal += budget;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }

                            else if (glid == 5160)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }

                            else if (glid == 5161)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }

                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);
                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }

                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }

                        }

                        else if (ValidityS.Day > 15 && trial2 == 0)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                toRevenue = invoiceamount * 1 / validity;

                                decimal amountalreadypaid = toRevenue * (Difference - 1);
                                openingbalance = invoiceamount - amountalreadypaid;
                                fromRevenue = 0;
                                closingbalance = invoiceamount - (amountalreadypaid + toRevenue);

                                invoicestat = "No";


                                if (!IsEmpty(opset))
                                {
                                    opp = opp;

                                }
                                else
                                    opp = 0;



                                if (Difference > 1 && IsEmpty(opset))
                                {
                                    toRevenue = toRevenue * Difference;
                                    fromRevenue = invoiceamount;
                                }
                            }
                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotcell += budget;
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }
                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += opp;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    budgettotal += budget;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }

                            else if (glid == 5160)
                            {

                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }

                            else if (glid == 5161)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;

                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }

                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);
                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }

                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }
                        }
                        else if (ValidityS.Month != defered.Month && ValidityS.Day > 15)
                        {
                            if (Difference >= 0 && trial2 >= 0)
                            {
                                toRevenue = invoiceamount * 1 / validity;
                                decimal amountalreadypaid = toRevenue * (Difference - 1);
                                openingbalance = invoiceamount - amountalreadypaid;
                                fromRevenue = 0;
                                closingbalance = invoiceamount - (amountalreadypaid + toRevenue);
                                invoicestat = "No";

                                if (!IsEmpty(opset))
                                {
                                    opp = opp;
                                }
                                else
                                    opp = 0;
                                //  balancebf += opp;

                                if (Difference > 1 && IsEmpty(opset))
                                {
                                    toRevenue = toRevenue * Difference;
                                    fromRevenue = invoiceamount;
                                }
                            }
                            if (glid == 5156)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotcell += budget;
                                    invoicetotcell += invoiceamount;
                                    torevcell += toRevenue;
                                    fromremcell += fromRevenue;
                                    closingtotcell += closingbalance;
                                    balancebfcell += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //cell.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_cell = new UIData();
                                    row_cell.licenseNumber = ccnum;
                                    row_cell.clientCompany = clientCompany;
                                    row_cell.invoiceID = invoiceid.ToString();
                                    row_cell.budget = formatMoney(Math.Round(budget, 2));
                                    row_cell.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_cell.thisMonthInv = invoicestat;
                                    row_cell.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_cell.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_cell.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_cell.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_cell.totalMonths = validity;
                                    row_cell.monthUtil = Difference;
                                    row_cell.monthRemain = trial2;
                                    row_cell.valPStart = ValiditySS;
                                    row_cell.valPEnd = ValidityFF;

                                    cell_table.records.Add(row_cell);
                                }
                            }

                            else if (glid == 5157)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotbb += budget;
                                    invoicetotbb += invoiceamount;
                                    torevbb += toRevenue;
                                    fromrembb += fromRevenue;
                                    closingtotbb += closingbalance;
                                    balancebfbb += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //bbrand.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_bbrand = new UIData();
                                    row_bbrand.licenseNumber = ccnum;
                                    row_bbrand.clientCompany = clientCompany;
                                    row_bbrand.invoiceID = invoiceid.ToString();
                                    row_bbrand.budget = formatMoney(Math.Round(budget, 2));
                                    row_bbrand.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_bbrand.thisMonthInv = invoicestat;
                                    row_bbrand.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_bbrand.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_bbrand.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_bbrand.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_bbrand.totalMonths = validity;
                                    row_bbrand.monthUtil = Difference;
                                    row_bbrand.monthRemain = trial2;
                                    row_bbrand.valPStart = ValiditySS;
                                    row_bbrand.valPEnd = ValidityFF;

                                    bbrand_table.records.Add(row_bbrand);
                                }
                            }
                            else if (glid == 5158)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmic += budget;
                                    invoicetotmicro += invoiceamount;
                                    torevmicro += toRevenue;
                                    fromrevmicro += fromRevenue;
                                    closingtotmicro += closingbalance;
                                    balancebfmicro += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //micro.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_micro = new UIData();
                                    row_micro.licenseNumber = ccnum;
                                    row_micro.clientCompany = clientCompany;
                                    row_micro.invoiceID = invoiceid.ToString();
                                    row_micro.budget = formatMoney(Math.Round(budget, 2));
                                    row_micro.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_micro.thisMonthInv = invoicestat;
                                    row_micro.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_micro.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_micro.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_micro.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_micro.totalMonths = validity;
                                    row_micro.monthUtil = Difference;
                                    row_micro.monthRemain = trial2;
                                    row_micro.valPStart = ValiditySS;
                                    row_micro.valPEnd = ValidityFF;

                                    micro_table.records.Add(row_micro);
                                }
                            }

                            else if (glid == 5159)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotds += budget;
                                    invoicetotds += invoiceamount;
                                    torevds += toRevenue;
                                    fromrevds += fromRevenue;
                                    closingtotds += closingbalance;
                                    balancebfds += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //dServices.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_dservices = new UIData();
                                    row_dservices.licenseNumber = ccnum;
                                    row_dservices.clientCompany = clientCompany;
                                    row_dservices.invoiceID = invoiceid.ToString();
                                    row_dservices.budget = formatMoney(Math.Round(budget, 2));
                                    row_dservices.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_dservices.thisMonthInv = invoicestat;
                                    row_dservices.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_dservices.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_dservices.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_dservices.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_dservices.totalMonths = validity;
                                    row_dservices.monthUtil = Difference;
                                    row_dservices.monthRemain = trial2;
                                    row_dservices.valPStart = ValiditySS;
                                    row_dservices.valPEnd = ValidityFF;

                                    dservices_table.records.Add(row_dservices);
                                }
                            }

                            else if (glid == 5160)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotvsat += budget;
                                    invoicetotvsat += invoiceamount;
                                    torevvsat += toRevenue;
                                    fromrevvsat += fromRevenue;
                                    closingtotvsat += closingbalance;
                                    balancebfvsat += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //vsat.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_vsat = new UIData();
                                    row_vsat.licenseNumber = ccnum;
                                    row_vsat.clientCompany = clientCompany;
                                    row_vsat.invoiceID = invoiceid.ToString();
                                    row_vsat.budget = formatMoney(Math.Round(budget, 2));
                                    row_vsat.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_vsat.thisMonthInv = invoicestat;
                                    row_vsat.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_vsat.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_vsat.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_vsat.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_vsat.totalMonths = validity;
                                    row_vsat.monthUtil = Difference;
                                    row_vsat.monthRemain = trial2;
                                    row_vsat.valPStart = ValiditySS;
                                    row_vsat.valPEnd = ValidityFF;

                                    vsat_table.records.Add(row_vsat);
                                }
                            }

                            else if (glid == 5161)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotaero += budget;
                                    invoicetotaero += invoiceamount;
                                    torevaero += toRevenue;
                                    fromrevaero += fromRevenue;
                                    closingtotaero += closingbalance;
                                    balancebfaero += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //aero.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_aero = new UIData();
                                    row_aero.licenseNumber = ccnum;
                                    row_aero.clientCompany = clientCompany;
                                    row_aero.invoiceID = invoiceid.ToString();
                                    row_aero.budget = formatMoney(Math.Round(budget, 2));
                                    row_aero.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_aero.thisMonthInv = invoicestat;
                                    row_aero.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_aero.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_aero.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_aero.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_aero.totalMonths = validity;
                                    row_aero.monthUtil = Difference;
                                    row_aero.monthRemain = trial2;
                                    row_aero.valPStart = ValiditySS;
                                    row_aero.valPEnd = ValidityFF;

                                    aero_table.records.Add(row_aero);
                                }
                            }

                            else if (glid == 5162)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotmar += budget;
                                    invoicetotmar += invoiceamount;
                                    torevmar += toRevenue;
                                    fromrevmar += fromRevenue;
                                    closingtotmar += closingbalance;
                                    balancebfmar += opp;
                                    budgettotal += budget;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //marine.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_marine = new UIData();
                                    row_marine.licenseNumber = ccnum;
                                    row_marine.clientCompany = clientCompany;
                                    row_marine.invoiceID = invoiceid.ToString();
                                    row_marine.budget = formatMoney(Math.Round(budget, 2));
                                    row_marine.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_marine.thisMonthInv = invoicestat;
                                    row_marine.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_marine.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_marine.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_marine.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_marine.totalMonths = validity;
                                    row_marine.monthUtil = Difference;
                                    row_marine.monthRemain = trial2;
                                    row_marine.valPStart = ValiditySS;
                                    row_marine.valPEnd = ValidityFF;

                                    marine_table.records.Add(row_marine);
                                }
                            }

                            else if (glid == 5163)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettottrunk += budget;
                                    invoicetottrunk += invoiceamount;
                                    torevtrunk += toRevenue;
                                    fromrevtrunk += fromRevenue;
                                    closingtottrunk += closingbalance;
                                    balancebftrunk += opp;
                                    fromRev += fromRevenue;
                                    budgettotal += budget;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //trunking.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_trunking = new UIData();
                                    row_trunking.licenseNumber = ccnum;
                                    row_trunking.clientCompany = clientCompany;
                                    row_trunking.invoiceID = invoiceid.ToString();
                                    row_trunking.budget = formatMoney(Math.Round(budget, 2));
                                    row_trunking.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_trunking.thisMonthInv = invoicestat;
                                    row_trunking.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_trunking.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_trunking.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_trunking.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_trunking.totalMonths = validity;
                                    row_trunking.monthUtil = Difference;
                                    row_trunking.monthRemain = trial2;
                                    row_trunking.valPStart = ValiditySS;
                                    row_trunking.valPEnd = ValidityFF;

                                    trunking_table.records.Add(row_trunking);
                                }
                            }

                            else if (glid == 5164)
                            {
                                if (Difference >= 0 && trial2 >= 0)
                                {
                                    budgettotother += budget;
                                    invoicetotother += invoiceamount;
                                    torevother += toRevenue;
                                    fromrevother += fromRevenue;
                                    closingtotother += closingbalance;
                                    balancebfother += opp;
                                    budgettotal += budget;
                                    fromRev += fromRevenue;
                                    balancebf += opp;
                                    invoiceTotalForYes += invoiceamount;
                                    invoiceTotal += invoiceamount;
                                    toRev += toRevenue;
                                    closeBal += closingbalance;
                                    //other.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), formatMoney(Math.Round(invoiceamount, 2)), invoicestat, formatMoney(Math.Round(opp, 2)), formatMoney(Math.Round(fromRevenue, 2)), formatMoney(Math.Round(-toRevenue, 2)), formatMoney(Math.Round(closingbalance, 2)), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);

                                    row_other = new UIData();
                                    row_other.licenseNumber = ccnum;
                                    row_other.clientCompany = clientCompany;
                                    row_other.invoiceID = invoiceid.ToString();
                                    row_other.budget = formatMoney(Math.Round(budget, 2));
                                    row_other.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                    row_other.thisMonthInv = invoicestat;
                                    row_other.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                    row_other.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                    row_other.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                    row_other.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                    row_other.totalMonths = validity;
                                    row_other.monthUtil = Difference;
                                    row_other.monthRemain = trial2;
                                    row_other.valPStart = ValiditySS;
                                    row_other.valPEnd = ValidityFF;

                                    other_table.records.Add(row_other);
                                }
                            }
                            else
                            {
                                //table.Rows.Add(ccnum, clientCompany, invoiceid.ToString(), formatMoney(Math.Round(budget, 2)), Math.Round(invoiceamount, 2), invoicestat, Math.Round(opp, 2), Math.Round(fromRevenue, 2), Math.Round(-toRevenue, 2), Math.Round(closingbalance, 2), validity, Difference, trial2/* ((DateTime.Now.Year - ValidityS.Year) * 12) + validity - Difference*/, ValiditySS, ValidityFF);
                                row_table = new UIData();
                                row_table.licenseNumber = ccnum;
                                row_table.clientCompany = clientCompany;
                                row_table.invoiceID = invoiceid.ToString();
                                row_table.budget = formatMoney(Math.Round(budget, 2));
                                row_table.invoiceTotal = formatMoney(Math.Round(invoiceamount, 2));
                                row_table.thisMonthInv = invoicestat;
                                row_table.balBFwd = formatMoney(Math.Round(openingbalance, 2));
                                row_table.fromRev = formatMoney(Math.Round(fromRevenue, 2));
                                row_table.toRev = formatMoney(Math.Round(-toRevenue, 2));
                                row_table.closingBal = formatMoney(Math.Round(closingbalance, 2));
                                row_table.totalMonths = validity;
                                row_table.monthUtil = Difference;
                                row_table.monthRemain = trial2;
                                row_table.valPStart = ValiditySS;
                                row_table.valPEnd = ValidityFF;

                                table_table.records.Add(row_table);
                            }
                        }
                    }

                    if (Difference > 0 || description == "Modification" && Difference == 0 || Difference == 0 && ValidityS.Month == defered.Month && ValidityS.Year == defered.Year && ValidityS.Day > 15)
                    {
                        DataSet opstatus = new DataSet();
                        opstatus = brian_businessClass.GetOpeningBalStat(ccnum, invoiceid, defered);

                        if (IsEmpty(opstatus))
                        {
                            brian_businessClass.InsertOpBalNew(ccnum, closingbalance, defered, invoiceid, ValidityS, ValidityF);
                        }
                        else
                        {
                            brian_businessClass.UpdateOpBalNew(ccnum, closingbalance, defered, invoiceid, ValidityS, ValidityF);
                        }
                    }
                }
            } //ends here
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            cell_table.subT_budget = budgettotcell;
            cell_table.subT_invoiceTotal = invoicetotcell;
            cell_table.subT_balBFwd = balancebfcell;
            cell_table.subT_closingBal = closingtotcell;
            cell_table.subT_fromRev = fromremcell;
            cell_table.subT_toRev = torevcell;

            bbrand_table.subT_budget = budgettotbb;
            bbrand_table.subT_invoiceTotal = invoicetotbb;
            bbrand_table.subT_balBFwd = balancebfbb;
            bbrand_table.subT_closingBal = closingtotbb;
            bbrand_table.subT_fromRev = fromrembb;
            bbrand_table.subT_toRev = torevbb;

            micro_table.subT_budget = budgettotmic;
            micro_table.subT_invoiceTotal = invoicetotmicro;
            micro_table.subT_balBFwd = balancebfmicro;
            micro_table.subT_closingBal = closingtotmicro;
            micro_table.subT_fromRev = fromrevmicro;
            micro_table.subT_toRev = torevmicro;

            vsat_table.subT_budget = budgettotvsat;
            vsat_table.subT_invoiceTotal = invoicetotvsat;
            vsat_table.subT_balBFwd = balancebfvsat;
            vsat_table.subT_closingBal = closingtotvsat;
            vsat_table.subT_fromRev = fromrevvsat;
            vsat_table.subT_toRev = torevvsat;

            aero_table.subT_budget = budgettotaero;
            aero_table.subT_invoiceTotal = invoicetotaero;
            aero_table.subT_balBFwd = balancebfaero;
            aero_table.subT_closingBal = closingtotaero;
            aero_table.subT_fromRev = fromrevaero;
            aero_table.subT_toRev = torevaero;

            marine_table.subT_budget = budgettotmar;
            marine_table.subT_invoiceTotal = invoicetotmar;
            marine_table.subT_balBFwd = balancebfmar;
            marine_table.subT_closingBal = closingtotmar;
            marine_table.subT_fromRev = fromrevmar;
            marine_table.subT_toRev = torevmar;

            dservices_table.subT_budget = budgettotds;
            dservices_table.subT_invoiceTotal = invoicetotds;
            dservices_table.subT_balBFwd = balancebfds;
            dservices_table.subT_closingBal = closingtotds;
            dservices_table.subT_fromRev = fromrevds;
            dservices_table.subT_toRev = torevds;

            trunking_table.subT_budget = budgettottrunk;
            trunking_table.subT_invoiceTotal = invoicetottrunk;
            trunking_table.subT_balBFwd = balancebftrunk;
            trunking_table.subT_closingBal = closingtottrunk;
            trunking_table.subT_fromRev = fromrevtrunk;
            trunking_table.subT_toRev = torevtrunk;

            other_table.subT_budget = budgettotother;
            other_table.subT_invoiceTotal = invoicetotother;
            other_table.subT_balBFwd = balancebfother;
            other_table.subT_closingBal = closingtotother;
            other_table.subT_fromRev = fromrevother;
            other_table.subT_toRev = torevother;

            Totals total = new Totals(Math.Round(invoiceTotal,2), Math.Round(balancebf,2), Math.Round(toRev,2), Math.Round(fromRev,2), Math.Round(budgettotal,2), Math.Round(closeBal,2));

            tables.Add(cell_table);
            tables.Add(micro_table);
            tables.Add(bbrand_table);
            tables.Add(vsat_table);
            tables.Add(other_table);
            tables.Add(trunking_table);
            tables.Add(aero_table);
            tables.Add(marine_table);
            tables.Add(dservices_table);

            intlink.saveReport(tables, total);
        }



        public int CheckMonths(DateTime sdate, DateTime edate)
        {
            TimeSpan ts = edate - sdate;
            int tss = Convert.ToInt32(ts.Days);

            int months = tss / 30;
            return months;
        }

        public int CheckMonthsModification(DateTime sdate, DateTime edate)
        {
            int months = ((edate.Year - sdate.Year) * 12) + edate.Month - sdate.Month;
            return months;
        }

        bool IsEmpty(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
                if (table.Rows.Count != 0) return false;

            return true;
        }

        string formatMoney(decimal inputs)
        {
            string input = Convert.ToString(inputs);
            string neg = " ";
            if (input[0] == '-')
            {
                neg = input;
                input = input.TrimStart('-');
            }
            if (inputs == 0) input = "0.00";
            bool append = true;
            string decival = "";
            string temp = "";
            string input2 = "";
            string formatted = "";
            int len = 0;
            int b = 1;

            for (int g = 0; g < input.Length; g++)
            {
                if (input[g] != '.' && append)
                {
                    input2 += input[g];
                }
                else
                {
                    if (append)
                    {
                        g++;
                    }

                    append = false;
                    decival += input[g];
                }
            }

            len = input2.Length - 1;
            if (input.Length > 3)
            {
                for (int i = len; i >= 0; i--)
                {
                    temp += input2[i];

                    if (b == 3 && i != 0)
                    {
                        temp += ",";
                        b = 0;
                    }

                    b++;
                }

                for (int l = temp.Length - 1; l >= 0; l--)
                {
                    formatted += temp[l];
                }

                if (decival.Length > 0)
                {
                    formatted += '.' + decival;
                }
                else
                {
                    formatted += ".00";
                }
            }
            else
            {
                formatted = input;
            }
            if (neg[0] == '-')
                return "(" + formatted + ")";
            else return formatted;

        }
    }
}
