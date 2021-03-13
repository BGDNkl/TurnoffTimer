using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms; 


namespace TurnoffTimer
{
    public partial class MainWindow : Window
    {
        int seconds;
        string input1, input2;      
        private static readonly Regex regex = new Regex("[^0-9.-]+"); // matches disallowed text
        
        // cheking TextBox'es
        string CheckInput(string str)
        {
            if (regex.IsMatch(str))           
                return "x";
                      
            if (string.IsNullOrWhiteSpace(str))
                return "0";
            
            return str;
        }

        void UpdateStatus(string msg, bool ch)
        {
            status.Content = msg;
            
            if (ch == true)
                status.FontStyle = FontStyles.Italic;
            else
                status.FontStyle = FontStyles.Normal;
        }

        void Shutdown(string str)
        {
            //// to check the code without shutting down :)
            //System.Windows.Forms.MessageBox.Show(str);
            //str = "explorer";

            // with hiding the cmd window
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + str;
            process.StartInfo = startInfo;
            process.Start();
        }

        public MainWindow()
        {
            InitializeComponent();
            inputH.Focus();
            UpdateStatus("🤔 (not set)", false);
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {    
            input1 = CheckInput(inputH.Text);
            input2 = CheckInput(inputM.Text);

            if (input1 != "0" || input2 != "0")
            {
                if (input1 == "x")
                {
                    System.Windows.Forms.MessageBox.Show("Only numbers are allowed, please try again.",
                                                            "Prohibited action",
                                                            MessageBoxButtons.OK,
                                                            MessageBoxIcon.Warning);
                    inputH.Clear();
                    inputH.Focus();
                }
                else if (input2 == "x")
                {
                    System.Windows.Forms.MessageBox.Show("Only numbers are allowed, please try again.",
                                        "Prohibited action",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                    inputM.Clear();
                    inputM.Focus();
                }
                else
                {
                    seconds = Int32.Parse(input1) * 3600 + Int32.Parse(input2) * 60;
                    Shutdown(String.Format("shutdown -s -t {0}", seconds));

                    // calculating the time:
                    DateTime date = DateTime.Now;
                    int hour_int = Int32.Parse(input1) + date.Hour,
                     minute_int = Int32.Parse(input2) + date.Minute,
                     days_int = 0;
                    string hour_str = "", minute_str = "";

                    if (hour_int > 24)
                    {
                        hour_int = (Int32.Parse(input1) + date.Hour) % 24;
                        days_int = (Int32.Parse(input1) + date.Hour) / 24;
                    }

                    if (minute_int > 59)
                    {
                        minute_int = (Int32.Parse(input2) + date.Minute) % 60;
                        hour_int += ((Int32.Parse(input2) + date.Minute) - minute_int) / 60;
                    }

                    if (hour_int == 24) hour_str = "00";
                    else hour_str = hour_int.ToString();

                    if (minute_int == 60) minute_str = "00";
                    else if (minute_int < 10) minute_str = "0" + minute_int.ToString();
                    else minute_str = minute_int.ToString();

                    string message = "Shutdown at ";
                    if (days_int > 0)
                        message = "Shutdown in " + days_int + " days at ";

                    UpdateStatus(message + hour_str + ":" + minute_str, true);

                    // fix the minutes in MessageBox
                    int msbH = Int32.Parse(input1), msbM = Int32.Parse(input2);
                    if (msbM > 59)
                    {
                        msbH += 1;
                        msbM -= 59;
                    }

                    DialogResult result = System.Windows.Forms.MessageBox.Show(String.Format("Shutting down in {0} hours and {1} minutes", msbH, msbM),
                                                            "Succeeded!",
                                                            MessageBoxButtons.OK,
                                                            MessageBoxIcon.Information
                                                            );                    
                    inputH.Clear();
                    inputM.Clear();
                }
            }
            else
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show("00 hr : 00 min\nDo you want to turn off PC now?", 
                                                                            "Confirm the action", 
                                                                            MessageBoxButtons.YesNo,
                                                                            MessageBoxIcon.Warning,
                                                                            MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Shutdown("shutdown -s");
                    UpdateStatus("Immitiate shutdown.", true);
                    inputH.Clear();
                    inputM.Clear();
                }            
            }

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {          
            DialogResult result = System.Windows.Forms.MessageBox.Show("Abort the shutdown?", "Confirm cancelling", 
                                                                        MessageBoxButtons.YesNo, 
                                                                        MessageBoxIcon.Stop, 
                                                                        MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Shutdown("shutdown -a");
                UpdateStatus("Shutdown aborted.", true);
            }
        }
    }
}
