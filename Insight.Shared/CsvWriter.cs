﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Insight.Shared
{
    /// <summary>
    /// Minimum version of a csv serializer.
    /// </summary>
    public sealed class CsvWriter
    {
        public bool Header { get; set; }

        /// <summary>
        /// Examples
        /// https://msdn.microsoft.com/de-de/library/kfsatb94(v=vs.110).aspx
        /// E3  1.054E+003, use e for lower case.
        /// F3  1054.322
        /// </summary>
        public string NumberFormat { get; set; } = "F3";

        public void Process<T>(List<T> items, Action<string> writeLine)
        {
            if (items.Any() == false)
            {
                return;
            }

            var type = items.First().GetType();

            var propertyInfos = type.GetProperties();
            var names = propertyInfos.Select(pi => pi.Name).ToList();

            WriteHeader(names, writeLine);
            WriteItems(propertyInfos, writeLine, items);
        }

        public string ToCsv<T>(List<T> items)
        {
            if (items == null || !items.Any())
            {
                return "";
            }

            var builder = new StringBuilder();
            Process(items, line => builder.AppendLine(line));
            return builder.ToString();
        }

        public void ToCsv<T>(string filePath, List<T> items)
        {
            using (var stream = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                Process(items, line => stream.WriteLine(line));
            }
        }

        private bool IsNumberFormat(PropertyInfo propertyInfo)
        {
            return (propertyInfo.PropertyType == typeof(double) ||
                    propertyInfo.PropertyType == typeof(float)) &&
                   !string.IsNullOrEmpty(NumberFormat);
        }

        private void WriteHeader(IEnumerable<string> names, Action<string> writeLine)
        {
            if (Header)
            {
                var header = new List<string>();
                foreach (var name in names)
                {
                    header.Add(name);
                }

                writeLine(string.Join(",", header));
            }
        }

        private void WriteItems<T>(PropertyInfo[] propertyInfos, Action<string> writeLine, List<T> items)
        {
            if (items == null || !items.Any())
            {
                return;
            }

            var numberFormat = "{0:" + NumberFormat + "}";
            var line = new List<string>(propertyInfos.Length);
            foreach (var item in items)
            {
                line.Clear();
                foreach (var propertyInfo in propertyInfos)
                {
                    var value = propertyInfo.GetValue(item);

                    if (IsNumberFormat(propertyInfo))
                    {
                        line.Add(string.Format(CultureInfo.InvariantCulture, numberFormat, value));
                    }
                    else
                    {
                        var str = value.ToString();
                        if (str.Any(c => c == ',' || c == ' ' || c == '\t'))
                        {
                            str = "\"" + str + "\"";
                        }

                        line.Add(str);
                    }
                }

                writeLine(string.Join(",", line));
            }
        }
    }
}