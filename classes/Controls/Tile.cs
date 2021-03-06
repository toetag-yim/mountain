﻿using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using Mountain.classes.dataobjects;

namespace Mountain.classes.controls {

    public partial class Tile : Control {
        protected Room Room { get { return Room; } set { Room = value; SetColor(); } }
        protected Area Area { get { return Room.Area; } }

        public Tile Root { get; private set; }
        public List<Tile> Children { get; private set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public string Identity { get; private set; }
        public roomType RoomType { get { return Room.roomType; } }

        protected Pen Pen;   // background color

        public Tile(int row, int column, Room room, Tile parent = null) {
            Room = room;
            Identity = room.Area.Name + ":" + room.Name;
            Root = parent;

            InitializeComponent();
            SetColor();
        }

        public void Add(Tile tile) {
            Children.Add(tile);
        }

        public void Remove(Tile tile) {
            Children.Remove(tile);
        }

        protected override void OnPaint(PaintEventArgs pe) {
            pe.Graphics.DrawRectangle(Pen, pe.ClipRectangle);

            base.OnPaint(pe);
        }

        protected void SetColor() {
            switch (RoomType) {
                case roomType.road: Pen = new Pen(Color.SaddleBrown); break;
                case roomType.path: Pen = new Pen(Color.SandyBrown); break;
                case roomType.street: Pen = new Pen(Color.Beige); break;
                case roomType.healing: Pen = new Pen(Color.Green); break;
                case roomType.home: Pen = new Pen(Color.RosyBrown); break;
                case roomType.leveling: Pen = new Pen(Color.Salmon); break;
                case roomType.pawn: Pen = new Pen(Color.Gold); break;
                case roomType.shop: Pen = new Pen(Color.Fuchsia); break;
                case roomType.sewer: Pen = new Pen(Color.DarkSlateBlue); break;
                case roomType.vault: Pen = new Pen(Color.DarkTurquoise); break;
                default: Pen = new Pen(Color.Silver); break;
            }
            Refresh();
        }
    }
}
