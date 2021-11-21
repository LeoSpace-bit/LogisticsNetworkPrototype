namespace Simulations
{
    internal class Way
    {
        internal City InitialCity { get; set; }
        internal City DestinationCity { get; set; }
        internal int Cost { get; set; }
        internal Type DeliveryType { get; set; }

        internal Way(City initialCity, City destinationCity, int cost, string deliveryType)
        {
            InitialCity = initialCity;
            DestinationCity = destinationCity;
            Cost = cost;

            Type value;
            switch(deliveryType)
            {
                case "Land":
                    value = Type.Land;
                    break;

                case "Water":
                    value = Type.Water;
                    break;
                default:
                    value = Type.Air;
                    break;
            }

            DeliveryType = value;
        }

    }
}
