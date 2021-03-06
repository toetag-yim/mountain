﻿using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Mountain.classes;
using Mountain.classes.dataobjects;

namespace Mountain.Dialogs {

    public partial class ExitEditor : Form {
        public Exit Exit { get; set; }
        Area SelectedArea;

        public ExitEditor(Exit exit) {
            InitializeComponent();
            Exit = exit.ShallowCopy();
            propertyGrid.SelectedObject = Exit;
            SelectedArea = Exit.Area;
            if (Common.Settings.World.Areas.Any()) {
                areaComboBox.Items.AddRange(Common.Settings.World.Areas.Select(x => x.Name).ToArray());
                areaComboBox.SelectedIndex = areaComboBox.Items.IndexOf(SelectedArea.Name);
            }
        }

        private void areaComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            string name = areaComboBox.SelectedItem.ToString();
            SelectedArea = Common.Settings.World.Areas.Find(area => area.Name == (string)areaComboBox.SelectedItem);
            roomListBox.Items.Clear();
            roomListBox.Items.AddRange(SelectedArea.Rooms.Select(room => room.Name).ToArray());
            if (roomListBox.Items.Count > 0) roomListBox.SelectedIndex = roomListBox.Items.IndexOf(Exit.Owner.Name);
            propertyGrid.Refresh();
        }

        private void roomListBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (roomListBox.SelectedIndex == -1) return;
            string name = roomListBox.Items[roomListBox.SelectedIndex].ToString();
            if (name != "None") {
                Room room = Common.Settings.World.RoomByName(name);
                linkDoorLabelTextBox.Text = room.Name;
                Exit.DoorLabel = room.Name;
                Exit.Room = room;
            }
            propertyGrid.Refresh();
        }      

        private void linkDoorLabelTextBox_KeyPress(object sender, KeyPressEventArgs e) {
            switch (e.KeyChar) {
                case (char)Keys.Return:
                    Exit.DoorLabel = linkDoorLabelTextBox.Text;
                    break;
                default: Exit.DoorLabel = linkDoorLabelTextBox.Text;
                    break;
            }
            propertyGrid.Refresh();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            var item = e.ChangedItem;
            var old = e.OldValue;
            switch (e.ChangedItem.Label) {
                case "DoorLabel":
                    this.linkDoorLabelTextBox.Text = e.ChangedItem.Value.ToString();
                    break;
                case "Visible": break;
                case "Breakable": break;
                case "LockType": break;
                case "DoorType": break;
                case "ExitType": break;
                case "Restrictions": break;
            }
        }
    }
}
