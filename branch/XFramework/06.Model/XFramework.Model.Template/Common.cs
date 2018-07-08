using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Data;
using SchemaExplorer;
using System.IO;
using System.Xml;

namespace XTemplate
{
    public class Common
    {
        /// <summary>
        /// 根据列名获取实体对应的字段名，如 _level
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static string GetFieldName(string columnName)
        {
            string name = string.Empty;
            if (!string.IsNullOrEmpty(columnName))
            {
                name = columnName.Substring(0, 1).ToLower();
                if (name.Length > 0) name += columnName.Substring(1);

                name = "_" + name;
            }

            return name;
        }

        /// <summary>
        /// 根据表名取实体名称
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string GetEntityName(TableSchema table)
        {
            string name = table.Name;

            //去掉可能存在的架构信息
            if (name.IndexOf('.') >= 0)
            {
                string[] namespaces = name.Split(new Char[] { '.' });
                name = namespaces[namespaces.Length - 1];
            }

            //去掉可能存在的模块信息
            if (name.IndexOf('_') >= 0)
            {
                //string[] namespaces = name.Split(new Char[] { '_' });
                //name = namespaces[namespaces.Length - 1];
            }

            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }

        ///// <summary>
        ///// ORDER BY 不能比较或排序 text、ntext 和 image 数据类型，除非使用 IS NULL 或 LIKE 运算符。
        ///// </summary>
        ///// <param name="table"></param>
        ///// <returns></returns>
        //public static string GetOrderBy(TableSchema table)
        //{
        //    //Row_Number() Over(Order By <%for(int i=0;i<SourceTable.Columns.Count;i++){%><% if("text|ntext|image|".IndexOf(SourceTable.Columns[i].NativeType ?? "")>0) continue; %>[<%= SourceTable.Columns[i].Name %>]<%if(i<SourceTable.Columns.Count-1){%>,<%}%><%} %>) as [XRowNum]
        //    string[] exclude = new[] { "text", "ntext", "image" };
        //    StringBuilder builder = new StringBuilder();

        //    int count = table.Columns.Count();
        //    for (int i = 0; i < count; i++)
        //    {
        //        string nativeType = table.Columns[]
        //    }

        //    return string.Empty;
        //}

        /// <summary>
        /// 将生成的文件添加到项目
        /// </summary>
        /// <param name="directory">csproj路径</param>
        /// <param name="names">文件列表</param>
        public static void IncludeFile(string directory, IList<string> names)
        {
            if (names == null || names.Count == 0) return;
            if (!Directory.Exists(directory)) 
                throw new DirectoryNotFoundException(string.Format("directory {0} not exists", directory));

            string xmlNs = "http://schemas.microsoft.com/developer/msbuild/2003";
            DirectoryInfo d = new DirectoryInfo(directory);

            string projFileName = string.Format("{0}.csproj", d.Name);
            string projFullName = Path.Combine(d.FullName, projFileName);
            if (!File.Exists(projFullName)) return;

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(projFullName);
            XmlNamespaceManager m = new XmlNamespaceManager(xdoc.NameTable);
            m.AddNamespace("u", xmlNs);

            foreach (string fileName in names)
            {
                XmlNodeList items = xdoc.SelectNodes("/u:Project/u:ItemGroup", m);
                for (int i = 0; i < items.Count; i++)
                {
                    XmlNode g = items[i];
                    XmlNodeList compiles = g.SelectNodes("u:Compile", m);
                    if (compiles == null || compiles.Count == 0) continue;

                    string includeName = string.Format(@"g\Table\{0}.cs", fileName); ;
                    XmlNode node = g.SelectSingleNode(string.Format("u:Compile[@Include='{0}']", includeName), m);
                    if (node == null)
                    {
                        XmlElement xel = xdoc.CreateElement("Compile", xmlNs);
                        xel.SetAttribute("Include", includeName);
                        g.AppendChild(xel);
                    }
                }
            }

            //save xml
            xdoc.Save(projFullName);
        }

        /// <summary>
        /// 获取对应的CSHARP变量类型
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetCSharpVariableType(ColumnSchema column)
        {
            if (column.Name.EndsWith("TypeCode"))
            {
                return column.Name;
            }

            switch (column.DataType)
            {
                case DbType.AnsiString: return "string";
                case DbType.AnsiStringFixedLength: return "string";
                case DbType.Binary: return "byte[]";
                case DbType.Boolean: return column.AllowDBNull ? "Nullable<bool>" : "bool";
                case DbType.Byte: return column.AllowDBNull ? "Nullable<byte>" : "byte";
                case DbType.Currency: return column.AllowDBNull ? "Nullable<decimal>" : "decimal";
                case DbType.Date: return column.AllowDBNull ? "Nullable<DateTime>" : "DateTime";
                case DbType.DateTime: return column.AllowDBNull ? "Nullable<DateTime>" : "DateTime"; ;
                case DbType.Decimal: return column.AllowDBNull ? "Nullable<decimal>" : "decimal";
                case DbType.Double: return column.AllowDBNull ? "Nullable<double>" : "double";
                case DbType.Guid: return "Guid";
                case DbType.Int16: return column.AllowDBNull ? "Nullable<short>" : "short";
                case DbType.Int32: return column.AllowDBNull ? "Nullable<int>" : "int";
                case DbType.Int64: return column.AllowDBNull ? "Nullable<long>" : "long";
                case DbType.Object: return "object";
                case DbType.SByte: return column.AllowDBNull ? "Nullable<sbyte>" : "sbyte";
                case DbType.Single: return column.AllowDBNull ? "Nullable<float>" : "float";
                case DbType.String: return "string";
                case DbType.StringFixedLength: return "string";
                case DbType.Time: return "TimeSpan";
                case DbType.UInt16: return column.AllowDBNull ? "Nullable<ushort>" : "ushort";
                case DbType.UInt32: return column.AllowDBNull ? "Nullable<uint>" : "uint";
                case DbType.UInt64: return column.AllowDBNull ? "Nullable<ulong>" : "ulong";
                case DbType.VarNumeric: return column.AllowDBNull ? "Nullable<decimal>" : "decimal";
                case DbType.Xml: return "string";
                default: return "__UNKNOWN__" + column.NativeType;
            }
        }

        /// <summary>
        /// 获取对应的CSHARP变量类型
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string GetCSharpVariableType(ParameterSchema param)
        {
            if (param.Name.EndsWith("TypeCode"))
            {
                return param.Name;
            }

            switch (param.DataType)
            {
                case DbType.AnsiString: return "string";
                case DbType.AnsiStringFixedLength: return "string";
                case DbType.Binary: return "byte[]";
                case DbType.Boolean: return param.AllowDBNull ? "Nullable<bool>" : "bool";
                case DbType.Byte: return param.AllowDBNull ? "Nullable<byte>" : "byte";
                case DbType.Currency: return param.AllowDBNull ? "Nullable<decimal>" : "decimal";
                case DbType.Date: return param.AllowDBNull ? "Nullable<DateTime>" : "DateTime";
                case DbType.DateTime: return param.AllowDBNull ? "Nullable<DateTime>" : "DateTime"; ;
                case DbType.Decimal: return param.AllowDBNull ? "Nullable<decimal>" : "decimal";
                case DbType.Double: return param.AllowDBNull ? "Nullable<double>" : "double";
                case DbType.Guid: return "Guid";
                case DbType.Int16: return param.AllowDBNull ? "Nullable<short>" : "short";
                case DbType.Int32: return param.AllowDBNull ? "Nullable<int>" : "int";
                case DbType.Int64: return param.AllowDBNull ? "Nullable<long>" : "long";
                case DbType.Object: return "object";
                case DbType.SByte: return param.AllowDBNull ? "Nullable<sbyte>" : "sbyte";
                case DbType.Single: return param.AllowDBNull ? "Nullable<float>" : "float";
                case DbType.String: return "string";
                case DbType.StringFixedLength: return "string";
                case DbType.Time: return "TimeSpan";
                case DbType.UInt16: return param.AllowDBNull ? "Nullable<ushort>" : "ushort";
                case DbType.UInt32: return param.AllowDBNull ? "Nullable<uint>" : "uint";
                case DbType.UInt64: return param.AllowDBNull ? "Nullable<ulong>" : "ulong";
                case DbType.VarNumeric: return param.AllowDBNull ? "Nullable<decimal>" : "decimal";
                default: return "__UNKNOWN__" + param.NativeType;
            }
        }
    }
}
