﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;


namespace Client_project
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();

            //Reading the public RSA key which will be used for encryption purposes
            string RSA3072PublicEncryptionKey;
            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("server_enc_dec_pub.txt"))
            {
                RSA3072PublicEncryptionKey = fileReader.ReadLine();
            }

            //Reading the public RSA key which will be used for signature verification purposes
            string RSA3072PublicVerificationKey;
            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("server_sign_verify_pub.txt"))
            {
                RSA3072PublicVerificationKey = fileReader.ReadLine();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ipVar = ip_field.Text;
            string portNumVar = port_field.Text;
            string usernameVar = username_field.Text;
            string passwordVar = password_field.Text;
            string channelVar = channel_combobox.SelectedItem.ToString();
            int port_num;


            if (Int32.TryParse(portNumVar, out port_num))
            {
                try
                {
                    clientSocket.Connect(ipVar, port_num);
                    connect_button.Enabled = false;
                    connected = true;
                    logs.AppendText("Connected to the server. \n");

                    Thread receiveThread = new Thread(new ThreadStart(Receive));
                    receiveThread.Start();

                }
                catch {
                    logs.AppendText("Could not connect to the server. \n");
                }

            }
            else {
                logs.AppendText("Check the port number. \n");
            }

        }

        private void Receive()
        {
            while (connected)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    clientSocket.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));
                    logs.AppendText(message + "\n");
                }
                catch {
                    if (!terminating)
                    {
                        logs.AppendText("Connection has lost with the server. \n");
                    }

                    clientSocket.Close();
                    connected = false;
                }
            }
        }



        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            terminating = true;
            clientSocket.Close();
            connected = false;
            logs.AppendText("Successfully disconnected.\n");

            disconnect_button.Enabled = false;
            connect_button.Enabled = true;
            logs.Enabled = false;

            connect_button.BackColor = SystemColors.Control;
            disconnect_button.BackColor = SystemColors.Control;
        }
    }
}
