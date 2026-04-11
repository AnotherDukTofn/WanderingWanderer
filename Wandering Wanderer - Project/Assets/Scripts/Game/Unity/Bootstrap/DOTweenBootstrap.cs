using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Unity.Bootstrap
{
    /// <summary>
    /// Runs once in Boot scene: initializes DOTween, then demonstrates awaiting a tween via UniTask.
    /// </summary>
    public sealed class DOTweenBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // Init is synchronous setup — it does not return a Tween, so there is nothing to await here.
            DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        }

        private void Start()
        {
            RunToUniTaskExampleAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// <see cref="Tween"/> is not awaitable by itself. <c>.ToUniTask()</c> wraps it in a
        /// <see cref="UniTask"/> so you can <c>await</c> completion (or cancellation via token).
        /// Matches TASK-002 sanity: compile + run in Play Mode.
        /// </summary>
        private static async UniTaskVoid RunToUniTaskExampleAsync(CancellationToken cancellationToken)
        {
            // TASK-002-style minimal tween: animates a float 0 → 1 over 0.5s (setter can be no-op for the test).
            await DOTween.To(() => 0f, _ => { }, 1f, 0.5f)
                .ToUniTask(cancellationToken: cancellationToken);

            Debug.Log("[DOTweenBootstrap] DOTween.To(...).ToUniTask() completed.");
        }
    }
}
