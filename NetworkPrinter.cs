//Hoàng Nguyên Vũ
// 202419020 
using System;
namespace DeviceManagement
{
    public class NetworkPrinter : Printer, INetworkable
    {
        private string ipAddress = "";
        private bool isConnected = false;
        public NetworkPrinter(string deviceID, string deviceName, int usageYear, decimal purchasePrice,
            string printType, int pageCount, bool hasNetworkSupport, string configuredIp,
            DeviceStatus status = DeviceStatus.Active)
            : base(deviceID, deviceName, usageYear, purchasePrice, printType, pageCount, hasNetworkSupport, status)
        {
        }
        public string IpAddress => ipAddress;
        public bool IsConnected => isConnected;
        public void Connect(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("Địa chỉ IP không được rỗng");
            if (isConnected)
                throw new InvalidOperationException("Thiết bị đang kết nối, không thể kết nối lại");
            this.ipAddress = ipAddress;
            this.isConnected = true;
        }
        public void Disconnect()
        {
            this.ipAddress = "";
            this.isConnected = false;
        }
    }
}