﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;

namespace GeradorExcelAnalitico
{

    /// <summary>
    /// Simple CSV export
    /// Example:
    ///   CsvExport myExport = new CsvExport();
    ///
    ///   myExport.AddRow();
    ///   myExport["Region"] = "New York, USA";
    ///   myExport["Sales"] = 100000;
    ///   myExport["Date Opened"] = new DateTime(2003, 12, 31);
    ///
    ///   myExport.AddRow();
    ///   myExport["Region"] = "Sydney \"in\" Australia";
    ///   myExport["Sales"] = 50000;
    ///   myExport["Date Opened"] = new DateTime(2005, 1, 1, 9, 30, 0);
    ///
    /// Then you can do any of the following three output options:
    ///   string myCsv = myExport.Export();
    ///   myExport.ExportToFile("Somefile.csv");
    ///   byte[] myCsvData = myExport.ExportToBytes();
    /// </summary>
    public  class CsvExport
    {
        /// <summary>
        /// To keep the ordered list of column names
        /// </summary>
        readonly List<string> _fields = new List<string>();

        /// <summary>
        /// The list of rows
        /// </summary>
        readonly List<Dictionary<string, object>> _rows = new List<Dictionary<string, object>>();

        /// <summary>
        /// The current row
        /// </summary>
        Dictionary<string, object> CurrentRow { get { return _rows[_rows.Count - 1]; } }

        /// <summary>
        /// Set a value on this column
        /// </summary>
        public object this[string field]
        {
            set
            {
                // Keep track of the field names, because the dictionary loses the ordering
                if (!_fields.Contains(field)) _fields.Add(field);
                CurrentRow[field] = value;
            }
        }

        /// <summary>
        /// Call this before setting any fields on a row
        /// </summary>
        public void AddRow()
        {
            _rows.Add(new Dictionary<string, object>());
        }

        /// <summary>
        /// Add a list of typed objects, maps object properties to CsvFields
        /// </summary>
        public void AddRows<T>(List<T> list)
        {
            if (list.Any())
            {
                foreach (var obj in list)
                {
                    AddRow();
                    var values = obj.GetType().GetProperties();
                    foreach (var value in values)
                    {
                        this[value.Name] = value.GetValue(obj, null);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a value to how it should output in a csv file
        /// If it has a comma, it needs surrounding with double quotes
        /// Eg Sydney, Australia -> "Sydney, Australia"
        /// Also if it contains any double quotes ("), then they need to be replaced with quad quotes[sic] ("")
        /// Eg "Dangerous Dan" McGrew -> """Dangerous Dan"" McGrew"
        /// </summary>
        private string MakeValueCsvFriendly(object value)
        {
            if (value == null) return "";
            if (value is INullable && ((INullable)value).IsNull) return "";
            if (value is DateTime)
            {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                    return ((DateTime)value).ToString("yyyy-MM-dd");
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            string output = value.ToString();
            if (output.Contains(",") || output.Contains("\"") || output.Contains("\n") || output.Contains("\r"))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            if (output.Length > 30000) //cropping value for stupid Excel
            {
                if (output.EndsWith("\""))
                    output = output.Substring(0, 30000) + "\"";
                else
                    output = output.Substring(0, 30000);
            }

            return output.Length <= 32767 ? output : output.Substring(0, 32767);
        }

        /// <summary>
        /// Output all rows as a CSV returning a string
        /// </summary>
        public string Export()
        {
            var sb = new StringBuilder();

            sb.AppendLine("sep=,");

            // The header
            sb.AppendLine(string.Join(",", _fields.ToArray()));
            //sb.AppendLine();

            // The rows
            foreach (Dictionary<string, object> row in _rows)
            {
                _fields.Where(f => !row.ContainsKey(f)).ToList().ForEach(k =>
                {
                    row[k] = null;
                });
                sb.AppendLine(string.Join(",", _fields.Select(field => MakeValueCsvFriendly(row[field])).ToArray()));
                
            }

            return sb.ToString();
        }

        /// <summary>
        /// Output all rows as a CSV returning a string for a append into a file
        /// </summary>
        private string ExportToAppend()
        {
            var sb = new StringBuilder();

            // The rows
            foreach (Dictionary<string, object> row in _rows)
            {
                _fields.Where(f => !row.ContainsKey(f)).ToList().ForEach(k =>
                {
                    row[k] = null;
                });
                sb.Append(string.Join(",", _fields.Select(field => MakeValueCsvFriendly(row[field])).ToArray()));
                //sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Export to a File append new lines to it
        /// </summary>
        /// <param name="path"></param>
        public void ExportToFile(string path)
        {
            if(File.Exists(path))
                File.AppendAllLines(path, new[] {ExportToAppend()});
            else
                File.WriteAllText(path, Export());
        }

        /// <summary>
        /// Exports as raw UTF8 bytes
        /// </summary>
        public byte[] ExportToBytes()
        {
            var data = Encoding.UTF8.GetBytes(Export());
            return Encoding.UTF8.GetPreamble().Concat(data).ToArray();
        }

    }
}
