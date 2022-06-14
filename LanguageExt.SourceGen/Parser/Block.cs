using System.Text;
using static LanguageExt.SourceGen.Parser.Prim;

namespace LanguageExt.SourceGen.Parser;

internal record Block(string Keyword, string Body, string Path)
{
    static readonly Parser<Seq<Block>> blocks;

    static Block()
    {
        var keyword = choice(str("using"), str("namespace"), str("alias"), str("record"), str("union"));
        var rest = new Parser<string>(s =>
        {
            if (s.IsEOS) return Result.EOS<string>(s);
            var sb = new StringBuilder();
            var ns = s;
            while (true)
            {
                if (ns.IsEOS) return Result.EOS<string>(s);
                var c = ns.Value;
                sb.Append(c);
                if (c == ';')
                {
                    return Result.Success(ns, sb.ToString());
                }
                ns = ns.Next();
            }
        });
        
        var block = from k in keyword
                    from b in rest
                    from p in path
                    select new Block(k, b, p);

        blocks = from bs in many1(block)
                 from __ in eos
                 select bs.ToSeq();
    }

    public static Result<Seq<Block>> Parse(string source, string path) =>
        blocks.Parse(source, path);
}
