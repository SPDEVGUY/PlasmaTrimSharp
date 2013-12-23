using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasmaSharpDriver
{

    public enum PlasmaTrimCommandEnum : byte
    {
        ImmediateWrite = 0x00,
        ImmediateRead = 0x01,
        StartPlayingSequence = 0x02,
        StopPlayingSequence = 0x03,
        WriteTableLength = 0x04, //Non-Volitile Write
        ReadTableLength = 0x05,
        WriteTableEntry = 0x06, //Non-Volitile Write
        ReadTableEntry = 0x07,
        WriteDeviceName = 0x08, //Non-Volitile Write
        ReadDeviceName = 0x09,
        ReadDeviceSerial = 0x0A,
        WriteBrightness = 0x0B, //Non-Volitile Write
        ReadBrightness = 0x0C,
    }
}
