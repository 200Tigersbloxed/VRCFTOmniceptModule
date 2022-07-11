using System;
using System.Collections.Generic;
using System.Threading;
using VRCFaceTracking;

namespace VRCFTOmniceptModule.EyeLidTools;

public class SmoothFloat
{
    private static int instances = 0;
    
    private float target;
    private float current;

    private List<float> validEyes = new()
    {
        0f,
        0.5f,
        1f
    };

    public float Value
    {
        get => current;
        set => target = value;
    }
    
    public SmoothFloat()
    {
        SmoothFloatWorkers.Tasks.Add($"sf{instances}", () =>
        {
            Smooth();
        });
        instances++;
    }

    public void Destroy()
    {
        for (int i = 0; i < instances; i++)
        {
            if (!SmoothFloatWorkers.RemoveTaskById($"sf{i}"))
            {
                Logger.Error("Failed to remove SmoothFloat worker with id sm" + i);
            }
        }
    }
    
    private void Smooth(float by = 0.1f)
    {
        current = current * (1 - by) + target * by;
    }
}

public static class SmoothFloatWorkers
{
    public static Dictionary<string, Action> Tasks = new();

    public static CancellationTokenSource cts = new();
    public static Thread worker = new(() =>
    {
        while (!cts.IsCancellationRequested)
        {
            foreach (Action task in Tasks.Values)
                task.Invoke();
            Thread.Sleep(10);
        }
    });

    public static bool RemoveTaskById(string id) => Tasks.Remove(id);

    private static bool didInit = false;

    public static void Init()
    {
        if (!didInit)
        {
            worker.Start();
            didInit = true;
        }
    }

    public static void Destroy()
    {
        if (didInit)
        {
            Tasks.Clear();
            cts.Cancel();
            worker.Abort();
        }
    }
}