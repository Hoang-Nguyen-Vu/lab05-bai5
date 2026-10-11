// Hoàng Nguyên Vũ
// 202419020
using System;
namespace DeviceManagement
{
    public abstract class Device
    {
        protected string deviceID {get; }
        protected string deviceName;
        protected int usageYear;
        protected decimal purchasePrice;
        public enum DeviceStatus
        {
            Active,
            UnderMaintenance,
            Retired
        }
        protected DeviceStatus status;
        public string DeviceID => deviceID;
        public DeviceStatus Status => status;
        public int UsageYear => usageYear;
        public int Age => DateTime.Now.Year - usageYear;   // số năm đã dùng
        public Device(string deviceID, string deviceName, int usageYear, decimal purchasePrice, DeviceStatus status = DeviceStatus.Active)
        {
            if (deviceID == null || deviceID == "")
            {
                throw new ArgumentException("Mã thiết bị không rỗng");
            }
            if (deviceName == null || deviceName == "")
            {
                throw new ArgumentException("Tên thiết bị không rỗng");
            }
            if (usageYear > 2026)
            {
                throw new ArgumentException("Năm sử dụng không lớn hơn năm hiện tại");
            }
            if (purchasePrice <= 0)
            {
                throw new ArgumentException("Giá mua lớn hơn 0");
            }
            this.deviceID = deviceID;
            this.deviceName = deviceName;
            this.usageYear = usageYear;
            this.purchasePrice = purchasePrice;
            this.status = status;
        }
        public abstract decimal CalculateAnnualMaintenanceCost();
        public override string ToString()
        {
            return $"Mã: {deviceID} - Tên: {deviceName} - Năm sử dụng: {usageYear} - Giá mua: {purchasePrice} - Trạng thái: {status}";
        }        
    }
}