﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;

namespace Telegram.Bot.Framework.UpdatePipeline
{
    internal class MapWhenMiddleware : IUpdateHandler
    {
        private readonly Predicate<IUpdateContext> _predicate;

        private readonly UpdateDelegate _branch;

        public MapWhenMiddleware(Predicate<IUpdateContext> predicate, UpdateDelegate branch)
        {
            _predicate = predicate;
            _branch = branch;
        }

        public Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
            => _predicate(context)
                ? _branch(context)
                : next(context);
    }
}
