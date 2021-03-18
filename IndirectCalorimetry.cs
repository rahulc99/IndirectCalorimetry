using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace IndirectCalorimetrys
{
    class IndirectCalorimetry
    {   
        // Constants to store the names of the properties of an IndirectCalorimetry.
        public const string DateTime = "DT";
        public const string Animal = "Animal";
        public const string VO2_M = "VO2_M";
        public const string VCO2_M = "VCO2_M";
        public const string VH2O_M = "VH2O_M";
        public const string kcal_hr_M = "kcal_hr_M";
        public const string RER_M = "RER_M";
        public const string VOC_M = "VOC_M";
        public const string H2_M = "H2_M";
        public const string FoodInA_M = "FoodInA_M";
        public const string WaterInA_M = "WaterInA_M";
        public const string PedSpeed_Mnz = "PedSpeed_Mnz";
        public const string PedMeters_M = "PedMeters_M";
        public const string PedMeters_R = "PedMeters_R";
        public const string AllMeters_M = "AllMeters_M";
        public const string AllMeters_R = "AllMeters_R";
        public const string XBreak_R = "XBreak_R";
        public const string YBreak_R = "YBreak_R";
        public const string ZBreak_R = "ZBreak_R";
        public const string EnviroTemp_M = "EnviroTemp_M";
        public const string EnviroRH_M = "EnviroRH_M";

        // A Map to store the values for each property for this IndirectCalorimetry.        
        private Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private string filepath;

        public DateTime ZTTime {get; set;}

        public string AnimalName {get;set;}

        /// <summary>
        /// Creates an instance of the IndirectCalorimetry for the specified proteinRefId
        /// </summary>
        /// <param name="filepath">
        /// The experiment file path 
        /// </param>
        /// <param name="proteinRefId">
        /// The Protein Reference Id 
        /// </param>
        public IndirectCalorimetry(string filepath)
        {
            this.filepath = filepath;
        }

        public string FilePath
        {
            get
            {
                return this.filepath;
            }
        }

        public void Add(string key, string val)
        {
            values[key] = val;
        }

        public void Add(int index, string val)
        {
            string key = GetKey(index);
            values[key] = val;
        }

        public string Get(int index)
        {
            string key = GetKey(index);
            return values[key];
        }

        private string GetKey(int index)
        {
            string key = Names[index];
            return key;
        }

        public string Get(string key, string defValue = null)
        {
            if (values.TryGetValue(key, out string val))
            {
                return val;
            }

            return defValue;
        }

        public static int GetIndex(string name)
        {
            for (int i = 0; i < Names.Length; i++)
            {
                if (string.Equals(name, Names[i], StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }  

            return -1;          
        }

        public static void EnsurePropertyNameIsValid (int index, string name)
        {
            if (index >= 0 && index < Names.Length)
            {
                if (string.Equals(name, Names[index], StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                throw new Exception($"The specified property name is not valid. Name={name}");
            }
        }


        public string[] PropertyNames
        {
            get
            {
                return Names;
            }
        }

        public void WriteHeader(TextWriter writer, string separator)
        {
            for (int i=0; i<Names.Length; i++)
            {
                if (i != 0)
                {
                    writer.Write(separator);
                }

                writer.Write(Names[i]);
            }

            writer.WriteLine();
        }

        public void WriteValues(TextWriter writer, string separator)
        {
            for (int i=0; i<Names.Length; i++)
            {
                if (i != 0)
                {
                    writer.Write(separator);
                }
                
                writer.Write(this.Get(i));
            }

            writer.WriteLine();
        }

        // Array of property names in the right order
        private static string[] Names
        {
            get
            {
                return new string[] {
                DateTime,
                Animal,
                VO2_M,
                VCO2_M,
                VH2O_M,
                kcal_hr_M,
                RER_M,
                VOC_M,
                H2_M,
                FoodInA_M,
                WaterInA_M,
                PedSpeed_Mnz,
                PedMeters_M,
                PedMeters_R,
                AllMeters_M,
                AllMeters_R,
                XBreak_R,
                YBreak_R,
                ZBreak_R,
                EnviroTemp_M,
                EnviroRH_M};
            }
        }
    }
}
