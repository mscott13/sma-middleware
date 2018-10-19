using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace SyncMon
{
    class Requery
    {
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader reader;

        public void Initialize(string connection)
        {
            conn = new SqlConnection(connection);
            cmd = new SqlCommand();   
        }


        public InvoiceData RecentInvoiceData(string invoiceId)
        {
            Thread.Sleep(500); //Waiting For Asms.
            cmd.CommandText = "Select CustomerID, ARInvoiceID, FeeType, notes, Amount, isvoided from tblARInvoices where ARInvoiceID=@id";
            cmd.Parameters.AddWithValue("@id", invoiceId);

            cmd.Connection = conn;
            InvoiceData dt = new InvoiceData();

            conn.Open();
            reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    dt.CustomerID = reader[0].ToString();
                    dt.ARInvoiceID = reader[1].ToString();
                    dt.FeeType = reader[2].ToString();
                    dt.notes = reader[3].ToString();
                    dt.Amount = Convert.ToDecimal(reader[4]);
                    dt.isvoid = Convert.ToInt32(reader[5]);
                }
            }
            conn.Close();
            return dt;
        }
    }
}
