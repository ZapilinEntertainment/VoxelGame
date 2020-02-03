public struct BlockMaterialsList
{
    public int[] mlist;
    public int mainMaterial;
    public const int MATERIALS_COUNT = 8;

    public BlockMaterialsList(int mat_fwd, int mat_right, int mat_back, int mat_left, int mat_up, int mat_down, int mat_surf, int mat_ceil, int i_mainMaterial)
    {
        mlist = new int[MATERIALS_COUNT];
        mlist[Block.FWD_FACE_INDEX] = mat_fwd;
        mlist[Block.RIGHT_FACE_INDEX] = mat_right;
        mlist[Block.BACK_FACE_INDEX] = mat_back;
        mlist[Block.LEFT_FACE_INDEX] = mat_left;
        mlist[Block.UP_FACE_INDEX] = mat_up;
        mlist[Block.DOWN_FACE_INDEX] = mat_down;
        mlist[Block.SURFACE_FACE_INDEX] = mat_surf;
        mlist[Block.CEILING_FACE_INDEX] = mat_ceil;
        mainMaterial = i_mainMaterial;
    }
    public BlockMaterialsList(int cubeMaterial)
    {
        mlist = new int[MATERIALS_COUNT];
        mainMaterial = cubeMaterial;
        mlist[Block.FWD_FACE_INDEX] = mainMaterial;
        mlist[Block.RIGHT_FACE_INDEX] = mainMaterial;
        mlist[Block.BACK_FACE_INDEX] = mainMaterial;
        mlist[Block.LEFT_FACE_INDEX] = mainMaterial;
        mlist[Block.UP_FACE_INDEX] = mainMaterial;
        mlist[Block.DOWN_FACE_INDEX] = mainMaterial;
        mlist[Block.SURFACE_FACE_INDEX] = PoolMaster.NO_MATERIAL_ID;
        mlist[Block.CEILING_FACE_INDEX] = PoolMaster.NO_MATERIAL_ID;
    }
    public int this[int i]
    {
        get
        {
            if (i >= 0 && i < MATERIALS_COUNT) return mlist[i];
            else return PoolMaster.NO_MATERIAL_ID;
        }
    }

    public byte GetExistenceMask()
    {
        byte m = 0; int nomat = PoolMaster.NO_MATERIAL_ID;
        if (mlist[0] != nomat) m += 1;
        if (mlist[1] != nomat) m += 2;
        if (mlist[2] != nomat) m += 4;
        if (mlist[3] != nomat) m += 8;
        if (mlist[4] != nomat) m += 16;
        if (mlist[5] != nomat) m += 32;
        if (mlist[6] != nomat) m += 64;
        if (mlist[7] != nomat) m += 128;
        return m;
    }
}
