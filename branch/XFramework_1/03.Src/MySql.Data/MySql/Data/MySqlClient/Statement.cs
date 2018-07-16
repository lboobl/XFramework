namespace MySql.Data.MySqlClient
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    internal abstract class Statement
    {
        private ArrayList buffers;
        protected MySqlCommand command;
        protected string commandText;

        private Statement(MySqlCommand cmd)
        {
            this.command = cmd;
            this.buffers = new ArrayList();
        }

        public Statement(MySqlCommand cmd, string text) : this(cmd)
        {
            this.commandText = text;
        }

        protected virtual void BindParameters()
        {
            ArrayList list = this.TokenizeSql(this.ResolvedCommandText);
            MySqlStream stream = new MySqlStream(this.Driver.Encoding) {
                Version = this.Driver.Version
            };
            string str = (string) list[list.Count - 1];
            if (str != ";")
            {
                list.Add(";");
            }
            foreach (string str2 in list)
            {
                if (str2.Trim().Length != 0)
                {
                    if (str2 == ";")
                    {
                        this.buffers.Add(stream.InternalBuffer);
                        stream = new MySqlStream(this.Driver.Encoding);
                    }
                    else if ((str2[0] != this.Parameters.ParameterMarker) || !this.SerializeParameter(this.Parameters, stream, str2))
                    {
                        stream.WriteStringNoNull(str2);
                    }
                }
            }
        }

        public virtual void Close()
        {
        }

        public virtual void Execute()
        {
            this.BindParameters();
            this.ExecuteNext();
        }

        public virtual bool ExecuteNext()
        {
            if (this.buffers.Count == 0)
            {
                return false;
            }
            MemoryStream stream = (MemoryStream) this.buffers[0];
            this.Driver.Query(stream.GetBuffer(), (int) stream.Length);
            this.buffers.RemoveAt(0);
            return true;
        }

        private MySqlParameter GetParameter(MySqlParameterCollection parameters, string name)
        {
            int index = parameters.IndexOf(name);
            if (index == -1)
            {
                name = name.Substring(1);
                index = this.Parameters.IndexOf(name);
                if (index == -1)
                {
                    return null;
                }
            }
            return parameters[index];
        }

        public virtual void Resolve()
        {
        }

        private bool SerializeParameter(MySqlParameterCollection parameters, MySqlStream stream, string parmName)
        {
            MySqlParameter parameter = this.GetParameter(parameters, parmName);
            if (parameter == null)
            {
                if (!this.Connection.Settings.UseOldSyntax)
                {
                    throw new MySqlException(string.Format(Resources.ParameterMustBeDefined, parmName));
                }
                return false;
            }
            parameter.Serialize(stream, false);
            return true;
        }

        public ArrayList TokenizeSql(string sql)
        {
            bool flag = this.Connection.Settings.AllowBatch & this.Driver.SupportsBatch;
            char ch = '\0';
            char parameterMarker = this.Connection.ParameterMarker;
            StringBuilder builder = new StringBuilder();
            bool flag2 = false;
            ArrayList list = new ArrayList();
            sql = sql.TrimStart(new char[] { ';' }).TrimEnd(new char[] { ';' });
            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];
                if (flag2)
                {
                    flag2 = !flag2;
                }
                else if (c == ch)
                {
                    ch = '\0';
                }
                else
                {
                    if (((c == ';') && !flag2) && ((ch == '\0') && !flag))
                    {
                        list.Add(builder.ToString());
                        list.Add(";");
                        builder.Remove(0, builder.Length);
                        continue;
                    }
                    if ((((c == '\'') || (c == '"')) || (c == '`')) && (!flag2 && (ch == '\0')))
                    {
                        ch = c;
                    }
                    else if (c == '\\')
                    {
                        flag2 = !flag2;
                    }
                    else if (((c == parameterMarker) && (ch == '\0')) && !flag2)
                    {
                        list.Add(builder.ToString());
                        builder.Remove(0, builder.Length);
                    }
                    else if (((((builder.Length > 0) && (builder[0] == parameterMarker)) && (!char.IsLetterOrDigit(c) && (c != '_'))) && (((c != '.') && (c != '$')) && ((c != '@') && (c != parameterMarker)))) && ((c != '?') && (c != parameterMarker)))
                    {
                        list.Add(builder.ToString());
                        builder.Remove(0, builder.Length);
                    }
                }
                builder.Append(c);
            }
            list.Add(builder.ToString());
            return list;
        }

        protected MySqlConnection Connection
        {
            get
            {
                return this.command.Connection;
            }
        }

        protected MySql.Data.MySqlClient.Driver Driver
        {
            get
            {
                return this.command.Connection.driver;
            }
        }

        protected MySqlParameterCollection Parameters
        {
            get
            {
                return this.command.Parameters;
            }
        }

        public virtual string ResolvedCommandText
        {
            get
            {
                return this.commandText;
            }
        }
    }
}

