using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace IndirectCalorimetrys
{   
    class Program
    {
        // Main driver method
        static void Main(string[] args)
        {
            
            int lightsOnTime = 6; // Read the lights on time from the args later.
            int lightsOffTime = 14; 

            string filepath = "/Users/rahul/Desktop/Schwartz_Project/2020.11.13_HFHS_Final-Calorimetry-1hbin_formatted.csv";
            
            // Create a FileInfo object using the filepath
            FileInfo fileInfo = new FileInfo(filepath);

            //Parse the file and get list of IndirectCalorimetry ojects
            List<IndirectCalorimetry> list = IndirectCalorimetryParser.Parse(fileInfo.FullName);
            
            List<IndirectCalorimetry> updatedList = new List<IndirectCalorimetry>();

            foreach (IndirectCalorimetry item in list)
            {
                string animalName = item.Get(IndirectCalorimetry.Animal);
                if (string.IsNullOrWhiteSpace(animalName) || animalName.Equals("NA", StringComparison.OrdinalIgnoreCase))
                {
                    // Invalid animal. Ignore this item.
                    continue;
                }

                string vo2_m = item.Get(IndirectCalorimetry.VO2_M);
                if (string.IsNullOrWhiteSpace(vo2_m) || vo2_m.Equals("NA", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                item.AnimalName = animalName;

                if (DateTime.TryParseExact(item.Get(IndirectCalorimetry.DateTime), 
                "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                {
                    DateTime newDt = dt.Subtract(TimeSpan.FromHours(lightsOnTime));
                    item.ZTTime = newDt;
                    updatedList.Add(item);
                }
            }

            Dictionary<string, List<IndirectCalorimetry>> itemsGroupedByAnimal = updatedList
                .GroupBy(o => o.AnimalName)
                .ToDictionary(g => g.Key, g => g.ToList());

            Dictionary<string, List<Result>> results = new Dictionary<string, List<Result>>();

            foreach(var animal in itemsGroupedByAnimal.Keys)
            {
                List<IndirectCalorimetry> items = itemsGroupedByAnimal[animal];

                int day = 0;
                DateTime? previousMaxDateTime = null;
                while (true)
                {                
                    Result result = new Result() { Animal = animal};
                    int i = 0;
                    for (; i < 24 ; i++)
                    {
                        int index = i + day * 24;

                        if (index >= items.Count)
                        {
                            break;
                        }

                        IndirectCalorimetry item = items[index];
                        float v02_m = float.Parse(item.Get(IndirectCalorimetry.VO2_M));
                        if (i == 0)
                        {
                            result.StartTime = item.ZTTime;
                            result.MaxVO2_M = v02_m;
                            result.MaxOccurredAtTime = item.ZTTime;
                            result.MinVO2_M = v02_m;
                            result.MinOccurredAtTime = item.ZTTime;
                        }
                        else
                        {
                            result.EndTime = item.ZTTime;
                            if (result.MaxVO2_M < v02_m)
                            {
                                result.MaxVO2_M = v02_m;
                                result.MaxOccurredAtTime = item.ZTTime;
                            } 
                            
                            if (result.MinVO2_M > v02_m)
                            {
                                result.MinVO2_M = v02_m;
                                result.MinOccurredAtTime = item.ZTTime;
                            }
                        }
                    }

                    DateTime tempDate = new DateTime(
                        result.MaxOccurredAtTime.Year,
                        result.MaxOccurredAtTime.Month,
                        result.MaxOccurredAtTime.Day,
                        lightsOffTime,
                        0,
                        0);

                    TimeSpan timeDiff = result.MaxOccurredAtTime - tempDate;
                    result.LatencyToPeakInMin = timeDiff.TotalMinutes;

                    result.PeakToPeakAmplitude = result.MaxVO2_M - result.MinVO2_M;

                    if (day == 0)
                    {
                        previousMaxDateTime = result.MaxOccurredAtTime;
                    }
                    else
                    {
                        result.PeriodInMin = (result.MaxOccurredAtTime - previousMaxDateTime.Value).TotalMinutes;
                        previousMaxDateTime = result.MaxOccurredAtTime;
                    }


                    List<Result> r;
                    if (!results.TryGetValue(animal, out r))
                    {
                        r = new List<Result>();
                        results.Add(animal, r);
                    }

                    r.Add(result);

                    if (i < 24)
                    {
                        break;
                    }

                    day++;
                }
            }

            string resultsFileName = $"{fileInfo.Directory.FullName}/{Path.GetFileNameWithoutExtension(fileInfo.FullName)}_Results.csv";
            using (var stream = new StreamWriter(
                File.Open(resultsFileName, 
                    FileMode.Create, 
                    FileAccess.ReadWrite, 
                    FileShare.ReadWrite)))
            {
                stream.Write("Animal");
                stream.Write(",");
                stream.Write("StartTime");
                stream.Write(",");
                stream.Write("EndTime");
                stream.Write(",");
                stream.Write("MaxOccurredAtTime");
                stream.Write(",");
                stream.Write("MaxVO2_M");
                stream.Write(",");
                stream.Write("MinOccurredAtTime");
                stream.Write(",");
                stream.Write("MinVO2_M");
                stream.Write(",");
                stream.Write("LatencyToPeakInMin");
                stream.Write(",");
                stream.Write("PeakToPeakAmplitude");
                stream.Write(",");
                stream.Write("PeriodInMin");
                stream.WriteLine();
                
                var animalNames = results.Keys.ToList();
                animalNames.Sort();

                foreach(var animalName in animalNames)
                {
                    var resultItems = results[animalName];
                    foreach(var item in resultItems)
                    {
                        stream.Write(item.Animal);
                        stream.Write(",");
                        stream.Write(item.StartTime);
                        stream.Write(",");
                        stream.Write(item.EndTime);
                        stream.Write(",");
                        stream.Write(item.MaxOccurredAtTime.ToString("G"));
                        stream.Write(",");
                        stream.Write(item.MaxVO2_M);
                        stream.Write(",");
                        stream.Write(item.MinOccurredAtTime.ToString("G"));
                        stream.Write(",");
                        stream.Write(item.MinVO2_M);
                        stream.Write(",");
                        stream.Write(item.LatencyToPeakInMin);
                        stream.Write(",");
                        stream.Write(item.PeakToPeakAmplitude);
                        stream.Write(",");
                        stream.Write(item.PeriodInMin);
                        stream.WriteLine();
                    }
                }
            }

            Console.WriteLine();
        }
    }

    class Result
    {
        public string Animal {get;set;}
        public DateTime StartTime {get;set;}
        public DateTime EndTime {get;set;}
        public float MaxVO2_M {get;set;}
        public DateTime MaxOccurredAtTime {get;set;}
        public float MinVO2_M {get;set;}
        public DateTime MinOccurredAtTime {get;set;}
        public Double LatencyToPeakInMin {get;set;}
        public float PeakToPeakAmplitude {get;set;}
        public Double PeriodInMin {get;set;}
    }
}
