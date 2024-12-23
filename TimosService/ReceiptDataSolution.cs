using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.ServiceProcess;
using System.Timers;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;

namespace TimosService
{

    //Entity Frameworks. Define the Receipts class for the Receipts table in the database
    public class Receipts
    {
        [Key]
        public int Recnum { get; set; }
        public int UserIDrecnum { get; set; }
    }
    //Define the Receiptscontext to connect to the database
    public class ReceiptsDatabaseContext : DbContext
    {
        public ReceiptsDatabaseContext(string connectionString) : base(connectionString)
        {
        }

        public DbSet<Receipts> Receipts { get; set; }
    }


    public partial class ReceiptDataSolution : ServiceBase
    {
        public static int eventId = 1;
        Timer timer = new Timer();
        public ReceiptDataSolution()
        {

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Event ID:" + eventId + "| Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in ms
            timer.Enabled = true;
            eventId++;
        }

        protected override void OnStop()
        {
            WriteToFile("Event ID:" + eventId + "| Service has stopped at " + DateTime.Now);
            eventId++;
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            AddReceipt();
            WriteToFile("Event ID:" + eventId + "| Service was ran at " + DateTime.Now);
            eventId++;
            

        }

        public static void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

        public void AddReceipt()
        {
            WriteToFile("Event ID:" + eventId + "| PUllingconnections string");

            string connectionString = ConfigurationManager.ConnectionStrings["ReceiptsDatabaseContext"].ConnectionString;

            WriteToFile(connectionString);
            WriteToFile("Event ID:" + eventId + "| Pulled connections string");
            try
            {
                WriteToFile("Event ID:" + eventId + "| Attempting to connect to database.");
                using (var context = new ReceiptsDatabaseContext(connectionString))
                {
                    WriteToFile("Event ID:" + eventId + "| Connected to database.");

                    // Add a new Receipt
                    var newReceipt = new Receipts { UserIDrecnum = 10 };
                    context.Receipts.Add(newReceipt);
                    context.SaveChanges();
                    WriteToFile("Event ID:" + eventId + "| Added new receipt with UserIDrecnum: " + newReceipt.UserIDrecnum);

                    // Query Receipts using IQueryable
                    IQueryable<Receipts> ReceiptsQuery = context.Receipts.Where(p => p.Recnum > 0);

                    foreach (var Receipt in ReceiptsQuery)
                    {
                        WriteToFile("Event ID:" + eventId + "| " + $"Receipt: {Receipt.Recnum}, UserIDrecnum: {Receipt.UserIDrecnum}");
                        eventId++;
                    }
                }
            }
            catch (SqlException ex)
            {
                WriteToFile("Event ID:" + eventId + "| SQL Exception: " + ex.Message);
                eventId++;
            }
            catch (Exception ex)
            {
                WriteToFile("Event ID:" + eventId + "| General Exception: " + ex.Message);
                eventId++;
            }
        }

        /*public static SqlConnection OpenSqlConnection(string connectionString)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                WriteToFile("Event ID:" + eventId + "| ServerVersion: " + connection.ServerVersion);
                eventId++;
                WriteToFile("Event ID:" + eventId + "| State: " + connection.State);
                eventId++;
                return connection;
            }
            catch (SqlException ex)
            {
                WriteToFile("Event ID:" + eventId + "| SQL Exception: " + ex.Message);
                eventId++;
                return null;
            }
            catch (InvalidOperationException ex)
            {
                WriteToFile("Event ID:" + eventId + "| Invalid Operation Exception: " + ex.Message);
                eventId++;
                return null;
            }
            catch (Exception ex)
            {
                WriteToFile("Event ID:" + eventId + "| General Exception: " + ex.Message);
                eventId++;
                return null;
            }
        }

        public static void ExecuteQuery(SqlConnection connection)
        {
            try
            {
                string queryString = "SELECT Recnum, UserIDrecnum FROM dbo.Receipts;";
                var command = new SqlCommand(queryString, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        WriteToFile("Event ID:" + eventId + "|" + String.Format(" {0}, {1}", reader[0], reader[1]));
                        eventId++;
                    }
                }
            }
            catch (SqlException ex)
            {
                WriteToFile("Event ID:" + eventId + "| SQL Exception: " + ex.Message);
                eventId++;
            }
            catch (Exception ex)
            {
                WriteToFile("Event ID:" + eventId + "| General Exception: " + ex.Message);
                eventId++;
            }
        }*/

    }

}
