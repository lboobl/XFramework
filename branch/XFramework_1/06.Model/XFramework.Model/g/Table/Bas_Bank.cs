using System;

namespace XFramework.Model
{
	public partial class Bas_Bank
	{
        #region 私有变量
        
        #endregion
    
        #region 公开属性
        
		public virtual string CompanyID
        {
            get;
            set;
        }

		public virtual string BankID
        {
            get;
            set;
        }

		public virtual string BankCode
        {
            get;
            set;
        }

		public virtual string BankName
        {
            get;
            set;
        }

		public virtual string SWIFT
        {
            get;
            set;
        }

		public virtual string AreaID
        {
            get;
            set;
        }

		public virtual string Address
        {
            get;
            set;
        }

		public virtual string Phone
        {
            get;
            set;
        }

		public virtual string ParentID
        {
            get;
            set;
        }

		public virtual int Level
        {
            get;
            set;
        }

		public virtual bool IsDetail
        {
            get;
            set;
        }

		public virtual string FullName
        {
            get;
            set;
        }

		public virtual string FullParentID
        {
            get;
            set;
        }

		public virtual DateTime ModifyDTM
        {
            get;
            set;
        }

		public virtual string Remark
        {
            get;
            set;
        }

		public virtual bool AllowUsed
        {
            get;
            set;
        }
        
        #endregion
        
        #region 构造函数
        
        #endregion
        
        #region 重写方法
        
        #endregion
        
        #region 公开方法
        
        #endregion
        
        #region 辅助方法
        
        #endregion
	}
}