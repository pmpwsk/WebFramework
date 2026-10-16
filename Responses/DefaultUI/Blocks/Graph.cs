using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A bar graph.
/// </summary>
public class Graph : OptionalIdElement
{
    /// <summary>
    /// A data point with a double-precision value and a string label.
    /// </summary>
    public readonly record struct DataPoint(double Value, string Label);
    
    private readonly ListWatchedContainer<GraphSegment> Segments;
    
    public Graph(List<DataPoint> dataPoints)
    {
        FixedAttributes.Add(("class", "wf-graph"));
        Segments = new(this, []);
        SetValues(dataPoints);
    }
    
    /// <summary>
    /// Replaces all data points with the provided ones.
    /// </summary>
    public void SetValues(List<DataPoint> dataPoints)
    {
        var max = dataPoints.Max(dataPoint => dataPoint.Value);
        var segments = new List<GraphSegment>();
        foreach (var dataPoint in dataPoints)
        {
            double percentageDouble = Math.Round(max <= 0 ? 0 : dataPoint.Value / max * 100, MidpointRounding.AwayFromZero);
            int percentageInt = percentageDouble switch
            {
                <= 0 => 0,
                >= 100 => 100,
                _ => Convert.ToInt32(percentageDouble)
            };
            segments.Add(new GraphSegment(percentageInt, dataPoint.Label));
        }
        Segments.ReplaceAll(segments);
    }
        
    public override string RenderedTag
        => "div";
    
    /// <summary>
    /// A graph segment covering the graph's full height.
    /// </summary>
    private class GraphSegment : WatchedElement
    {
        private readonly RequiredWatchedContainer<GraphBar> Bar;
        
        public GraphSegment(int percentage, string label)
        {
            Bar = new(this, new(percentage));
            FixedAttributes.Add(("class", "wf-graph-segment"));
            FixedAttributes.Add(("data-wf-label", label));
        }
        
        public override string RenderedTag
            => "span";
    }
    
    /// <summary>
    /// A graph bar with the correct height within a segment.
    /// </summary>
    private class GraphBar : WatchedElement
    {
        public GraphBar(int percentage)
        {
            FixedAttributes.Add(("class", "wf-graph-bar"));
            FixedAttributes.Add(("style", $"height: {percentage}%;"));
        }
        
        public override string RenderedTag
            => "span";
    }
}