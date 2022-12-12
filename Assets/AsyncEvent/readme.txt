AsyncEvent works just like the integrated UnityEvent except it's asynchronous!

Being asynchronous, you can call methods, that return tasks.
When invoking the event you will also be able to await the invoke.

You'll notice theres a little dropdown on the event. This enables you to choose between 3 types of invoking:
* WaitAll	(this will invoke all methods immediately, but won't finish until all are finished)
* Sequence	(this will invoke methods one after another and finish when the last invoke finishes)
* Synchronous	(this will invoke synchronously, meaning the behaviour is the same as the normal UnityEvent)

5 minute tutorial and explanation:
<youtube link here>