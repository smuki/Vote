using System;

namespace Igs.Hcms.Volt
{
    class TypeRef {
        readonly Type _type;

        public TypeRef(Type type)
        {
            _type = type;
        }

        public Type Type
        {
            get {
                return _type;
            }
        }
    }
}
