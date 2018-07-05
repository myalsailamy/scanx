﻿using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using System.Linq;
using System.Management;
using WIA;
using ScanX.Core.Models;
using System.Runtime.InteropServices;
using ScanX.Core.Args;
using System.Drawing;

namespace ScanX.Core
{
    //for more info https://ourcodeworld.com/articles/read/382/creating-a-scanning-application-in-winforms-with-csharp
    public class DeviceClient
    {
        public const uint WIA_ERROR_PAPER_EMPTY = 0x80210003;

        public event EventHandler OnTransferCompleted;
        public event EventHandler OnImageScanned;

        public List<string> GetAllPrinters()
        {
            List<string> result = new List<string>();

            var printers = PrinterSettings.InstalledPrinters;

            foreach (string item in printers)
            {
                result.Add(item);
            }

            return result;

        }

        public List<ScannerDevice> GetAllScanners()
        {
            var result = new List<ScannerDevice>();

            foreach (IDeviceInfo info in new DeviceManagerClass().DeviceInfos)
            {
                if (info.Type == WiaDeviceType.ScannerDeviceType)
                {
                    result.Add(new ScannerDevice()
                    {
                        DeviceId = info.DeviceID,
                        Name = info.Properties["Name"].get_Value().ToString(),
                        Description = info.Properties["Description"]?.get_Value()?.ToString(),
                        Port = info.Properties["Port"]?.get_Value()?.ToString()
                    });
                }
            }

            return result;
        }

        public void Scan(int deviceID)
        {
            var deviceManager = new DeviceManager();

            try
            {
                var device = deviceManager.DeviceInfos[deviceID];

                var connectedDevice = device.Connect();

                int page = 1;

                do
                {
                    try
                    {
                        var img = (ImageFile)connectedDevice.Items[1].Transfer(FormatID.wiaFormatJPEG);

                        byte[] data = (byte[])img.FileData.get_BinaryData();

                        OnImageScanned?.Invoke(this, new DeviceImageScannedEventArgs(data, img.FileExtension, page));

                        page++;
                    }
                    catch (COMException ex)
                    {
                        if ((uint)ex.HResult != WIA_ERROR_PAPER_EMPTY)
                        {
                            OnTransferCompleted?.Invoke(this, new DeviceTransferCompletedEventArgs(page));
                            break;
                        }


                        throw;
                    }
                }
                while (true);



            }
            catch (Exception ex)
            {

            }
        }

        public void ScanSinglePage(string deviceID)
        {

            IDeviceInfo device = null;

            foreach (IDeviceInfo info in new DeviceManagerClass().DeviceInfos)
            {
                if (info.DeviceID == deviceID)
                {
                    device = info;
                    break;
                }
            }

            if (device == null)
                throw new Exception($"Unable to find device id {deviceID}");

            var connectedDevice = device.Connect();

            int page = 1;

            var img = (ImageFile)connectedDevice.Items[1].Transfer(FormatID.wiaFormatJPEG);

            byte[] data = (byte[])img.FileData.get_BinaryData();

            OnImageScanned?.Invoke(this, new DeviceImageScannedEventArgs(data, img.FileExtension, page));

            page++;


        }

        public void ScanWithUI(int deviceID)
        {
            CommonDialogClass dlg = new CommonDialogClass();


        }
    }
}
