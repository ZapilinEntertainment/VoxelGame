// класс, потому что многократно будет передаваться между функциями
public sealed class BlockAnnihilationOrder : MyObject
{
    public bool compensateResources { get { return _vals[0]; } }
    public bool chunkClearing { get { return _vals[1]; } }
    // DEPENDENCE - in structure order - get order for IPlanable
    private bool[] _vals;

    public static readonly BlockAnnihilationOrder ManualDestruction = new BlockAnnihilationOrder(true);
    public static readonly BlockAnnihilationOrder DamageDestruction = new BlockAnnihilationOrder(false);
    public static BlockAnnihilationOrder SystemDestruction { get { return DamageDestruction; } }
    public static BlockAnnihilationOrder ManualCreationError { get { return ManualDestruction; } }
    public static BlockAnnihilationOrder BlockWasDiggedOut { get { return DamageDestruction; } }
    public static readonly BlockAnnihilationOrder ChunkClearingOrder = new BlockAnnihilationOrder();
    public static BlockAnnihilationOrder BlockersClearOrder { get { return DamageDestruction;} }

    public static BlockAnnihilationOrder ReplacedBySystem(bool compensation) {
        return compensation ? ManualDestruction : DamageDestruction;
    }
    //
    private BlockAnnihilationOrder()
    {
        _vals = new bool[2] { false, true };
    }
    private BlockAnnihilationOrder(bool i_compensate)
    {
        _vals = new bool[2] { i_compensate, false };        
    }

    public StructureAnnihilationOrder GetStructureOrder()
    {
        // for IPlanables
        if (chunkClearing) return StructureAnnihilationOrder.ChunkClearing;
        else
        {
            if (compensateResources) return StructureAnnihilationOrder.ManualDestructed;
            else return StructureAnnihilationOrder.SystemDestruction;
        }
    }
    public PlaneAnnihilationOrder GetPlaneOrder()
    {
        if (chunkClearing) return PlaneAnnihilationOrder.ClearByChunk;
        else return new PlaneAnnihilationOrder(false, compensateResources, false);
    }
}

//

public sealed class PlaneAnnihilationOrder : MyObject
{
    public bool recalculateSurface { get { return _vals[0]; } }
    public bool compensateResources { get { return _vals[1]; } private set { _vals[1] = value; } }
    public bool deleteExtensionLink { get { return _vals[2]; } }
    public bool chunkClearing { get { return _vals[3]; } }
    private bool[] _vals;

    public static readonly PlaneAnnihilationOrder ClearByStructure = new PlaneAnnihilationOrder(false, true, false);
    public static readonly PlaneAnnihilationOrder ClearByChunk = new PlaneAnnihilationOrder();
    public static readonly PlaneAnnihilationOrder ExtensionRemovedByFullScaledStructure = new PlaneAnnihilationOrder(false, true, true);
    private static readonly PlaneAnnihilationOrder DeletedBySystem_compensated = new PlaneAnnihilationOrder(true, true, false),
    DeletedBySystem_uncompensated = DeletedBySystem_compensated.ChangeCompensation(false);
    public static readonly PlaneAnnihilationOrder BlockbuildingPartsReplacement = new PlaneAnnihilationOrder(false, false, true);
    //
    public static PlaneAnnihilationOrder DeletedBySystem (bool compensate) {
        return compensate ? DeletedBySystem_compensated : DeletedBySystem_uncompensated;
    }
    //
    private PlaneAnnihilationOrder()
    {
        _vals = new bool[4] { false, false, true, true };
    }
    private PlaneAnnihilationOrder(PlaneAnnihilationOrder origin)
    {
        _vals = origin._vals;
    }
    public PlaneAnnihilationOrder(bool i_surfaceRecalc, bool i_compensation, bool i_delExtlink)
    {
        _vals = new bool[4] { i_surfaceRecalc, i_compensation, i_delExtlink, false };
    }
    //
    private PlaneAnnihilationOrder ChangeCompensation(bool x)
    {
        var copy = new PlaneAnnihilationOrder(this);
        copy.compensateResources = x;
        return copy;
    }
    //
    public StructureAnnihilationOrder GetStructuresOrder()
    {
        if (chunkClearing) return StructureAnnihilationOrder.ChunkClearing;
        else return new StructureAnnihilationOrder(recalculateSurface, compensateResources, !deleteExtensionLink);
    }
    public GrasslandAnnihilationOrder GetGrasslandOrder()
    {
        if (chunkClearing) return GrasslandAnnihilationOrder.TotalClearing;
        else
        {
            if (deleteExtensionLink) return GrasslandAnnihilationOrder.PlaneDestruction;
            else return GrasslandAnnihilationOrder.DestroyedByPlane;
        }
    }
}
//
public sealed class GrasslandAnnihilationOrder : MyObject
{
    private bool[] _vals;
    public bool destroyPlants { get { return _vals[0]; } }
    public bool leaveRuins { get { return _vals[1]; } }
    public bool sendMessageToPlane { get { return _vals[2]; } }
    public bool doSpecialChecks { get { return _vals[3]; } }

    public static readonly GrasslandAnnihilationOrder SelfDestruction = new GrasslandAnnihilationOrder(true,true, true);
    public static readonly GrasslandAnnihilationOrder PlaneDestruction = new GrasslandAnnihilationOrder(true);
    public static readonly GrasslandAnnihilationOrder TotalClearing = new GrasslandAnnihilationOrder(false);
    public static readonly GrasslandAnnihilationOrder DestroyedByPlane = new GrasslandAnnihilationOrder(true, true, false);

    private GrasslandAnnihilationOrder() { }
    private GrasslandAnnihilationOrder(bool i_destroyPlants,bool i_leaveRuins, bool i_sendMessage)
    {
        _vals = new bool[4] { i_destroyPlants,i_leaveRuins, i_sendMessage, false };
    }
    private GrasslandAnnihilationOrder(bool i_totalClearing)
    {
        _vals = new bool[4] { false, false,false, i_totalClearing };
    }


    public StructureAnnihilationOrder GetStructureOrder()
    {
        if (!doSpecialChecks) return StructureAnnihilationOrder.ChunkClearing;
        else return new PlantAnnihilationOrder(false, sendMessageToPlane, leaveRuins);
    }
}
//

public class StructureAnnihilationOrder : MyObject
{
    public bool sendMessageToBasement { get { return _vals[0]; } private set { _vals[0] = value; } }
    public bool returnResources { get { return _vals[1] & doSpecialChecks; } }
    public bool leaveRuins { get { return _vals[2]; } }
    public bool doSpecialChecks { get { return _vals[3]; } }
    public bool sendMessageToGrassland { get { return doSpecialChecks; } }

    protected bool[] _vals;

    public static readonly StructureAnnihilationOrder DamageDestruction = new StructureAnnihilationOrder(true, false, true);
    public static readonly StructureAnnihilationOrder ManualDestructed = new StructureAnnihilationOrder(true, true, false);
    public static readonly StructureAnnihilationOrder SystemDestruction = new StructureAnnihilationOrder(true, false, false);
    public static readonly StructureAnnihilationOrder NoAction = new StructureAnnihilationOrder(false, false, false);
    public static readonly StructureAnnihilationOrder ChunkClearing = new StructureAnnihilationOrder();
    public static StructureAnnihilationOrder DecayDestruction { get { return DamageDestruction; } }
    public static StructureAnnihilationOrder GrindedByWorksite { get { return ManualDestructed; } }
    //
    public static StructureAnnihilationOrder HasNoBasementError { get { return NoAction; } }
    public static StructureAnnihilationOrder sectionCollapseOrder { get { return DamageDestruction; } }
    public static StructureAnnihilationOrder blockHaveNoSupport { get { return NoAction; } }
    //
    protected StructureAnnihilationOrder() {
        _vals = new bool[4] { false, false, false, false };
    }
    protected StructureAnnihilationOrder(StructureAnnihilationOrder origin)
    {
        _vals = origin._vals;
    }
    public StructureAnnihilationOrder(bool sendMsg, bool compensateResources, bool ruins)
    {
        _vals = new bool[4] { sendMsg, compensateResources, ruins, true };
    }
    private StructureAnnihilationOrder (bool sendMsg, bool chunkClearing)
    {
        _vals = new bool[4] { sendMsg, false, false, chunkClearing };
    }

    public static StructureAnnihilationOrder GetReplacingOrder(bool compensation)
    {
        return new StructureAnnihilationOrder(false, compensation, false);
    }
    //

    public BlockAnnihilationOrder GetOrderForIPlanable()
    {
        if (!doSpecialChecks) return BlockAnnihilationOrder.ChunkClearingOrder;
        else
        {
            if (returnResources) return BlockAnnihilationOrder.ManualDestruction;
            else return BlockAnnihilationOrder.SystemDestruction;
        }
    }
    public StructureAnnihilationOrder ChangeMessageSending(bool x)
    {
        var c = new StructureAnnihilationOrder(this);
        c.sendMessageToBasement = x;
        return c;
    }
}

//
public sealed class PlantAnnihilationOrder : StructureAnnihilationOrder
{
    new public bool sendMessageToGrassland { get { return _vals[4]; } }
    //
    public static readonly PlantAnnihilationOrder Gathered = new PlantAnnihilationOrder(true, true, false);
    public static readonly PlantAnnihilationOrder Dryed = new PlantAnnihilationOrder(true, true, true);
        //

    public PlantAnnihilationOrder(bool msgToGrassland, bool msgToBasement, bool ruins)
    {
        _vals = new bool[5] { msgToBasement, false, ruins, true, msgToGrassland };
    }

    public PlantAnnihilationOrder(bool i_msgToGrassland)
    {
        _vals = new bool[5] { true, false, false, true, i_msgToGrassland};
    }   
}

