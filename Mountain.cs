﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Mountain.classes;
using Mountain.classes.helpers;

namespace Mountain {

    public partial class Mountain : Form {
        protected ApplicationSettings settings;
        protected MessageQueue Messages;
        protected World world;

        public Mountain() {
            Messages = new MessageQueue(settings);
            settings = new ApplicationSettings(Messages);
            InitializeComponent();
            this.Messages.OnMessageReceived += Messages_OnMessageReceived;
            world = BuildDefaultWorld();
            Console.Items.Add("Server has started");
        }

        private void Messages_OnMessageReceived(object myObject, string msg) {
            try {
                string message = Messages.Pop();
                if (message == null) message = msg;
                this.Invoke((MethodInvoker) delegate {
                    logRichTextBox.AppendText("\r\n" + message);
                 //   Console.Items.Add(message); // runs on UI thread
                });
            } catch ( Exception e) {
                this.Invoke((MethodInvoker)delegate {
                    logRichTextBox.AppendText("\r\n" + e.ToString());
                  //  Console.Items.Add(e.ToString()); // runs on UI thread
                });

            }
        }

        private void exitProgram_ToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
            //this.world.Save(string.Empty);
        }
        
        private void serverStart(object sender, EventArgs e) { // start
            if (world == null) {
                world = BuildDefaultWorld();
                Console.Items.Add("Server has started");
            } else {
                Console.Items.Add("Server is already running");
            }
        }

        private void button6_Click(object sender, EventArgs e) { //stop server
        //    world.Shutdown();
         //   world = null;
            Console.Items.Add("Shutdown not implemented yet"); // settings.players needs each player to disconnect/save
        }

        private World BuildDefaultWorld() {
            if (world != null) { world = null; }
            world = new World(settings);
            world.settings.Void = world.Areas[0].Rooms.Find(room => room.Name == "Void");
            areaListBox.Items.AddRange(world.Areas.Select(x => x.Name).ToArray());
            areaListBox.SelectedIndex = 0;
            return world;
        }

        private void areaForm_Button_Click(object sender, EventArgs e) { 
            AreaForm areaForm = new AreaForm(new Area(), settings);
            DialogResult dialogresult = areaForm.ShowDialog();
            if (dialogresult == DialogResult.OK) {
                world.Areas.Add(areaForm.area);
                world.settings.Void = areaForm.area.Rooms.Find(room => room.Name == "Void");                
                areaListBox.Items.AddRange(world.Areas.Select(x => x.Name).ToArray());
                areaListBox.SelectedIndex = 0;
            } else {
                if (dialogresult == DialogResult.Cancel) {

                }
            }
            areaForm.Dispose();
        }

        private void areaListBox_SelectedIndexChanged(object sender, EventArgs e) {
            string name = areaListBox.SelectedItem.ToString();
            Area selectedArea = world.Areas.Find(area => area.Name == (string)areaListBox.SelectedItem);
            roomsListBox.Items.Clear();
            roomsListBox.Items.AddRange(selectedArea.Rooms.Select(room => room.Name).ToArray());
        }
    }
}
