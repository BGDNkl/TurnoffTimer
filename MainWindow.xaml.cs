using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;


namespace TurnoffTimer
{
    public partial class MainWindow : Window
    {
        int seconds;
        string input1, input2;
        const string path = @".h\log.txt";
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

            if (ch)
            {
                //status.FontStyle = FontStyles.Italic;
                status.Foreground = Brushes.Red;
                status.FontFamily = new FontFamily("Consolas");
                
            }
            else
            {
                //status.FontStyle = FontStyles.Normal;               
                status.FontWeight = FontWeights.Medium;
                status.Foreground = Brushes.DimGray;
                status.FontFamily = new FontFamily("Consolas");

            }
        }

        void FolderCheck()
        {
            string folderPath = @".h"; 
            if (!Directory.Exists(folderPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(folderPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }

        public async void WriteToFile(string s, string h, string m, string d)              
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    await sw.WriteLineAsync(s);
                    await sw.WriteLineAsync(h);
                    await sw.WriteLineAsync(m);
                    await sw.WriteLineAsync(d);
                }
                //System.Windows.Forms.MessageBox.Show("Writing Succeeded!", " WriteToFile()");                     /*check*/
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "WriteToFile() - Exception");                      /*check*/
            }
        }

        void Shutdown(string str)
        {
            //// to check the code without shutting down :)
            //str = "explorer";

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + str;
            process.StartInfo = startInfo; 

            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Shutdown() - process exception");                 /* check */
            }

            if (process.ExitCode == 1116)
            {
                UpdateStatus("🤔 (not set)", false);

                try
                {
                    File.WriteAllText(path, String.Empty);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "txt cleanup exception - Shutdown()");         /* check */
                }

            }
            else if (str.Contains("-t") == false || str.Contains("-s") == false)
            {
                if (new FileInfo(path).Length != 0)
                {
                    //read file
                    string[] allLines = File.ReadAllLines(path);
                    string sec = allLines.ElementAtOrDefault(0);
                    string hr = allLines.ElementAtOrDefault(1);
                    string mn = allLines.ElementAtOrDefault(2);
                    string days = allLines.ElementAtOrDefault(3);

                    // set shutdown
                    Shutdown(String.Format("shutdown -s -t {0}", Int32.Parse(sec)));
                    string msg = "Shutdown at ";
                    if (days != null && Int32.Parse(days) > 0)
                        if (Int32.Parse(days) == 1) msg = "Shutdown tomorrow at ";
                        else msg = "Shutdown in " + days + " days at ";

                    UpdateStatus(msg + hr + ":" + mn, true);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            inputH.Focus();
            FolderCheck();

            // check at the beginning of the application -
            //  - if we have already set the shutdown the previous time
            Shutdown("shutdown -a");
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
                    else if (hour_int < 10) hour_str = "0" + hour_int.ToString();
                    else hour_str = hour_int.ToString();

                    if (minute_int == 60) minute_str = "00";
                    else if (minute_int < 10) minute_str = "0" + minute_int.ToString();
                    else minute_str = minute_int.ToString();

                    string message = "Shutdown at ";
                    if (days_int > 0) 
                        if (days_int == 1) message = "Shutdown tomorrow at ";
                        else message = "Shutdown in " + days_int + " days at ";

                    UpdateStatus(message + hour_str + ":" + minute_str, true);

                    // fix the minutes in MessageBox:
                    int msbH = Int32.Parse(input1), msbM = Int32.Parse(input2);
                    if (msbM > 59)
                    {
                        msbH += 1;
                        msbM -= 59;
                    }

                    WriteToFile(seconds.ToString(), hour_str.ToString(), minute_str.ToString(), days_int.ToString());

                    System.Windows.Forms.MessageBox.Show(String.Format("Shutting down in {0} hours and {1} minutes", msbH, msbM),
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
                try
                {
                    //File.SetAttributes(path, FileAttributes.Normal);
                    File.WriteAllText(path, String.Empty);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "txt cleanup exception - ButtonCancel");       /* check */
                }

                Shutdown("shutdown -a");               
                UpdateStatus("Shutdown aborted.", true);                             
            }
        }
    }
}
