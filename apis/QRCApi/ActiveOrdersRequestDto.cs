namespace QRCApi
{
    public class ActiveOrdersRequestDto
    {
        public ActiveOrderRequestPositionDto[] Positions { get; set; }
    }

    public class ActiveOrderRequestPositionDto
    {
        public string ClientId { get; set; }
        public string Security { get; set; }
    }
}