namespace EagleEye.Photo.Domain.Commands.Inner
{
    public class Timestamp
    {
        public int Year { get; set; }

        public int? Month { get; set; }

        public int? Day { get; set; }

        public int? Hour { get; set; }

        public int? Minutes { get; set; }

        public int? Seconds { get; set; }

        public static Timestamp Create(
            int year,
            int? month = null,
            int? day = null,
            int? hour = null,
            int? minutes = null,
            int? seconds = null)
        {
            return new Timestamp
            {
                Year = year,
                Month = month,
                Day = day,
                Hour = hour,
                Minutes = minutes,
                Seconds = seconds,
            };
        }
    }
}
