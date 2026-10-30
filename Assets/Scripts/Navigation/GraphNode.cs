public class GraphNode
{
  public int X { get; }
  public int Y { get; }
  public float GValue { get; set; }
  public float HValue { get; set; }
  public GraphNode CameFrom { get; set; }

  public GraphNode(int x, int y)
  {
    X = x;
    Y = y;
    ResetSearch();
  }

  public float F()
  {
    return GValue + HValue;
  }

  public void ResetSearch()
  {
    GValue = float.PositiveInfinity;
    HValue = 0f;
    CameFrom = null;
  }
}
