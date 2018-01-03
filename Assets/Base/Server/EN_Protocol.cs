﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ErnstNetworking.Protocol
{
    public enum EN_UDP_PACKET_TYPE
    {
        TRANSFORM = 0
    }

    public enum EN_TCP_PACKET_TYPE
    {
        CONNECT = 0,
        DISCONNECT,
        MESSAGE
    }

    struct EN_ClientInfo
    {
        public EN_ClientInfo(int i, string n)
        {
            id = i;
            name = n;
        }
        public int id;
        public string name;
    }
    
    struct EN_TransformData
    {
        public float tX; public float tY; public float tZ;
        public float rX; public float rY; public float rZ;
    }

    struct EN_PacketTransform
    {
        public EN_UDP_PACKET_TYPE   packet_type;
        public int                  packet_client_id;

        public float tX; public float tY; public float tZ;
        public float rX; public float rY; public float rZ;
    }

    struct EN_PacketConnect
    {
        public EN_TCP_PACKET_TYPE   packet_type;
        public int                  packet_client_id;
        public Guid                 packet_client_guid;
        // ...invisible name string <---
    }

    struct EN_PacketDisconnect
    {
        public EN_TCP_PACKET_TYPE   packet_type;
        public int                  packet_client_id;
        public Guid                 packet_client_guid;
    }

    struct EN_PacketMessage
    {
        public EN_TCP_PACKET_TYPE   packet_type;
        public byte[]               packet_data;
    }

    public class EN_Protocol
    {
        public static void Connect(UdpClient client, IPEndPoint server, string name, Guid guid)
        {
            client.Connect(server);
            //client.Send(bytes, bytes.Length);
        }

        public static void Connect(TcpClient client, IPEndPoint server, string name, Guid guid)
        {
            try
            {
                EN_PacketConnect packet;
                packet.packet_type = EN_TCP_PACKET_TYPE.CONNECT;
                packet.packet_client_id = -1;
                packet.packet_client_guid = guid;

                byte[] b1 = ObjectToBytes(packet);
                byte[] b2 = StringToBytes(name);

                byte[] bytes = new byte[b1.Length + b2.Length];
                Buffer.BlockCopy(b1, 0, bytes, 0, b1.Length);
                Buffer.BlockCopy(b2, 0, bytes, b1.Length, b2.Length);

                client.Connect(server);

                NetworkStream stream = client.GetStream();
                stream.Write(bytes, 0, bytes.Length);
                //connected
            }
            catch
            {
                //not connected
            }
        }

        public static void Send(UdpClient client, object msg)
        {
            byte[] bytes = ObjectToBytes(msg);
            client.Send(bytes, bytes.Length);
        }
        
        public static void SendText(NetworkStream stream, string msg)
        {
            byte[] b1 = ObjectToBytes((int)EN_TCP_PACKET_TYPE.MESSAGE);
            byte[] b2 = StringToBytes(msg);

            byte[] bytes = new byte[b1.Length + b2.Length];
            Buffer.BlockCopy(b1, 0, bytes, 0, b1.Length);
            Buffer.BlockCopy(b2, 0, bytes, b1.Length, b2.Length);

            stream.Write(bytes, 0, bytes.Length);
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

        public static byte[] ObjectToBytes(object o)
        {
            int size = Marshal.SizeOf(o);

            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            
            Marshal.StructureToPtr(o, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            
            return arr;
        }

        public static byte[] ObjectToBytes(object o, int skip)
        {
            int size = Marshal.SizeOf(o);

            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(o, ptr, true);
            Marshal.Copy(ptr, arr, skip, size);
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

        public static EN_UDP_PACKET_TYPE BytesToUDPType(byte[] bytes)
        {
            return (EN_UDP_PACKET_TYPE)BitConverter.ToInt32(bytes, 0);
        }

        public static EN_TCP_PACKET_TYPE BytesToTCPType(byte[] bytes)
        {
            return (EN_TCP_PACKET_TYPE)BitConverter.ToInt32(bytes, 0);
        }
    }
}