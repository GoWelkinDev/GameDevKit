// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

using System.Collections.Concurrent;

namespace GameDevKit.Core
{
    /// <summary>
    /// 主线程任务调度器，用于在主线程上执行立即任务或延迟任务。
    /// <para>所有任务的最终执行都在调用 <see cref="Flush"/> 的同一线程上完成</para>
    /// <para><see cref="Flush"/> 应始终由同一个主线程调用，否则会引发线程安全问题</para>
    /// </summary>
    public static class MainThreadScheduler
    {
        private static readonly ConcurrentQueue<Action<double>> _actions = new();
        private static readonly List<ScheduledAction> _scheduled = new();
        private static readonly object _scheduleLock = new();
        private static int? _mainThreadId;

        /// <summary>
        /// 安排一个在下次 <see cref="Flush"/> 时立即执行的任务
        /// </summary>
        /// <param name="caller">任务注册者</param>
        /// <param name="action">要执行的任务，为 <c>null</c> 时直接忽略</param>
        public static void Execute(object caller, Action<double> action)
        {
            Execute(caller, action, 0, 0);
        }

        /// <summary>
        /// 安排一个延迟任务，在指定的毫秒数后通过 <see cref="Flush"/> 执行
        /// <para>实际执行时间取决于 <see cref="Flush"/> 的调用频率，延迟仅保证不会早于指定时间执行</para>
        /// </summary>
        /// <param name="caller">任务注册者</param>
        /// <param name="action">要执行的任务，为 <c>null</c> 时直接忽略</param>
        /// <param name="delayMs">延迟的毫秒数，小于等于 0 时等同于立即执行</param>
        public static void Execute(object caller, Action<double> action, int delayMs)
        {
            Execute(caller, action, delayMs, 0);
        }

        /// <summary>
        /// 安排一个延迟任务，在指定的毫秒数后通过 <see cref="Flush"/> 执行，之后每隔指定的间隔毫秒数重复执行
        /// </summary>
        /// <param name="caller">任务注册者</param>
        /// <param name="action">要执行的任务，为 <c>null</c> 时直接忽略</param>
        /// <param name="delayMs">首次执行的延迟毫秒数，小于等于 0 时等同于立即执行</param>
        /// <param name="intervalMs">重复执行的间隔毫秒数，小于等于 0 时等同于仅执行一次</param>
        public static void Execute(object caller, Action<double> action, int delayMs, int intervalMs)
        {
            if (caller == null || action == null) return;

            if (delayMs <= 0 && intervalMs <= 0)
            {
                _actions.Enqueue(action);
                return;
            }

            var scheduled = new ScheduledAction
            {
                Caller = caller,
                Action = action,
                ExecuteAt = Environment.TickCount64 + delayMs,
                IntervalMs = intervalMs > 0 ? intervalMs : 0
            };

            lock (_scheduleLock) _scheduled.Add(scheduled);
        }

        /// <summary>
        /// 在主线程调用此方法以处理所有到期的延迟任务，并依次执行立即队列中的所有任务
        /// <para>首次调用时会记录主线程 Id，后续若从其他线程调用会抛出异常</para>
        /// </summary>
        public static void Flush(double delta)
        {
            int currentId = Environment.CurrentManagedThreadId;
            if (_mainThreadId == null)
            {
                _mainThreadId = currentId;
            }
            else if (_mainThreadId.Value != currentId)
            {
                throw new InvalidOperationException($"Flush must be called on the main thread (expected Id={_mainThreadId}, actual Id={currentId}).");
            }


            long now = Environment.TickCount64;
            List<ScheduledAction> toExecute = new List<ScheduledAction>();

            // 取出到期任务
            lock (_scheduleLock)
            {
                for (int i = _scheduled.Count - 1; i >= 0; i--)
                {
                    var s = _scheduled[i];
                    if (s.ExecuteAt <= now)
                    {
                        toExecute.Add(s);
                        _scheduled.RemoveAt(i);
                    }
                }
            }

            // 执行到期任务
            foreach (var s in toExecute)
            {
                try
                {
                    _actions.Enqueue(s.Action);
                }
                catch (Exception ex)
                {
                    Log.PrintErr("MainThreadScheduler failed to execute scheduled task.", ex);
                }

                if (s.IntervalMs > 0)
                {
                    lock (_scheduleLock)
                    {
                        _scheduled.Add(new ScheduledAction
                        {
                            Caller = s.Caller,
                            Action = s.Action,
                            ExecuteAt = Environment.TickCount64 + s.IntervalMs,
                            IntervalMs = s.IntervalMs
                        });
                    }
                }
            }

            while (_actions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke(delta);
                }
                catch (Exception ex)
                {
                    Log.PrintErr("MainThreadScheduler failed to execute scheduled task.", ex);
                }
            }

        }

        /// <summary>
        /// 取消指定对象注册的所有尚未执行的延迟任务（包括重复任务）
        /// </summary>
        /// <typeparam name="T">注册者类型</typeparam>
        /// <param name="obj">注册者实例</param>
        public static void CancelTasks<T>(T obj) where T : class
        {
            if (obj == null) return;

            lock (_scheduleLock)
            {
                for (int i = _scheduled.Count - 1; i >= 0; i--)
                {
                    if (_scheduled[i].Caller == obj)
                        _scheduled.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 清空所有尚未执行的立即任务和延迟任务
        /// <para>已经移入队列的延迟任务不会被回退，但会被清掉</para>
        /// </summary>
        public static void Clear()
        {
            while (_actions.TryDequeue(out _)) { }
            lock (_scheduleLock)
                _scheduled.Clear();
        }

        private struct ScheduledAction
        {
            public object Caller { get; set; }
            public Action<double> Action { get; set; }
            public long ExecuteAt { get; set; }
            public int IntervalMs { get; set; }
        }
    }
}
