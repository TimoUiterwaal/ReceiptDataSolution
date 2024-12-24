

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
using System.Data.Entity.Migrations;

namespace TimosService
{
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
            
            WriteToFile("Event ID:" + eventId + "| Service was ran at " + DateTime.Now);
            eventId++;
            TestDBConnection();
            TestBasePath();

        }
        //Entity Frameworks. Define the Receipts class for the Receipts table in the database
        public class Receipts
        {
            [Key]
            public int Recnum { get; set; }
            public int UserIDrecnum { get; set; }
        }
        public class SystemSpecs
        {
            [Key]
            public string Version { get; set; }
            public string BasePath { get; set; }
            public DateTime LastStartDateTimeUTC { get; set; }
        }

        //Define the Receiptscontext to connect to the database
        public class ReceiptsDatabaseContext : DbContext
        {
            public ReceiptsDatabaseContext(string connectionString) : base(connectionString)
            {
            }

            public DbSet<Receipts> Receipts { get; set; }
            public DbSet<SystemSpecs> SystemSpecs { get; set; }
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

        public void TestDBConnection()
        {
            
            string connectionString = ConfigurationManager.ConnectionStrings["ReceiptsDatabaseContext"].ConnectionString;

            try
            {
                //WriteToFile("Event ID:" + eventId + "| Attempting to connect to database.");
                using (var context = new ReceiptsDatabaseContext(connectionString))
                {
                    //WriteToFile("Event ID:" + eventId + "| Connected to database.");

                    var SystemSpec = context.SystemSpecs.FirstOrDefault();

                    // Update the LastStartDateTimeUTC field
                    SystemSpec.LastStartDateTimeUTC = DateTime.UtcNow;
                    context.SystemSpecs.AddOrUpdate(SystemSpec);
                    context.SaveChanges();
                    //WriteToFile("Event ID:" + eventId + "| Updated LastStartDateTimeUTC to: " + SystemSpec.LastStartDateTimeUTC);

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
        public void TestBasePath()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ReceiptsDatabaseContext"].ConnectionString;

                using (var context = new ReceiptsDatabaseContext(connectionString))
                {
                    var SystemSpec = context.SystemSpecs.FirstOrDefault();

                    if(SystemSpec.BasePath != null)
                    {

                        try
                        {
                            Directory.CreateDirectory(SystemSpec.BasePath);
                        }
                        catch (Exception ex)
                        {
                            WriteToFile("Event ID:" + eventId + "| General Exception: " + ex.Message);
                            eventId++;
                        }
                    }
                    else
                    {
                        WriteToFile("Event ID:" + eventId + "| BasePath Field in SystemSpecs is NULL");
                        eventId++;
                    }
                }
            }
        public void AddReceipt()
        {

            string connectionString = ConfigurationManager.ConnectionStrings["ReceiptsDatabaseContext"].ConnectionString;

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

    }

}
