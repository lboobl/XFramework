﻿
using System.Collections;
using System.Collections.Generic;

namespace ICS.XFramework.Data
{
    /// <summary>
    /// 导航属性描述集合
    /// </summary>
    public class NavigationDescriptorCollection : IEnumerable<KeyValuePair<string, NavigationDescriptor>>
    {
        private IDictionary<string, NavigationDescriptor> _navCollection = null;
        private int? _minIndex;

        /// <summary>
        /// 所有导航属性的最小开始索引
        /// </summary>
        public int MinIndex { get { return _minIndex == null ? 0 : _minIndex.Value; } }

        /// <summary>
        /// 包含的元素数
        /// </summary>
        public int Count { get { return _navCollection.Count; } }

        /// <summary>
        /// 实例化<see cref="NavigationDescriptorCollection"/>类的新实例
        /// </summary>
        public NavigationDescriptorCollection()
        {
            _navCollection = new Dictionary<string, NavigationDescriptor>(8);
        }

        /// <summary>
        /// 返回一个循环访问集合的枚举数。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, NavigationDescriptor>> GetEnumerator()
        {
            return _navCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _navCollection.GetEnumerator();
        }

        /// <summary>
        /// 添加一个带有所提供的键和值的元素。
        /// </summary>
        public void Add(string key, NavigationDescriptor descriptor)
        {
            _navCollection.Add(key, descriptor);
            if (descriptor != null && descriptor.FieldCount != 0)
            {
                if (_minIndex == null)
                {
                    _minIndex = descriptor.Start;
                }
                else
                {
                    if (descriptor.Start < _minIndex.Value) _minIndex = descriptor.Start;
                }
            }
        }

        /// <summary>
        /// 是否包含具有指定键的元素
        /// </summary>
        public bool ContainsKey(string key)
        {
            return _navCollection.ContainsKey(key);
        }

        /// <summary>
        /// 获取与指定的键相关联的值。
        /// </summary>
        public bool TryGetValue(string key, out NavigationDescriptor descriptor)
        {
            return _navCollection.TryGetValue(key, out descriptor);
        }
    }
}
