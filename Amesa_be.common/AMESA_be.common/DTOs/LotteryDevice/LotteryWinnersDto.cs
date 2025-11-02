namespace AMESA_be.common.DTOs.LotteryDevice
{
    public class LotteryWinnersDto
    {
        public long LotteryId { get; set; }
        public long FirstPlaceWinnerId { get; set; }
        public long SecondPlaceWinnerId { get; set; }
        public long ThirdPlaceWinnerId { get; set; }
        public DateTime ExecutedOn { get; set; }
    }
}