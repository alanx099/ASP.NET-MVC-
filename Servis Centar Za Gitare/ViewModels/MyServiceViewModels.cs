namespace Servis_Centar_Za_Gitare.ViewModels
{
    public class MyServiceViewModel
    {
        public bool IsLinked { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<MyGuitarViewModel> Guitars { get; set; } = Array.Empty<MyGuitarViewModel>();
    }

    public class MyGuitarViewModel
    {
        public long Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public IEnumerable<MyServiceOrderViewModel> ServiceOrders { get; set; } = Array.Empty<MyServiceOrderViewModel>();
    }

    public class MyServiceOrderViewModel
    {
        public long Id { get; set; }
        public string RepairType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
