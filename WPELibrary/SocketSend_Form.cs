﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WPELibrary
{
    public partial class SocketSend_Form : Form
    {
        private int SendPacketCNT = 0;
        private int Send_Success_CNT = 0;
        private int Send_Fail_CNT = 0;
        private SocketInfo Socket_Info = new SocketInfo();
        public string Send_Index;
        public string Send_Socket;
        public string Send_Len;
        public string Send_IPTo;
        public byte[] Send_Byte;

        public SocketSend_Form()
        {
            InitializeComponent();
        }

        private void SocketSend_Form_Load(object sender, EventArgs e)
        {
            this.Text = "发送封包 -【 序号 " + this.Send_Index + " 】 by RNShinoa";
            this.bSend.Enabled = true;
            this.bSendStop.Enabled = false;
            this.InitSendSocketInfo();
        }

        private void tSend_Tick(object sender, EventArgs e)
        {
            this.tlSendPacket_CNT.Text = this.SendPacketCNT.ToString();
            this.tlSend_Success_CNT.Text = this.Send_Success_CNT.ToString();
            this.tlSend_Fail_CNT.Text = this.Send_Fail_CNT.ToString();
        }

        private void bSend_Click(object sender, EventArgs e)
        {
            this.bSend.Enabled = false;
            this.bSendStop.Enabled = true;
            this.SendPacketCNT = 0;
            this.Send_Success_CNT = 0;
            this.Send_Fail_CNT = 0;
            if (!this.bgwSendPacket.IsBusy)
            {
                this.bgwSendPacket.RunWorkerAsync();
            }
        }

        private void bSendStop_Click(object sender, EventArgs e)
        {
            this.bgwSendPacket.CancelAsync();
            this.bSend.Enabled = true;
            this.bSendStop.Enabled = false;
        }

        private void bgwSendPacket_DoWork(object sender, DoWorkEventArgs e)
        {
            this.SendPacket();
        }

        private void InitSendSocketInfo()
        {
            try
            {
                this.txtSend_Socket.Text = this.Send_Socket;
                this.txtSend_Len.Text = this.Send_Len;
                this.txtSend_IP.Text = this.Send_IPTo.Split(new char[] { ':' })[0];
                this.txtSend_Port.Text = this.Send_IPTo.Split(new char[] { ':' })[1];
                this.rtbSocketSend_Data.Text = this.Socket_Info.Byte_To_Hex(this.Send_Byte);
            }
            catch (Exception)
            {
            }
        }

        [DllImport("ws2_32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int send(int socket, IntPtr buffer, int length, int flags);
        private void SendPacket()
        {
            try
            {
                int socket = int.Parse(this.txtSend_Socket.Text.Trim());
                int len = int.Parse(this.txtSend_Len.Text.Trim());
                byte[] buffer = this.Socket_Info.Hex_To_Byte(this.rtbSocketSend_Data.Text);
                int number = int.Parse(this.txtSend_CNT.Text.Trim());
                int times = int.Parse(this.txtSend_Int.Text.Trim());
                IntPtr destination = Marshal.AllocHGlobal(buffer.Length);
                Marshal.Copy(buffer, 0, destination, buffer.Length);
                if ((socket > 0 && buffer.Length != 0) && number > 0)
                {
                    for (int i = 0; i < number; i++)
                    {
                        if (this.bgwSendPacket.CancellationPending) return;

                        if (send(socket, destination, buffer.Length, 0) > 0)
                        {
                            this.Send_Success_CNT++;
                        }
                        else
                        {
                            this.Send_Fail_CNT++;
                        }
                        this.SendPacketCNT++;
                        int cnt = number - this.SendPacketCNT;
                        if (cnt > 0)
                        {
                            this.txtSend_CNT.Text = cnt.ToString();
                        }
                        Thread.Sleep(times);
                    }
                }
            }
            catch (Exception)
            {
                //this.ShowDebug(ex.Message);
            }
            this.bSend.Enabled = true;
            this.bSendStop.Enabled = false;
        }
    }
}
