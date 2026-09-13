using System.Threading;
using UnityEngine;

public interface IInititalizeStart {

    int StartPriority { get; }
    Awaitable InititalizeStart(CancellationToken ct);
}
