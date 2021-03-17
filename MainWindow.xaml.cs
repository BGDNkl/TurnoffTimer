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
        private int seconds;       
        private const string path = @".h\log.txt";
        private static readonly Regex regex = new Regex("[^0-9.]"); // matches disallowed text       

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
            }
            else
            {
                //status.FontStyle = FontStyles.Normal;               
                status.FontWeight = FontWeights.Medium;
                status.Foreground = Brushes.DimGray;
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

        public async void WriteToFile(string t, string h, string m, string d, string s)              
        {
            if (Int32.Parse(h) < 10)          
                h = "0" + h; 
            
            if (Int32.Parse(m) < 10)
                m = "0" + m;
            
            try
            {
                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    await sw.WriteLineAsync(t);
                    await sw.WriteLineAsync(h);
                    await sw.WriteLineAsync(m);
                    await sw.WriteLineAsync(d);
                    await sw.WriteLineAsync(s);
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
                    string[] data = new string[allLines.Length];
                    
                    for (int i = 0; i < allLines.Length; i++)
                    {
                        if (!regex.IsMatch(allLines.ElementAtOrDefault(i)))
                        {
                            data[i] = allLines.ElementAtOrDefault(i);
                        }
                        else
                        {
                            // disallowed log symbols, so stop
                            data[i] = "-?";
                            return;
                        }
                    }

                    // set shutdown
                    Shutdown("shutdown -s -t " + data[0]);

                    string message = "Shutdown at ";
                    if (data[3] != null && Int32.Parse(data[3]) > 0)
                    {
                        if (Int32.Parse(data[3]) == 1)
                            message = "Shutdown tomorrow at ";
                        else
                            message = "Shutdown in " + data[3] + " days at ";
                    }
                    UpdateStatus(message + data[1] + ":" + data[2], true);                  
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
            string input1 = "", input2 = "";
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
                    int globalSeconds = (date.Hour * 3600) + (date.Minute * 60) + date.Second + seconds;                   
                    int d = globalSeconds / 86400;
                    int h = (globalSeconds - (d * 86400)) / 3600;
                    int m = (globalSeconds - ((d * 86400) + (h * 3600))) / 60;
                    int s = globalSeconds - ((d * 86400) + (h * 3600) + (m * 60));

                    string message = "Shutdown at ";
                    if (d > 0)
                        if (d == 1) message = "Shutdown tomorrow at ";
                        else message = "Shutdown in " + d + " days at ";
                    
                    if (h < 10) message += "0" + h.ToString();
                    else message += h.ToString();

                    message += " : ";

                    if (m < 10) message += "0" + m.ToString();
                    else message += m.ToString();

                    UpdateStatus(message, true);                

                    // fix the minutes in MessageBox:
                    int msbH = Int32.Parse(input1), msbM = Int32.Parse(input2);
                    if (msbM > 60)
                    {
                        msbH += 1;
                        msbM -= 60;
                    }

                    WriteToFile(seconds.ToString(), h.ToString(), m.ToString(), d.ToString(), s.ToString());

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
                                                                        MessageBoxDefaultButton.Button1);
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
