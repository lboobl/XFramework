namespace MySql.Data.MySqlClient
{
    using MySql.Data.Types;
    using System;
    using System.Data;
    using System.Globalization;
    using System.Text;

    internal class StoredProcedure : PreparableStatement
    {
        private string hash;
        private string outSelect;
        private DataTable parametersTable;
        private string resolvedCommandText;

        public StoredProcedure(MySqlCommand cmd, string text) : base(cmd, text)
        {
            this.hash = ((uint) DateTime.Now.GetHashCode()).ToString();
        }

        public override void Close()
        {
            base.Close();
            if (this.outSelect.Length != 0)
            {
                char parameterMarker = base.Connection.ParameterMarker;
                MySqlDataReader reader = new MySqlCommand("SELECT " + this.outSelect, base.Connection).ExecuteReader();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    name = parameterMarker + name.Remove(0, this.hash.Length + 1);
                    reader.values[i] = MySqlField.GetIMySqlValue(base.Parameters[name].MySqlDbType);
                }
                if (reader.Read())
                {
                    for (int j = 0; j < reader.FieldCount; j++)
                    {
                        string str2 = reader.GetName(j);
                        str2 = parameterMarker + str2.Remove(0, this.hash.Length + 1);
                        base.Parameters[str2].Value = reader.GetValue(j);
                    }
                }
                reader.Close();
            }
        }

        private DataSet GetParameters(string procName)
        {
            if (base.Connection.Settings.UseProcedureBodies)
            {
                return base.Connection.ProcedureCache.GetProcedure(base.Connection, procName);
            }
            DataSet set = new DataSet();
            string[] restrictionValues = new string[4];
            int index = procName.IndexOf('.');
            restrictionValues[1] = procName.Substring(0, index++);
            restrictionValues[2] = procName.Substring(index, procName.Length - index);
            set.Tables.Add(base.Connection.GetSchema("procedures", restrictionValues));
            DataTable routines = new DataTable();
            DataTable procedureParameters = new ISSchemaProvider(base.Connection).GetProcedureParameters(null, routines);
            procedureParameters.TableName = "procedure parameters";
            set.Tables.Add(procedureParameters);
            int num2 = 1;
            foreach (MySqlParameter parameter in base.command.Parameters)
            {
                if (!parameter.TypeHasBeenSet)
                {
                    throw new InvalidOperationException(Resources.NoBodiesAndTypeNotSet);
                }
                DataRow row = procedureParameters.NewRow();
                row["PARAMETER_NAME"] = parameter.ParameterName;
                row["PARAMETER_MODE"] = "IN";
                if (parameter.Direction == ParameterDirection.InputOutput)
                {
                    row["PARAMETER_MODE"] = "INOUT";
                }
                else if (parameter.Direction == ParameterDirection.Output)
                {
                    row["PARAMETER_MODE"] = "OUT";
                }
                else if (parameter.Direction == ParameterDirection.ReturnValue)
                {
                    row["PARAMETER_MODE"] = "OUT";
                    row["ORDINAL_POSITION"] = 0;
                }
                else
                {
                    row["ORDINAL_POSITION"] = num2++;
                }
                procedureParameters.Rows.Add(row);
            }
            return set;
        }

        private string GetReturnParameter()
        {
            if (base.Parameters != null)
            {
                foreach (MySqlParameter parameter in base.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.ReturnValue)
                    {
                        string str = parameter.ParameterName.Substring(1);
                        return (this.hash + str);
                    }
                }
            }
            return null;
        }

        public override void Resolve()
        {
            string commandText = base.commandText;
            if (commandText.IndexOf(".") == -1)
            {
                commandText = base.Connection.Database + "." + commandText;
            }
            DataSet parameters = this.GetParameters(commandText);
            DataTable table = parameters.Tables["procedures"];
            this.parametersTable = parameters.Tables["procedure parameters"];
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            this.outSelect = string.Empty;
            string returnParameter = this.GetReturnParameter();
            foreach (DataRow row in this.parametersTable.Rows)
            {
                if (row["ORDINAL_POSITION"].Equals(0))
                {
                    continue;
                }
                string str3 = (string) row["PARAMETER_MODE"];
                string str4 = (string) row["PARAMETER_NAME"];
                MySqlParameter parameter = base.command.Parameters[str4];
                if (!parameter.TypeHasBeenSet)
                {
                    string typeName = (string) row["DATA_TYPE"];
                    bool unsigned = row["FLAGS"].ToString().IndexOf("UNSIGNED") != -1;
                    bool realAsFloat = table.Rows[0]["SQL_MODE"].ToString().IndexOf("REAL_AS_FLOAT") != -1;
                    parameter.MySqlDbType = MetaData.NameToType(typeName, unsigned, realAsFloat, base.Connection);
                }
                string str6 = str4.Substring(1);
                string str7 = string.Format("@{0}{1}", this.hash, str6);
                switch (str3)
                {
                    case "OUT":
                    case "INOUT":
                        this.outSelect = this.outSelect + str7 + ", ";
                        builder.Append(str7);
                        builder.Append(", ");
                        break;

                    default:
                        builder.Append(str4);
                        builder.Append(", ");
                        break;
                }
                if (str3 == "INOUT")
                {
                    builder2.AppendFormat(CultureInfo.InvariantCulture, "SET {0}={1};", new object[] { str7, str4 });
                    this.outSelect = this.outSelect + str7 + ", ";
                }
            }
            string str8 = builder.ToString().TrimEnd(new char[] { ' ', ',' });
            this.outSelect = this.outSelect.TrimEnd(new char[] { ' ', ',' });
            if (table.Rows[0]["ROUTINE_TYPE"].Equals("PROCEDURE"))
            {
                str8 = string.Format("call {0} ({1})", base.commandText, str8);
            }
            else
            {
                if (returnParameter == null)
                {
                    returnParameter = this.hash + "dummy";
                }
                else
                {
                    this.outSelect = string.Format("@{0}", returnParameter);
                }
                str8 = string.Format("set @{0}={1}({2})", returnParameter, base.commandText, str8);
            }
            if (builder2.Length > 0)
            {
                str8 = builder2 + str8;
            }
            this.resolvedCommandText = str8;
        }

        public override string ResolvedCommandText
        {
            get
            {
                return this.resolvedCommandText;
            }
        }
    }
}

