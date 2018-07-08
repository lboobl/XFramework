
using System;
using System.Data;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 数据上下文，表示 Xfw 框架的主入口点
    /// </summary>
    public partial class DataContext : IDisposable
    {
        /// <summary>
        /// 计算要插入、更新或删除的已修改对象的集，并执行相应命令以实现对数据库的更改
        /// <para>要求不能出现有 SELECT 语法</para>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<int> SubmitChangesAsync()
        {
            int count = _dbQueryables.Count;
            if (count == 0) return 0;

            List<string> sqlList = this.Resolve(false);
            List<int> identitys = new List<int>();
            IDataReader reader = null;

            try
            {
                DbQueryProviderBase provider = _provider as DbQueryProviderBase;
                Func<IDbCommand, Task<object>> func = async p =>
                {
                    reader = await provider.ExecuteReaderAsync(p);
                    TypeDeserializer<int> deserializer = new TypeDeserializer<int>(reader, null);
                    do
                    {
                        if (reader.Read()) identitys.Add(deserializer.Deserialize());
                    }
                    while (reader.NextResult());

                    // 释放当前的reader
                    if (reader != null) reader.Dispose();

                    return null;
                };

                await provider.DoExecuteAsync<object>(sqlList, func, null);
                // 回写自增列的ID值
                this.SetAutoIncrementValue(identitys);
            }
            finally
            {
                if (reader != null) reader.Dispose();
            }

            return count;
        }
    }
}
