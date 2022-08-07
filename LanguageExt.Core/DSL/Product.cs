#nullable enable
namespace LanguageExt.DSL;

public record Product<A, B>(Obj<A> First, Obj<B> Second) : Obj<(A, B)>
{
    public override Prim<(A, B)> Interpret<RT>(State<RT> state)
    {
        var pa = First.Interpret(state);
        var pb = Second.Interpret(state);
        return pa.Bind(state, a => pb.Map(b => (a, b)));
    }
}
