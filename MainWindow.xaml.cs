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
            {
                // wrong symbols in textbox
                return "x";
            }
            
            if (string.IsNullOrWhiteSpace(str))
                return "0";
            
            return str;
        }

        void Shutdown(string str)
        {
            // to check the code without shutting down :)
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
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {    
            input1 = CheckInput(inputH.Text);
            input2 = CheckInput(inputM.Text);

            if (input1 != "0" || input2 != "0")
            {
                if (input1 == "x")
                {
                    System.Windows.MessageBox.Show("Enter only numbers, please try again.");
                    inputH.Clear();
                    inputH.Focus();
                }
                else if (input2 == "x")
                {
                    System.Windows.MessageBox.Show("Enter only numbers, please try again.");
                    inputM.Clear();
                    inputM.Focus();
                }
                else
                {
                    seconds = Int32.Parse(input1) * 3600 + Int32.Parse(input2) * 60;
                    Shutdown(String.Format("shutdown -s -t {0}", seconds));                   
                    DialogResult result = System.Windows.Forms.MessageBox.Show(String.Format("Shutting down in {0} hours and {1} minutes", Int32.Parse(input1), Int32.Parse(input2)),
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
                    inputH.Clear();
                    inputM.Clear();
                }            
            }

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            const string message = "Abort the shutdown?";
            const string title = "Confirm cancelling";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            
            DialogResult result = System.Windows.Forms.MessageBox.Show(message, title, buttons, 
                                                                        MessageBoxIcon.Stop, 
                                                                        MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.Yes)
                Shutdown("shutdown -a");
        }
    }
}
