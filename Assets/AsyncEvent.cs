using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncEvent
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

        public async Task Invoke()
		{
			switch (Type)
			{
				case AsyncEventType.WaitAll:	 await InvokeWhenAll();	  break;
				case AsyncEventType.Sequenced:	 await InvokeSequenced(); break;
				case AsyncEventType.Synchronous: InvokeSync();			  break;
			}
		}

		public async Task InvokeWhenAll()
		{
			List<Task> tasks = new List<Task>();
			
			foreach (var call in calls)
				tasks.Add(call.Invoke());

			await Task.WhenAll(tasks);
		}

		public void InvokeSync()
		{
			foreach (var call in calls)
				call.Invoke();
		}

		public async Task InvokeSequenced()
		{
			foreach (var call in calls)
				await call.Invoke();
		}
	}
}