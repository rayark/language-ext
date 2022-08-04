#nullable enable
using System;

namespace LanguageExt.Core.DSL;

public record ObservableEach<A>(IObservable<A> items);
