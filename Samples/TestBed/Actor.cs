namespace TestBed;

using System;
using LanguageExt;
using LanguageExt.Pipes;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;
using static LanguageExt.Pipes.Proxy;

public record Actor<RT, A>(Func<A, Eff<RT, Unit>> post, Eff<Unit> shutdown)
    where RT : struct, HasCancel<RT>;

public static class Actor<RT, S, A>
    where RT : struct, HasCancel<RT>
{
    public static Eff<RT, Actor<RT, A>> observe(IObservable<A> stream, S initial, Func<S, A, Aff<RT, S>> inbox) =>
        observe(stream, initial, map<A, A>(identity), inbox);
    
    public static Eff<RT, Actor<RT, A>> observe<B>(
        IObservable<A> stream, 
        S initial,
        Pipe<RT, A, B, Unit> pipe,
        Func<S, B, Aff<RT, S>> inbox)
    {
        var state = Atom(initial);
        var queue = Queue<RT, A>();
        var items = Producer.merge(queue, Proxy.observe(stream));
        return from cancel in fork(items | pipe | message(state, inbox))
               select new Actor<RT, A>(queue.EnqueueEff, cancel);
    }

    public static Eff<RT, Actor<RT, A>> spawn(S initial, Func<S, A, Aff<RT, S>> inbox) =>
        spawn(initial, map<A, A>(identity), inbox);
    
    public static Eff<RT, Actor<RT, A>> spawn<B>(
        S initial, 
        Pipe<RT, A, B, Unit> pipe,
        Func<S, B, Aff<RT, S>> inbox)
    {
        var state = Atom(initial);
        var queue = Queue<RT, A>();
        return from cancel in fork(queue | pipe | message(state, inbox))
               select new Actor<RT, A>(queue.EnqueueEff, cancel);
    }

    static Consumer<RT, MSG, Unit> message<MSG>(Atom<S> state, Func<S, MSG, Aff<RT, S>> inbox) =>
        from m in awaiting<MSG>()
        from _ in state.SwapAff(s => inbox(s, m))
        select unit;
}
