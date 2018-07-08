
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XFramework.Core
{
    /// <summary>
    /// 简化索引写法－字典
    /// </summary>
    public class XDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        #region 公开属性

        /// <summary>
        /// 获取或设置与指定的键相关联的值。
        /// </summary>
        /// <param name="key">要获取或设置的值的键。</param>
        /// <returns>与指定的键相关联的值</returns>
        public new TValue this[TKey key]
        {
            get
            {
                if (!base.ContainsKey(key)) return default(TValue);
                return base[key];
            }
            set
            {
                base[key] = value;
            }
        }

        #endregion

        #region 公开方法

        public void AddRange(IDictionary<TKey, TValue> KeyValues)
        {
            if (KeyValues != null)
            {
                foreach (KeyValuePair<TKey, TValue> kv in KeyValues)
                {
                    if (this[kv.Key] == null) base.Add(kv.Key, kv.Value);
                }
            }
        }

        #endregion
    }

}
