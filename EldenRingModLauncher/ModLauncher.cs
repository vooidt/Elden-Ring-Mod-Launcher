﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace EldenRingModLauncher
{
    public partial class ModLauncher : Form
    {
        public struct Mod
        {
            public string type;
            public string name;
            public string bat_file;
        }

        public static List<Mod> modList = new List<Mod>();

        private static bool selected = false;
        private static string selected_mod_name = string.Empty;
        private static string selected_bat_path = string.Empty;

        public ModLauncher()
        {
            InitializeComponent();

            this.Icon = new Icon("ermodlauncher.ico");

            try
            {
                // reads the csv file
                using (var reader = new StreamReader("mod_list.csv"))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(',');

                        // Debug.WriteLine("Mod Name: " + values[0] + " | Bat File Path : " + values[1]);

                        Mod mod = new Mod
                        {
                            type = values[0],
                            name = values[1],
                            bat_file = values[2],
                        };

                        modList.Add(mod);
                    }
                }

            }
            catch (FileNotFoundException)
            {
                // the csv file does not exist; makes a csv file
                using (StreamWriter sw = new StreamWriter("mod_list.csv"))
                {
                }
            }

            InitializeMods();
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (selected == false) { /* skip */ }
                // try to launch the selected mod
                else
                {
                    ProcessStartInfo psi = new ProcessStartInfo(selected_bat_path);
                    psi.WorkingDirectory = Path.GetDirectoryName(selected_bat_path);
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The selected mod does not exist: " + ex.Message);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            // opens up a file dialog
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            // setup
            dialog.Title = "Select the mod folder";
            dialog.InitialDirectory = "C:\\";
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folderPath = dialog.FileName;
                string folderName = new DirectoryInfo(folderPath).Name;
                // checks if the launchmod_eldenring.bat file exists
                if (!modList.Any(mod => mod.name == folderName) && File.Exists(Path.Combine(folderPath, "launchmod_eldenring.bat")))
                {
                    Mod mod = new Mod
                    {
                        type = "mod",
                        name = folderName,
                        bat_file = Path.Combine(folderPath, "launchmod_eldenring.bat"),
                    };
                    modList.Add(mod);
                    ModifyModCSV();
                    InitializeMods();
                }
                else
                {
                    MessageBox.Show("Error! Two possible reasons:\n1. You already have a mod in this program with same name\n2. launchmod_eldenring.bat was not found in folder");
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (selected == false)
            {
                // skip
            }
            else
            {
                List<Mod> newList = new List<Mod>();
                foreach (Mod mod in modList)
                {
                    // skips the selected mod
                    if (mod.name == selected_mod_name)
                    {
                        //skip
                    }
                    // adds every unselected mod to new list
                    else
                    {
                        newList.Add(mod);
                    }
                }
                // updates the csv and replace the mod list variable
                modList.Clear();
                modList = newList;
                ModifyModCSV();
                InitializeMods();
            }
        }

        public void InitializeMods()
        {
            // setup the panel with buttons from all mods saved in the csv
            ModsPanel.Controls.Clear();

            int button_width = 175; // Width of each checkbox
            int button_height = 175; // Height of each checkbox
            int x_spacing = 20; // Horizontal spacing between checkboxes
            int y_spacing = 20; // Vertical spacing between checkboxes
            int start_x = 22; // X position of the first checkbox
            int start_y = 20; // Y position of the first checkbox

            int row;
            int col;

            int i = 0;

            foreach (Mod mod in modList)
            {
                if (mod.type == "mod")
                {
                    row = i / 3; // Calculate the row index
                    col = i % 3; // Calculate the column index

                    Button button = new Button();
                    button.Text = mod.name;
                    button.Font = new Font("Comic Sans MS", 15, FontStyle.Bold);
                    button.Size = new Size(button_width, button_height);
                    button.Location = new System.Drawing.Point(start_x + col * (button_width + x_spacing), start_y + row * (button_height + y_spacing));
                    button.ForeColor = Color.White;
                    button.BackColor = Color.FromArgb(40, 40, 40);
                    button.Click += SelectMod;

                    i++;

                    ModsPanel.Controls.Add(button);
                }
            }
        }

        private void SelectMod(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                selected = false;

                // makes all mods into unselected color
                foreach (Control control in ModsPanel.Controls)
                {
                    if (control is Button)
                    {
                        control.BackColor = Color.FromArgb(40, 40, 40);
                    }
                }

                // finds the mod selected and give it selected background color
                foreach (Mod mod in modList)
                {
                    if (mod.name == button.Text)
                    {
                        selected = true;
                        selected_mod_name = mod.name;
                        selected_bat_path = mod.bat_file;
                        button.BackColor = Color.FromArgb(15, 15, 15);
                        // Debug.WriteLine("The selected mod name : " + selected_mod_name);
                        // Debug.WriteLine("The selected mod's bat file path : " + selected_bat_path);
                        break;
                    }
                }
            }
        }

        public void ModifyModCSV()
        {
            // fixes the csv file
            using (StreamWriter sw = new StreamWriter("mod_list.csv"))
            {
                foreach (Mod mod in modList)
                {
                    sw.WriteLine(mod.type + "," + mod.name + "," + mod.bat_file);
                }
            }
        }

        private void InfoButton_Click(object sender, EventArgs e)
        {
            ModLauncherInfo modLauncherInfo = new ModLauncherInfo();
            modLauncherInfo.Show();
        }

        private void LaunchCOOPButton_Click(object sender, EventArgs e)
        {
            bool hasCoopMod = modList.Any(mod => mod.type == "coop");

            if (hasCoopMod)
            {
                Mod coopMod = modList.Find(mod => mod.type == "coop");
                // Console.WriteLine("THE SEAMLESS COOP VALUE: " + coopMod);

                try // try to run the ersc_launcher.exe
                {
                    ProcessStartInfo psi = new ProcessStartInfo(coopMod.bat_file);
                    psi.WorkingDirectory = Path.GetDirectoryName(coopMod.bat_file);
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The Seamless coop executable is not working\n" + ex.Message);
                }
            }
            else
            {
                // opens up a file dialog
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                // setup
                dialog.Title = "Select the Elden Ring folder - MUST HAVE SEAMLESS COOP";
                dialog.InitialDirectory = "C:\\";
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    // gets the folder path
                    string folderPath = dialog.FileName;
                    string folderName = new DirectoryInfo(folderPath).Name;
                    // if user selects the ELDEN RING folder, change path into Game folder.
                    if (folderName == "ELDEN RING")
                    {
                        folderPath = Path.Combine(folderPath, "Game");
                        folderName = "Game";
                    }
                    // Checks if the executable exists
                    if (folderName == "Game" && File.Exists(Path.Combine(folderPath, "ersc_launcher.exe")))
                    {
                        Mod mod = new Mod
                        {
                            type = "coop",
                            name = folderName,
                            bat_file = Path.Combine(folderPath, "ersc_launcher.exe"),
                        };
                        modList.Add(mod);
                        ModifyModCSV();
                        InitializeMods();
                    }
                    else
                    {
                        MessageBox.Show("You need to select ELDEN RING folder that has Seamless Coop executable");
                    }
                }
            }
        }
    }
}
