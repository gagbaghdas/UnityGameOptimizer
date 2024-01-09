using System;

public interface IAnalyzer
{
    void Analyze(Action<float, string> progressCallback = null);
}
