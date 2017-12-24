﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ErnstNetworking.Protocol
{
    public enum EN_PACKET_TYPE
    {
        CONNECT = 0,
        DISCONNECT,
        MESSAGE,
        CONNECT_CONFIRMED
    }


    struct EN_PacketConnect
    {
        public EN_PACKET_TYPE   packet_type;
        public int              packet_data;
    }

    struct EN_PacketDisconnect
    {
        public EN_PACKET_TYPE   packet_type;
        public int              packet_data;
    }

    struct EN_PacketMessage
    {
        public EN_PACKET_TYPE   packet_type;
        public byte[]           packet_data;
    }

    public class EN_Protocol
    {
        public static void Connect(UdpClient client, IPEndPoint target)
        {
            EN_PacketConnect packet;
            packet.packet_type = EN_PACKET_TYPE.CONNECT;
            packet.packet_data = -1;

            client.Connect(target);

            Send(client, packet);
        }

        public static void Send(UdpClient client, object msg)
        {
            byte[] bytes = ObjectToBytes(msg);
            client.Send(bytes, bytes.Length);
        }
        
        public static void SendText(UdpClient client, string msg)
        {
            byte[] b1 = ObjectToBytes((int)EN_PACKET_TYPE.MESSAGE);
            byte[] b2 = StringToBytes(msg);

            byte[] bytes = new byte[b1.Length + b2.Length];
            Buffer.BlockCopy(b1, 0, bytes, 0, b1.Length);
            Buffer.BlockCopy(b2, 0, bytes, b1.Length, b2.Length);

            client.Send(bytes, bytes.Length);
        }

        public static byte[] StringToBytes(string str)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
            return bytes;
        }

        public static string BytesToString(byte[] bytes)
        {
            string s = System.Text.Encoding.ASCII.GetString(bytes);
            return s;
        }

        public static byte[] ObjectToBytes(object str)
        {
            int size = Marshal.SizeOf(str);

            byte[] arr = new byte[size];
            System.IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static T BytesToObject<T>(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            object result = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

            handle.Free();

            return (T)result;
        }

        public static EN_PACKET_TYPE BytesToType(byte[] bytes)
        {
            return (EN_PACKET_TYPE)BitConverter.ToInt32(bytes, 0);
        }
    }
}



/*
 * 
 *             EN_Message msg;
            msg.message_type = (EN_PACKET_TYPE)BitConverter.ToInt32(bytes, 0);

            byte[] bytes_data = new byte[bytes.Length-4];
            Buffer.BlockCopy(bytes, 4, bytes_data, 0, bytes.Length - 4);
*/