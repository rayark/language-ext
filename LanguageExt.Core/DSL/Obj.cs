#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using LanguageExt.TypeClasses;

namespace LanguageExt.Core.DSL;

public static class ObjAny
{
    public static DSL<MError, Error>.Obj<A> Fail<A>(Error value) =>
        new DSL<MError, Error>.LeftObj<A>(value);
    
    public static DSL<MErr, E>.Obj<A> Flatten<MErr, E, A>(this DSL<MErr, E>.Obj<DSL<MErr, E>.Obj<A>> value)
        where MErr : struct, Semigroup<E>, Convertable<Exception, E> =>
        new DSL<MErr, E>.FlattenObj<A>(value);
}

public static partial class DSL<MErr, E>
    where MErr : struct, Semigroup<E>, Convertable<Exception, E>
{
    public static class Obj
    {
        public static Obj<A> Left<A>(E value)  =>
            new LeftObj<A>(value);
        
        public static readonly Obj<Unit> Unit = Pure(Prelude.unit);

        public static Obj<A> Pure<A>(A value) =>
            new PureObj<A>(value);

        public static Obj<A> Many<A>(Seq<Obj<A>> values) =>
            new ManyObj<A>(values);

        public static Obj<B> Apply<A, B>(Morphism<A, B> morphism, Obj<A> value) =>
            new ApplyObj<A, B>(morphism, value);

        public static Obj<A> Choice<A>(params Obj<A>[] values) =>
            new ChoiceObj<A>(values.ToSeq());

        public static Obj<A> Switch<X, A>(Obj<X> subject,
            params (Morphism<X, bool> Match, Morphism<X, A> Body)[] cases) =>
            new SwitchObj<X, A>(subject, cases.ToSeq());

        public static Obj<A> If<A>(Obj<bool> predicate, Obj<A> then, Obj<A> @else) =>
            Switch(predicate,
                (IdentityMorphism<bool>.Default, Morphism.constant<bool, A>(then)),
                (NotMorphism.Default, Morphism.constant<bool, A>(@else)));

        public static Obj<Unit> Guard(Obj<bool> predicate, Obj<E> onFalse) =>
            If(predicate, Unit, Morphism.bind<E, Unit>(Left<Unit>).Apply(onFalse));

        public static Obj<Unit> When(Obj<bool> predicate, Obj<Unit> onTrue) =>
            If(predicate, onTrue, Unit);

        public static Obj<A> Use<A>(Obj<A> value) where A : IDisposable =>
            new UseObj<A>(value);

        public static Obj<A> Use<A>(Obj<A> value, Morphism<A, Unit> release) =>
            new UseObj2<A>(value, release);
        
        public static Obj<A> Release<A>(Obj<A> value) =>
            new ReleaseObj<A>(value);
    }

// ---------------------------------------------------------------------------------------------------------------------
// Interpreter state 

    public record State<RT>(RT Runtime, object This)
    {
        int resource;
        ConcurrentDictionary<object, IDisposable>? disps;

        public static State<RT> Create(RT runtime) =>
            new(null, runtime, Prim<Unit>.None);

        State(ConcurrentDictionary<object, IDisposable>? disps, RT runtime, object @this) : this(runtime, @this) =>
            this.disps = disps;

        public State<NRT> LocalRuntime<NRT>(Func<RT, NRT> f) =>
            new(disps, f(Runtime), This);

        public State<RT> SetThis<NEW_THIS>(Prim<NEW_THIS> @this) =>
            new(disps, Runtime, @this);

        public State<RT> LocalResources() =>
            new(null, Runtime, This);

        public Unit Use(object key, IDisposable d)
        {
            SpinWait sw = default;
            while (true)
            {
                if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
                {
                    disps = disps ?? new ConcurrentDictionary<object, IDisposable>();
                    disps.TryAdd(key, d);
                    resource = 0;
                    return default;
                }

                sw.SpinOnce();
            }
        }

        public Unit Release(object key)
        {
            SpinWait sw = default;
            while (true)
            {
                if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
                {
                    disps = disps ?? new ConcurrentDictionary<object, IDisposable>();
                    disps.TryRemove(key, out var d);
                    d.Dispose();
                    resource = 0;
                    return default;
                }

                sw.SpinOnce();
            }
        }

        public Unit CleanUp()
        {
            SpinWait sw = default;
            while (true)
            {
                if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
                {
                    if (disps == null) return default;
                    foreach (var disp in disps)
                    {
                        disp.Value.Dispose();
                    }

                    disps.Clear();
                    disps = null;
                    resource = 0;
                    return default;
                }

                sw.SpinOnce();
            }
        }
    }


// ---------------------------------------------------------------------------------------------------------------------
// Objects 

    public abstract record Obj<A>
    {
        public abstract Prim<A> Interpret<RT>(State<RT> state);

        public static readonly Obj<A> This = new ThisObj<A>();
    }

    internal sealed record PureObj<A>(A Value) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            new PurePrim<A>(Value);
    }

    internal sealed record LeftObj<A>(E Value) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            new LeftPrim<A>(Value);
    }

    internal sealed record ManyObj<A>(Seq<Obj<A>> Values) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            Prim.Many(Values.Map(x => x.Interpret(state)));
    }

    internal sealed record ThisObj<A> : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            (Prim<A>)state.This;
    }

    internal sealed record ApplyObj<X, A>(Morphism<X, A> Morphism, Obj<X> Argument) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            Morphism.Invoke(state, Argument.Interpret(state));
    }

    internal sealed record UseObj<A>(Obj<A> Value) : Obj<A>
        where A : IDisposable
    {
        public override Prim<A> Interpret<RT>(State<RT> state)
        {
            var r = Value.Interpret(state);
            state.Use(r, r);
            return r;
        }
    }

    internal sealed record UseObj2<A>(Obj<A> Value, Morphism<A, Unit> Release) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state)
        {
            var r = Value.Interpret(state);
            state.Use(r, new Acq<RT>(state, r, Release));
            return r;
        }

        record Acq<RT>(State<RT> State, Prim<A> Value, Morphism<A, Unit> Release) : IDisposable
        {
            public void Dispose() =>
                Release.Apply(Value).Interpret(State);
        }
    }

    internal sealed record ReleaseObj<A>(Obj<A> Value) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state)
        {
            var r = Value.Interpret(state);
            state.Release(Value);
            return r;
        }
    }

    internal sealed record FlattenObj<A>(Obj<Obj<A>> Value) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            Value.Interpret(state).Map(o => o.Interpret(state)).Flatten();
    }

    internal sealed record ChoiceObj<A>(Seq<Obj<A>> Values) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state)
        {
            foreach (var obj in Values)
            {
                var r = obj.Interpret(state);
                if (r.IsSucc) return r;
            }

            return Prim<A>.None;
        }
    }

    internal sealed record SwitchObj<X, A>(Obj<X> Subject, Seq<(Morphism<X, bool> Match, Morphism<X, A> Body)> Cases) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state)
        {
            var px = Subject.Interpret(state);
            foreach (var c in Cases)
            {
                var pr = c.Match.Invoke(state, px);
                var fl = false;
                var rs = pr.Bind(x =>
                {
                    if (x)
                    {
                        fl = true;
                        return c.Body.Invoke(state, px);
                    }
                    else
                    {
                        return Prim<A>.None;
                    }
                });
                if (fl) return rs;
            }

            return Prim<A>.None;
        }
    }

    internal sealed record HeadObj<A>(Obj<A> Value) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            Value.Interpret(state).Head;
    }

    internal sealed record TailObj<A>(Obj<A> Value) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            Value.Interpret(state).Tail;
    }

    internal sealed record LastObj<A>(Obj<A> Value) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            Value.Interpret(state).Last;
    }

    internal sealed record SkipObj<A>(Obj<A> Value, int amount) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            Value.Interpret(state).Skip(amount);
    }

    internal sealed record TakeObj<A>(Obj<A> Value, int amount) : Obj<A>
    {
        public override Prim<A> Interpret<RT>(State<RT> state) =>
            Value.Interpret(state).Take(amount);
    }

// ---------------------------------------------------------------------------------------------------------------------
// Morphisms 

    public static class Morphism
    {
        public static Morphism<A, B> function<A, B>(Func<A, B> f) =>
            new FunMorphism<A, B>(f);

        public static Morphism<A, B> function<A, B>(Obj<Func<A, B>> f) =>
            new ObjFunMorphism<A, B>(f);

        public static Morphism<A, B> bind<A, B>(Func<A, Obj<B>> f) =>
            new BindMorphism<A, B>(f);

        public static Morphism<A, C> bind<A, B, C>(Morphism<A, B> obj, Func<B, Morphism<A, C>> f) =>
            new BindMorphism2<A, B, C>(obj, f);

        public static Morphism<A, D> bind<A, B, C, D>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind,
            Func<B, C, D> Project) =>
            new BindProjectMorphism<A, B, C, D>(Obj, Bind, Project);

        public static Morphism<A, B> map<A, B>(Func<Obj<A>, Obj<B>> f) =>
            new MapMorphism<A, B>(f);

        public static Morphism<A, C> compose<A, B, C>(Morphism<A, B> f, Morphism<B, C> g) =>
            new ComposeMorphism<A, B, C>(f, g);

        public static Morphism<A, B> constant<A, B>(Obj<B> value) =>
            new ConstMorphism<A, B>(value);

        public static Morphism<A, A> filter<A>(Func<A, bool> predicate) =>
            new FilterMorphism<A>(predicate);

        public static Morphism<A, A> filter<A>(Morphism<A, bool> predicate) =>
            new FilterMorphism2<A>(predicate);

        public static Morphism<A, B> lambda<A, B>(Obj<B> body) =>
            new LambdaMorphism<A, B>(body);

        public static Morphism<A, A> skip<A>(int amount) =>
            new SkipMorphism<A>(amount);

        public static Morphism<A, A> take<A>(int amount) =>
            new TakeMorphism<A>(amount);
    }

    public static class Morphism<A>
    {
        public static readonly Morphism<A, A> head = new HeadMorphism<A>();
        public static readonly Morphism<A, A> tail = new TailMorphism<A>();
        public static readonly Morphism<A, A> last = new LastMorphism<A>();
        public static readonly Morphism<A, A> identity = new IdentityMorphism<A>();
    }

    public abstract record Morphism<A, B>
    {
        public Obj<B> Apply(Obj<A> value) =>
            new ApplyObj<A, B>(this, value);

        public Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
            value.IsFail || value.IsNone
                ? value.Cast<B>()
                : InvokeProtected(state, value);

        protected abstract Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value);
    }

    internal sealed record ConstMorphism<A, B>(Obj<B> Value) : Morphism<A, B>
    {
        protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Value.Interpret(state);
    }

    internal sealed record FunMorphism<A, B>(Func<A, B> Value) : Morphism<A, B>
    {
        protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value.Map(Value);
    }

    internal sealed record ObjFunMorphism<A, B>(Obj<Func<A, B>> Value) : Morphism<A, B>
    {
        protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Value.Interpret(state).Bind(value.Map);
    }

    internal sealed record BindMorphism<A, B>(Func<A, Obj<B>> Value) : Morphism<A, B>
    {
        protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value.Bind(x => Value(x).Interpret(state));
    }

    internal sealed record BindMorphism2<A, B, C>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind) : Morphism<A, C>
    {
        protected override Prim<C> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Obj.Invoke(state, value).Bind(b => Bind(b).Invoke(state, value));
    }

    internal sealed record BindProjectMorphism<A, B, C, D>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind,
        Func<B, C, D> Project) : Morphism<A, D>
    {
        protected override Prim<D> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Obj.Invoke(state, value).Bind(b => Bind(b).Invoke(state, value).Map(c => Project(b, c)));
    }

    internal sealed record MapMorphism<A, B>(Func<Obj<A>, Obj<B>> Value) : Morphism<A, B>
    {
        protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Value(value).Interpret(state);
    }

    internal sealed record LambdaMorphism<A, B>(Obj<B> Body) : Morphism<A, B>
    {
        protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Body.Interpret(state.SetThis(value));
    }

    internal sealed record FilterMorphism<A>(Func<A, bool> Predicate) : Morphism<A, A>
    {
        protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value.Bind(v => Predicate(v) ? value : Prim<A>.None);
    }

    internal sealed record FilterMorphism2<A>(Morphism<A, bool> Predicate) : Morphism<A, A>
    {
        protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Predicate.Invoke(state, value)
                .Bind(x => x ? value : Prim<A>.None);
    }

    internal sealed record ComposeMorphism<A, B, C>(Morphism<A, B> Left, Morphism<B, C> Right) : Morphism<A, C>
    {
        protected override Prim<C> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Right.Invoke(state, Left.Invoke(state, value));
    }

    internal sealed record ManyMorphism<A, B>(Seq<Morphism<A, B>> Values) : Morphism<A, B>
    {
        protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            Prim.Many(Values.Map(m => m.Invoke(state, value)));
    }

    internal sealed record IdentityMorphism<A> : Morphism<A, A>
    {
        public static readonly Morphism<A, A> Default = new IdentityMorphism<A>();

        protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value;
    }

    internal sealed record NotMorphism : Morphism<bool, bool>
    {
        public static readonly Morphism<bool, bool> Default = new NotMorphism();

        protected override Prim<bool> InvokeProtected<RT>(State<RT> state, Prim<bool> value) =>
            value.Map(static x => !x);
    }
    
    internal sealed record ToUnitObj<A>(Obj<A> Value) : Obj<Unit>
    {
        public override Prim<Unit> Interpret<RT>(State<RT> state)
        {
            var result = Value.Interpret(state).ToUnit();

            switch (result)
            {
                case LeftPrim<A> l: 
                    return l.Cast<Unit>();
                
                case ManyPrim<A> m: 
                    m.Items.Strict();  // Force side-effects to happen
                    break;
                
                case ObservablePrim<A> o:
                    o.ToResult();  // Force observable to run
                    break;
            }
            
            return Prim.Unit;
        }
    }
    
    internal sealed record CollectObj<A>(Obj<A> Value) : Obj<Seq<A>>
    {
        public override Prim<Seq<A>> Interpret<RT>(State<RT> state)
        {
            var presult = Value.Interpret(state);
            var result = presult.ToResult();

            switch (result)
            {
                case ResultFail<E, A> f:
                    return Prim.Left<Seq<A>>(f.Value);
                
                case ResultMany<E, A> m:
                    return Prim.Pure(m.Value);
                
                case ResultPure<E, A> p:
                    return Prim.Pure(Prelude.Seq1(p.Value));
            }
            return Prim<Seq<A>>.None;
        }
    }

    internal sealed record EachMorphism2<A, B>(Morphism<A, B> Morphism) : Morphism<IObservable<A>, Unit>
    {
        protected override Prim<Unit> InvokeProtected<RT>(State<RT> state, Prim<IObservable<A>> stream) =>
            stream.Bind(awaiting =>
            {
                var waiter = new Waiter();
                var sub = awaiting.Subscribe(waiter);
                try
                {
                    while (true)
                    {
                        waiter.Wait.WaitOne();
                        var nstate = state.LocalResources();
                        try
                        {
                            switch (waiter.State)
                            {
                                case 0:
                                    if (waiter.Current != null)
                                    {
                                        var r = Morphism.Invoke(nstate, Prim.Pure(waiter.Current));
                                        if (r.IsFail) return r.Cast<Unit>();
                                    }

                                    break;

                                case 1:
                                    return Prim.Unit;

                                case 2:
                                    return waiter.Exception == null
                                        ? Prim.Unit
                                        : Prim.Left<Unit>(default(MErr).Convert(waiter.Exception));
                            }
                        }
                        finally
                        {
                            nstate.CleanUp();
                        }
                    }
                }
                finally
                {
                    sub.Dispose();
                    waiter.Dispose();
                }
            });

        class Waiter : IObserver<A>, IDisposable
        {
            public AutoResetEvent Wait = new(false);
            public volatile int State;
            public Exception? Exception;
            public A? Current;

            public void OnNext(A value)
            {
                State = 0;
                Current = value;
            }

            public void OnCompleted()
            {
                State = 1;
                Wait.Set();
            }

            public void OnError(Exception error)
            {
                State = 2;
                Exception = error;
                Wait.Set();
            }

            public void Dispose() =>
                Wait.Dispose();
        }
    }

    internal sealed record CollectMorphism2<A, B>(Morphism<A, B> Morphism) : Morphism<IObservable<A>, B>
    {
        protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<IObservable<A>> stream) =>
            stream.Bind(awaiting =>
            {
                var items = new List<Prim<B>>();
                var waiter = new Waiter();
                var sub = awaiting.Subscribe(waiter);
                try
                {
                    while (true)
                    {
                        waiter.Wait.WaitOne();
                        var nstate = state.LocalResources();
                        try
                        {
                            switch (waiter.State)
                            {
                                case 0:
                                    if (waiter.Current != null)
                                    {
                                        items.Add(Morphism.Invoke(nstate, Prim.Pure(waiter.Current)));
                                    }

                                    break;

                                case 1:
                                    return Prim.Many(items.ToSeq());

                                case 2:
                                    return waiter.Exception == null
                                        ? Prim<B>.None
                                        : Prim.Left<B>(default(MErr).Convert(waiter.Exception));
                            }
                        }
                        finally
                        {
                            nstate.CleanUp();
                        }
                    }
                }
                finally
                {
                    sub.Dispose();
                    waiter.Dispose();
                }
            });

        class Waiter : IObserver<A>, IDisposable
        {
            public AutoResetEvent Wait = new(false);
            public volatile int State;
            public Exception? Exception;
            public A? Current;

            public void OnNext(A value)
            {
                State = 0;
                Current = value;
            }

            public void OnCompleted()
            {
                State = 1;
                Wait.Set();
            }

            public void OnError(Exception error)
            {
                State = 2;
                Exception = error;
                Wait.Set();
            }

            public void Dispose() =>
                Wait.Dispose();
        }
    }

    internal sealed record SkipMorphism<A>(int Amount) : Morphism<A, A>
    {
        protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value.Skip(Amount);
    }

    internal sealed record TakeMorphism<A>(int Amount) : Morphism<A, A>
    {
        protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value.Take(Amount);
    }

    internal sealed record HeadMorphism<A> : Morphism<A, A>
    {
        protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value.Head;
    }

    internal sealed record TailMorphism<A> : Morphism<A, A>
    {
        protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value.Tail;
    }

    internal sealed record LastMorphism<A> : Morphism<A, A>
    {
        protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
            value.Tail;
    }
}

