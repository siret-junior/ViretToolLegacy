using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ViretTool.BasicClient
{
    class Settings
    {
        const string SETTINGS_FILE = "ViretToolSettings.txt";

        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string TeamName { get; set; }

        private SettingsWindow mWindow;

        public delegate void SettingsChangedEventHandler(Settings settings);
        public event SettingsChangedEventHandler SettingsChangedEvent;

        public static Settings LoadSettings(string filename)
        {
            Settings settings = new Settings();

            using (StreamReader reader = new StreamReader(File.OpenRead(filename)))
            {
                try
                {
                    // TODO:
                    settings.IPAddress = reader.ReadLine();
                    settings.Port = int.Parse(reader.ReadLine());
                    settings.TeamName = reader.ReadLine();
                }
                catch
                {
                    // TODO log
                    throw;
                }
            }
            return settings;
        }

        public static Settings LoadSettings()
        {
            if (File.Exists(SETTINGS_FILE))
            {
                return LoadSettings(SETTINGS_FILE);
            }
            else
            {
                return new Settings();
            }
        }

        public static void StoreSettings(Settings settings, string filename)
        {
            using (StreamWriter writer = new StreamWriter(File.Create(filename)))
            {
                writer.WriteLine(settings.IPAddress);
                writer.WriteLine(settings.Port);
                writer.WriteLine(settings.TeamName);
            }
        }

        public static void StoreSettings(Settings settings)
        {
            StoreSettings(settings, SETTINGS_FILE);
        }

        public void OpenSettingsWindow()
        {
            mWindow = new SettingsWindow(this);
            mWindow.ShowDialog();
            StoreSettings(this);
            SettingsChangedEvent?.Invoke(this);
        }


        private class SettingsWindow : Window
        {
            Settings mSettings;
            TextBox mIpTextbox;
            TextBox mPortTextbox;
            TextBox mTeamTextbox;

            public SettingsWindow(Settings settings)
            {
                mSettings = settings;
                this.SizeToContent = SizeToContent.WidthAndHeight;

                Grid grid = new Grid();
                grid.HorizontalAlignment = HorizontalAlignment.Left;
                grid.VerticalAlignment = VerticalAlignment.Top;
                grid.ShowGridLines = true;
                grid.Background = new SolidColorBrush(Colors.LightSteelBlue);
                this.AddChild(grid);

                ColumnDefinition gridCol1 = new ColumnDefinition();
                gridCol1.Width = new GridLength(200);
                ColumnDefinition gridCol2 = new ColumnDefinition();
                gridCol2.Width = new GridLength(200);
                grid.ColumnDefinitions.Add(gridCol1);
                grid.ColumnDefinitions.Add(gridCol2);

                RowDefinition gridRow1 = new RowDefinition();
                gridRow1.Height = new GridLength(32);
                RowDefinition gridRow2 = new RowDefinition();
                gridRow2.Height = new GridLength(32);
                RowDefinition gridRow3 = new RowDefinition();
                gridRow3.Height = new GridLength(32);
                RowDefinition gridRow4 = new RowDefinition();
                gridRow4.Height = new GridLength(32);
                grid.RowDefinitions.Add(gridRow1);
                grid.RowDefinitions.Add(gridRow2);
                grid.RowDefinitions.Add(gridRow3);
                grid.RowDefinitions.Add(gridRow4);


                // IP
                Label ipLabel = new Label();
                ipLabel.Content = "IP address:";
                Grid.SetColumn(ipLabel, 0);
                Grid.SetRow(ipLabel, 0);
                grid.Children.Add(ipLabel);

                mIpTextbox = new TextBox();
                mIpTextbox.Text = mSettings.IPAddress;
                Grid.SetColumn(mIpTextbox, 1);
                Grid.SetRow(mIpTextbox, 0);
                grid.Children.Add(mIpTextbox);


                // port
                Label portLabel = new Label();
                portLabel.Content = "port:";
                Grid.SetColumn(portLabel, 0);
                Grid.SetRow(portLabel, 1);
                grid.Children.Add(portLabel);

                mPortTextbox = new TextBox();
                mPortTextbox.Text = mSettings.Port.ToString();
                Grid.SetColumn(mPortTextbox, 1);
                Grid.SetRow(mPortTextbox, 1);
                grid.Children.Add(mPortTextbox);


                // team name
                Label teamLabel = new Label();
                teamLabel.Content = "Team name:";
                Grid.SetColumn(teamLabel, 0);
                Grid.SetRow(teamLabel, 2);
                grid.Children.Add(teamLabel);

                mTeamTextbox = new TextBox();
                mTeamTextbox.Text = mSettings.TeamName;
                Grid.SetColumn(mTeamTextbox, 1);
                Grid.SetRow(mTeamTextbox, 2);
                grid.Children.Add(mTeamTextbox);

                // cancel
                Button cancelButton = new Button();
                cancelButton.Content = "Cancel";
                cancelButton.Click += (s, e) => { this.Close(); };
                Grid.SetColumn(cancelButton, 0);
                Grid.SetRow(cancelButton, 3);
                grid.Children.Add(cancelButton);

                Button submitButton = new Button();
                submitButton.Content = "Save";
                submitButton.Click += Submit;
                Grid.SetColumn(submitButton, 1);
                Grid.SetRow(submitButton, 3);
                grid.Children.Add(submitButton);

            }


            private void Submit(object sender, RoutedEventArgs e)
            {
                mSettings.IPAddress = mIpTextbox.Text.ToString();
                try
                {
                    mSettings.Port = int.Parse(mPortTextbox.Text.ToString());
                }
                catch
                {
                    // TODO:
                    mSettings.Port = -1;
                }
                mSettings.TeamName = mTeamTextbox.Text.ToString();
                this.Close();
            }
        }


    }
}
