using Domain.InterFaces;


namespace Infrastructure.Services
{
    public class DateTimeProvider :IDateTimeProvider
    {
        public const string TimeZone = "Arab Standard Time";
        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                               TimeZoneInfo.FindSystemTimeZoneById(TimeZone));

        public DateTimeKind Kind => DateTimeKind.Local;

        public bool SupportsMultipleTimeZone => false;

        public DateTime Normalize(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            }

            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime.ToLocalTime();
            }

            return dateTime;
        }
      
    }
}
