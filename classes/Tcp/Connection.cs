﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Mountain.classes.dataobjects;
using Mountain.classes.handlers;
using Mountain.classes.functions;

namespace Mountain.classes.tcp {

    [XmlRoot("Player")] public class Connection : Identity, IDisposable {
        [XmlIgnore] public StateObject state { get; set; }
        [XmlIgnore] public TcpClient Socket { get; set; }
        [XmlIgnore] public IPAddress IPAddress;
        [XmlIgnore] public int Port;
        [XmlIgnore] public ApplicationSettings settings;
        [XmlIgnore] public Stack<string> ResponseStack;
        [XmlIgnore] public CommandHandler Commands;
        [XmlIgnore] public Room Room { get { return room; } set { SetRoom(value); } }
        [XmlIgnore] public ManualResetEvent MessageReceivedDone;
        [XmlIgnore] public ManualResetEvent MessageSentDone;
        [XmlIgnore] public bool Connected { get {
                bool read = Socket.Client.Poll(500, SelectMode.SelectRead);
                bool status = (Socket.Client.Available == 0);
                return !(read & status);
            }
        }
        private Room room;
        private LoginDispatcher LoginDispatcher;   // login functions
        private PlayerDispatcher PlayerDispatcher; // player functions
        public Account Account { get; set; }
        public Stats Stats { get; set; }
        public Equipment Equipment { get; set; }
        public delegate void CommandHandler(object myObject, string message);

        public Connection(TcpClient socket, ApplicationSettings appSettings) {
            Socket = socket;
            IPAddress = ((IPEndPoint)socket.Client.RemoteEndPoint).Address;
            Port = ((IPEndPoint)socket.Client.RemoteEndPoint).Port;
            settings = appSettings;
            Account = new Account();
            Stats = new Stats();
            Equipment = new Equipment();
            ResponseStack = new Stack<string>();
            MessageReceivedDone = new ManualResetEvent(false);
            MessageSentDone = new ManualResetEvent(false);
            state = new StateObject((socket));
            LoginDispatcher = new LoginDispatcher(this, settings);
            StartReceiving();
        }
        
        public Connection() { // for xml serialization only
        }
        protected void StartReceiving() {
            SystemEventPacket packet = new SystemEventPacket(EventType.connection, "New Connection begun from " + this.IPAddress.ToString(), this);
            settings.SystemEventQueue.Push(packet);
            try {
                state.Socket.Client.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception e) {
                settings.SystemMessageQueue.Push("Connection closed: " + e.ToString());               
            }
        }

        public void ReceiveCallback(IAsyncResult ar) {
            try {
                if (state.Socket.Client == null)
                    return;
                int read = state.Socket.Client.EndReceive(ar); 
                if (read > 0) {
                    string incomingMessage = Encoding.ASCII.GetString(state.Buffer, 0, read).StripNewLine();
                    Task HandleMessage = new Task(() => Commands(this, incomingMessage)); // setup thread for dispatching incoming
                    MessageReceivedDone.Set(); 
                    HandleMessage.Start(); // start processing message - (in separate thread)
                }
                try {
                    state.Socket.Client.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReceiveCallback, state); // wait for next
                } catch (Exception ex) when (ex is SocketException || ex is NullReferenceException) {
                    if(ex is NullReferenceException) return;
                }
            }
            catch (SocketException e) {
                settings.SystemMessageQueue.Push("CC0: " + e.ToString());
            }
            catch (ObjectDisposedException e) {
                settings.SystemMessageQueue.Push("CC1: " + e.ToString());
            }
            catch (Exception e) {
                settings.SystemMessageQueue.Push("CC2: " + e.ToString());
            }
        }

        public void StartLogin() {
            Commands = LoginDispatcher.OnMessageReceived; 
            LoginDispatcher.Start(); 
        }

        public void StartPlayer() {
            PlayerDispatcher = new PlayerDispatcher(this, settings);
            LoginDispatcher = null;
            Commands = PlayerDispatcher.OnPlayerMessageReceived;
            SystemEventPacket packet = new SystemEventPacket(EventType.login, this.Account.Name + " has entered the world.", this);
            settings.SystemEventQueue.Push(packet);
            Room.AddPlayer(this);

        }

        private void SetRoom(Room room) {
            this.room = room;
            Account.RoomID = room.RoomID;
            Account.Room = room;
        }

        public void Send(string data, bool indent = true) {
            if (indent) data = data.Indent(); 
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            try {
                if (state.Socket.Client == null) return;
                state.Socket.Client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, SendCallback, state);
            } catch (SocketException) {
                return;
            }
        }

        private void SendCallback(IAsyncResult ar) {
            if (state.Socket.Client == null) return;
            try {
                int bytesSent = state.Socket.Client.EndSend(ar);
                MessageSentDone.Set(); // tell parent thread we're finished
            }
            catch (Exception e) {
                settings.SystemMessageQueue.Push("CC3: " + e.ToString());
            }
        }

        public void Shutdown() {
            Socket.Client.Shutdown(SocketShutdown.Both);
            Socket.Close();
            Save();
        }
        public void Dispose() {
            Socket.Close();
            if(Socket.Client != null) Socket.Client.Dispose();
        }
        protected void Save() {
            string file = settings.PlayersDirectory + "\\" + Account.Name + "_test.xml";
            TextWriter textWriter = new StreamWriter(file);
            try {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Connection));
                xmlSerializer.Serialize(textWriter, this);
            } catch (Exception ex) {
                settings.SystemMessageQueue.Push(ex.ToString());
            } finally {
                textWriter.Close();
            }
            textWriter.Close();
        }


    }
    
    public class StateObject {  
        private const int BUFFER_SIZE = 1024;
        public byte[] Buffer = new byte[BUFFER_SIZE];
        public TcpClient Socket { get; set; }
        public StateObject(TcpClient workSocket) {
            Socket = workSocket;
        }
    }
}
