#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LanguageExt.Common;

namespace LanguageExt.Core.DSL;

public static class Obj
{
    public static readonly Obj<Unit> Unit = Pure(Prelude.unit);
    
    public static Obj<A> Pure<A>(A value) =>
        new PureObj<A>(value);
    
    public static Obj<A> Many<A>(Seq<Obj<A>> values) =>
        new ManyObj<A>(values);
    
    public static Obj<A> Fail<A>(Error value) =>
        new LeftObj<Error, A>(value);
    
    public static Obj<R> Left<L, R>(L value) =>
        new LeftObj<L, R>(value);
    
    public static Obj<B> Apply<A, B>(Morphism<A, B> morphism, Obj<A> value) =>
        new ApplyObj<A, B>(morphism, value);

    public static Obj<A> Flatten<A>(this Obj<Obj<A>> value) =>
        new FlattenObj<A>(value);

    public static Obj<A> Choice<A>(params Obj<A>[] values) =>
        new ChoiceObj<A>(values.ToSeq());

    public static Obj<A> Switch<X, A>(Obj<X> subject, params (Morphism<X, bool> Match, Morphism<X, A> Body)[] cases) =>
        new SwitchObj<X, A>(subject, cases.ToSeq());

    public static Obj<A> If<A>(Obj<bool> predicate, Obj<A> then, Obj<A> @else) =>
        Switch(predicate,
            (IdentityMorphism<bool>.Default, Morphism.constant<bool, A>(then)),
            (NotMorphism.Default, Morphism.constant<bool, A>(@else)));

    public static Obj<Unit> Guard(Obj<bool> predicate, Obj<Error> onFalse) =>
        If(predicate, Unit, Morphism.bind<Error, Unit>(Fail<Unit>).Apply(onFalse));

    public static Obj<Unit> When(Obj<bool> predicate, Obj<Unit> onTrue) =>
        If(predicate, onTrue, Unit);
}

// ---------------------------------------------------------------------------------------------------------------------
// Interpreter state 

public  record State<RT>(RT Runtime, object This)
{
    int resource = 0;
    ConcurrentDictionary<IDisposable, Unit>? disps;
    
    public static State<RT> Create(RT runtime) => 
        new (null, runtime, Prim<Unit>.None);

    State(ConcurrentDictionary<IDisposable, Unit>? disps, RT runtime, object @this) : this(runtime, @this) =>
        this.disps = disps;

    public State<NRT> LocalRuntime<NRT>(Func<RT, NRT> f) =>
        new (disps, f(Runtime), This);

    public State<RT> SetThis<NEW_THIS>(Prim<NEW_THIS> @this) =>
        new (disps, Runtime, @this);

    public State<RT> LocalResources() =>
        new(null, Runtime, This);

    public Unit Use(IDisposable d)
    {
        SpinWait sw = default;
        while (true)
        {
            if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
            {
                disps = disps ?? new ConcurrentDictionary<IDisposable, Unit>();
                disps.TryAdd(d, default);
                resource = 0;
                return default;
            }
            sw.SpinOnce();
        }
    }

    public Unit Release(IDisposable d)
    {
        SpinWait sw = default;
        while (true)
        {
            if (Interlocked.CompareExchange(ref resource, 1, 0) == 0)
            {
                disps = disps ?? new ConcurrentDictionary<IDisposable, Unit>();
                disps.TryRemove(d, out _);
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
                    disp.Key.Dispose();
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

sealed record PureObj<A>(A Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        new PurePrim<A>(Value);
}
    
sealed record LeftObj<X, A>(X Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        new LeftPrim<X, A>(Value);
}

sealed record ManyObj<A>(Seq<Obj<A>> Values) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Prim.Many(Values.Map(x => x.Interpret(state)));
}

sealed record ThisObj<A> : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) => 
        (Prim<A>)state.This;
}

sealed record ApplyObj<X, A>(Morphism<X, A> Morphism, Obj<X> Argument) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Morphism.Invoke(state, Argument.Interpret(state));
}

sealed record FlattenObj<A>(Obj<Obj<A>> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) =>
        Value.Interpret(state).Map(o => o.Interpret(state)).Flatten();
}

sealed record ChoiceObj<A>(Seq<Obj<A>> Values) : Obj<A>
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

sealed record SwitchObj<X, A>(Obj<X> Subject, Seq<(Morphism<X, bool> Match, Morphism<X, A> Body)> Cases) : Obj<A>
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

sealed record HeadObj<A>(Obj<A> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) => 
        Value.Interpret(state).Head;
}

sealed record TailObj<A>(Obj<A> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) => 
        Value.Interpret(state).Tail;
}

sealed record LastObj<A>(Obj<A> Value) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) => 
        Value.Interpret(state).Last;
}

sealed record SkipObj<A>(Obj<A> Value, int amount) : Obj<A>
{
    public override Prim<A> Interpret<RT>(State<RT> state) => 
        Value.Interpret(state).Skip(amount);
}

sealed record TakeObj<A>(Obj<A> Value, int amount) : Obj<A>
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

    public static Morphism<A, D> bind<A, B, C, D>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind, Func<B, C, D> Project) =>
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

    public static Morphism<Unit, B> each<A, B>(IObservable<A> awaiting, Morphism<A, B> morphism) =>
        new EachMorphism<A, B>(awaiting, morphism);
    
    public static Morphism<A, A> skip<A>(int amount)=> 
        new SkipMorphism<A>(amount);
    
    public static Morphism<A, A> take<A>(int amount)=> 
        new TakeMorphism<A>(amount);
}

public static class Morphism<A>
{
    public static readonly Morphism<A, A> head = new HeadMorphism<A>(); 
    public static readonly Morphism<A, A> tail = new TailMorphism<A>(); 
    public static readonly Morphism<A, A> last = new LastMorphism<A>(); 
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


sealed record ConstMorphism<A, B>(Obj<B> Value) : Morphism<A, B>
{
    protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Value.Interpret(state);
}

sealed record FunMorphism<A, B>(Func<A, B> Value) : Morphism<A, B>
{
    protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        value.Map(Value);
}
    
sealed record ObjFunMorphism<A, B>(Obj<Func<A, B>> Value) : Morphism<A, B>
{
    protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Value.Interpret(state).Bind(value.Map);
}
    
sealed record BindMorphism<A, B>(Func<A, Obj<B>> Value) : Morphism<A, B>
{
    protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(x => Value(x).Interpret(state));
}
    
sealed record BindMorphism2<A, B, C>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind) : Morphism<A, C>
{
    protected override Prim<C> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Obj.Invoke(state, value).Bind(b => Bind(b).Invoke(state, value));
}
    
sealed record BindProjectMorphism<A, B, C, D>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> Bind, Func<B, C, D> Project) : Morphism<A, D>
{
    protected override Prim<D> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Obj.Invoke(state, value).Bind(b => Bind(b).Invoke(state, value).Map(c => Project(b, c)));
}

sealed record MapMorphism<A, B>(Func<Obj<A>, Obj<B>> Value) : Morphism<A, B>
{
    protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Value(value).Interpret(state);
}

sealed record LambdaMorphism<A, B>(Obj<B> Body) : Morphism<A, B>
{
    protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Body.Interpret(state.SetThis(value));
}
    
sealed record FilterMorphism<A>(Func<A, bool> Predicate) : Morphism<A, A>
{
    protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(v => Predicate(v) ? value : Prim<A>.None);
}

sealed record FilterMorphism2<A>(Morphism<A, bool> Predicate) : Morphism<A, A>
{
    protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Predicate.Invoke(state, value)
            .Bind(x => x ? value : Prim<A>.None);
}

sealed record ComposeMorphism<A, B, C>(Morphism<A, B> Left, Morphism<B, C> Right) : Morphism<A, C>
{
    protected override Prim<C> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Right.Apply(Left.Invoke(state, value)).Interpret(state);
}
    
sealed record ManyMorphism<A, B>(Seq<Morphism<A, B>> Values) : Morphism<A, B>
{
    protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        Prim.Many(Values.Map(m => m.Invoke(state, value)));
}

sealed record IdentityMorphism<A> : Morphism<A, A>
{
    public static readonly Morphism<A, A> Default = new IdentityMorphism<A>();

    protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) =>
        value;
}

sealed record NotMorphism : Morphism<bool, bool>
{
    public static readonly Morphism<bool, bool> Default = new NotMorphism();

    protected override Prim<bool> InvokeProtected<RT>(State<RT> state, Prim<bool> value) =>
        value.Map(static x => !x);
}

sealed record EachMorphism<A, B>(IObservable<A> Awaiting, Morphism<A, B> Morphism) : Morphism<Unit, B>
{
    protected override Prim<B> InvokeProtected<RT>(State<RT> state, Prim<Unit> value)
    {
        var waiter = new Waiter();
        var sub = Awaiting.Subscribe(waiter);
        var last = Prim<B>.None;
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
                                last = Morphism.Invoke(state, Prim.Pure(waiter.Current));
                            }
                            break;
                        
                        case 1:
                            return last;
                        
                        case 2:
                            return waiter.Exception == null
                                ? Prim<B>.None
                                : Prim.Fail<B>(waiter.Exception);
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
    }
    
    class Waiter : IObserver<A>, IDisposable
    {
        public AutoResetEvent Wait = new (false);
        public int State = 0;
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

sealed record SkipMorphism<A>(int Amount) : Morphism<A, A>
{
    protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) => 
        value.Skip(Amount);
}

sealed record TakeMorphism<A>(int Amount) : Morphism<A, A>
{
    protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) => 
        value.Take(Amount);
}

sealed record HeadMorphism<A> : Morphism<A, A>
{
    protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) => 
        value.Head;
}

sealed record TailMorphism<A> : Morphism<A, A>
{
    protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) => 
        value.Tail;
}

sealed record LastMorphism<A> : Morphism<A, A>
{
    protected override Prim<A> InvokeProtected<RT>(State<RT> state, Prim<A> value) => 
        value.Tail;
}

public record Eff<RT, A>(Morphism<RT, A> Op) : Morphism<RT, A>
{
    public static Eff<RT, A> operator |(Eff<RT, A> ma, Eff<RT, A> mb) =>
        new(Morphism.map<RT, A>(rt => Obj.Choice(ma.Op.Apply(rt), mb.Op.Apply(rt))));

    protected override Prim<A> InvokeProtected<RT1>(State<RT1> state, Prim<RT> value) =>
        value.Bind(nrt => Op.Invoke(state.LocalRuntime(_ => nrt), Prim.Pure(nrt)));
}

public static class PreludeExample
{
    public static Eff<RT, A> SuccessEff<RT, A>(A value) =>
        new (Morphism.constant<RT, A>(Obj.Pure(value)));
    
    public static Eff<RT, A> FailEff<RT, A>(Error value) =>
        new (Morphism.constant<RT, A>(Obj.Fail<A>(value)));

    public static Eff<RT, A> EffectMaybe<RT, A>(Func<RT, Fin<A>> f) =>
        new(Morphism.bind<RT, A>(rt => f(rt).Match(Succ: Obj.Pure, Fail: Obj.Fail<A>)));

    public static Eff<RT, A> Effect<RT, A>(Func<RT, A> f) =>
        new(Morphism.function(f));

    public static Prim<A> Run<RT, A>(this Eff<RT, A> ma, RT runtime)
    {
        var state = State<RT>.Create(runtime);
        return ma.Op.Invoke(state, Prim.Pure(runtime));
    }

    public static ObservableEach<A> each<A>(IObservable<A> ma) =>
        new (ma);

    public static Prim<A> each<A>(IEnumerable<A> ma) =>
        Prim.Many(ma.Map(Prim.Pure).ToSeq());

    public static Eff<RT, B> Map<RT, A, B>(this Eff<RT, A> ma, Func<A, B> f) =>
        new(Morphism.compose(ma.Op, Morphism.function(f)));

    public static Eff<RT, B> Bind<RT, A, B>(this Eff<RT, A> ma, Func<A, Eff<RT, B>> f) =>
        new(Morphism.bind(ma.Op, f));
    
    public static Eff<RT, B> SelectMany<RT, A, B>(this Eff<RT, A> ma, Func<A, Eff<RT, B>> f) =>
        new(Morphism.bind(ma.Op, f));
    
    public static Eff<RT, C> SelectMany<RT, A, B, C>(this Eff<RT, A> ma, Func<A, Eff<RT, B>> bind, Func<A, B, C> project) =>
        new(Morphism.bind(ma.Op, bind, project));

    public static Eff<RT, A> Filter<RT, A>(this Eff<RT, A> ma, Func<A, bool> f) =>
        new(Morphism.lambda<RT, A>(Morphism.filter(f).Apply(ma.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, A> Where<RT, A>(this Eff<RT, A> ma, Func<A, bool> f) =>
        new(Morphism.lambda<RT, A>(Morphism.filter(f).Apply(ma.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, A> Head<RT, A>(this Eff<RT, A> ma) =>
        new(Morphism.compose(ma.Op, Morphism<A>.head));
    
    public static Eff<RT, A> Tail<RT, A>(this Eff<RT, A> ma)=> 
        new(Morphism.compose(ma.Op, Morphism<A>.tail));
    
    public static Eff<RT, A> Last<RT, A>(this Eff<RT, A> ma)=> 
        new(Morphism.compose(ma.Op, Morphism<A>.last));
    
    public static Eff<RT, A> Skip<RT, A>(this Eff<RT, A> ma, int amount)=> 
        new(Morphism.compose(ma.Op, Morphism.skip<A>(amount)));
    
    public static Eff<RT, A> Take<RT, A>(this Eff<RT, A> ma, int amount)=> 
        new(Morphism.compose(ma.Op, Morphism.take<A>(amount)));

    public static Eff<RT, B> Apply<RT, A, B>(this Eff<RT, Func<A, B>> ff, Eff<RT, A> fa) =>
        new(Morphism.lambda<RT, B>(Morphism.function(ff.Op.Apply(Obj<RT>.This)).Apply(fa.Op.Apply(Obj<RT>.This))));

    public static Eff<RT, Func<B, C>> Apply<RT, A, B, C>(this Eff<RT, Func<A, B, C>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, D>>> Apply<RT, A, B, C, D>(this Eff<RT, Func<A, B, C, D>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, E>>>> Apply<RT, A, B, C, D, E>(this Eff<RT, Func<A, B, C, D, E>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);

    public static Eff<RT, Func<B, Func<C, Func<D, Func<E, F>>>>> Apply<RT, A, B, C, D, E, F>(this Eff<RT, Func<A, B, C, D, E, F>> ff, Eff<RT, A> fa) =>
        ff.Map(Prelude.curry).Apply(fa);
}
