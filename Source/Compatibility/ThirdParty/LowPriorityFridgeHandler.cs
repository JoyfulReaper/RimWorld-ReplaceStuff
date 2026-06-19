using Replace_Stuff.Interfaces;
using Replace_Stuff.Replace;
using Verse;

namespace Replace_Stuff.Compatibility.ThirdParty;

#if DEBUG
[ReplacementHandler("Building_Refrigerator", priority: -10)]
public class LowPriorityFridgeHandler : IReplacementHandler
{
    public void PreAction(ReplaceData d, Thing o, Thing n) { }
    public void PostAction(ReplaceData d, Thing o, Thing n)
    {
        Verse.Log.Message("Low Priority Handler Triggered!");
    }
}
#else
// Don't include this unless we are in a debug release
#endif