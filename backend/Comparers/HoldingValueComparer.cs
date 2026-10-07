using backend.Dtos;

namespace backend.Comparers;

public class HoldingValueComparer : IComparer<HoldingResponse>
{
  public int Compare(HoldingResponse? x, HoldingResponse? y)
  {
    if (ReferenceEquals(x, y)) return 0;
    if (x is null) return 1;
    if (y is null) return -1;

    var byValue = y.CurrentValue.CompareTo(x.CurrentValue);

    return byValue != 0 ? byValue : string.Compare(x.Symbol, y.Symbol, StringComparison.Ordinal);
  }
}