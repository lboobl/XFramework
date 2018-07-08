using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.
using System.Reflection.Emit;

namespace XFramework.Core
{
    public class MapperBase
    {
        //T to = mapImpl.Map<T>(T From);
        protected LocalBuilder _locFrom;
        protected LocalBuilder _locTo;
        protected ILGenerator _il;

        public MapperBase(LocalBuilder locFrom, LocalBuilder locTo, ILGenerator il)
        {
            _locFrom = locFrom;
            _locTo = locTo;
            _il = il;
            Methi
        }
    }
}
