using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;

using XFramework.Core;
using System.Reflection;

namespace XFramework.DataAccess
{
    /// <summary>
    /// 配置助手类
    /// </summary>
    public  class ConfigHelper
    {
        //配置文件结构说明：
        //MapFile\
        //MapFile.c\
        //Configs\

        #region 私有属性

        //映射文件缓存
        private static XDictionary<string, EntityMapper> _mappers = null;
        //数据提供者配置
        private static List<DbProvider> _providers = null;
        //数据源
        private static IDataSource _dataSource = null;

        //映射文件子目录
        private static string[] _mapFilePaths = new[] { "MapFile", "MapFile.c" };
        //配置文件子目录
        private static string _cfgFilePath = "Configs";
        //提供者文件名
        private static string _providersFileName = "Providers.config";
        //数据源文件名
        private static string _dataSourceFileName = "DataSource.config";

        #endregion

        #region 公开属性

        /// <summary>
        /// 数据源
        /// </summary>
        public static IDataSource DataSource
        {
            get { return _dataSource; }
        }

        #endregion

        #region 构造函数

        static ConfigHelper()
        {
            //读取映射信息
            _mappers = new XDictionary<string, EntityMapper>();
            string root = Common.GetExecutingDirectory();
            foreach (string path in _mapFilePaths)
            {
                if (Directory.Exists(Path.Combine(root, path))) ReadMapper(new DirectoryInfo(Path.Combine(root, path)));
            }

            //读取数据提供者
            string providersFullName = Path.Combine(root, _cfgFilePath, _providersFileName);
            string xml = File.ReadAllText(providersFullName);
            _providers = SerializeHelper.DeserialFromXml<List<DbProvider>>(xml, new XmlRootAttribute("providers"), "http://ibatis.apache.org/providers");

            //读取数据源
            string dataSourceFullName = Path.Combine(root, _cfgFilePath, _dataSourceFileName);
            xml = File.ReadAllText(dataSourceFullName);
            _dataSource = SerializeHelper.DeserialFromXml<DataSource>(xml);
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 取指定类型的映射器
        /// </summary>
        /// <param name="tableType">实体类型</param>
        /// <returns></returns>
        public static EntityMapper GetMapper(Type tableType)
        {
            if (tableType == null) throw new ArgumentNullException("tableType");
            EntityMapper mapper = _mappers[tableType.FullName];
            //如果子类没有，则继续找基类
            if (mapper == null &&
                tableType.BaseType != null &&
                tableType.BaseType != typeof(object))
            {
                return ConfigHelper.GetMapper(tableType.BaseType);
            }

            if (mapper == null)
            {
                throw new NotSupportedException(
                    string.Format("can't find the mapfile for type {0}.", tableType.FullName));
            }
            return mapper;
        }

        /// <summary>
        /// 取指定键值的映射器
        /// </summary>
        /// <returns></returns>
        public static EntityMapper GetMapper(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName)) 
                throw new ArgumentNullException("typeFullName");

            string assName = typeFullName.Substring(0, typeFullName.LastIndexOf('.')) + ".dll";
            Assembly assembly = Assembly.LoadFrom(assName);
            Type tableType = assembly.GetType(typeFullName);
            return GetMapper(tableType);
        }

        /// <summary>
        /// 获取指定的provider
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static DbProvider GetProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName)) 
                throw new ArgumentNullException("providerName");

            DbProvider provier = _providers.FirstOrDefault<DbProvider>(w => w.Name.ToUpper() == providerName.ToUpper());
            if (provier == null) throw new Exception("can't find dbProvider" + providerName);
            return provier;
        }

        #endregion

        #region 辅助方法

        //从指定路径读取脚本配置文件
        private static void ReadMapper(DirectoryInfo directory)
        {
            //先读取指定路径下的文件
            FileInfo[] mapFiles = directory.GetFiles("*.xml");
            foreach (FileInfo file in mapFiles) ReadMapper(file);

            //再读取子目录
            DirectoryInfo[] directories = directory.GetDirectories();
            foreach (DirectoryInfo d in directories) ReadMapper(d);
        }

        //从指定文件读取脚本配置文件
        private static void ReadMapper(FileInfo file)
        {
            EntityMapper mapper = null;
            using (StreamReader reader = new StreamReader(file.FullName))
            {
                string xml = reader.ReadToEnd();
                mapper = SerializeHelper.DeserialFromXml<EntityMapper>(xml);
            }

            if (_mappers[mapper.TableType.TypeFullName] != null)
            {
                //已存在该类型的映射文件，添加不存在的脚本[Command]
                EntityMapper myMapper = _mappers[mapper.TableType.TypeFullName];
                IEnumerable<Command> queryN =
                    from a in mapper.Commands
                    join b in myMapper.Commands on a.Key equals b.Key into temp
                    from c in temp.DefaultIfEmpty()
                    where c == null
                    select a;
                myMapper.Commands.AddRange(queryN);
            }
            else
            {
                //没有该类的映射文件，添加到缓存
                _mappers.Add(mapper.TableType.TypeFullName, mapper);
            }
        }

        #endregion
    }
}
