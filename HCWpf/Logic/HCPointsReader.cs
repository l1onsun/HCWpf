using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;

namespace HCWpf
{
    class HCPointsReader
    {
        public int NameOrder = 0;
        public int XOrder = 1;
        public int YOrder = 2;
        public List<HCPoint> Points;

        private int skipedRows = 0;


        public HCPointsReader()
        {
            Points = new();
        }

        public int SkipedRows
        {
            get => skipedRows;
        }

        private bool TrySavePoint(string[] fields)
        {
            if (fields.Length <= Math.Max(NameOrder, Math.Max(XOrder, YOrder)))
            {
                return false;
            }

            bool xIsDouble = double.TryParse(fields[XOrder], out double x);
            bool yIsDouble = double.TryParse(fields[YOrder], out double y);
            if (xIsDouble && yIsDouble)
            {
                Points.Add(new(fields[NameOrder], x, y));
                return true;
            }
            return false;
        }

        public void ReadCsv(string path, string delimiters = ",")
        {
            using TextFieldParser csvParser = new(path);
            csvParser.SetDelimiters(delimiters);
            csvParser.HasFieldsEnclosedInQuotes = true;

            while (!csvParser.EndOfData)
            {
                string[] fields = csvParser.ReadFields();
                bool success = TrySavePoint(fields);
                if (!success)
                {
                    skipedRows += 1;
                }
            }
        }
    }
}
