# How to write a Timer?

计时器这个东西以前学过怎么写，但当时只是觉得老师写的特好用，并没有搞明白为什么要这样写，这次写ECS的时候本想再写一个，结果发现，事情似乎并没有这么简单。

# What's the problem?

一开始我想的是，我用一个变量数组储存entity到达目的地的这个时间，记为初始时间，如果即时时间 > 初始时间，运行结束，执行行为。

# How many Bool is required?

1. 一个bool
```C#
if (inSomeState)
{
    if (!initialized)
    {
        initialized = true;
        elapsedTime = 0;
        duration = x;
    }
    elapsedTime += deltaTime;
    if (elapsedTime > duration)
    {
        initialized = false;
        // do sth cool, turn to another state.
    }
}
```
2. 两个bool
```C#
if (inSomeState)
{
    if (!started)
    {
        started = true;
        elapsedTime = 0;
        duration = x;
    }
    elapsedTime += deltaTime;
    finished = elapsedTime > duration;
    if (finished)
    {
        started = false;
        // do sth cool, turn to another state.
    }
}
```

3. 3个bool
```C#
if (inSomeState)
{
    if (!running)
    {
        started = true;
        running = true;
        elapsedTime = 0;
        duration = x;
    }
    elapsedTime += deltaTime;
    if ( elapsedTime > duration)
        running = false;
    var finished = started && !running;
    if (finished)
    {
        // do sth cool, turn to another state.
    }
}
```