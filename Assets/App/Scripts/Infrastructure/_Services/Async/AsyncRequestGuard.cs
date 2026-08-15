using System;
using System.Threading;

namespace UtinComputerTest.Infrastructure.Services.Async
{
    public sealed class AsyncRequestGuard : IDisposable
    {
        private CancellationTokenSource _cts;
        private int _requestId;
        private bool _disposed;

        public RequestScope Begin(CancellationToken lifetimeToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncRequestGuard));

            CancelCurrent();

            _cts = lifetimeToken.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken)
                : new CancellationTokenSource();

            return new RequestScope(++_requestId, _cts.Token);
        }

        public bool IsCurrent(RequestScope scope)
        {
            if (_disposed)
                return false;

            if (scope.Token.IsCancellationRequested)
                return false;

            return scope.RequestId == _requestId;
        }

        public void CancelCurrent()
        {
            if (_cts == null)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            CancelCurrent();
        }

        public readonly struct RequestScope
        {
            public RequestScope(int requestId, CancellationToken token)
            {
                RequestId = requestId;
                Token = token;
            }

            public int RequestId { get; }
            public CancellationToken Token { get; }
        }
    }
}
