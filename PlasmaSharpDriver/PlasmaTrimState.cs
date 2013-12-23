using System;
using System.Runtime.InteropServices;

namespace PlasmaSharpDriver
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlasmaTrimState
    {
        public byte R0;
        public byte G0;
        public byte B0;
        public byte R1;
        public byte G1;
        public byte B1;
        public byte R2;
        public byte G2;
        public byte B2;
        public byte R3;
        public byte G3;
        public byte B3;
        public byte R4;
        public byte G4;
        public byte B4;
        public byte R5;
        public byte G5;
        public byte B5;
        public byte R6;
        public byte G6;
        public byte B6;
        public byte R7;
        public byte G7;
        public byte B7;
        public byte Brightness;
        public byte XX0;
        public byte XX1;
        public byte XX2;
        public byte XX3;
        public byte XX4;
        public byte XX5;
    }


    public static class StateSerializationExtensions
    {
        public static byte[] ToBytes(this PlasmaTrimState state)
        {
            return SerializeMessage(state);
        }
        public static PlasmaTrimState ToPlasmaTrimState(this byte[] bytes)
        {
            return DeserializeMsg<PlasmaTrimState>(bytes);
        }



        public static Byte[] SerializeMessage<T>(T msg) where T : struct
        {
            int objsize = Marshal.SizeOf(typeof(T));
            Byte[] ret = new Byte[objsize];
            IntPtr buff = Marshal.AllocHGlobal(objsize);
            Marshal.StructureToPtr(msg, buff, true);
            Marshal.Copy(buff, ret, 0, objsize);
            Marshal.FreeHGlobal(buff);
            return ret;
        }
        public static T DeserializeMsg<T>(Byte[] data) where T : struct
        {
            int objsize = Marshal.SizeOf(typeof(T));
            IntPtr buff = Marshal.AllocHGlobal(objsize);
            Marshal.Copy(data, 0, buff, objsize);
            T retStruct = (T)Marshal.PtrToStructure(buff, typeof(T));
            Marshal.FreeHGlobal(buff);
            return retStruct;
        }
    }
}
