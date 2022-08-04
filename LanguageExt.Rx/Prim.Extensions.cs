using System;
using System.Reactive.Linq;
using LanguageExt.Core.DSL;

namespace LanguageExt.Core.DSL
{
    public sealed record ObservablePrim<A>(IObservable<Prim<A>> Values) : Prim<A>
    {
        public override Prim<B> Bind<B>(Func<A, Prim<B>> f) =>
            new ObservablePrim<B>(Values.Select(px => px.Bind(f)));

        public override Prim<B> Map<B>(Func<A, B> f) =>
            new ObservablePrim<B>(Values.Select(px => px.Map(f)));

        public override Prim<A> Interpret<RT>(State<RT> state) =>
            this;

        public override Prim<A> Head =>
            new ObservablePrim<A>(Values.Take(1));

        public override Prim<A> Tail =>
            new ObservablePrim<A>(Values.Skip(1));

        public override Prim<A> Skip(int amount) =>
            new ObservablePrim<A>(Values.Skip(amount));
    
        public override Prim<A> Take(int amount) =>
            new ObservablePrim<A>(Values.Take(amount));
    
        public override Prim<A> Last =>
            new ObservablePrim<A>(Values.TakeLast());
        
        public override bool IsNone => false;
        public override bool IsMany => true;
        public override bool IsSucc => true;
        public override bool IsFail => false;
    }
}

public static class PrimRxExtensions
{
    public static ObservableEach<B> Map<A, B>(this ObservableEach<A> ma, Func<A, B> f) => 
        new (ma.items.Select(f));

    public static ObservableEach<B> Select<A, B>(this ObservableEach<A> ma, Func<A, B> f) => 
        new (ma.items.Select(f));

    public static ObservableEach<A> Filter<A>(this ObservableEach<A> ma, Func<A, bool> f) => 
        new (ma.items.Where(f));

    public static ObservableEach<A> Where<A>(this ObservableEach<A> ma, Func<A, bool> f) => 
        new (ma.items.Where(f));

    public static ObservableEach<B> Bind<A, B>(this ObservableEach<A> ma, Func<A, ObservableEach<B>> f) => 
        new (ma.items.SelectMany(x => f(x).items));

    public static ObservableEach<B> SelectMany<A, B>(this ObservableEach<A> ma, Func<A, ObservableEach<B>> f) => 
        new (ma.items.SelectMany(x => f(x).items));

    public static ObservableEach<C> SelectMany<A, B, C>(this ObservableEach<A> ma, Func<A, ObservableEach<B>> f, Func<A, B, C> project) => 
        new (ma.items.SelectMany(x => f(x).items.Select(y => project(x, y))));
    
    public static LanguageExt.Core.DSL.Eff<RT, B> SelectMany<RT, A, B>(
        this LanguageExt.Core.DSL.Eff<RT, A> ma,
        Func<A, ObservableEach<B>> bind) =>
        new(Morphism.compose(ma.Op, Morphism.bind<A, B>(x => new ObservablePrim<B>(bind(x).items.Select(Prim.Pure)))));

    public static LanguageExt.Core.DSL.Eff<RT, C> SelectMany<RT, A, B, C>(
        this LanguageExt.Core.DSL.Eff<RT, A> ma,
        Func<A, ObservableEach<B>> bind,
        Func<A, B, C> project) =>
        new(Morphism.map<RT, C>(rt =>
                Morphism.bind<A, C>(a =>
                    Morphism.each(
                        bind(a).items,
                        Morphism.function<B, C>(b => project(a, b)))
                       .Apply(Prim.Unit)).Apply(ma.Op.Apply(rt))));

    public static Eff<RT, B> SelectMany<RT, A, B>(
        this ObservableEach<A> ma,
        Func<A, Eff<RT, B>> bind) =>
        new(Morphism.map<RT, B>(rt =>
                Morphism.each(ma.items, 
                    Morphism.bind<A, B>(a => bind(a).Op.Apply(rt))).Apply(Prim.Unit)));
}
