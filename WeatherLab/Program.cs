using System;
using System.Linq;


namespace WeatherLab
{
    class Program
    {
        static string dbfile = @".\data\climate.db";

        static void Main(string[] args)
        {
            var measurements = new WeatherSqliteContext(dbfile).Weather;

            var total_2020_precipitation = (from measurement in measurements
                                           where measurement.year == 2020
                                           select (measurement.precipitation)).Sum();
            Console.WriteLine($"Total precipitation in 2020: {total_2020_precipitation} mm\n");
            /*foreach(var measurement in measurements)
            {
                Console.WriteLine($"{measurement.day}");
            }*/
            //
            // Heating Degree days have a mean temp of < 18C
            //   see: https://en.wikipedia.org/wiki/Heating_degree_day
            //

            var hdd_list = from measurement in measurements
                           where measurement.meantemp < 18
                           group measurement by measurement.year into grp
                           select new { year = grp.Key, count = grp.Count() };

            //
            // Cooling degree days have a mean temp of >=18C
            //

            var cdd_list = from measurement in measurements
                           where measurement.meantemp >= 18
                           group measurement by measurement.year into grp
                           select new { year = grp.Key, count = grp.Count() };

            //
            // Most Variable days are the days with the biggest temperature
            // range. That is, the largest difference between the maximum and
            // minimum temperature
            //
            // Oh: and number formatting to zero pad.
            // 
            // For example, if you want:
            //      var x = 2;
            // To display as "0002" then:
            //      $"{x:d4}"
            //

            Console.WriteLine("Year\tHDD\tCDD");

            var result = from hdd in hdd_list
                         join cdd in cdd_list on hdd.year equals cdd.year
                         orderby cdd.year
                         select new { year = cdd.year, hdd = hdd.count, cdd = cdd.count };
            foreach (var row in result)
            {
                Console.WriteLine($"{row.year:d4}\t{row.hdd:d4}\t{row.cdd:d4}");
            }

            Console.WriteLine("\nTop 5 Most Variable Days");
            Console.WriteLine("YYYY-MM-DD\tDelta");

            var mostVariableDays = (from measurement in measurements
                                   orderby measurement.maxtemp - measurement.mintemp descending
                                   select new { year = measurement.year, month = measurement.month, day = measurement.day, delta = measurement.maxtemp - measurement.mintemp }).Take(5) ;
             foreach(var mostVariableDay in mostVariableDays)
            {
                Console.WriteLine($"{mostVariableDay.year:d4}-{mostVariableDay.month:d2}-{mostVariableDay.day:d2}\t{mostVariableDay.delta:0.00}");
            }                      
        }
    }
}
