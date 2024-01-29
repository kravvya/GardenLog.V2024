using Xunit.Abstractions;

namespace PlantHarvest.IntegrationTest
{
    public  class DateAlgorithmTests
    {
        private readonly ITestOutputHelper _output;

        public DateAlgorithmTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Get_NumberOfMondaysEachMonth_Should()
        {
            DateTime newYear = (new DateTime(DateTime.Now.Year, 1, 1)).AddDays(-1);
            KeyValuePair<DateTime, DateTime> oldDates = new KeyValuePair<DateTime, DateTime>(newYear, newYear);

            for (int month = 1; month < 13; month++)
            {
                var weeks = GetNumberOfMondays(month);
                _output.WriteLine($"{month} month has {weeks} weeks.");

                for (int week = 1; week <= weeks; week++)
                {
                    var dates = GetWeekRange(month, week, oldDates.Value);

                    _output.WriteLine($"Week range {dates.Key} - {dates.Value} for {month} month - {week} week");

                    if (month == 1 && week == 1)
                    {
                        oldDates = new(dates.Key.AddDays(-6), dates.Key.AddDays(-1));
                    }
                        Assert.False(dates.Key == oldDates.Key);
                        Assert.True(dates.Key.AddDays(-1) == oldDates.Value);
                    

                    oldDates = dates;

                }
            }
            Assert.True(true);
        }

        private KeyValuePair<DateTime, DateTime> GetWeekRange(int month, int week, DateTime previousDate)
        {
            int year = DateTime.Now.Year;
            DateTime firstDayOfMonth = new DateTime(year, month, 1);

            int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            int dayOfWeek = 1;

            DateTime targetDate = firstDayOfMonth.AddDays((week - 1) * 7 + dayOfWeek - firstDayOfWeek);
            //if we ended up in the previous month - lets roll to current month
            if (previousDate > targetDate)
            {
                targetDate = targetDate.AddDays(7);
            }
            DateTime targetDateEnd = targetDate.AddDays(6);

            return new KeyValuePair<DateTime, DateTime>(targetDate, targetDateEnd);
        }

        private int GetNumberOfMondays(int month)
        {
            int numberOfMondays = 0;


            int year = DateTime.Now.Year;


            for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
            {
                if (new DateTime(year, month, day).DayOfWeek == DayOfWeek.Monday)
                {
                    numberOfMondays++;
                }
            }
          
            Console.WriteLine($"There is {numberOfMondays} Mondays in {month}");


            return numberOfMondays;
        }
    }
}
