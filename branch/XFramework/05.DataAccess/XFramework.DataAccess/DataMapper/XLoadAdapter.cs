using System;
using System.Data;
using System.Data.Common;


namespace XFramework.DataAccess
{

    /// <summary>
    /// 数据适配器，扩展Fill方法
    /// .NET的DataSet.Load方法，底层调用DataAdapter.Fill(DataTable[], IDataReader, int, int)
    /// Dapper想要返回DataSet，需要重写Load方法，不必传入DataTable[]，因为数组长度不确定
    /// </summary>
    public class XLoadAdapter : DataAdapter
    {
        public XLoadAdapter()
        {
        }

        public int FillFromReader(DataSet ds, IDataReader dataReader, int startRecord, int maxRecords)
        {
            return this.Fill(ds, "Table", dataReader, startRecord, maxRecords);
        }
    }

    /// <summary>
    /// 扩展Load方法
    /// </summary>
    public class XDataSet : DataSet
    {
        public override void Load(IDataReader reader, LoadOption loadOption, FillErrorEventHandler handler, params DataTable[] tables)
        {
            XLoadAdapter adapter = new XLoadAdapter
            {
                FillLoadOption = loadOption,
                MissingSchemaAction = MissingSchemaAction.AddWithKey
            };
            if (handler != null)
            {
                adapter.FillError += handler;
            }
            adapter.FillFromReader(this, reader, 0, 0);
            if (!reader.IsClosed && !reader.NextResult())
            {
                reader.Close();
            }
        }
    }
}
