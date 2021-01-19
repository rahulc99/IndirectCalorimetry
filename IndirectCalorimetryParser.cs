using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace IndirectCalorimetrys
{
    /// <summary>
    /// Class that implements the logic for parsing the experiment file.
    /// </summary>
    class IndirectCalorimetryParser
    {
        /// <summary>
        /// A static method that parses the specified file and returns a list of IndirectCalorimetry instances
        /// </summary>
        /// <param name="args">
        /// The experiment file path.
        /// </param>
        /// <returns>
        /// Returns list of experiments that are contained in the specified file.
        /// </returns>
        public static List<IndirectCalorimetry> Parse(string filepath)
        {
            bool parsingHeaderCompleted = false;
            List<IndirectCalorimetry> list = new List<IndirectCalorimetry>();

            // Open and read the file line by line until we reach the end of the file.
            using (StreamReader reader = File.OpenText(filepath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();


                    // If the line starts with "DateTime", we know that we have reached the header line
                    if (line.StartsWith($"\"{IndirectCalorimetry.DateTime}\"") || 
                        line.StartsWith(IndirectCalorimetry.DateTime))
                    {
                        ParseHeader(filepath, line);
                        parsingHeaderCompleted = true;
                        continue;
                    }
                    

                    // Only after the header is processed, we know that the rest of the file
                    // contains the experiment records. Ignore the line if we have not yet 
                    // processed the header.
                    if (parsingHeaderCompleted)
                    {
                        IndirectCalorimetry exp = ParseRow(filepath, line);
                        list.Add(exp);
                    }
                }
            }

            return list;
        }

        private static string[] ParseHeader(string filepath, string line)
        {
            // The lines in experiment file is separated by Comma. Split the line into tokens
            string[] columns = line.Split(',');
            for (int i= 0; i < columns.Length; i++)
            {
                string column = columns[i].Trim();
                if (column.StartsWith("\""))
                {
                    column = column.Substring(1);
                }

                if (column.EndsWith("\""))
                {
                    column = column.Substring(0, column.Length - 1);
                }

                // Ensure that the column name is valid for this index
                IndirectCalorimetry.EnsurePropertyNameIsValid(i, column);
            }

            return columns;
        }

        private static IndirectCalorimetry ParseRow(string filepath, string line)
        {
            IndirectCalorimetry exp = new IndirectCalorimetry(filepath);

            // The lines in experiment file is separated by TABs. Split the line into tokens 
            string[] values = line.Split(',');
            for (int i=0; i<values.Length; i++)
            {
                string value = values[i].Trim();

                if (value.StartsWith("\""))
                {
                    value = value.Substring(1);
                }

                if (value.EndsWith("\""))
                {
                    value = value.Substring(0, value.Length - 1);
                }

                exp.Add(i, value);                   
            }

            return exp;
        }
    }
}
