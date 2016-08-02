using System;

namespace Trinity.Framework.Helpers
{
    public class StaticRouter<T>
    {
        public T Value => _expr();
        private readonly Func<T> _expr;

        public StaticRouter(Type parentType)
        {
            _expr = ReflectionHelper.GetStaticPropertyAccessor<T>(parentType);
        }
    }
}