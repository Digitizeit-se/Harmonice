InMemoryBound channel
=====

This channel type is same as InMemoryUnBound it works fast and jobs is stored in memory.
Where this channel differ from InMemoryUnBound is the number of jobs stored can be controlled by settings.
Number of jobs in queue is set with parameter **(int) MaxQueuedWork** in worker configuration, if value is not set it will default to Int.MaxValue, and have no bound effect.
Using a **MaxQueuedWork** value can take of some back pressure where one plugin works faster then the next plugin, and prevent application from using up all memory with jobs waiting in memory to be processed.
when **MaxQueuedWork** value is reached the plugin or worker posting on the channel will wait for the job count to go below **MaxQueuedWork** and then start posting.

**(int) WaterMark** in worker configuration is used to set low point that need to be reached before the channel will accept posting on the channel.
The benefit of using **Watermark** is to prevent the posting process to wake up post and go to sleep every time there is a free slot on the channel witch will preserving some resources. 

Example scenario
--------

with a MaxQueuedWork set to 10 and a Watermark set to 5.
When the job count on queue reaches 10, posting on the queue will return false and worker posting on the channel will wait.
The worker processes the job items in queue and when queue count goes below 5 worker posting on channel will wake up and start posting.


what values to use ?
--------

what MaxQueuedWork value to set ?
It depends on the size of the job items and the amount of ram on the machine running Harmonize.
You want to prevent the channel queue to use up all the ram on the machine and having loads of job items in memory will not be beneficial.
In some cases setting the MaxQueuedWork to 1 will save memory and prevent data loss in case of system failure or shutdown.

.. note::

    This is subject to change in future, but for now the plan is for workers to do work synchronously.
    Worker will take one job item at a time and process it pass it on and then take the next job item and process it.
    Suggestion is a setting on how many consecutive job items to process before passing control to next worker.
    This could be done using parallel.ForEach and setting parallel option to what ever value is set in setting.
    



The worker manager and all of its workers can not process more work at any given time then the sum of all worker processing time.
Given that, in many cases processing one job item at a time is the way to go.

There are cases where work needs to be collected immediately for processing, then this is not the right channel given that you reach MaxQueuedWork the first plugin will wait to post its last job and thus will stop collecting new job items.
