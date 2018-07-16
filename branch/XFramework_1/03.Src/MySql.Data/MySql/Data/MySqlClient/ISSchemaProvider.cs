namespace MySql.Data.MySqlClient
{
    using MySql.Data.Common;
    using MySql.Data.Types;
    using System;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Text;

    internal class ISSchemaProvider : SchemaProvider
    {
        public ISSchemaProvider(MySqlConnection connection) : base(connection)
        {
        }

        protected override DataTable GetCollections()
        {
            DataTable collections = base.GetCollections();
            object[][] data = new object[][] { new object[] { "Views", 2, 3 }, new object[] { "ViewColumns", 3, 4 }, new object[] { "Procedure Parameters", 5, 1 }, new object[] { "Procedures", 4, 3 }, new object[] { "Triggers", 2, 4 } };
            SchemaProvider.FillTable(collections, data);
            return collections;
        }

        public override DataTable GetColumns(string[] restrictions)
        {
            string[] keys = new string[] { "TABLE_CATALOG", "TABLE_SCHEMA", "TABLE_NAME", "COLUMN_NAME" };
            DataTable table = this.Query("COLUMNS", null, keys, restrictions);
            table.Columns.Remove("CHARACTER_OCTET_LENGTH");
            table.TableName = "Columns";
            return table;
        }

        public override DataTable GetDatabases(string[] restrictions)
        {
            string[] keys = new string[] { "SCHEMA_NAME" };
            DataTable table = this.Query("SCHEMATA", "", keys, restrictions);
            table.Columns[1].ColumnName = "database_name";
            table.TableName = "Databases";
            return table;
        }

        internal void GetParametersFromShowCreate(DataTable parametersTable, string[] restrictions, DataTable routines)
        {
            if (routines == null)
            {
                routines = this.GetSchema("procedures", restrictions);
            }
            MySqlCommand command = base.connection.CreateCommand();
            foreach (DataRow row in routines.Rows)
            {
                string str = string.Format("SHOW CREATE {0} `{1}`.`{2}`", row["ROUTINE_TYPE"], row["ROUTINE_SCHEMA"], row["ROUTINE_NAME"]);
                command.CommandText = str;
                try
                {
                    string nameToRestrict = null;
                    if (((restrictions != null) && (restrictions.Length == 5)) && (restrictions[4] != null))
                    {
                        nameToRestrict = restrictions[4];
                    }
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        this.ParseProcedureBody(parametersTable, reader.GetString(2), row, nameToRestrict);
                    }
                    continue;
                }
                catch (SqlNullValueException exception)
                {
                    throw new InvalidOperationException(Resources.UnableToRetrieveSProcData, exception);
                }
            }
        }

        private string GetProcedureParameterLine(DataRow isRow)
        {
            string format = "SHOW CREATE {0} {1}.{2}";
            MySqlCommand command = new MySqlCommand(string.Format(format, isRow["ROUTINE_TYPE"], isRow["ROUTINE_SCHEMA"], isRow["ROUTINE_NAME"]), base.connection);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                string str4;
                reader.Read();
                if (reader.IsDBNull(2))
                {
                    return null;
                }
                string str2 = reader.GetString(1);
                string input = reader.GetString(2);
                SqlTokenizer tokenizer = new SqlTokenizer(input) {
                    AnsiQuotes = str2.IndexOf("ANSI_QUOTES") != -1,
                    BackslashEscapes = str2.IndexOf("NO_BACKSLASH_ESCAPES") == -1
                };
                for (str4 = tokenizer.NextToken(); str4 != "("; str4 = tokenizer.NextToken())
                {
                }
                int startIndex = tokenizer.Index + 1;
                str4 = tokenizer.NextToken();
                while ((str4 != ")") || tokenizer.Quoted)
                {
                    str4 = tokenizer.NextToken();
                    if ((str4 == "(") && !tokenizer.Quoted)
                    {
                        while ((str4 != ")") || tokenizer.Quoted)
                        {
                            str4 = tokenizer.NextToken();
                        }
                        str4 = tokenizer.NextToken();
                    }
                }
                return input.Substring(startIndex, tokenizer.Index - startIndex);
            }
        }

        public virtual DataTable GetProcedureParameters(string[] restrictions, DataTable routines)
        {
            DataTable parametersTable = new DataTable("Procedure Parameters");
            parametersTable.Columns.Add("ROUTINE_CATALOG", typeof(string));
            parametersTable.Columns.Add("ROUTINE_SCHEMA", typeof(string));
            parametersTable.Columns.Add("ROUTINE_NAME", typeof(string));
            parametersTable.Columns.Add("ROUTINE_TYPE", typeof(string));
            parametersTable.Columns.Add("PARAMETER_NAME", typeof(string));
            parametersTable.Columns.Add("ORDINAL_POSITION", typeof(int));
            parametersTable.Columns.Add("PARAMETER_MODE", typeof(string));
            parametersTable.Columns.Add("IS_RESULT", typeof(string));
            parametersTable.Columns.Add("DATA_TYPE", typeof(string));
            parametersTable.Columns.Add("FLAGS", typeof(string));
            parametersTable.Columns.Add("CHARACTER_SET", typeof(string));
            parametersTable.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
            parametersTable.Columns.Add("CHARACTER_OCTET_LENGTH", typeof(int));
            parametersTable.Columns.Add("NUMERIC_PRECISION", typeof(byte));
            parametersTable.Columns.Add("NUMERIC_SCALE", typeof(int));
            this.GetParametersFromShowCreate(parametersTable, restrictions, routines);
            return parametersTable;
        }

        private DataTable GetProcedures(string[] restrictions)
        {
            string[] keys = new string[] { "ROUTINE_CATALOG", "ROUTINE_SCHEMA", "ROUTINE_NAME", "ROUTINE_TYPE" };
            DataTable table = this.Query("ROUTINES", null, keys, restrictions);
            table.TableName = "Procedures";
            return table;
        }

        private DataTable GetProceduresWithParameters(string[] restrictions)
        {
            DataTable procedures = this.GetProcedures(restrictions);
            procedures.Columns.Add("ParameterList", typeof(string));
            foreach (DataRow row in procedures.Rows)
            {
                row["ParameterList"] = this.GetProcedureParameterLine(row);
            }
            return procedures;
        }

        protected override DataTable GetRestrictions()
        {
            DataTable restrictions = base.GetRestrictions();
            object[][] data = new object[][] { 
                new object[] { "Procedure Parameters", "Database", "", 0 }, new object[] { "Procedure Parameters", "Schema", "", 1 }, new object[] { "Procedure Parameters", "Name", "", 2 }, new object[] { "Procedure Parameters", "Type", "", 3 }, new object[] { "Procedure Parameters", "Parameter", "", 4 }, new object[] { "Procedures", "Database", "", 0 }, new object[] { "Procedures", "Schema", "", 1 }, new object[] { "Procedures", "Name", "", 2 }, new object[] { "Procedures", "Type", "", 3 }, new object[] { "Views", "Database", "", 0 }, new object[] { "Views", "Schema", "", 1 }, new object[] { "Views", "Table", "", 2 }, new object[] { "ViewColumns", "Database", "", 0 }, new object[] { "ViewColumns", "Schema", "", 1 }, new object[] { "ViewColumns", "Table", "", 2 }, new object[] { "ViewColumns", "Column", "", 3 }, 
                new object[] { "Triggers", "Database", "", 0 }, new object[] { "Triggers", "Schema", "", 1 }, new object[] { "Triggers", "Name", "", 2 }, new object[] { "Triggers", "EventObjectTable", "", 3 }
             };
            SchemaProvider.FillTable(restrictions, data);
            return restrictions;
        }

        protected override DataTable GetSchemaInternal(string collection, string[] restrictions)
        {
            DataTable schemaInternal = base.GetSchemaInternal(collection, restrictions);
            if (schemaInternal != null)
            {
                return schemaInternal;
            }
            switch (collection)
            {
                case "views":
                    return this.GetViews(restrictions);

                case "procedures":
                    return this.GetProcedures(restrictions);

                case "procedures with parameters":
                    return this.GetProceduresWithParameters(restrictions);

                case "procedure parameters":
                    return this.GetProcedureParameters(restrictions, null);

                case "triggers":
                    return this.GetTriggers(restrictions);

                case "viewcolumns":
                    return this.GetViewColumns(restrictions);
            }
            return null;
        }

        private DataTable GetTable(string sql)
        {
            DataTable dataTable = new DataTable();
            new MySqlDataAdapter(sql, base.connection).Fill(dataTable);
            return dataTable;
        }

        public override DataTable GetTables(string[] restrictions)
        {
            string[] keys = new string[] { "TABLE_CATALOG", "TABLE_SCHEMA", "TABLE_NAME", "TABLE_TYPE" };
            DataTable table = this.Query("TABLES", "TABLE_TYPE != 'VIEW'", keys, restrictions);
            table.TableName = "Tables";
            return table;
        }

        private DataTable GetTriggers(string[] restrictions)
        {
            string[] keys = new string[] { "TRIGGER_CATALOG", "TRIGGER_SCHEMA", "EVENT_OBJECT_TABLE", "TRIGGER_NAME" };
            DataTable table = this.Query("TRIGGERS", null, keys, restrictions);
            table.TableName = "Triggers";
            return table;
        }

        private DataTable GetViewColumns(string[] restrictions)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder("SELECT C.* FROM information_schema.columns C");
            builder2.Append(" JOIN information_schema.views V ");
            builder2.Append("ON C.table_schema=V.table_schema AND C.table_name=V.table_name ");
            if (((restrictions != null) && (restrictions.Length >= 2)) && (restrictions[1] != null))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "C.table_schema='{0}' ", new object[] { restrictions[1] });
            }
            if (((restrictions != null) && (restrictions.Length >= 3)) && (restrictions[2] != null))
            {
                if (builder.Length > 0)
                {
                    builder.Append("AND ");
                }
                builder.AppendFormat(CultureInfo.InvariantCulture, "C.table_name='{0}' ", new object[] { restrictions[2] });
            }
            if (((restrictions != null) && (restrictions.Length == 4)) && (restrictions[3] != null))
            {
                if (builder.Length > 0)
                {
                    builder.Append("AND ");
                }
                builder.AppendFormat(CultureInfo.InvariantCulture, "C.column_name='{0}' ", new object[] { restrictions[3] });
            }
            if (builder.Length > 0)
            {
                builder2.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", new object[] { builder });
            }
            DataTable table = this.GetTable(builder2.ToString());
            table.TableName = "ViewColumns";
            table.Columns[0].ColumnName = "VIEW_CATALOG";
            table.Columns[1].ColumnName = "VIEW_SCHEMA";
            table.Columns[2].ColumnName = "VIEW_NAME";
            return table;
        }

        private DataTable GetViews(string[] restrictions)
        {
            string[] keys = new string[] { "TABLE_CATALOG", "TABLE_SCHEMA", "TABLE_NAME" };
            DataTable table = this.Query("VIEWS", null, keys, restrictions);
            table.TableName = "Views";
            return table;
        }

        private static void InitParameterRow(DataRow procedure, DataRow parameter)
        {
            parameter["ROUTINE_CATALOG"] = null;
            parameter["ROUTINE_SCHEMA"] = procedure["ROUTINE_SCHEMA"];
            parameter["ROUTINE_NAME"] = procedure["ROUTINE_NAME"];
            parameter["ROUTINE_TYPE"] = procedure["ROUTINE_TYPE"];
            parameter["PARAMETER_MODE"] = "IN";
            parameter["ORDINAL_POSITION"] = 0;
            parameter["IS_RESULT"] = "NO";
        }

        private static string ParseDataType(DataRow row, SqlTokenizer tokenizer)
        {
            row["DATA_TYPE"] = tokenizer.NextToken().ToUpper(CultureInfo.InvariantCulture);
            string token = tokenizer.NextToken();
            while (SetParameterAttribute(row, token, tokenizer.IsSize, tokenizer))
            {
                token = tokenizer.NextToken();
            }
            return token;
        }

        private void ParseProcedureBody(DataTable parametersTable, string body, DataRow row, string nameToRestrict)
        {
            string str = row["SQL_MODE"].ToString();
            int num = 1;
            SqlTokenizer tokenizer = new SqlTokenizer(body) {
                AnsiQuotes = str.IndexOf("ANSI_QUOTES") != -1,
                BackslashEscapes = str.IndexOf("NO_BACKSLASH_ESCAPES") == -1
            };
            string str2 = tokenizer.NextToken();
            while (str2 != "(")
            {
                str2 = tokenizer.NextToken();
            }
            while (str2 != ")")
            {
                str2 = tokenizer.NextToken();
                if (str2 == ")")
                {
                    break;
                }
                DataRow parameter = parametersTable.NewRow();
                InitParameterRow(row, parameter);
                parameter["ORDINAL_POSITION"] = num++;
                string str3 = str2;
                if (!tokenizer.Quoted)
                {
                    string str4 = null;
                    switch (str2.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "in":
                            str4 = "IN";
                            break;

                        case "inout":
                            str4 = "INOUT";
                            break;

                        case "out":
                            str4 = "OUT";
                            break;
                    }
                    if (str4 != null)
                    {
                        parameter["PARAMETER_MODE"] = str4;
                        str3 = tokenizer.NextToken();
                    }
                }
                parameter["PARAMETER_NAME"] = string.Format("{0}{1}", base.connection.ParameterMarker, str3);
                str2 = ParseDataType(parameter, tokenizer);
                if ((nameToRestrict == null) || (parameter["PARAMETER_NAME"].ToString().ToLower() == nameToRestrict))
                {
                    parametersTable.Rows.Add(parameter);
                }
            }
            if (tokenizer.NextToken().ToLower(CultureInfo.InvariantCulture) == "returns")
            {
                DataRow row3 = parametersTable.NewRow();
                InitParameterRow(row, row3);
                row3["PARAMETER_NAME"] = string.Format("{0}RETURN_VALUE", base.connection.ParameterMarker);
                row3["IS_RESULT"] = "YES";
                ParseDataType(row3, tokenizer);
                parametersTable.Rows.Add(row3);
            }
        }

        private DataTable Query(string table_name, string initial_where, string[] keys, string[] values)
        {
            StringBuilder builder = new StringBuilder(initial_where);
            StringBuilder builder2 = new StringBuilder("SELECT * FROM INFORMATION_SCHEMA.");
            builder2.Append(table_name);
            if (values != null)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (i >= values.Length)
                    {
                        break;
                    }
                    if ((values[i] != null) && (values[i] != string.Empty))
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(" AND ");
                        }
                        builder.AppendFormat(CultureInfo.InvariantCulture, "{0}='{1}'", new object[] { keys[i], values[i] });
                    }
                }
            }
            if (builder.Length > 0)
            {
                builder2.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", new object[] { builder });
            }
            return this.GetTable(builder2.ToString());
        }

        private static bool SetParameterAttribute(DataRow row, string token, bool isSize, SqlTokenizer tokenizer)
        {
            string typename = row["DATA_TYPE"].ToString().ToLower(CultureInfo.InvariantCulture);
            if (isSize)
            {
                switch (typename)
                {
                    case "enum":
                    case "set":
                        return true;
                }
                string[] strArray = token.Split(new char[] { ',' });
                if (MetaData.IsNumericType(typename))
                {
                    row["NUMERIC_PRECISION"] = int.Parse(strArray[0]);
                }
                else
                {
                    row["CHARACTER_OCTET_LENGTH"] = int.Parse(strArray[0]);
                }
                if (strArray.Length == 2)
                {
                    row["NUMERIC_SCALE"] = int.Parse(strArray[1]);
                }
                return true;
            }
            string str2 = token.ToLower(CultureInfo.InvariantCulture);
            switch (str2)
            {
                case "unsigned":
                case "zerofill":
                    row["FLAGS"] = string.Format("{0} {1}", row["FLAGS"], token);
                    return true;

                case "character":
                case "charset":
                    if (str2 == "character")
                    {
                        tokenizer.NextToken().ToLower(CultureInfo.InvariantCulture);
                    }
                    row["CHARACTER_SET"] = tokenizer.NextToken();
                    return true;

                case "ascii":
                    row["CHARACTER_SET"] = "latin1";
                    return true;

                case "unicode":
                    row["CHARACTER_SET"] = "ucs2";
                    return true;

                case "binary":
                    return true;
            }
            return false;
        }
    }
}

