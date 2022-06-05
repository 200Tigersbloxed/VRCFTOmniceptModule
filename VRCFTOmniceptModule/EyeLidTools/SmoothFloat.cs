using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCFTOmniceptModule.EyeLidTools;

public class SmoothFloat
{
    private float target;
    private float current;
    public float Value
    {
        get => current;
        set => target = value;
    }
    
    private readonly CancellationTokenSource TokenSource = new();
    
    public SmoothFloat()
    {
        Task.Factory.StartNew(() =>
        {
            while (!TokenSource.IsCancellationRequested)
            {
                if(!CompareFloats(current, target))
                    Smooth(target, 60);
                else
                    Thread.Sleep(10);
            }
        });
    }

    public void Destroy() => TokenSource.Cancel();
    
    private void Smooth(float target, float frames, float increment = 0.001f)
    {
        while(CompareFloats(current, target)){
            if(current > target)
                current -= increment * frames;
            if(current < target)
                current += increment * frames;
            Thread.Sleep(Convert.ToInt32(frames * 0.01f));
        }
        current = target;
    }

    private bool CompareFloats(float x, float y) => Math.Round(x, 1) != Math.Round(y, 1);
}