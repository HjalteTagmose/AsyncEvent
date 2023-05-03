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

		public AsyncEventType Type 
		{
			get => type;
			set => type = value;
		}

		public void AddListener(Action action)
		{
			var call = new AsyncMethodCall(action);
			var temp = new AsyncMethodCall[calls.Length + 1];
			Array.Copy(calls, temp, calls.Length);
			temp[calls.Length] = call;
			calls = temp;
		}

		public async Task Invoke()
		{
			switch (Type)
			{
				case AsyncEventType.WhenAll:   await InvokeWhenAll();	break;
				case AsyncEventType.Sequence:  await InvokeSequenced(); break;
				case AsyncEventType.Synchronous:     InvokeSync();		break;
			}
		}

		public async Task Invoke(AsyncEventType type)
		{
			switch (type)
			{
				case AsyncEventType.WhenAll:   await InvokeWhenAll();	break;
				case AsyncEventType.Sequence:  await InvokeSequenced(); break;
				case AsyncEventType.Synchronous:     InvokeSync();		break;
			}
		}

		private async Task InvokeWhenAll()
		{
			List<Task> tasks = new List<Task>();
			
			foreach (var call in calls)
				tasks.Add(call.Invoke());

			await Task.WhenAll(tasks);
        }

        private async Task InvokeSequenced()
        {
            foreach (var call in calls)
                await call.Invoke();
        }

        private void InvokeSync()
		{
			foreach (var call in calls)
				_ = call.Invoke();
		}
	}
}