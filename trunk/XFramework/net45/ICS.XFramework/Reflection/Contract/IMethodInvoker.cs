

namespace ICS.XFramework.Reflection
{
    public interface IMethodInvoker
    {
        object Invoke(object target, params object[] parameters);
    }
}
