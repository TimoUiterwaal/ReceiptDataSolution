using System;
using System.Data.SqlClient;
using System.IO;
using System.ServiceProcess;
using System.Timers;

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

            string connectionString = GetConnectionString();

            using (SqlConnection connection = OpenSqlConnection(connectionString))
            {
                if (connection != null)
                {
                    ExecuteQuery(connection);
                }
            }
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

        public static SqlConnection OpenSqlConnection(string connectionString)
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
        }

        static private string GetConnectionString()
        {           
             //TODO
            // To avoid storing the connection string in your code, 
            // you can retrieve it from a configuration file, using the 
            // System.Configuration.ConfigurationSettings.AppSettings property
            //(local)/
            return "Persist Security Info=False;User ID=ReceiptDataSolution;Password=ReceiptDataSolution;Initial Catalog=ReceiptDataSolution;Server=DESKTOP-NATENFH";
        }
    }

}
