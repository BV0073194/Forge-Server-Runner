using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ForgeServer_1._16._5__36._2._42_
{
    public partial class MainWindow : Window
    {
        private Process _minecraftProcess;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            string xmx = string.IsNullOrWhiteSpace(XmxTextBox.Text) ? "2G" : XmxTextBox.Text.Trim();
            string xms = string.IsNullOrWhiteSpace(XmsTextBox.Text) ? "2G" : XmsTextBox.Text.Trim();
            StartMinecraftServer(xmx, xms);
        }

        private void StartMinecraftServer(string xmx, string xms)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jarFilePath = Path.Combine(appDirectory, "forge-1.16.5-36.2.42.jar");

            string javaArgs = $"-Xmx{xmx} -Xms{xms} -XX:+UnlockExperimentalVMOptions -XX:+AlwaysPreTouch " +
                               "-XX:NewSize=1G -XX:MaxNewSize=2G -XX:SurvivorRatio=2 -XX:+DisableExplicitGC -d64 " +
                               $"-XX:+UseConcMarkSweepGC -XX:+AggressiveOpts -jar \"{jarFilePath}\" nogui";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = javaArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true, // Allow input to the Java process
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _minecraftProcess = new Process
            {
                StartInfo = startInfo
            };

            _minecraftProcess.OutputDataReceived += Process_OutputDataReceived;
            _minecraftProcess.ErrorDataReceived += Process_OutputDataReceived;

            _minecraftProcess.Start();
            _minecraftProcess.BeginOutputReadLine();
            _minecraftProcess.BeginErrorReadLine();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return; // Ignore empty lines

            Dispatcher.Invoke(() =>
            {
                ConsoleOutput.AppendText(e.Data + Environment.NewLine);
                ConsoleOutput.ScrollToEnd();
            });
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            if (_minecraftProcess != null && !_minecraftProcess.HasExited)
            {
                _minecraftProcess.Kill();
                _minecraftProcess.Dispose();
            }
        }

        // Method to send command to the server
        public void SendCommand(string command)
        {
            if (_minecraftProcess != null && !_minecraftProcess.HasExited)
            {
                _minecraftProcess.StandardInput.WriteLine(command);
                _minecraftProcess.StandardInput.Flush(); // Ensure the command is sent
            }
        }

        // Modified to send command from CommandInputBox and clear it
        private void CommandButton_Click(object sender, RoutedEventArgs e)
        {
            string command = CommandTextBox.Text.Trim(); // Assume CommandInputBox is the TextBox for command input
            if (!string.IsNullOrWhiteSpace(command))
            {
                SendCommand(command);
                CommandTextBox.Clear(); // Clear the input after sending
            }
        }
        private void CommandTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // Check if the Enter key was pressed
            {
                CommandButton_Click(sender, e); // Call the same method as the button click
                e.Handled = true; // Mark the event as handled
            }
        }

        // Focus handling methods for Xmx and Xms text boxes
        private void XmxTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = XmxTextBox;
            if (textBox != null && textBox.Text == "Xmx (default: 2G)")
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black; // Change text color to black
            }
        }

        private void XmxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = XmxTextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Xmx (default: 2G)";
                textBox.Foreground = System.Windows.Media.Brushes.Gray; // Change text color to gray for placeholder
            }
        }

        private void XmsTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = XmsTextBox;
            if (textBox != null && textBox.Text == "Xms (default: 2G)")
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black; // Change text color to black
            }
        }

        private void XmsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = XmsTextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Xms (default: 2G)";
                textBox.Foreground = System.Windows.Media.Brushes.Gray; // Change text color to gray for placeholder
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_minecraftProcess != null && _minecraftProcess.Responding)
            {
                _minecraftProcess.Kill(true);
                _minecraftProcess.Dispose();
            }
        }
    }
}
