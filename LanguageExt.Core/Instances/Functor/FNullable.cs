﻿using System;
using LanguageExt.TypeClasses;
using static LanguageExt.Prelude;

namespace LanguageExt.Instances
{
    public struct FNullable<A, B> : 
        Functor<A?, B?, A, B>,
        BiFunctor<A?, B?, Unit, A, B>
        where A : struct
        where B : struct
    {
        public B? BiMap(A? ma, Func<Unit, B> fa, Func<A, B> fb) =>
            ma.HasValue
                ? fb == null
                    ? (B?)null
                    : fb(ma.Value)
                : fa == null
                    ? (B?)null
                    : fa(unit);

        public B? Map(A? ma, Func<A, B> f) =>
            ma.HasValue && f != null
                ? f(ma.Value)
                : (B?)null;
    }
}
