using System;
using Nager.Date;
using TollFeeCalculator;

public class TollCalculator
{
    /// <summary>
    /// Calculate the total toll fee for one day.
    /// Dates must all belong to the same day. Unsorted input is handled internally.
    /// </summary>
    public int GetTollFee(Vehicle vehicle, DateTime[] dates)
    {
        if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
        if (dates == null || dates.Length == 0) return 0;

        Array.Sort(dates);

        if (dates[0].Year < 2013)
            throw new ArgumentException("Congestion tax was not applicable before 2013.", nameof(dates));

        if (dates[dates.Length - 1].Date != dates[0].Date)
            throw new ArgumentException("All dates must belong to the same day.", nameof(dates));

        DateTime intervalStart = dates[0];
        int intervalMaxFee = 0;
        int totalFee = 0;

        foreach (DateTime date in dates)
        {
            int fee = GetTollFee(date, vehicle);
            double minutes = (date - intervalStart).TotalMinutes;

            if (minutes <= 60)
            {
                if (fee > intervalMaxFee) intervalMaxFee = fee;
            }
            else
            {
                totalFee += intervalMaxFee;
                intervalStart = date;
                intervalMaxFee = fee;
            }
        }

        totalFee += intervalMaxFee;
        return Math.Min(totalFee, 60);
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        return vehicle?.IsTollFree ?? false;
    }

    public int GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        int hour = date.Hour;
        int minute = date.Minute;

        if (hour == 6 && minute < 30) return 8;
        if (hour == 6) return 13;
        if (hour == 7) return 18;
        if (hour == 8 && minute < 30) return 13;
        if ((hour == 8 && minute >= 30) || (hour > 8 && hour < 15)) return 8;
        if (hour == 15 && minute < 30) return 13;
        if (hour == 15 || hour == 16) return 18;
        if (hour == 17) return 13;
        if (hour == 18 && minute < 30) return 8;
        return 0;
    }

    private bool IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;

        if (date.Month == 7)
            return true;

        // Day before a public holiday is also toll-free
        return DateSystem.IsPublicHoliday(date, CountryCode.SE) ||
               DateSystem.IsPublicHoliday(date.AddDays(1), CountryCode.SE);
    }


}