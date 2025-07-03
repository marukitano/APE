namespace ChaosMap_V4.Models;

public class FeatureCollection
{
    public string type { get; set; }
    public List<Feature> features { get; set; }
}

public class Feature
{
    public string type { get; set; }
    public Geometry geometry { get; set; }
    public Properties properties { get; set; }
}

public class Geometry
{
    public string type { get; set; }
    public List<double> coordinates { get; set; }
}

public class Properties
{
    public string marker_symbol { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public string address { get; set; }
    public string zip { get; set; }
    public string city { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string source { get; set; }
    public string sourcetype { get; set; }
}
