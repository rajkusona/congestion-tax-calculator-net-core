using System;
using congestion.calculator;
using System.Linq;

public partial class CongestionTaxCalculator
{
    /**
         * Calculate the total toll fee for one day
         *
         * @param vehicle - the vehicle
         * @param dates   - date and time of all passes on one day
         * @return - the total congestion tax for that day
         */

    public int GetTax(Vehicle vehicle, DateTime[] dates)
    {
        DateTime intervalStart = dates[0];
        int totalFee = 0;
        foreach (var (nextFee, tempFee, minutes) in from DateTime date in dates
                                                    let nextFee = GetTollFee(date, vehicle)
                                                    let tempFee = GetTollFee(intervalStart, vehicle)
                                                    let diffInMillies = date.Millisecond - intervalStart.Millisecond
                                                    let minutes = diffInMillies / 1000 / 60
                                                    select (nextFee, tempFee, minutes))
        {
            int tempFee2 = tempFee;

            if (minutes <= 60)
            {
                if (totalFee > 0) totalFee -= tempFee2;
                if (nextFee >= tempFee2) tempFee2 = nextFee;
                totalFee += tempFee2;
            }
            else
            {
                totalFee += nextFee;
            }
        }

        if (totalFee > 60) totalFee = 60;
        return totalFee;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return false;
        String vehicleType = vehicle.GetVehicleType();
        return IsTollFreeVehicleValue(vehicleType);
    }

    private static bool IsTollFreeVehicleValue(string vehicleType)
    {
        return vehicleType.Equals(TollFreeVehicles.Motorcycle.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Tractor.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Military.ToString());
    }

    public int GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        int hour = date.Hour;
        int minute = date.Minute;

        switch (hour)
        {
            case 6 when minute >= 0 && minute <= 29:
                return 8;
            case 6 when minute >= 30 && minute <= 59:
                return 13;
            case 7 when minute >= 0 && minute <= 59:
                return 18;
            case 8 when minute >= 0 && minute <= 29:
                return 13;
            default:
                return DefaultTollFee(hour, minute);
        }
    }

    private static int DefaultTollFee(int hour, int minute)
    {
        if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
        else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
        else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
        else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
        else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
        else return 0;
    }

    private Boolean IsTollFreeDate(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        if (year == 2013 && (month == 1 && day == 1 ||
                month == 3 && (day == 28 || day == 29) ||
                month == 4 && (day == 1 || day == 30) ||
                month == 5 && (day == 1 || day == 8 || day == 9) ||
                month == 6 && (day == 5 || day == 6 || day == 21) ||
                month == 7 ||
                month == 11 && day == 1 ||
                month == 12 && (day == 24 || day == 25 || day == 26 || day == 31)))
        {
            return true;
        }
        return false;
    }
    private enum TollFreeVehicles
    {
        Motorcycle = 0,
        Tractor = 1,
        Emergency = 2,
        Diplomat = 3,
        Foreign = 4,
        Military = 5
    }
}