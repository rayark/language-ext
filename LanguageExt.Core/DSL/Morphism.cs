#nullable enable
using System;
using LanguageExt.TypeClasses;
using System.Reactive.Linq;
using System.Threading;
using LanguageExt.Common;

namespace LanguageExt.DSL;

public static class Morphism
{
    public static Morphism<A, B> function<A, B>(Func<A, B> f) =>
        new FunMorphism<A, B>(f);

    public static Morphism<A, Morphism<B, C>> function<A, B, C>(Func<A, B, C> f) =>
        new FunMorphism<A, Morphism<B, C>>(x => function((B y) => f(x, y)));

    public static Morphism<A, B> function<A, B>(Obj<Func<A, B>> f) =>
        new ObjFunMorphism<A, B>(f);

    public static Morphism<A, B> bind<A, B>(Func<A, Obj<B>> f) =>
        new BindMorphism<A, B>(f);

    public static Morphism<A, C> bind<A, B, C>(Func<A, Obj<B>> bind, Func<A, B, C> project) =>
        new BindProjectMorphism2<A, B, C>(bind, project);

    public static Morphism<A, C> bind<A, B, C>(Morphism<A, B> obj, Func<B, Morphism<A, C>> f) =>
        new BindMorphism2<A, B, C>(obj, f);

    public static Morphism<A, D> bind<A, B, C, D>(Morphism<A, B> Obj, 
        Func<B, Morphism<A, C>> Bind,
        Func<B, C, D> Project) =>
        new BindProjectMorphism<A, B, C, D>(Obj, Bind, Project);

    public static Morphism<A, B> map<A, B>(Func<Obj<A>, Obj<B>> f) =>
        new MapMorphism<A, B>(f);

    public static Morphism<A, C> compose<A, B, C>(Morphism<A, B> f, Morphism<B, C> g) =>
        f.Compose(g);

    public static Morphism<RT, CoProduct<E, B>> kleisli<RT, E, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<A, Morphism<RT, CoProduct<E, B>>> MY) =>
        new KleisliMorphism<RT, E, A, B>(MX, MY);

    public static Morphism<RT, CoProduct<E, B>> kleisli<RT, E, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<A, Morphism<RT, CoProduct<E, B>>> MY) =>
        new KleisliMorphism<RT, E, A, B>(MX, function(MY));

    public static Morphism<RT, CoProduct<E, B>> kleisli<MR, RT, E, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<A, MR> MY) 
        where MR : IsMorphism<RT, CoProduct<E, B>> =>
        new KleisliMorphism2<MR, RT, E, A, B>(MX, MY);

    public static Morphism<RT, CoProduct<E, B>> kleisli<MR, RT, E, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<A, MR> MY)
        where MR : IsMorphism<RT, CoProduct<E, B>> =>
        new KleisliMorphism2<MR, RT, E, A, B>(MX, function(MY));
    
    public static Morphism<RT, CoProduct<E, B>> kleisli<RT, E, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<A, CoProduct<E, B>> MY) =>
        new KleisliMorphism3<RT, E, A, B>(MX, MY);

    public static Morphism<RT, CoProduct<E, B>> kleisli<RT, E, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<A, CoProduct<E, B>> MY) =>
        new KleisliMorphism3<RT, E, A, B>(MX, function(MY));

    public static Morphism<RT, CoProduct<F, B>> bikleisli<RT, E, F, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<E, Morphism<RT, CoProduct<F, B>>> Left,
        Morphism<A, Morphism<RT, CoProduct<F, B>>> Right) =>
        new BiKleisliMorphism<RT, E, F, A, B>(MX, Left, Right);

    public static Morphism<RT, CoProduct<F, B>> bikleisli<MR, RT, E, F, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<E, MR> Left,
        Morphism<A, MR> Right) 
        where MR : IsMorphism<RT, CoProduct<F, B>> =>
        new BiKleisliMorphism2<MR, RT, E, F, A, B>(MX, Left, Right);

    public static Morphism<RT, CoProduct<F, B>> bikleisli<MR, RT, E, F, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<E, MR> Left,
        Func<A, MR> Right) 
        where MR : IsMorphism<RT, CoProduct<F, B>> =>
        new BiKleisliMorphism2<MR, RT, E, F, A, B>(MX, function(Left), function(Right));

    public static Morphism<RT, CoProduct<F, B>> bikleisli<RT, E, F, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<E, CoProduct<F, B>> Left,
        Morphism<A, CoProduct<F, B>> Right) =>
        new BiKleisliMorphism3<RT, E, F, A, B>(MX, Left, Right);

    public static Morphism<RT, CoProduct<F, B>> bikleisli<RT, E, F, A, B>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<E, CoProduct<F, B>> Left,
        Func<A, CoProduct<F, B>> Right) =>
        new BiKleisliMorphism3<RT, E, F, A, B>(MX, function(Left), function(Right));

    
    public static Morphism<RT, CoProduct<E, C>> kleisliProject<RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<A, Morphism<RT, CoProduct<E, B>>> MY,
        Morphism<A, Morphism<B, C>> project) =>
        new KleisliProjectMorphism<RT, E, A, B, C>(MX, MY, project);

    public static Morphism<RT, CoProduct<E, C>> kleisliProject<RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<A, Morphism<RT, CoProduct<E, B>>> MY,
        Func<A, B, C> project) =>
        new KleisliProjectMorphism<RT, E, A, B, C>(MX, MY, function(project));

    
    public static Morphism<RT, CoProduct<E, C>> kleisliProject<RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<A, Morphism<RT, CoProduct<E, B>>> MY,
        Func<A, B, C> project) =>
        new KleisliProjectMorphism<RT, E, A, B, C>(MX, function(MY), function(project));
    
    public static Morphism<RT, CoProduct<E, C>> kleisliProject<RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<A, Morphism<RT, CoProduct<E, B>>> MY,
        Morphism<A, Morphism<B, C>> project) =>
        new KleisliProjectMorphism<RT, E, A, B, C>(MX, function(MY), project);

    
    public static Morphism<RT, CoProduct<E, C>> kleisliProject<MR, RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<A, MR> MY,
        Morphism<A, Morphism<B, C>> project) 
        where MR : IsMorphism<RT, CoProduct<E, B>> =>
        new KleisliProjectMorphism2<MR, RT, E, A, B, C>(MX, MY, project);

    public static Morphism<RT, CoProduct<E, C>> kleisliProject<MR, RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<A, MR> MY,
        Func<A, B, C> project)
        where MR : IsMorphism<RT, CoProduct<E, B>> =>
        new KleisliProjectMorphism2<MR, RT, E, A, B, C>(MX, function(MY), function(project));

    
    public static Morphism<RT, CoProduct<E, C>> kleisliProject<RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<A, CoProduct<E, B>> MY,
        Morphism<A, Morphism<B, C>> project) =>
        new KleisliProjectMorphism3<RT, E, A, B, C>(MX, MY, project);

    public static Morphism<RT, CoProduct<E, C>> kleisliProject<RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Morphism<A, CoProduct<E, B>> MY,
        Func<A, B, C> project) =>
        new KleisliProjectMorphism3<RT, E, A, B, C>(MX, MY, function(project));

    
    public static Morphism<RT, CoProduct<E, C>> kleisliProject<RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<A, CoProduct<E, B>> MY,
        Func<A, B, C> project) =>
        new KleisliProjectMorphism3<RT, E, A, B, C>(MX, function(MY), function(project));
    
    public static Morphism<RT, CoProduct<E, C>> kleisliProject<RT, E, A, B, C>(
        Morphism<RT, CoProduct<E, A>> MX,
        Func<A, CoProduct<E, B>> MY,
        Morphism<A, Morphism<B, C>> project) =>
        new KleisliProjectMorphism3<RT, E, A, B, C>(MX, function(MY), project);

    
    
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

    public static Morphism<A, B> many<A, B>(Seq<Morphism<A, B>> ms) =>
        new ManyMorphism<A, B>(ms);
    
    public static Morphism<A, A> use<A>() where A : IDisposable => 
        UseMorphism<A>.Default;

    public static Morphism<A, A> use<A>(Morphism<A, Unit> release) =>
        new UseMorphism2<A>(release);

    public static Morphism<A, B> isObj<ObjB, MB, A, B>(Morphism<A, MB> morphism) 
        where ObjB : struct, IsObj<MB, B> =>
        new IsObjMorphism<ObjB, MB, A, B>(morphism);

    public static Morphism<A, B> isObj<ObjB, MB, A, B>(Func<A, MB> morphism) 
        where ObjB : struct, IsObj<MB, B> =>
        new IsObjMorphism2<ObjB, MB, A, B>(morphism);
    
    public static Morphism<A, bool> forall<A>(Morphism<A, bool> morphism) => 
        new ForAllMorphism<A>(morphism);
    
    public static Morphism<A, bool> exists<A>(Morphism<A, bool> morphism) => 
        new ExistsMorphism<A>(morphism);
    
    public static Morphism<A, A> appendRight<A>(Obj<A> left) => 
        new AppendRightMorphism<A>(left);
    
    public static Morphism<A, A> appendLeft<A>(Obj<A> left) => 
        new AppendLeftMorphism<A>(left);

    public static Morphism<A, CoProduct<X, B>> attempt<X, A, B>(Morphism<A, B> @try, Func<Exception, X> @catch) =>
        new TryMorphism<X, A, B>(@try, @catch);

    public static Morphism<A, CoProduct<X, B>> attempt<FailX, X, A, B>(Morphism<A, B> @try)
        where FailX : struct, Convertable<Exception, X> =>
        new TryMorphism<X, A, B>(@try, default(FailX).Convert);

    public static Morphism<Unit, A> each<A>(IObservable<A> items) =>
        new ObservableMorphism<A, A>(items.Select(Obj.Pure), Morphism<A>.identity);
    
    public static Morphism<bool, bool> not => 
        NotMorphism.Default;

    public static Morphism<A, B> ToMorphism<A, B>(this Morphism<A, CoProduct<Error, B>> Morphism) =>
        new ToMorphism<A, B>(Morphism);

    /// <summary>
    /// Schedule morphism
    /// </summary>
    /// <param name="Morphism">Morphism to 'schedule' (i.e. this is what's repeated, retried, etc.)</param>
    /// <param name="Schedule">Schedule</param>
    /// <param name="State">Initial state (if the effect is a fold)</param>
    /// <param name="FoldM">Fold morphism</param>
    /// <param name="Predicate">Continue processing predicate morphism</param>
    /// <returns></returns>
    public static Morphism<A, S> schedule<S, A, B>(
        Morphism<A, B> Morphism,
        Schedule Schedule,
        Obj<S> State,
        Morphism<S, Morphism<B, S>> FoldM,
        Morphism<B, bool> Predicate) =>
        new ScheduleMorphism<S, A, B>(Morphism, Schedule, State, FoldM, Predicate);

    /// <summary>
    /// Schedule morphism
    /// </summary>
    /// <param name="Morphism">Morphism to 'schedule' (i.e. this is what's repeated, retried, etc.)</param>
    /// <param name="Schedule">Schedule</param>
    /// <param name="State">Initial state (if the effect is a fold)</param>
    /// <param name="FoldM">Fold morphism</param>
    /// <param name="Predicate">Continue processing predicate morphism</param>
    /// <returns></returns>
    public static Morphism<CoProduct<X, A>, CoProduct<X, S>> schedule<S, X, A, B>(
        Morphism<CoProduct<X, A>, CoProduct<X, B>> Morphism,
        Schedule Schedule,
        Obj<S> State,
        Morphism<S, Morphism<B, S>> FoldM,
        Morphism<CoProduct<X, B>, bool> Predicate) =>
        new ScheduleMorphism2<S, X, A, B>(Morphism, Schedule, State, FoldM, Predicate);

    /// <summary>
    /// Schedule morphism
    /// </summary>
    /// <param name="Morphism">Morphism to 'schedule' (i.e. this is what's repeated, retried, etc.)</param>
    /// <param name="Schedule">Schedule</param>
    /// <param name="State">Initial state (if the effect is a fold)</param>
    /// <param name="FoldM">Fold morphism</param>
    /// <param name="Predicate">Continue processing predicate morphism</param>
    /// <returns></returns>
    public static Morphism<E, CoProduct<A, S>> schedule<S, E, A, B>(
        Morphism<E, CoProduct<A, B>> Morphism,
        Schedule Schedule,
        Obj<S> State,
        Morphism<S, Morphism<B, S>> FoldM,
        Morphism<CoProduct<A, B>, bool> Predicate) =>
        new ScheduleMorphism3<S, E, A, B>(Morphism, Schedule, State, FoldM, Predicate);
}

public static class Morphism<A>
{
    public static readonly Morphism<A, A> head = new HeadMorphism<A>();
    public static readonly Morphism<A, A> tail = new TailMorphism<A>();
    public static readonly Morphism<A, A> last = new LastMorphism<A>();
    public static readonly Morphism<A, A> identity = new IdentityMorphism<A>();
    public static readonly Morphism<A, Unit> release = ReleaseMorphism<A>.Default;
}

public static class Morphism<RT, A, B>
{
    public static readonly Morphism<A, Morphism<RT, CoProduct<A, B>>> left =
        Morphism.function<A, Morphism<RT, CoProduct<A, B>>>(a => Morphism.function<RT, CoProduct<A, B>>(_ => CoProduct.Left<A, B>(a)));
    
    public static readonly Morphism<B, Morphism<RT, CoProduct<A, B>>> right =
        Morphism.function<B, Morphism<RT, CoProduct<A, B>>>(b => Morphism.function<RT, CoProduct<A, B>>(_ => CoProduct.Right<A, B>(b)));
}

public abstract record Morphism<A, B>
{
    public Obj<B> Apply(Obj<A> value) =>
        value.Bind(this);

    public Morphism<A, CoProduct<Error, B>> ToBiMorphism() =>
        new ToBiMorphism<A, B>(this);
    
    public abstract Prim<B> Invoke<RT>(State<RT> state, Prim<A> value);
    
    public virtual Morphism<A, C> Compose<C>(Morphism<B, C> f) =>
        new ComposeMorphism<A,B,C>(this, f);

    public virtual Morphism<A, B> Head =>
        new ComposeMorphism<A, B, B>(this, Morphism<B>.head);

    public virtual Morphism<A, B> Last => 
        new ComposeMorphism<A, B, B>(this, Morphism<B>.last);

    public virtual Morphism<A, B> Tail => 
        new ComposeMorphism<A, B, B>(this, Morphism<B>.tail);

    public Morphism<A, B> Filter(Func<B, bool> f) => 
        Filter(Morphism.function(f));

    public virtual Morphism<A, B> Filter(Morphism<B, bool> f) => 
        new ComposeMorphism<A, B, B>(this, Morphism.filter(f));

    public virtual Morphism<A, B> Skip(int amount) => 
        new ComposeMorphism<A, B, B>(this, Morphism.skip<B>(amount));

    public virtual Morphism<A, B> Take(int amount) => 
        new ComposeMorphism<A, B, B>(this, Morphism.take<B>(amount));

    public virtual Morphism<A, B> TakeWhile(Morphism<B, bool> predicate) =>
        Morphism.map<A, B>(a =>
        {
            var b = Apply(a);
            return Morphism.bind<bool, B>(x => x ? b : Prim<B>.None).Apply(predicate.Apply(b));
        });
    
    public Morphism<A, B> TakeUntil(Morphism<B, bool> predicate) => 
        TakeWhile(predicate.Compose(Morphism.not));
    
    public virtual Morphism<A, S> Fold<S>(Obj<S> state, Morphism<S, Morphism<B, S>> f) =>
        Morphism.map<A, S>(a =>
            Morphism.bind<Morphism<A, S>, S>(m => m.Apply(a))
                .Apply(Morphism.function<Morphism<B, S>, Morphism<A, S>>(Compose).Apply(f.Apply(state))));

    public Morphism<A, int> Count =>
        Fold(Prim.Pure(0), Morphism.function<int, B, int>(static (s, _) => s + 1));

    public Morphism<A, B> Sum<NumB>() where NumB : struct, Num<B> =>
        Fold(Prim.Pure(default(NumB).FromInteger(0)), Morphism.function<B, B, B>(default(NumB).Plus));
    
    public Morphism<A, C> Select<C>(Func<B, C> f) =>
        Compose(Morphism.function(f));

    public Morphism<A, C> Map<C>(Func<B, C> f) =>
        Compose(Morphism.function(f));

    public Morphism<A, C> SelectMany<C>(Func<B, Obj<C>> f) =>
        Compose(Morphism.bind(f));

    public Morphism<A, C> Bind<C>(Func<B, Obj<C>> f) =>
        Compose(Morphism.bind(f));

    //  TODO
    //public Morphism<A, D> SelectMany<C, D>(Func<B, Obj<C>> bind, Func<B, C, D> project) =>
    //    Compose(Morphism.bind<B, D>(b => bind(b).Bind(Morphism.bind<C, D>(c => Obj.Pure(project(b, c))))));
}

internal sealed record ConstMorphism<A, B>(Obj<B> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value.Interpret(state);
}

internal sealed record FunMorphism<A, B>(Func<A, B> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Map(Value);
}

internal sealed record ObjFunMorphism<A, B>(Obj<Func<A, B>> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value.Interpret(state).Bind(value.Map);
}

internal sealed record BindMorphism<A, B>(Func<A, Obj<B>> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(x => Value(x).Interpret(state));
}

internal sealed record BindMorphism2<A, B, C>(Morphism<A, B> Obj, Func<B, Morphism<A, C>> BindM) : Morphism<A, C>
{
    public override Prim<C> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Obj.Invoke(state, value).Bind(b => BindM(b).Invoke(state, value));
}

internal sealed record BindProjectMorphism<A, B, C, D>(
    Morphism<A, B> Obj, 
    Func<B, Morphism<A, C>> BindM, 
    Func<B, C, D> Project) 
    : Morphism<A, D>
{
    public override Prim<D> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Obj.Invoke(state, value).Bind(b => BindM(b).Invoke(state, value).Map(c => Project(b, c)));
}

internal sealed record BindProjectMorphism2<A, B, C>(
    Func<A, Obj<B>> bind,
    Func<A, B, C> project)
    : Morphism<A, C>
{
    public override Prim<C> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(a => bind(a).Interpret(state).Map(b => project(a, b)));
}
    
internal sealed record MapMorphism<A, B>(Func<Obj<A>, Obj<B>> Value) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Value(value).Interpret(state);
}

internal sealed record LambdaMorphism<A, B>(Obj<B> Body) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Body.Interpret(state.SetThis(value));
}

internal sealed record FilterMorphism<A>(Func<A, bool> Predicate) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Bind(v => Predicate(v) ? value : Prim<A>.None);
}

internal sealed record FilterMorphism2<A>(Morphism<A, bool> Predicate) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Predicate.Invoke(state, value)
            .Bind(x => x ? value : Prim<A>.None);
}

internal sealed record ComposeMorphism<A, B, C>(Morphism<A, B> Left, Morphism<B, C> Right) : Morphism<A, C>
{
    public override Prim<C> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Right.Invoke(state, Left.Invoke(state, value));

    public override Morphism<A, D> Compose<D>(Morphism<C, D> f) =>
        new ComposeMorphism<A, B, D>(Left, Right.Compose(f));
}

internal sealed record ManyMorphism<A, B>(Seq<Morphism<A, B>> Values) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Prim.Many(Values.Map(m => m.Invoke(state, value)));
}

internal sealed record IdentityMorphism<A> : Morphism<A, A>
{
    public static readonly Morphism<A, A> Default = new IdentityMorphism<A>();

    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value;
}

internal sealed record NotMorphism : Morphism<bool, bool>
{
    public static readonly Morphism<bool, bool> Default = new NotMorphism();

    public override Prim<bool> Invoke<RT>(State<RT> state, Prim<bool> value) =>
        value.Map(static x => !x);
}

internal sealed record SkipMorphism<A>(int Amount) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Skip(Amount);
}

internal sealed record TakeMorphism<A>(int Amount) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Take(Amount);
}

internal sealed record HeadMorphism<A> : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Head;
}

internal sealed record TailMorphism<A> : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Tail;
}

internal sealed record LastMorphism<A> : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Last;
}

internal sealed record UseMorphism<A> : Morphism<A, A>
    where A : IDisposable
{
    public static Morphism<A, A> Default = new UseMorphism<A>();
    
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value)
    {
        state.Use(value, value);
        return value;
    }
}

internal sealed record UseMorphism2<A>(Morphism<A, Unit> Release) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value)
    {
        state.Use(value, new Acq<RT>(state, value, Release));
        return value;
    }
    record Acq<RT>(State<RT> State, Prim<A> Value, Morphism<A, Unit> Release) : IDisposable
    {
        public void Dispose() =>
            Release.Apply(Value).Interpret(State);
    }

}

internal sealed record ReleaseMorphism<A> : Morphism<A, Unit>
{
    public static Morphism<A, Unit> Default = new ReleaseMorphism<A>();
    
    public override Prim<Unit> Invoke<RT>(State<RT> state, Prim<A> value)
    {
        state.Release(value);
        return Prim.Unit;
    }
}

internal sealed record IsObjMorphism<ObjB, MB, A, B>(Morphism<A, MB> Morphism) : Morphism<A, B>
    where ObjB : struct, IsObj<MB, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Morphism.Invoke(state, value).Map(default(ObjB).ToObject).Flatten().Interpret(state);
}

internal sealed record IsObjMorphism2<ObjB, MB, A, B>(Func<A, MB> Morphism) : Morphism<A, B>
    where ObjB : struct, IsObj<MB, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Map(Morphism).Map(default(ObjB).ToObject).Flatten().Interpret(state);
}

internal sealed record ForAllMorphism<A>(Morphism<A, bool> Morphism) : Morphism<A, bool>
{
    public override Prim<bool> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Prim.Pure(Morphism.Apply(value).Interpret(state).ForAll(static x => x));
}

internal sealed record ExistsMorphism<A>(Morphism<A, bool> Morphism) : Morphism<A, bool>
{
    public override Prim<bool> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Prim.Pure(Morphism.Apply(value).Interpret(state).Exists(static x => x));
}

internal sealed record AppendLeftMorphism<A>(Obj<A> Left) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Left.Interpret(state).Append(value.Interpret(state));
}

internal sealed record AppendRightMorphism<A>(Obj<A> Right) : Morphism<A, A>
{
    public override Prim<A> Invoke<RT>(State<RT> state, Prim<A> value) =>
        value.Interpret(state).Append(Right.Interpret(state));
}

internal sealed record TryMorphism<X, A, B>(Morphism<A, B> Morphism, Func<Exception, X> Catch) : Morphism<A, CoProduct<X, B>>
{
    public override Prim<CoProduct<X, B>> Invoke<RT>(State<RT> state, Prim<A> value)
    {
        try
        {
            return Morphism.Invoke(state, value).Map(CoProduct.Right<X, B>);
        }
        catch (Exception e)
        {
            return Prim.Pure(CoProduct.Left<X, B>(Catch(e)));
        }
    }

    public override string ToString() => 
        $"Try";
}

internal sealed record ObservableMorphism<A, B>(IObservable<Obj<A>> Items, Morphism<A, B> Next) : Morphism<Unit, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<Unit> _)
    {
        using var collect = new Collector<B>();
        using var sub = Items.Select(x => Next.Invoke(state, x.Interpret(state))).Subscribe(collect);
        collect.Wait.WaitOne();
        collect.Error?.Rethrow<Unit>();
        return collect.Value;
    }

    public override Morphism<Unit, C> Compose<C>(Morphism<B, C> f) =>
        new ObservableMorphism<A, C>(Items, Morphism.compose(Next, f));

    public override Morphism<Unit, S> Fold<S>(Obj<S> state, Morphism<S, Morphism<B, S>> f) =>
        new ObservableMorphism<S, S>(Items.Aggregate(state, (s, a) => f.Apply(s).ApplyT(Next.Apply(a))), Morphism<S>.identity);

    public override Morphism<Unit, B> Head => 
        new ObservableMorphism<A, B>(Items.FirstAsync(), Next);

    public override Morphism<Unit, B> Last => 
        new ObservableMorphism<A, B>(Items.LastAsync(), Next);

    public override Morphism<Unit, B> Tail => 
        new ObservableMorphism<A, B>(Items.Skip(1), Next);

    public override Morphism<Unit, B> Skip(int amount) => 
        new ObservableMorphism<A, B>(Items.Skip(amount), Next);

    public override Morphism<Unit, B> Take(int amount) => 
        new ObservableMorphism<A, B>(Items.Take(amount), Next);

    public override Morphism<Unit, B> TakeWhile(Morphism<B, bool> predicate) =>
        new ObservableMorphism<A, B>(Items.TakeWhile(x =>
        {
            var state = State<Unit>.Create(default);
            try
            {
                return Next.Compose(predicate)
                           .Invoke(state, x.Interpret(state))
                           .ForAll(static x => x);
            }
            finally
            {
                state.CleanUp();
            }

        }), Next);

    public override Morphism<Unit, B> Filter(Morphism<B, bool> f) => 
        new ObservableMorphism<A, B>(Items.Where(x =>
        {
            var state = State<Unit>.Create(default);
            try
            {
                return Next.Compose(f)
                           .Invoke(state, x.Interpret(state))
                           .ForAll(static x => x);
            }
            finally
            {
                state.CleanUp();
            }

        }), Next);

    public override string ToString() => 
        $"Obj.Observable<{typeof(A).Name}>";
}

internal class Collector<A> : IObserver<Prim<A>>, IDisposable
{
    public readonly AutoResetEvent Wait = new(false);
    public Exception? Error;
    public Prim<A> Value = Prim<A>.None;

    public void OnNext(Prim<A> prim)
    {
        Value = Value.Append(prim);
    }            

    public void OnCompleted() =>
        Wait.Set();

    public void OnError(Exception error)
    {
        Error = error;
        Wait.Set();
    }

    public void Dispose() =>
        Wait.Dispose();
}

internal record ScheduleMorphism<S, A, B>(
    Morphism<A, B> Morphism, 
    Schedule Schedule,
    Obj<S> State,
    Morphism<S, Morphism<B, S>> FoldM, 
    Morphism<B, bool> Predicate) :
    Morphism<A, S>
{
    public override Prim<S> Invoke<RT>(State<RT> state, Prim<A> value)
    {
        static (Prim<B> EffectResult, Prim<S> State) RunAndFold(
            State<RT> state, 
            Morphism<A, B> effect, 
            Prim<A> value,
            Prim<S> foldState, 
            Morphism<S, Morphism<B, S>> fold)
        {
            try
            {
                var newResult = effect.Invoke(state, value);
                var newState = newResult.IsSucc
                    ? fold.Apply(foldState).ApplyT(newResult).Interpret(state)
                    : foldState;

                return (newResult, newState);
            }
            catch (Exception e)
            {
                return (Prim.Fail<B>(e), foldState);
            }
        }
        
        var durations = Schedule.Run();

        var results = RunAndFold(state, Morphism, value, State.Interpret(state), FoldM);
        
        if(!Predicate.Invoke(state, results.EffectResult).ForAll(static x => x))
            return FinalResult(results.EffectResult, results.State);

        var wait = new AutoResetEvent(false);
        using var enumerator = durations.GetEnumerator();
        while (enumerator.MoveNext() && Predicate.Invoke(state, results.EffectResult).ForAll(static x => x))
        {
            if (enumerator.Current != Duration.Zero) wait.WaitOne((int)enumerator.Current);
            results = RunAndFold(state, Morphism, value, results.State, FoldM);
        }

        return FinalResult(results.EffectResult, results.State);
    }

    static Prim<S> FinalResult(Prim<B> effectResult, Prim<S> state) =>
        effectResult is FailPrim<B> f ? Prim.Fail<S>(f.Value) : state;
}

internal record ScheduleMorphism2<S, E, A, B>(
    Morphism<CoProduct<E, A>, CoProduct<E, B>> Morphism, 
    Schedule Schedule,
    Obj<S> State,
    Morphism<S, Morphism<B, S>> FoldM, 
    Morphism<CoProduct<E, B>, bool> Predicate) :
    Morphism<CoProduct<E, A>, CoProduct<E, S>>
{
    public override Prim<CoProduct<E, S>> Invoke<RT>(State<RT> state, Prim<CoProduct<E, A>> value)
    {
        static (Prim<CoProduct<E, B>> EffectResult, Prim<S> State) RunAndFold(
            State<RT> state, 
            Morphism<CoProduct<E, A>, CoProduct<E, B>> effect, 
            Prim<CoProduct<E, A>> value,
            Prim<S> foldState, 
            Morphism<S, Morphism<B, S>> fold)
        {
            try
            {
                var newResult = effect.Invoke(state, value);
                var newState = newResult.ForAll(x => x.IsRight)
                    ? fold.Apply(foldState).ApplyT(newResult.Map(x => ((CoProductRight<E, B>)x).Value)).Interpret(state)
                    : foldState;

                return (newResult, newState);
            }
            catch (Exception e)
            {
                return (Prim.Fail<CoProduct<E, B>>(e), foldState);
            }
        }
        
        var durations = Schedule.Run();

        var results = RunAndFold(state, Morphism, value, State.Interpret(state), FoldM);
        
        if(!Predicate.Invoke(state, results.EffectResult).ForAll(static x => x))
            return results.EffectResult.Bind(r => FinalResult(r, results.State));

        var wait = new AutoResetEvent(false);
        using var enumerator = durations.GetEnumerator();
        while (enumerator.MoveNext() && Predicate.Invoke(state, results.EffectResult).ForAll(static x => x))
        {
            if (enumerator.Current != Duration.Zero) wait.WaitOne((int)enumerator.Current);
            results = RunAndFold(state, Morphism, value, results.State, FoldM);
        }

        return results.EffectResult.Bind(r => FinalResult(r, results.State));
    }

    static Prim<CoProduct<E, S>> FinalResult(CoProduct<E, B> effectResult, Prim<S> state) =>
        effectResult switch
        {
            CoProductRight<E, B>  => state.Map(CoProduct.Right<E, S>),
            CoProductLeft<E, B> l => Prim.Pure(CoProduct.Left<E, S>(l.Value)),
            CoProductFail<E, B> f => Prim.Fail<CoProduct<E, S>>(f.Value),
            _ => throw new NotSupportedException()
        };
}

internal record ScheduleMorphism3<S, X, A, B>(
    Morphism<X, CoProduct<A, B>> Morphism, 
    Schedule Schedule,
    Obj<S> State,
    Morphism<S, Morphism<B, S>> FoldM, 
    Morphism<CoProduct<A, B>, bool> Predicate) :
    Morphism<X, CoProduct<A, S>>
{
    public override Prim<CoProduct<A, S>> Invoke<RT>(State<RT> state, Prim<X> value)
    {
        static (Prim<CoProduct<A, B>> EffectResult, Prim<S> State) RunAndFold(
            State<RT> state, 
            Morphism<X, CoProduct<A, B>> effect, 
            Prim<X> value,
            Prim<S> foldState, 
            Morphism<S, Morphism<B, S>> fold)
        {
            try
            {
                var newResult = effect.Invoke(state, value);
                var newState = newResult.ForAll(x => x.IsRight)
                    ? fold.Apply(foldState).ApplyT(newResult.Map(x => ((CoProductRight<A, B>)x).Value)).Interpret(state)
                    : foldState;

                return (newResult, newState);
            }
            catch (Exception e)
            {
                return (Prim.Fail<CoProduct<A, B>>(e), foldState);
            }
        }
        
        var durations = Schedule.Run();

        var results = RunAndFold(state, Morphism, value, State.Interpret(state), FoldM);
        
        if(!Predicate.Invoke(state, results.EffectResult).ForAll(static x => x))
            return results.EffectResult.Bind(r => FinalResult(r, results.State));

        var wait = new AutoResetEvent(false);
        using var enumerator = durations.GetEnumerator();
        while (enumerator.MoveNext() && Predicate.Invoke(state, results.EffectResult).ForAll(static x => x))
        {
            if (enumerator.Current != Duration.Zero) wait.WaitOne((int)enumerator.Current);
            results = RunAndFold(state, Morphism, value, results.State, FoldM);
        }

        return results.EffectResult.Bind(r => FinalResult(r, results.State));
    }

    static Prim<CoProduct<A, S>> FinalResult(CoProduct<A, B> effectResult, Prim<S> state) =>
        effectResult switch
        {
            CoProductRight<A, B>  => state.Map(CoProduct.Right<A, S>),
            CoProductLeft<A, B> l => Prim.Pure(CoProduct.Left<A, S>(l.Value)),
            CoProductFail<A, B> f => Prim.Fail<CoProduct<A, S>>(f.Value),
            _ => throw new NotSupportedException()
        };
}

internal record ToBiMorphism<A, B>(Morphism<A, B> Morphism) : Morphism<A, CoProduct<Error, B>>
{
    public override Prim<CoProduct<Error, B>> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Morphism.Invoke(state, value) switch
        {
            FailPrim<A> f => Prim.Pure(CoProduct.Left<Error, B>(f.Value)),
            var p =>  p.Map(CoProduct.Right<Error, B>)
        };
}

internal record ToMorphism<A, B>(Morphism<A, CoProduct<Error, B>> Morphism) : Morphism<A, B>
{
    public override Prim<B> Invoke<RT>(State<RT> state, Prim<A> value) =>
        Morphism.Invoke(state, value).ToObj().Interpret(state);
}

internal record KleisliMorphism<RT, E, A, B>(Morphism<RT, CoProduct<E, A>> MX, Morphism<A, Morphism<RT, CoProduct<E, B>>> MY) : 
    Morphism<RT, CoProduct<E, B>>
{
    public override Prim<CoProduct<E, B>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
            {
                CoProductRight<E, A> p => MY.Invoke(state, Prim.Pure(p.Value)).ApplyT(value).Interpret(state),
                CoProductLeft<E, A> p  => Prim.Pure(CoProduct.Left<E, B>(p.Value)),
                CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<E, B>(p.Value)),
                _ => throw new NotSupportedException()
            });
}

internal record KleisliMorphism2<MR, RT, E, A, B>(
    Morphism<RT, CoProduct<E, A>> MX, 
    Morphism<A, MR> MY) : 
    Morphism<RT, CoProduct<E, B>>
    where MR : IsMorphism<RT, CoProduct<E, B>>
{
    public override Prim<CoProduct<E, B>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
        {
            CoProductRight<E, A> p => MY.Invoke(state, Prim.Pure(p.Value))
                                        .Bind(mr => mr.ToMorphism().Invoke(state, value)),
            CoProductLeft<E, A> p  => Prim.Pure(CoProduct.Left<E, B>(p.Value)),
            CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<E, B>(p.Value)),
            _ => throw new NotSupportedException()
        });
}

internal record KleisliMorphism3<RT, E, A, B>(Morphism<RT, CoProduct<E, A>> MX, Morphism<A, CoProduct<E, B>> MY) : 
    Morphism<RT, CoProduct<E, B>>
{
    public override Prim<CoProduct<E, B>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
        {
            CoProductRight<E, A> p => MY.Invoke(state, Prim.Pure(p.Value)),
            CoProductLeft<E, A> p  => Prim.Pure(CoProduct.Left<E, B>(p.Value)),
            CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<E, B>(p.Value)),
            _ => throw new NotSupportedException()
        });
}

internal record BiKleisliMorphism<RT, E, F, A, B>(
    Morphism<RT, CoProduct<E, A>> MX, 
    Morphism<E, Morphism<RT, CoProduct<F, B>>> Left,
    Morphism<A, Morphism<RT, CoProduct<F, B>>> Right
    ) : 
    Morphism<RT, CoProduct<F, B>>
{
    public override Prim<CoProduct<F, B>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
        {
            CoProductRight<E, A> p => Right.Invoke(state, Prim.Pure(p.Value)).ApplyT(value).Interpret(state),
            CoProductLeft<E, A> p  => Left.Invoke(state, Prim.Pure(p.Value)).ApplyT(value).Interpret(state),
            CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<F, B>(p.Value)),
            _ => throw new NotSupportedException()
        });
}

internal record BiKleisliMorphism2<MR, RT, E, F, A, B>(
    Morphism<RT, CoProduct<E, A>> MX, 
    Morphism<E, MR> Left,
    Morphism<A, MR> Right
    ) : 
    Morphism<RT, CoProduct<F, B>>
    where MR : IsMorphism<RT, CoProduct<F, B>>
{
    public override Prim<CoProduct<F, B>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
        {
            CoProductRight<E, A> p => Right.Invoke(state, Prim.Pure(p.Value)).Bind(mr => mr.ToMorphism().Invoke(state, value)),
            CoProductLeft<E, A> p  => Left.Invoke(state, Prim.Pure(p.Value)).Bind(mr => mr.ToMorphism().Invoke(state, value)),
            CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<F, B>(p.Value)),
            _ => throw new NotSupportedException()
        });
}


internal record BiKleisliMorphism3<RT, E, F, A, B>(
    Morphism<RT, CoProduct<E, A>> MX, 
    Morphism<E, CoProduct<F, B>> Left,
    Morphism<A, CoProduct<F, B>> Right
) : 
    Morphism<RT, CoProduct<F, B>>
{
    public override Prim<CoProduct<F, B>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
        {
            CoProductRight<E, A> p => Right.Invoke(state, Prim.Pure(p.Value)),
            CoProductLeft<E, A> p  => Left.Invoke(state, Prim.Pure(p.Value)),
            CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<F, B>(p.Value)),
            _ => throw new NotSupportedException()
        });
}

internal record KleisliProjectMorphism<RT, E, A, B, C>(
    Morphism<RT, CoProduct<E, A>> MX, 
    Morphism<A, Morphism<RT, CoProduct<E, B>>> MY, 
    Morphism<A, Morphism<B, C>> Project) : 
    Morphism<RT, CoProduct<E, C>>
{
    public override Prim<CoProduct<E, C>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
            {
                CoProductRight<E, A> a => 
                    MY.Invoke(state, Prim.Pure(a.Value)).InvokeT(state, value).Map(cb => cb switch
                    {
                        CoProductRight<E, B> b => Project.Invoke(state, Prim.Pure(a.Value))
                                                         .InvokeT(state, Prim.Pure(b.Value))
                                                         .Map(CoProduct.Right<E, C>),   
                        CoProductLeft<E, B> p  => Prim.Pure(CoProduct.Left<E, C>(p.Value)),
                        CoProductFail<E, B> p  => Prim.Pure(CoProduct.Fail<E, C>(p.Value)),
                        _ => throw new NotSupportedException()
                    }).Flatten(),
                CoProductLeft<E, A> p  => Prim.Pure(CoProduct.Left<E, C>(p.Value)),
                CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<E, C>(p.Value)),
                _ => throw new NotSupportedException()
            });

}

internal record KleisliProjectMorphism2<MR, RT, E, A, B, C>(
    Morphism<RT, CoProduct<E, A>> MX, 
    Morphism<A, MR> MY, 
    Morphism<A, Morphism<B, C>> Project) : 
    Morphism<RT, CoProduct<E, C>>
    where MR : IsMorphism<RT, CoProduct<E, B>>
{
        public override Prim<CoProduct<E, C>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
            {
                CoProductRight<E, A> a => 
                    MY.Invoke(state, Prim.Pure(a.Value)).Bind(mr => mr.ToMorphism().Invoke(state, value).Map(cb => cb switch
                    {
                        CoProductRight<E, B> b => Project.Invoke(state, Prim.Pure(a.Value))
                                                         .InvokeT(state, Prim.Pure(b.Value))
                                                         .Map(CoProduct.Right<E, C>),   
                        CoProductLeft<E, B> p  => Prim.Pure(CoProduct.Left<E, C>(p.Value)),
                        CoProductFail<E, B> p  => Prim.Pure(CoProduct.Fail<E, C>(p.Value)),
                        _ => throw new NotSupportedException()
                    })).Flatten(),
                CoProductLeft<E, A> p  => Prim.Pure(CoProduct.Left<E, C>(p.Value)),
                CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<E, C>(p.Value)),
                _ => throw new NotSupportedException()
            });
}

internal record KleisliProjectMorphism3<RT, E, A, B, C>(
    Morphism<RT, CoProduct<E, A>> MX, 
    Morphism<A, CoProduct<E, B>> MY, 
    Morphism<A, Morphism<B, C>> Project) : 
    Morphism<RT, CoProduct<E, C>>
{
    public override Prim<CoProduct<E, C>> Invoke<RT1>(State<RT1> state, Prim<RT> value) =>
        MX.Invoke(state, value).Bind(c => c switch
            {
                CoProductRight<E, A> a => 
                    MY.Invoke(state, Prim.Pure(a.Value)).Map(cb => cb switch
                    {
                        CoProductRight<E, B> b => Project.Invoke(state, Prim.Pure(a.Value))
                                                         .InvokeT(state, Prim.Pure(b.Value))
                                                         .Map(CoProduct.Right<E, C>),   
                        CoProductLeft<E, B> p  => Prim.Pure(CoProduct.Left<E, C>(p.Value)),
                        CoProductFail<E, B> p  => Prim.Pure(CoProduct.Fail<E, C>(p.Value)),
                        _ => throw new NotSupportedException()
                    }).Flatten(),
                CoProductLeft<E, A> p  => Prim.Pure(CoProduct.Left<E, C>(p.Value)),
                CoProductFail<E, A> p  => Prim.Pure(CoProduct.Fail<E, C>(p.Value)),
                _ => throw new NotSupportedException()
            });    
}
