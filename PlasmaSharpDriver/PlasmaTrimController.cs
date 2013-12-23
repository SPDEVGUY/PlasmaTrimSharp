using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace PlasmaSharpDriver
{
    //USB HID Commands from:
    //http://www.thephotonfactory.com/forum/viewtopic.php?f=5&t=104&p=169#p168
    //Pasted here for Permanence...
    /*
IMPORTANT NOTE: The flash memory on the device (like all flash memory) has a finite life of several hundred thousand writes, so it is vitally important that commands which alter the non-volatile parameters are not issued continuously in a loop or similar which could lead to premature wear on the long-term storage capabilities. All commands that write to non-volatile flash memory as marked as such as a reminder.

HID Commands are 32-byte packets: 1 command byte + 31 parameter bytes. Unused parameter bytes are generally just padded with zeros as the values. Note that for each command issued, there will be an echo'd reply of 32 bytes back from the device as an acknowledgement of valid command.

0x00 - Immediate Write To device
Note: In immediate mode, LEDs are written as 24-bit values for color, and a 0-100 in the Brightness field that will globally scale the output without reducing the dynamic range. Also note that the brightness is only retained in the immediate operation of the device and does not set the non-volatile brightness value recalled for stand-alone operation, etc. See command 11 (0x0B) for writing non-volatile brightness
cmd: [0x00] [R0] [G0] [B0] [R1] [G1] [B1] [R2] [G2] [B2] [R3] [G3] [B3] [R4] [G4] [B4] [R5] [G5] [B5] [R6] [G6] [B6] [R7] [G7] [B7] [brightness] [xx] [xx] [xx] [xx] [xx] [xx]
reply: (echo of the above line)

0x01 - Immediate Read From Device
cmd: [0x01]
rply:[001] [R0] [G0] [B0] [R1] [G1] [B1] [R2] [G2] [B2] [R3] [G3] [B3] [R4] [G4] [B4] [R5] [G5] [B5] [R6] [G6] [B6] [R7] [G7] [B7]

0x02 - Start Playing Sequence
cmd: [0x02]
reply: (echo of the above line)

0x03 - Stop Playing Sequence
cmd: [0x03]
reply: (echo of the above line)

0x04 - Write Table Length NOTE: THIS COMMAND WRITES TO NON-VOLATILE MEMORY
cmd: [0x04] [length]
reply: (echo of the above line)

0x05 - Read Table Length
cmd: [0x05]
reply:[0x05] [length]

0x06 - Write Table Entry NOTE: THIS COMMAND WRITES TO NON-VOLATILE MEMORY
Note: Table entries are a packed 4-bit value for each LED. S1/S2 are the speeds for the hold/fade times
cmd: [0x06] [index] [R1G1] [B1R2] [G2B2] [R3G3] [B3R4] [G4B4] [R5G5] [B5R6] [G6B6] [R7G7] [B7R8] [G8B8] [S1S2]
reply: (echo of the above line)

0x07 - Read Table Entry
Note: Table entries are a packed 4-bit value for each LED. S1/S2 are the speeds for the hold/fade times
cmd: [0x07] [index] [R1G1] [B1R2] [G2B2] [R3G3] [B3R4] [G4B4] [R5G5] [B5R6] [G6B6] [R7G7] [B7R8] [G8B8] [S1S2]
rply:[0x07] [index] [R1G1] [B1R2] [G2B2] [R3G3] [B3R4] [G4B4] [R5G5] [B5R6] [G6B6] [R7G7] [B7R8] [G8B8] [S1S2]

[b]0x08 - Write Device Name NOTE: THIS COMMAND WRITES TO NON-VOLATILE MEMORY
Note: This allows writing a "plaintext" name to easily identify the device when used in a multi-unit setup. The name stored can be up to 26 characters, and should have a null-terminator placed at the end for any name less than the full 26 characters.
cmd: [0x08] [N00] [N01] [N02] [N03] [N04] [N05] [N06] [N07] [N08] [N09] [N10] [N11] [N12] [N13] [N14] [N15] [N16] [N17] [N18] [N19] [N20] [N21] [N22] [N23] [N24] [N25]
reply: (echo of the above line)

[b]0x09 - Read Device Name
cmd: [0x09]
reply: [0x09] [N00] [N01] [N02] [N03] [N04] [N05] [N06] [N07] [N08] [N09] [N10] [N11] [N12] [N13] [N14] [N15] [N16] [N17] [N18] [N19] [N20] [N21] [N22] [N23] [N24] [N25]

[b]0x0A - Read Device Serial Number
Note: This returns the serial number printed on the label on the back of the PlasmaTrim, and can be used to absolutely identify a unit even though the USB OS enumeration order may change
cmd: [0x0A]
reply: [0x0A] [S00] [S01] [S02] [S03]

[b]0x0B - Store Brightness NOTE: THIS COMMAND WRITES TO NON-VOLATILE MEMORY
Note: Obviously it would be a bad idea to program this with a zero, as the PlasmaTrim would then appear to not be functioning when playing external sequences 
cmd: [0x0B] [brightness 0-100%]
reply: (echo of the above line)

[b]0x0C - Recall Brightness
cmd: [0x0C]
reply:[0x0C] [brightness]
     */
    public class PlasmaTrimController :  IDisposable
    {
        protected const int PlasmaTrimVid = 0x26f3;
        protected const int PlasmaTrimPid = 0x1000; //Note: This may change with new product versions?
        //protected old_UsbHidController PlasmaTrimIx;
        protected bool IsDisposed;
        protected bool IsConnected;
        protected UsbRegistry Registry;
        protected UsbDevice Device;
        protected UsbEndpointWriter Writer;
        

        protected PlasmaTrimController(UsbRegistry deviceRegistry)
        {
            Registry = deviceRegistry;
        }

        public static List<PlasmaTrimController> GetPlasmatrims()
        {
            var devices = UsbDevice.AllDevices;
            var result = new List<PlasmaTrimController>();
            foreach (UsbRegistry device in devices)
            {
                if(device.Vid == PlasmaTrimVid && device.Pid == PlasmaTrimPid)
                    result.Add(new PlasmaTrimController(device));
            }

            return result;
        }

        public bool Open()
        {
            IsConnected = Registry.Open(out Device);
            if (IsConnected)
            {
                var wholeUsbDevice = Device as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                // open write endpoint 1.
                Writer = Device.OpenEndpointWriter(WriteEndpointID.Ep02);
            }
            return IsConnected;
        }
        
        public bool StopPlayingSequence()
        {
            if (IsDisposed || !IsConnected) return false;
            
            return Send(PlasmaTrimCommandEnum.StopPlayingSequence);
        }
        public bool StartPlayingSequence()
        {
            if (IsDisposed || !IsConnected) return false;

            return Send(PlasmaTrimCommandEnum.StartPlayingSequence);
        }

        public bool SetImmediateState(PlasmaTrimState state)
        {
            if (IsDisposed || !IsConnected) return false;

            return Send(PlasmaTrimCommandEnum.ImmediateWrite, state.ToBytes());
        }

        /// <summary>
        /// Send a command with options.
        /// </summary>
        /// <param name="cmd">The command to send</param>
        /// <param name="options">A serialized byte array of options</param>
        /// <returns>True if bytes were written</returns>
        protected bool Send(PlasmaTrimCommandEnum cmd, params byte[] options)
        {
            var b = GetPadded((byte)cmd);
            if(options != null && options.Length > 0)
                for (var i = 0; i < options.Length; i++) b[i + 1] = options[i];

            int written;
            Writer.Write(b, 100, out written);
            return written > 0;
        }

        

        #region Serialization helpers

        protected byte[] GetPadded(params byte[] bytes)
        {
            var result = new byte[32];
            bytes.CopyTo(result, 0);
            return result;
        }
        

        #endregion Serialization helpers

        public void Dispose()
        {
            Registry = null;
            IsDisposed = true;
            if (Device != null)
            {
                Writer.Abort();
                Writer.Dispose();
                Writer = null;
                if (Device.IsOpen)
                {
                    // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                    // it exposes an IUsbDevice interface. If not (WinUSB) the 
                    // 'wholeUsbDevice' variable will be null indicating this is 
                    // an interface of a device; it does not require or support 
                    // configuration and interface selection.
                    IUsbDevice wholeUsbDevice = Device as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // Release interface #0.
                        wholeUsbDevice.ReleaseInterface(0);

                    }

                    Device.Close();
                }
                Device = null;

                // Free usb resources
                UsbDevice.Exit();
            }


        }
    }
}
