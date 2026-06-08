using Replace_Stuff.Replace;
using RimWorld;
using Verse;

public class AttachedBuildingData : IExposable
{
    public ThingDef def;
    public ThingDef stuff;

    public IntVec3 position;
    public Rot4 rotation;

    public int hitPoints;
    public Faction faction;

    public QualityCategory? quality;

    public ReplaceData state;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "def");
        Scribe_Defs.Look(ref stuff, "stuff");
        Scribe_Values.Look(ref position, "position");
        Scribe_Values.Look(ref rotation, "rotation");
        Scribe_Values.Look(ref hitPoints, "hitPoints");
    }
}
