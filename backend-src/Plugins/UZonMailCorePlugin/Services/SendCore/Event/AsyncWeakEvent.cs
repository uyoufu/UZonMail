using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UZonMail.Core.Services.SendCore.Event
{
    /// <summary>
    /// 异步弱事件
    /// 修改自 https://github.com/walterlv/Walterlv.Packages/blob/master/src/Utils/Walterlv.WeakEvents/WeakEvent.cs
    /// </summary>
    public class AsyncWeakEvent<TSender, TArgs>
    {
        /// <summary>
        /// 提供线程安全的锁。
        /// </summary>
        private readonly object _locker = new();

        /// <summary>
        /// 保留所有已订阅了弱事件的对象的弱引用实例。
        /// <para>
        /// 例如，某对象用了 a.Changed += this.a_Changed; 来注册事件，那么这里将保存 this 的实例。
        /// </para>
        /// <para>
        /// 这是因为，对于以上订阅，a_Changed 是一个方法，这里编译器会利用隐式转换将方法转换为事件对应的 EventHandler 的新实例。
        /// 由于每次转换都会生成不同的实例，所以如果保留此实例后依然弱引用，将会被 GC 回收。
        /// 因此我们需要保留原始的对象实例，来保证事件注册的生命周期与原始对象的生命周期相同。
        /// </para>
        /// </summary>
        private readonly List<WeakReference<object>> _relatedInstances = new();

        /// <summary>
        /// 包含所有事件订阅的原始对象到当前已订阅的事件处理函数的弱引用关系。
        /// </summary>
        /// <remarks>
        /// 注意，这里使用的是 <see cref="ConditionalWeakTable{TKey, TValue}"/> 类型。
        /// 它可以存储一组键值对，但键和值均不是强引用的。但注意，它不是字典，不可枚举不可遍历！
        /// 如果键没有被回收，那么值一定不会被回收；如果键被回收，那么值在没有被引用的情况也会被回收。
        /// 另外，如果你不能传入 Key 的实例，你绝无可能找到 Value！
        /// 另请参见：https://blog.walterlv.com/post/conditional-weak-table.html
        /// </remarks>
        private readonly ConditionalWeakTable<object, WeakEventHandler> _handlers = new();

        /// <summary>
        /// 订阅弱事件处理函数。
        /// </summary>
        /// <param name="originalHandler">原始处理函数，请始终传入 <c>value</c>。</param>
        /// <param name="castedHandler">可被隐式转换为 Action 的方法组，请始终传入 <c>value.Invoke</c>。</param>
        public void Add(MulticastDelegate originalHandler, Func<TSender, TArgs, Task> castedHandler)
        {
            // 获取委托对应的目标实例。
            var target = originalHandler.Target;
            var method = originalHandler.Method;

            // 当 target 为 null 时，说明是静态方法。
            // 此时我们将 target 设置为方法所在的类型，这样可以保证静态方法的生命周期与类型的生命周期相同。
            target ??= method.DeclaringType;

            if (target is null)
            {
                // 如果 target 仍为 null，说明进入了一个未知情况，我们需要有复现步骤来辅助编写这里的代码。
                throw new NotSupportedException($"弱事件订阅时，事件的订阅者必须是一个对象或一个类型。此委托中的目标实例是 null：{originalHandler}");
            }

            lock (_locker)
            {
                // 找到目前是否有已经存储过的对 target 的弱引用实例，如果有，我们将复用此实例，而不是加入到集合中。
                // 注意，这里的判定使用的是 ReferenceEquals，因为 ConditionalWeakTable 的比较用的是此方法，这可以确保回收时机两者一致。
                var reference = _relatedInstances.Find(x => x.TryGetTarget(out var instance) && ReferenceEquals(target, instance));
                if (reference is null)
                {
                    // 如果没有找到已经存储过的弱引用实例，我们将创建一个新的。
                    reference = new WeakReference<object>(target);
                    _relatedInstances.Add(reference);
                    var weakEventHandler = new WeakEventHandler();
                    weakEventHandler.Add(originalHandler, castedHandler);
                    _handlers.Add(target, weakEventHandler);
                }
                else if (_handlers.TryGetValue(target, out var weakEventHandler))
                {
                    // 如果找到了已经存储过的弱引用实例，则为其添加一个新的事件处理器。
                    weakEventHandler.Add(originalHandler, castedHandler);
                }
                else
                {
                    // 如果找不到弱引用实例，说明有一个已经被 GC 掉的对象竟然还能 += 事件。逗我？！
                    throw new InvalidOperationException("有一个已经被 GC 掉的对象正在试图注册事件处理函数，可能代码写错了。");
                }
            }
        }

        /// <summary>
        /// 注销弱事件处理函数。
        /// </summary>
        /// <param name="originalHandler">原始处理函数，请始终传入 <c>value</c>。</param>
        public void Remove(MulticastDelegate originalHandler)
        {
            // 获取委托对应的目标实例。
            var target = originalHandler.Target;

            // 当 target 为 null 时，说明是静态方法。
            // 此时我们将 target 设置为方法所在的类型，这样可以保证静态方法的生命周期与类型的生命周期相同。
            target ??= originalHandler.Method.DeclaringType;

            if (target is null)
            {
                // 因为 Add 的时候已经抛出了异常，所以 Remove 能进来的委托，一定不可能被 Add 过。
                // 所以这里的 Remove 就像普通 -= 那些未曾订阅过的事件行为一样（什么都不做）即可。
                return;
            }

            lock (_locker)
            {
                // 找到目前是否有已经存储过的对 target 的弱引用实例，如果有，我们将复用此实例，而不是加入到集合中。
                // 注意，这里的判定使用的是 ReferenceEquals，因为 ConditionalWeakTable 的比较用的是此方法，这可以确保回收时机两者一致。
                var reference = _relatedInstances.Find(x => x.TryGetTarget(out var instance) && ReferenceEquals(target, instance));
                if (reference is null)
                {
                    // 如果都没有找到已经存储过的弱引用实例，那我们还移除个啥，有什么好移除的？
                }
                else if (_handlers.TryGetValue(target, out var weakEventHandler))
                {
                    // 如果找到了已经存储过的弱引用实例，则注销此事件处理器。
                    weakEventHandler.Remove(originalHandler);
                }
                else
                {
                    // 如果找不到弱引用实例，说明有一个已经被 GC 掉的对象竟然还能 -= 事件。逗我？！
                    throw new InvalidOperationException("有一个已经被 GC 掉的对象正在试图注销事件处理函数，可能代码写错了。");
                }
            }
        }

        /// <summary>
        /// 引发弱事件，并传入事件引发源和事件参数。
        /// </summary>
        /// <param name="sender">事件引发源。</param>
        /// <param name="e">事件参数。</param>
        /// <returns>
        /// 如果在引发事件后发现已经没有任何对象订阅了此事件，则返回 false，这表明可以着手回收事件中继了。
        /// 相反，如果返回了 true，说明还有存活的对象正在订阅此事件。
        /// </returns>
        public async Task<bool> InvokeAsync(TSender sender, TArgs e)
        {
            List<Func<TSender, TArgs, Task>>? invokingHandlers = null;
            lock (_locker)
            {
                var weakEventHandlerList = _relatedInstances.ConvertAll(x =>
                    // 从原始的委托集合中查找需要引发事件的对象。
                    x.TryGetTarget(out var relatedInstance)
                    // 如果能找到目标对象，那么从 ConditionalWeakTable 中查找对应的弱事件处理器（实际上只要上面的委托存在，这里就 100% 一定存在，所以实际上我们只是为了拿 value）。
                    && _handlers.TryGetValue(relatedInstance, out var value)
                    // 如果找到了弱事件处理器，那么返回此处理器。
                    ? value
                    // 如果没有找到弱事件处理器，那么返回 null，等待被过滤。
                    : null);

                // 确认订阅事件的原始对象是否仍然存活。
                var anyHandlerAlive = weakEventHandlerList.Exists(x => x != null);
                if (anyHandlerAlive)
                {
                    // 如果依然存活，则引发事件（无论是否还剩余订阅，这可以与一般事件行为保持一致）。
                    invokingHandlers = weakEventHandlerList.OfType<WeakEventHandler>().SelectMany(x => x.GetInvokingHandlers()).ToList();
                }
                else
                {
                    // 如果没有存活，则回收事件中继。
                    invokingHandlers = null;
                    _relatedInstances.Clear();
                }
            }

            if (invokingHandlers != null)
            {
                foreach (var handler in invokingHandlers)
                {
                    var strongHandler = handler;
                    await strongHandler(sender, e);
                }
            }

            return invokingHandlers != null;
        }

        /// <summary>
        /// 用于关联每一个订阅弱事件的事件处理函数。
        /// <para>
        /// 对于一次形如 Target.Changed += Target_Changed 的事件注册，编译器会隐式将方法组 Target_Changed 转换成新的事件处理函数实例；我们需要代替的，就是这个新的事件处理函数实例。
        /// </para>
        /// </summary>
        private sealed class WeakEventHandler
        {
            internal void Add(MulticastDelegate handler, Func<TSender, TArgs, Task> castedHandler)
            {
                if (handler is null)
                {
                    throw new ArgumentNullException(nameof(handler));
                }

                if (Target != null && Target != handler.Target)
                {
                    throw new ArgumentException("如果代码没有写错，不可能在这里传入不一致的 Target。", nameof(handler));
                }

                Target = handler.Target;

                if (MethodHandlers.TryGetValue(handler.Method, out var handlers))
                {
                    handlers.Add(castedHandler);
                }
                else
                {
                    handlers = new List<Func<TSender, TArgs, Task>>
                {
                    castedHandler,
                };
                    MethodHandlers[handler.Method] = handlers;
                }
            }

            internal void Remove(MulticastDelegate handler)
            {
                if (handler is null)
                {
                    throw new ArgumentNullException(nameof(handler));
                }

                if (Target != null && Target != handler.Target)
                {
                    throw new ArgumentException("如果代码没有写错，不可能在这里传入不一致的 Target。", nameof(handler));
                }

                Target = handler.Target;

                if (MethodHandlers.TryGetValue(handler.Method, out var handlers))
                {
                    handlers.RemoveAt(handlers.Count - 1);
                    if (handlers.Count == 0)
                    {
                        MethodHandlers.Remove(handler.Method);
                    }
                }
            }

            internal IReadOnlyList<Func<TSender, TArgs, Task>> GetInvokingHandlers()
            {
                return MethodHandlers.SelectMany(x => x.Value).ToList();
            }

            /// <summary>
            /// 获取此弱事件处理器关联的目标对象。
            /// </summary>
            internal object? Target { get; private set; }

            /// <summary>
            /// 获取此弱事件处理器关联的目标方法或方法组，以及所有基于此方法组转换而得的可以直接调用的委托。
            /// 在实际上引发事件的时候，应该使用此转换后的实例，以避免使用原始事件处理函数导致的反射、IL 生成等耗性能的执行。
            /// </summary>
            private Dictionary<MethodInfo, List<Func<TSender, TArgs, Task>>> MethodHandlers { get; } = new Dictionary<MethodInfo, List<Func<TSender, TArgs, Task>>>();
        }
    }

    /// <summary>
    /// 只有一个参数的异步弱事件。
    /// </summary>
    /// <typeparam name="TArgs"></typeparam>
    public class AsyncWeakEvent<TArgs> : AsyncWeakEvent<object?, TArgs>
    {
    }
}
