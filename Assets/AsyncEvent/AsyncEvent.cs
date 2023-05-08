using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncEvents
{
	[Serializable]
	public class AsyncEvent
	{
		[SerializeField] private AsyncEventType type;
		[SerializeField] private AsyncMethodCall[] calls;
		[SerializeField] private List<AsyncMethodCall> tempCalls;

		public AsyncEventType Type
		{
			get => type;
			set => type = value;
		}

		public void AddListener(Action action)
		{
			var call = new AsyncMethodCall(action);
			tempCalls.Add(call);
		}

		public void RemoveListener(Action action)
		{
			var call = tempCalls.FirstOrDefault(c => c.HasAction(action));
			if (call != null)
				tempCalls.Remove(call);
		}

		public async Task Invoke()
		{
			switch (Type)
			{
				case AsyncEventType.WhenAll: await InvokeWhenAll(); break;
				case AsyncEventType.Sequence: await InvokeSequenced(); break;
				case AsyncEventType.Synchronous: InvokeSync(); break;
			}
		}

		public async Task Invoke(AsyncEventType type)
		{
			switch (type)
			{
				case AsyncEventType.WhenAll: await InvokeWhenAll(); break;
				case AsyncEventType.Sequence: await InvokeSequenced(); break;
				case AsyncEventType.Synchronous: InvokeSync(); break;
			}
		}

		private async Task InvokeWhenAll()
		{
			List<Task> tasks = new List<Task>();

			foreach (var call in calls)
				tasks.Add(call.Invoke());

			await Task.WhenAll(tasks);

			foreach (var call in tempCalls)
				call.InvokeAction();
		}

		private async Task InvokeSequenced()
		{
			foreach (var call in calls)
				await call.Invoke();
			foreach (var call in tempCalls)
				call.InvokeAction();
		}

		private void InvokeSync()
		{
			foreach (var call in calls)
				_ = call.Invoke();
			foreach (var call in tempCalls)
				call.InvokeAction();
		}
	}
}