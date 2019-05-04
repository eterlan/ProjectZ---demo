 

## How Unity’s C# Job Types Are Implemented

Jul 30, 2018	

Unity provides `IJob`, `IJobParallelFor`, and `IJobParallelForTransform` and it turns out these are written in C# so we can learn how they’re implemented. Today’s article goes through each of them so we can learn more about how they work and even see how we can write our own custom job types.



We can see the implementations of `IJob`, `IJobParallelFor`, and `IJobParallelForTransform` by looking at Unity’s [open source C# code](https://github.com/Unity-Technologies/UnityCsReference/tree/15562ea1a55a98587abcea8f0c4aa5bfa379788c/Runtime/Jobs/Managed). To start, let’s look at `IJob.cs` to see how it’s implemented. The following [source code](https://github.com/Unity-Technologies/UnityCsReference/blob/02f8e8ca594f156dd6b2088ad89451143ca1b87e/Runtime/Jobs/Managed/IJob.cs) had no comments aside from a legal header and some very long lines, so I’ve added many comments to explain what’s going on and formatted some whitespace for clarity:

~~~C#
```C#
// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License
 
using System;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
 
namespace Unity.Jobs
{
    // This is the IJob interface that our job structs implement.
    // This attribute is a hint for the Burst compiler that tells it which
    // struct has the Execute function for this type of job.
    [JobProducerType(typeof(IJobExtensions.JobStruct < >))]
    public interface IJob
    {
        // Job structs must implement everything here. This is called by the
        // Execute function of the struct marked by [JobProducerType] above.
        void Execute();
    }
 
    // Extension functions related to IJob
    public static class IJobExtensions
    {
        // This is the struct marked by [JobProducerType] above. It has the
        // Execute function that's called when the job executes.
        // This is marked internal so code using IJob never sees it.
        internal struct JobStruct<T> where T : struct, IJob
        {
            // This is a pointer to reflection data for the job type. The
            // reflection data lives within the Unity engine and is used by the
            // jobs system to know what this type of job contains.
            public static IntPtr                    jobReflectionData;
 
            // Initializes the job reflection data
            public static IntPtr Initialize()
            {
                // Only create the job reflection data if it's not already
                // created. When creating it, tell it about the C# job type,
                // what type of job (Single in this case) it is, and what
                // delegate to call when the job should execute.
                if (jobReflectionData == IntPtr.Zero)
                    jobReflectionData = JobsUtility.CreateJobReflectionData(
                        typeof(T),
                        JobType.Single,
                        (ExecuteJobFunction)Execute);
                return jobReflectionData;
            }
 
            // Delegate type for the delegate to call when the job should execute
            public delegate void ExecuteJobFunction(
                ref T data,
                IntPtr additionalPtr,
                IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex);
 
            // Function to call when the job should execute
            public static void Execute(
                ref T data,
                IntPtr additionalPtr,
                IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex)
            {
                // Since IJob is a trivial Single type of job, just call the
                // job struct's Execute once with no parameters.
                data.Execute();
            }
        }
 
        // This is the extension function that allows user code to call:
        //   myJob.Schedule();
        // or
        //   myJob.Schedule(myDependency);
        unsafe public static JobHandle Schedule<T>(
            this T jobData,
            JobHandle dependsOn = new JobHandle())
            where T : struct, IJob
        {
            // Create the parameters used when scheduling the job.
            // First, pass a pointer (the address of) to the job struct.
            // Second, pass the reflection data for the job type.
            // Third, pass the dependencies.
            // Fourth, tell it to run the job batched.
            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref jobData),
                JobStruct<T>.Initialize(),
                dependsOn,
                ScheduleMode.Batched);
 
            // Schedule the job to be run
            return JobsUtility.Schedule(ref scheduleParams);
        }
 
        // This is the extension function that allows user code to call:
        //   myJob.Run();
        unsafe public static void Run<T>(this T jobData) where T : struct, IJob
        {
            // Create the scheduling parameters as above, except tell it to
            // run the job immediately and synchronously instead of batching it.
            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref jobData),
                JobStruct<T>.Initialize(),
                new JobHandle(),
                ScheduleMode.Run);
 
            // Run the job immediately and synchronously
            JobsUtility.Schedule(ref scheduleParams);
        }
    }
}
~~~

This code uses some esoteric Unity APIs and conventions, but as we’ll see it’s mostly boilerplate that can be copied and pasted when creating new types of jobs. To illustrate this, let’s look at the [implementation](https://github.com/Unity-Technologies/UnityCsReference/blob/02f8e8ca594f156dd6b2088ad89451143ca1b87e/Runtime/Jobs/Managed/IJobParallelFor.cs) of `IJobParallelFor`. I’ll again add comments and clean up whitespace, but I’ll limit the comments to just the differences from `IJob`:

```C#
// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License
 
using System;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
 
namespace Unity.Jobs
{
    [JobProducerType(typeof(IJobParallelForExtensions.ParallelForJobStruct < >))]
    public interface IJobParallelFor
    {
        // Notice that this job type has a different signature for its Execute.
        void Execute(int index);
    }
 
    public static class IJobParallelForExtensions
    {
        internal struct ParallelForJobStruct<T> where T : struct, IJobParallelFor
        {
            public static IntPtr                            jobReflectionData;
 
            public static IntPtr Initialize()
            {
                // IJobParallelFor uses JobType.ParallelFor instead of Single
                if (jobReflectionData == IntPtr.Zero)
                    jobReflectionData = JobsUtility.CreateJobReflectionData(
                        typeof(T),
                        JobType.ParallelFor,
                        (ExecuteJobFunction)Execute);
                return jobReflectionData;
            }
 
            // The Execute delegate and function have the same signature as IJob
            public delegate void ExecuteJobFunction(
                ref T data,
                IntPtr additionalPtr,
                IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex);
 
            public static unsafe void Execute(
                ref T jobData,
                IntPtr additionalPtr,
                IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex)
            {
                // Loop until we're done executing ranges of indices
                while (true)
                {
                    // Get the range of indices to execute
                    // If this returns false, we're done
                    int begin;
                    int end;
                    if (!JobsUtility.GetWorkStealingRange(
                        ref ranges,
                        jobIndex,
                        out begin,
                        out end))
                        break;
 
                    // Call the job's Execute for each index in the range
                    for (var i = begin; i < end; ++i)
                        jobData.Execute(i);
                }
            }
        }
 
        unsafe public static JobHandle Schedule<T>(
            this T jobData,
            int arrayLength,
            int innerloopBatchCount,
            JobHandle dependsOn = new JobHandle())
            where T : struct, IJobParallelFor
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref jobData),
                ParallelForJobStruct<T>.Initialize(),
                dependsOn,
                ScheduleMode.Batched);
            return JobsUtility.ScheduleParallelFor(
                ref scheduleParams,
                arrayLength,
                innerloopBatchCount);
        }
 
        unsafe public static void Run<T>(this T jobData, int arrayLength)
            where T : struct, IJobParallelFor
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref jobData),
                ParallelForJobStruct<T>.Initialize(),
                new JobHandle(),
                ScheduleMode.Run);
            JobsUtility.ScheduleParallelFor(
                ref scheduleParams,
                arrayLength,
                arrayLength);
        }
    }
}
```

It turns out that not much changed when implementing `IJobParallelFor`. The only substantial change was in `Execute`, which was used to implement how this particular type of job works. Finally, let’s look at the last of Unity’s built-in job types: `IJobParallelForTransform`. I’ll once again mark up the [implementation](https://github.com/Unity-Technologies/UnityCsReference/blob/02f8e8ca594f156dd6b2088ad89451143ca1b87e/Runtime/Jobs/Managed/IJobParallelForTransform.cs) with comments for what’s new and some whitespace cleanup:

```C#
// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License
 
using UnityEngine;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
 
//@TODO: Move this into Runtime/Transform folder with the test of Transform component
namespace UnityEngine.Jobs
{
    [JobProducerType(
        typeof(IJobParallelForTransformExtensions.TransformParallelForLoopStruct < >))]
    public interface IJobParallelForTransform
    {
        // Execute's signature has changed again
        void Execute(int index, TransformAccess transform);
    }
 
    public static class IJobParallelForTransformExtensions
    {
        internal struct TransformParallelForLoopStruct<T>
            where T : struct, IJobParallelForTransform
        {
            static public IntPtr                    jobReflectionData;
 
            public static IntPtr Initialize()
            {
                // This is still just a ParallelFor job, not anything special
                // for transforms.
                if (jobReflectionData == IntPtr.Zero)
                    jobReflectionData = JobsUtility.CreateJobReflectionData(
                        typeof(T),
                        JobType.ParallelFor,
                        (ExecuteJobFunction)Execute);
                return jobReflectionData;
            }
 
            // The Execute function signature is also the same
            public delegate void ExecuteJobFunction(
                ref T jobData,
                System.IntPtr additionalPtr,
                System.IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex);
            public static unsafe void Execute(
                ref T jobData,
                System.IntPtr jobData2,
                System.IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex)
            {
                // Make a copy of jobData2
                IntPtr transformAccessArray;
                UnsafeUtility.CopyPtrToStructure(
                    (void*)jobData2,
                    out transformAccessArray);
 
                // Call a couple internal, undocumented functions to get the
                // TransformAccess array
                int* sortedToUserIndex = (int*)TransformAccessArray.GetSortedToUserIndex(
                    transformAccessArray);
                TransformAccess* sortedTransformAccess = (TransformAccess*)TransformAccessArray.GetSortedTransformAccess(
                    transformAccessArray);
 
                // Get the range of sorted indices for this job. Note that this
                // is different than the work stealing range in IJobParallelFor.
                int begin;
                int end;
                JobsUtility.GetJobRange(
                    ref ranges,
                    jobIndex,
                    out begin,
                    out end);
 
                // Call the job's Execute for every index in the range
                for (int i = begin; i < end; i++)
                {
                    // Convert the sorted index to the user index
                    int sortedIndex = i;
                    int userIndex = sortedToUserIndex[sortedIndex];
 
                    // Call the job's Execute with the user index and the
                    // corresponding TransformAccess
                    jobData.Execute(
                        userIndex,
                        sortedTransformAccess[sortedIndex]);
                }
            }
        }
 
        unsafe static public JobHandle Schedule<T>(
            this T jobData,
            TransformAccessArray transforms,
            JobHandle dependsOn = new JobHandle())
            where T : struct, IJobParallelForTransform
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref jobData),
                TransformParallelForLoopStruct<T>.Initialize(),
                dependsOn,
                ScheduleMode.Batched);
            return JobsUtility.ScheduleParallelForTransform(
                ref scheduleParams,
                transforms.GetTransformAccessArrayForSchedule());
        }
 
        //@TODO: Run
    }
}
```

We can see that `IJobParallelForTransform` is a job just like `IJobParallelFor` in terms of how it’s scheduled by the job system. The difference lies in its `Execute` where it uses internal APIs to get the `TransformAccess` structures. That part is off-limits for our own code, but it shows a bit about how flexible the system is.

Now that we know how each of these job types are implemented, it’s easy to see how we could write our own job types. The bulk of the work is just to fill out an `Execute` function to do what we want. For inspiration, check out `IJobParallelForBatch` and `IJobParallelForFilter` in Unity’s ECS package as of version `0.0.12-preview.8`.

[Share on Facebook](http://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fjacksondunstan.com%2Farticles%2F4857)[Share on Twitter](http://twitter.com/intent/tweet/?text=How+Unity's+C%23+Job+Types+Are+Implemented&url=https%3A%2F%2Fjacksondunstan.com%2Farticles%2F4857)[Share on Linkedin](http://www.linkedin.com/shareArticle?mini=true&url=https%3A%2F%2Fjacksondunstan.com%2Farticles%2F4857&title=How+Unity's+C%23+Job+Types+Are+Implemented)[Share on Reddit](http://reddit.com/submit?url=https%3A%2F%2Fjacksondunstan.com%2Farticles%2F4857&title=How+Unity's+C%23+Job+Types+Are+Implemented)

### Comments

- [#1](https://jacksondunstan.com/articles/4857#comment-713059) by **ahtur** on October 23rd, 2018	· [Reply](https://jacksondunstan.com/articles/4857?replytocom=713059#respond) | Quote

  Thanks for this introduction. Is there currently a way to implement an IJobParallelForXXX (and the XXXAccess that goes with it, I guess) for other Unity classes, such as MeshRenderer, for example ?

  

  - [#2](https://jacksondunstan.com/articles/4857#comment-713067) by [jackson](http://jacksondunstan.com/) on October 23rd, 2018	· [Reply](https://jacksondunstan.com/articles/4857?replytocom=713067#respond) | Quote

    Regardless of what job type you choose or whether it was written by Unity or you, you’ll still have to comply with the rules of the job system. At this point (2018.2), that unfortunately leaves off a very large majority of the Unity and .NET APIs. The good news is that Unity is rapidly adding more and more job-compatible APIs with each release. Until then you’ll need to still write a lot of code that only runs on the main thread (i.e. outside of jobs).

    

- [#3](https://jacksondunstan.com/articles/4857#comment-713512) by **Alph** on November 25th, 2018	· [Reply](https://jacksondunstan.com/articles/4857?replytocom=713512#respond) | Quote

  Thanks

  





**Name** 

**E-Mail**  *(will not be published)*

**Website**  *(optional)*

**Comment**





Use `<pre lang="csharp">CODE</pre>` for C# code blocks
Use `<pre lang="actionscript3">CODE</pre>` for AS3 code blocks
Use `<code>CODE</code>` for inline code snippets.

[![Creative Commons BY 4.0 License](https://jacksondunstan.com/wp-content/themes/jacksondunstancom/images/ccBy.png)](https://creativecommons.org/licenses/by/4.0/) [![MIT License](https://jacksondunstan.com/wp-content/themes/jacksondunstancom/images/mit.png)](http://www.opensource.org/licenses/mit-license.php) [![Send Jackson Mail](https://jacksondunstan.com/wp-content/themes/jacksondunstancom/images/mail.png)](mailto:jackson@jacksondunstan.com)