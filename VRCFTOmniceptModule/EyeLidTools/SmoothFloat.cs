namespace VRCFTOmniceptModule.EyeLidTools;

public class SmoothFloat
{
    private static int instances;
    
    private float target;
    private float current;

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

    private static bool didInit;

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
        }
    }
}