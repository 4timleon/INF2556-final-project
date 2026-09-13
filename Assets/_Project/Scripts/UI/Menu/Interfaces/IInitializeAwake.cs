using System.Threading;
using UnityEngine;

public interface IInitializeAwake {

    int AwakePriority { get; }
    Awaitable InitializeAwake(CancellationToken ct);
}
