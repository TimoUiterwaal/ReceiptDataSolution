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

            string s = GetConnectionString();

            OpenSqlConnection(s);

        }


        //This is a method that is accepting input variabled
        public static void WriteToFile(string Message)
        {

            //this defines a string path to the current base diretory + subfolder
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";

            //If the path variable doesn't  exist then create the subfolder
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //define a file path base string
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            if (!File.Exists(filepath))
            {
                //create a file to write to
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
        //This is a method to connect to the SQL DB

        public static void OpenSqlConnection(string connectionString)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                WriteToFile("Event ID:" + eventId + "|" + "ServerVersion: {0}" + connection.ServerVersion);
                eventId++;
                WriteToFile("Event ID:" + eventId + "|" + "State: {0}" + connection.State);
                eventId++;

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
