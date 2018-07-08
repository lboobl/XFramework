using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XFramework.Core
{
    public class AccFacHelper
    {
        private static AccessorFactory _accFactory = null;

        static AccFacHelper()
        {
            _accFactory = new AccessorFactory(new SetAccessorFactory(true), new GetAccessorFactory(true));
        }

        public static object Get(object entity, string memberName)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (string.IsNullOrEmpty(memberName))
                throw new ArgumentNullException("memberName");

            IGetAccessor getAcc = _accFactory.GetAccessorFactory.CreateGetAccessor(entity.GetType(), memberName);
            return getAcc.Get(entity);
        }

        public static void Set(object entity, string memberName, object value)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (string.IsNullOrEmpty(memberName))
                throw new ArgumentNullException("memberName");

            ISetAccessor setAcc = _accFactory.SetAccessorFactory.CreateSetAccessor(entity.GetType(), memberName);
            setAcc.Set(entity, value);
        }
    }
}
