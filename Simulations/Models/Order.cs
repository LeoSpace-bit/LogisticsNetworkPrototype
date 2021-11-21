namespace Simulations
{
    internal class Order
    {
        internal Customer Customer { get; set; }
        internal Customer Recipient { get; set; }
        internal Road Path { get; set; }
        internal string CurrentState { get; set; }
        internal int Cost { get; set; }
        internal Department ReceivingPoint { get; set; } // FROM
        internal Department DeliveryPoint { get; set; }  // TO
        internal string TrackingCode { get; set; }

    }
}
