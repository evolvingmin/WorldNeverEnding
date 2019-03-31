using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ChallengeKit
{
    // todo : 언리얼 프로젝트에서 본거긴 한데, 일종의 config 프로퍼티를 줘서 해당 config가 있으면
    // 런타임에서 알아서 해당 데이터를 파싱해서 채워 넣어주도록 하자. 편하더라고...
    public static class CsvTableHandler
    {
        public enum StreamMode
        {
            AppData,
            Resource
        }

        public struct ColumnInfo
        {
            public int index;
            public string name;
        }

        public class Row
        {
            private Table table = null;
            private List<string> data = new List<string>();

            public Row(Table table, List<string> data)
            {
                this.table = table;
                this.data.AddRange(data);
            }

            public T Get<T>(string name)
            {
                int targetIndex = table.GetIndex(name);

                if (targetIndex == -1)
                {
                    T returnValue = (T)Convert.ChangeType(0, typeof(T));
                    return returnValue;
                }
                else
                {

                    T returnValue = (T)Convert.ChangeType(data[targetIndex], typeof(T));
                    return returnValue;
                }
            }

            public object Get(string name, Type type)
            {
                int targetIndex = table.GetIndex(name);

                if (targetIndex == -1)
                {
                    return Convert.ChangeType(0, type); // 타입.
                }
                else
                {
                    return Convert.ChangeType(data[targetIndex], type); // 타입.
                }
            }

            public object CovertToParsedRow(Type ParsingType)
            {
                var Fields = ParsingType.GetFields();
                object ParsedRow = Activator.CreateInstance(ParsingType);

                foreach (var Field in Fields)
                {
                    Field.SetValue(ParsedRow, Get(Field.Name, Field.FieldType));
                }

                return ParsedRow;
            }

            public Dictionary<string, T> QueryByContainedString<T>(string value)
            {
                Dictionary<string, T> returnTable = new Dictionary<string, T>();

                List<string> matched = new List<string>();
                foreach (var key in table.ColumnHeader.Keys)
                {
                    if (key.Contains(value))
                    {
                        matched.Add(key);
                    }
                }

                foreach (var key in matched)
                {
                    returnTable.Add(key, Get<T>(key));
                }

                return returnTable;
            }

            public T GetAt<T>(int index)
            {
                return (T)Convert.ChangeType(data[index], typeof(T));
            }

            public List<string> GetAllData()
            {
                return data;
            }

            public bool Replace(List<string> newValue)
            {
                data.Clear();
                data.AddRange(newValue);
                return true;
            }

            public void ReplaceColumn(string columnName, string newValue)
            {
                int targetIndex = table.GetIndex(columnName);

                if (targetIndex == -1)
                {
                    Debug.LogWarning("TargetColumn Does not Exist" + columnName);
                }
                else
                {
                    data[table.GetIndex(columnName)] = newValue;
                }
            }

            public void Save()
            {
                table.Save();
            }
        }

        public class Table
        {
            private Dictionary<string, ColumnInfo> columnHeader = new Dictionary<string, ColumnInfo>();
            private List<Row> rows = new List<Row>();
            private string path = null;
            private string name = "";

            public List<Row> Rows { get { return rows; } }
            public int Length {  get { return rows.Count; } }
            public Dictionary<string, ColumnInfo> ColumnHeader { get { return columnHeader; } }

            public Table(string TableName, StreamMode LoadMode)
            {
                name = TableName;
                switch (LoadMode)
                {
                    case StreamMode.AppData:
                        path = AppDataPath;
                        path += ( TableName + ".csv" );
                        break;
                    case StreamMode.Resource:
                        path = ResourcePath + TableName;
                        break;
                }

                switch (LoadMode)
                {
                    case StreamMode.AppData:
                        using (CsvFile.CsvFileReader reader = new CsvFile.CsvFileReader(path))
                        {
                            ParseCsvToTable(reader);
                        }
                        break;
                    case StreamMode.Resource:
                        TextAsset testAsset = Resources.Load<TextAsset>(path);

                        if (testAsset == null)
                            return;

                        MemoryStream stream = new MemoryStream(testAsset.bytes);
                        using (CsvFile.CsvFileReader reader = new CsvFile.CsvFileReader(stream))
                        {
                            ParseCsvToTable(reader);
                        }
                        stream.Close();
                        break;
                }

            }

            private void ParseCsvToTable(CsvFile.CsvFileReader reader)
            {
                int index = 0;
                List<string> column = new List<string>();
                while (reader.ReadRow(column))
                {
                    // Column Header.
                    if (index == 0)
                    {
                        for (int i = 0; i < column.Count; i++)
                        {
                            columnHeader.Add(column[i], new ColumnInfo() { index = i, name = column[i] });
                        }
                    }
                    // Datas
                    else
                    {
                        rows.Add(new Row(this, column));
                    }
                    index++;
                }
            }

            public void Add(List<string> item)
            {
                int newIndex = Rows.Count + 1;
                item[0] = Convert.ToString(newIndex);
                rows.Add(new Row(this, item));
            }

            public bool Save(StreamMode SaveMode = StreamMode.AppData)
            {
                switch (SaveMode)
                {
                    case StreamMode.AppData:
                        path = ( AppDataPath + name + ".csv" );
                        break;
                    case StreamMode.Resource:
                        return false;
                }

                return Save(path);
            }

            public bool Save(string path)
            {
                if (!System.IO.File.Exists(path))
                {
                    if (!System.IO.Directory.Exists(AppDataPath))
                    {
                        System.IO.Directory.CreateDirectory(AppDataPath);
                    }

                    using (System.IO.FileStream fs = System.IO.File.Create(path))
                    {
                    }
                }

                using (var writer = new CsvFile.CsvFileWriter(path))
                {
                    List<string> columns = new List<string>(columnHeader.Count);

                    var headerEnumerator = columnHeader.Values.GetEnumerator();
                    while (headerEnumerator.MoveNext())
                    {
                        var current = headerEnumerator.Current;
                        columns.Add(current.name);
                    }

                    writer.WriteRow(new List<string>(columns));
                    columns.Clear();

                    foreach (Row row in rows)
                    {
                        columns.AddRange(row.GetAllData());
                        writer.WriteRow(new List<string>(columns));
                        columns.Clear();
                    }
                }

                return true;
            }

            public Row GetAt(int index)
            {
                return rows[index];
            }

            public Row FindRow<T>(string columnName, T condition)
            {

                return rows.Find(x => Compare(x.Get<T>(columnName), condition));
            }

            public bool Replace<T>(string columnName, T condition, List<string> newValue)
            {
                Row target = FindRow(columnName, condition);
                return target.Replace(newValue);
            }

            // 현 테이블에 있는 모든 Data를 제거한다.
            public void Refresh()
            {
                rows.Clear();
            }

            public List<Row> FindRows<T>(string columnName, T condition)
            {
                return rows.FindAll(x => Compare(x.Get<T>(columnName), condition));
            }

            public ColumnInfo GetColumnInfo(string name)
            {
                return columnHeader[name];
            }

            public int GetIndex(string name)
            {
                //Debug.LogFormat("({0}) GetIndex From ({1})", name, this.name);
                if (columnHeader.ContainsKey(name))
                {
                    return columnHeader[name].index;
                }
                else
                {
                    return -1;
                }
            }

            public bool Compare<T>(T x, T y)
            {
                return EqualityComparer<T>.Default.Equals(x, y);
            }

            public List<T> ConvertoGenericList<T>()
            {
                List<T> ConvertedList = new List<T>();

                foreach (var row in rows)
                {
                    ConvertedList.Add((T)row.CovertToParsedRow(typeof(T)));
                }

                return ConvertedList;
            }

        }

        private static Dictionary<string, Table> tableMap = new Dictionary<string, Table>();

        public static string AppDataPath { get { return string.Format("{0}/Table/", UnityEngine.Application.persistentDataPath); } }
        public static string ResourcePath = "Table/";

        private static Table GenerateTable(string tableName, StreamMode LoadSteam)
        {
            return new Table(tableName, LoadSteam);
        }

        public static Table Get(string tableName, StreamMode LoadSteam)
        {
            if (!tableMap.ContainsKey(tableName))
            {
                Table newTable = GenerateTable(tableName, LoadSteam);
                tableMap.Add(tableName, newTable);
            }

            return tableMap[tableName];
        }


        public static bool Save(string tableName, StreamMode saveMode = StreamMode.AppData)
        {
            bool ReturnValue = false;
            if (!tableMap.ContainsKey(tableName))
            {
                //Warning!
                return ReturnValue;
            }

            switch (saveMode)
            {
                case StreamMode.AppData:
                    ReturnValue = tableMap[tableName].Save(string.Format("{0}{1}.csv", AppDataPath, tableName));
                    break;
                case StreamMode.Resource:
                    Debug.Log("Not Supported : (" + tableName + ") " + saveMode);
                    break;
            }

            return ReturnValue;
        }
    }
}


