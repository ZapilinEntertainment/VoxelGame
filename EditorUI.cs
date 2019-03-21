using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class EditorUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] GameObject actionsPanel, listPanel, menuPanel, settingsPanel;
    [SerializeField] RawImage currentActionIcon, materialButtonImage;
    [SerializeField] Image[] buttonsImages;
    [SerializeField] Image saveButtonImage, loadButtonImage;
    [SerializeField] Text materialNameTextField;
#pragma warning restore 0649  

    private Image chosenMaterialButton;
    private enum ClickAction { CreateBlock, DeleteBlock, AddGrassland, DeleteGrassland, MakeSurface, MakeCave, AddLifepower, TakeLifepower, CreateLifesource }
    private ClickAction currentAction;
    private LifeSource lifesource;
    private SaveSystemUI saveSystem;
    private int chosenMaterialId = ResourceType.STONE_ID;
    private bool visualBorderDrawn = false, listPrepared;
    private readonly int[] availableMaterials = new int[]
    {
        ResourceType.STONE_ID,
        ResourceType.DIRT_ID,
        ResourceType.LUMBER_ID,
        ResourceType.METAL_K_ID,
        ResourceType.METAL_M_ID ,
        ResourceType.METAL_E_ID ,
        ResourceType.METAL_N_ID ,
        ResourceType.METAL_P_ID ,
        ResourceType.METAL_S_ID ,
        ResourceType.MINERAL_F_ID ,
        ResourceType.MINERAL_L_ID ,
        ResourceType.PLASTICS_ID ,
        ResourceType.CONCRETE_ID ,
        ResourceType.FERTILE_SOIL_ID ,
        ResourceType.GRAPHONIUM_ID ,
        ResourceType.SNOW_ID,
        PoolMaster.MATERIAL_ADVANCED_COVERING_ID
    };

    private const int LIFEPOWER_PORTION = 100;

    private void Start()
    {
        buttonsImages[(int)currentAction].overrideSprite = PoolMaster.gui_overridingSprite;
        materialButtonImage.uvRect = ResourceType.GetResourceIconRect(chosenMaterialId);
        materialNameTextField.text = Localization.GetResourceName(chosenMaterialId);
        if (saveSystem == null) saveSystem = SaveSystemUI.Initialize(transform.root);
        saveSystem.ingame = true;
        ActionsPanel();

        FollowingCamera.main.ResetTouchRightBorder();
        FollowingCamera.main.CameraRotationBlock(false);
        LocalizeTitles();        
    }

    public void Click()
    {
        if (menuPanel.activeSelf) return;
        if (FollowingCamera.touchscreen)
        {
            if (Input.touchCount > 1 | FollowingCamera.camRotateTrace > 0) return;
        }
        RaycastHit rh;
        if (Physics.Raycast(FollowingCamera.cam.ScreenPointToRay(Input.mousePosition), out rh))
        {
            var chunk = GameMaster.realMaster.mainChunk;
            var bh = chunk.GetBlock(rh.point, rh.normal);
            var b = bh.block;
            if (b == null) return;
            bool action = false;
            switch (currentAction)
            {
                case ClickAction.CreateBlock:
                    {
                        switch (bh.faceIndex)
                        {
                            case 0:
                                {
                                    if (b.pos.z < Chunk.CHUNK_SIZE - 1)
                                    {
                                        chunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y, b.pos.z + 1), BlockType.Cube, chosenMaterialId, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    if (b.pos.x < Chunk.CHUNK_SIZE - 1)
                                    {
                                        chunk.AddBlock(new ChunkPos(b.pos.x + 1, b.pos.y, b.pos.z), BlockType.Cube, chosenMaterialId, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (b.pos.z > 0)
                                    {
                                        chunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y, b.pos.z - 1), BlockType.Cube, chosenMaterialId, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    if (b.pos.x  > 0)
                                    {
                                        chunk.AddBlock(new ChunkPos(b.pos.x - 1, b.pos.y, b.pos.z), BlockType.Cube, chosenMaterialId, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    if (b.pos.y < Chunk.CHUNK_SIZE - 1)
                                    {
                                        chunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y + 1, b.pos.z), BlockType.Cube, chosenMaterialId, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    if (b.pos.y > 0)
                                    {
                                        chunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y - 1, b.pos.z), BlockType.Cube, chosenMaterialId, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case 6:
                            case 7:
                                {
                                    chunk.ReplaceBlock(b.pos, BlockType.Cube, chosenMaterialId, true);
                                    action = true;
                                    break;
                                }
                        }
                    }
                    break;
                case ClickAction.DeleteBlock:
                    {
                        if (b.type == BlockType.Surface)
                        {
                            var lb = chunk.GetBlock(b.pos.x, b.pos.y - 1, b.pos.z);
                            if (lb != null && (lb.type == BlockType.Cube | lb.type == BlockType.Cave))
                            {
                                chunk.DeleteBlock(lb.pos);
                            }
                            else chunk.DeleteBlock(b.pos);
                        }
                        else
                        {
                            if (b.type == BlockType.Cube)    chunk.DeleteBlock(b.pos);
                            else
                            {
                                if (bh.faceIndex == 6) (b as CaveBlock).DestroySurface();
                                else chunk.DeleteBlock(b.pos);
                            }
                        }
                        action = true;
                        break;
                    }
                case ClickAction.AddGrassland:
                    {
                        SurfaceBlock sb = b as SurfaceBlock;
                        if (sb == null & b.pos.y < Chunk.CHUNK_SIZE - 1)
                        {
                            sb = b.myChunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y + 1, b.pos.z), BlockType.Surface, ResourceType.DIRT_ID, true) as SurfaceBlock;
                            action = true;
                        }
                        if (sb != null && sb.grassland == null)
                        {
                            if (sb.material_id != ResourceType.DIRT_ID | sb.material_id != ResourceType.FERTILE_SOIL_ID) sb.ReplaceMaterial(ResourceType.DIRT_ID);
                            Grassland.CreateOn(sb);
                            sb.grassland.AddLifepowerAndCalculate(LIFEPOWER_PORTION);
                            action = true;
                        }
                        break;
                    }
                case ClickAction.DeleteGrassland:
                    {
                        SurfaceBlock sb = b as SurfaceBlock;
                        if (sb != null && sb.grassland != null)
                        {
                            sb.grassland.Annihilation(true, true);
                            action = true;
                        }
                        break;
                    }
                case ClickAction.MakeSurface:
                    {
                        if (bh.faceIndex == 4)
                        {
                            if (b.type != BlockType.Surface)
                            {
                                if (b.pos.y < Chunk.CHUNK_SIZE - 1) b.myChunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y + 1, b.pos.z), BlockType.Surface, chosenMaterialId, true);
                            }
                            else b.ReplaceMaterial(chosenMaterialId);
                            action = true;
                        }
                        else
                        {
                            int newY;
                            if (b.type == BlockType.Cave) newY = b.pos.y;
                            else newY = b.pos.y - 1;
                            if (bh.faceIndex < 4 & b.pos.y - 1 >= 0)
                            {
                                ChunkPos cpos = b.pos;
                                bool correctFace = false;
                                switch (bh.faceIndex)
                                {                                  
                                    case 0:
                                        {
                                            if (b.pos.z + 1 <= Chunk.CHUNK_SIZE - 1)
                                            {
                                                cpos = new ChunkPos(b.pos.x, newY, b.pos.z + 1);
                                                correctFace = true;
                                            }
                                            break;
                                        }
                                    case 1:
                                        {
                                            if (b.pos.x + 1 <= Chunk.CHUNK_SIZE - 1)
                                            {
                                                cpos = new ChunkPos(b.pos.x + 1, newY, b.pos.z);
                                                correctFace = true;
                                            }
                                            break;
                                        }
                                    case 2:
                                        {
                                            if (b.pos.z - 1 >= 0)
                                            {
                                                cpos = new ChunkPos(b.pos.x, newY, b.pos.z - 1);
                                                correctFace = true;
                                            }
                                            break;
                                        }
                                    case 3:
                                        {
                                            if (b.pos.x - 1 >= 0)
                                            {
                                                cpos = new ChunkPos(b.pos.x - 1, newY, b.pos.z);
                                                correctFace = true;
                                            }
                                            break;
                                        }
                                }
                                if (correctFace)
                                {
                                    var lb = chunk.GetBlock(cpos);
                                    if (lb == null)
                                    {
                                        chunk.AddBlock(cpos, BlockType.Cave, PoolMaster.NO_MATERIAL_ID, chosenMaterialId, true);
                                        action = true;
                                    }
                                    else
                                    {
                                        if (lb.type == BlockType.Surface | lb.type == BlockType.Shapeless)
                                        {
                                            chunk.ReplaceBlock(cpos, BlockType.Cube, lb.material_id, chosenMaterialId, true);
                                            action = true;
                                        }
                                    }
                                }
                            }
                        }                        
                        break;
                    }
                case ClickAction.MakeCave:
                    {
                        if (b.type == BlockType.Cube)
                        {
                            var cb = b as CubeBlock;
                            float x = cb.myChunk.CalculateSupportPoints(cb.pos.x, cb.pos.y, cb.pos.z);
                            if (x >= Chunk.SUPPORT_POINTS_ENOUGH_FOR_HANGING)
                            {
                                Block lb = cb.myChunk.GetBlock(cb.pos.x, cb.pos.y - 1, cb.pos.z); 
                                cb.myChunk.ReplaceBlock(cb.pos, BlockType.Cave, lb != null ? lb.material_id : -1, true);
                                action = true;
                            }
                            else
                            {
                                bool correctFace = false;
                                ChunkPos cpos = b.pos;
                                switch (bh.faceIndex)
                                {
                                    case 0:
                                        if (b.pos.z + 1 <= Chunk.CHUNK_SIZE - 1)
                                        {
                                            cpos = new ChunkPos(b.pos.x, b.pos.y, b.pos.z + 1);
                                            correctFace = true;
                                        }
                                        break;
                                    case 1:
                                        if (b.pos.x + 1 <= Chunk.CHUNK_SIZE - 1)
                                        {
                                            cpos = new ChunkPos(b.pos.x + 1, b.pos.y, b.pos.z);
                                            correctFace = true;
                                        }
                                        break;
                                    case 2:
                                        if (b.pos.z - 1 >= 0)
                                        {
                                            cpos = new ChunkPos(b.pos.z, b.pos.y, b.pos.z - 1);
                                            correctFace = true;
                                        }
                                        break;
                                    case 3:
                                        if (b.pos.x - 1 <= Chunk.CHUNK_SIZE - 1)
                                        {
                                            cpos = new ChunkPos(b.pos.x - 1, b.pos.y, b.pos.z);
                                            correctFace = true;
                                        }
                                        break;
                                }
                                if (correctFace)
                                {
                                    chunk.AddBlock(cpos, BlockType.Cave, chosenMaterialId, true);
                                    action = true;
                                }
                            }
                        }
                        else
                        {
                            if (b.type == BlockType.Cave)
                            {
                                var cvb = b as CaveBlock;
                                if (!cvb.haveSurface)
                                {
                                    cvb.RestoreSurface(chosenMaterialId);
                                }
                                else
                                {
                                    bool correctFace = false;
                                    ChunkPos cpos = b.pos;
                                    switch (bh.faceIndex)
                                    {
                                        case 0:
                                            if (b.pos.z + 1 <= Chunk.CHUNK_SIZE - 1)
                                            {
                                                cpos = new ChunkPos(b.pos.x, b.pos.y, b.pos.z + 1);
                                                correctFace = true;
                                            }
                                            break;
                                        case 1:
                                            if (b.pos.x + 1 <= Chunk.CHUNK_SIZE - 1)
                                            {
                                                cpos = new ChunkPos(b.pos.x + 1, b.pos.y, b.pos.z);
                                                correctFace = true;
                                            }
                                            break;
                                        case 2:
                                            if (b.pos.z - 1 >= 0)
                                            {
                                                cpos = new ChunkPos(b.pos.z, b.pos.y, b.pos.z - 1);
                                                correctFace = true;
                                            }
                                            break;
                                        case 3:
                                            if (b.pos.x - 1 <= Chunk.CHUNK_SIZE - 1)
                                            {
                                                cpos = new ChunkPos(b.pos.x - 1, b.pos.y, b.pos.z);
                                                correctFace = true;
                                            }
                                            break;
                                    }
                                    if (correctFace)
                                    {
                                        chunk.AddBlock(cpos, BlockType.Cave, chosenMaterialId, true);
                                        action = true;
                                    }
                                }
                            }
                            else
                            {
                                chunk.ReplaceBlock(b.pos, BlockType.Cave, b.material_id, chosenMaterialId, true);
                                action = true;
                            }
                        }
                        break;
                    }
                case ClickAction.AddLifepower:
                    {
                        SurfaceBlock sb = b as SurfaceBlock;
                        if (sb != null && sb.grassland != null) sb.grassland.AddLifepowerAndCalculate(LIFEPOWER_PORTION);
                    }
                    break;
                case ClickAction.TakeLifepower:
                    {
                        SurfaceBlock sb = b as SurfaceBlock;
                        if (sb != null && sb.grassland != null) sb.grassland.TakeLifepowerAndCalculate(LIFEPOWER_PORTION);
                        break;
                    }
                case ClickAction.CreateLifesource:
                    {
                        SurfaceBlock sb = b as SurfaceBlock;
                        if (sb != null)
                        {
                            if (lifesource != null) lifesource.Annihilate(false);
                            if (b.pos.y < Chunk.CHUNK_SIZE / 2)
                            {
                                lifesource = Structure.GetStructureByID(Structure.LIFESTONE_ID) as LifeSource;
                            }
                            else lifesource = Structure.GetStructureByID(Structure.TREE_OF_LIFE_ID) as LifeSource;
                            lifesource.SetBasement(sb, PixelPosByte.zero);
                        }
                        break;
                    }
            }
            if (action) chunk.RenderStatusUpdate();
        }

        if (!visualBorderDrawn)
        {
            GameMaster.realMaster.mainChunk.DrawBorder();
            visualBorderDrawn = true;
        }
    }

    public void ChangeClickAction(int x)
    {
        int lastAction = (int)currentAction;
        if (lastAction == x) return;
        currentAction = (ClickAction)x;
        currentActionIcon.uvRect = new Rect((x % 4) * 0.25f, (x / 4) * 0.25f, 0.25f, 0.25f);
        buttonsImages[lastAction].overrideSprite = null;
        buttonsImages[x].overrideSprite = PoolMaster.gui_overridingSprite;
        listPanel.SetActive(false);
    }

    public void ActionsPanel()
    {
        if (!actionsPanel.activeSelf)
        {
            actionsPanel.SetActive(true);
            menuPanel.SetActive(false);
        }
        else
        {
            actionsPanel.SetActive(false);
            listPanel.SetActive(false);
        }
    }

    public void MaterialButtonToggle()
    {
        if (listPanel.activeSelf)
        {
            listPanel.SetActive(false);            
        }
        else
        {
            if (!listPrepared)
            {
                int count = availableMaterials.Length;
                RectTransform contentHolder = listPanel.transform.GetChild(0).GetChild(0) as RectTransform;
                RectTransform example = contentHolder.GetChild(0) as RectTransform;
                float h = example.rect.height;
                Vector3 startPos = example.localPosition, down = Vector3.down;
                contentHolder.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, h * count);

                example.GetChild(0).GetComponent<Text>().text = Localization.GetResourceName(availableMaterials[0]);
                chosenMaterialButton = example.GetComponent<Image>();
                chosenMaterialButton.overrideSprite = PoolMaster.gui_overridingSprite;
                
                for (int i = 1; i < count; i++)
                {
                    Transform t = Instantiate(example, contentHolder);
                    t.localPosition = startPos + down * h * i;
                    t.GetChild(0).GetComponent<Text>().text = Localization.GetResourceName(availableMaterials[i]);
                    int x = i;
                    t.GetComponent<Button>().onClick.AddListener(() => { this.ChangeMaterial(x); });
                }
                example.GetComponent<Button>().onClick.AddListener(() => { this.ChangeMaterial(0); });                
                listPrepared = true;
            }
            listPanel.SetActive(true);
        }
    }
    public void ChangeMaterial(int i)
    {
        if (chosenMaterialId == availableMaterials[i]) return;
        chosenMaterialId = availableMaterials[i];
        materialButtonImage.uvRect = ResourceType.GetResourceIconRect(chosenMaterialId);
        materialNameTextField.text = Localization.GetResourceName(chosenMaterialId);
        Transform contentHolder = listPanel.transform.GetChild(0).GetChild(0);
        if (chosenMaterialButton != null) chosenMaterialButton.overrideSprite = null;
        chosenMaterialButton = contentHolder.GetChild(i).GetComponent<Image>();
        chosenMaterialButton.overrideSprite = PoolMaster.gui_overridingSprite;
    }


    public void PlayWithThisTerrain()
    {
        if (visualBorderDrawn) GameMaster.realMaster.mainChunk.HideBorderLine();
        Destroy(transform.root.gameObject);
        GameMaster.realMaster.SaveTerrain("lastCreatedTerrain");
        GameMaster.realMaster.ChangeModeToPlay();
    }

    


    public void MenuPanelToggle()
    {
        if (!menuPanel.activeSelf)
        {
            menuPanel.SetActive(true);
            actionsPanel.SetActive(false);
            listPanel.SetActive(false);
            FollowingCamera.main.CameraRotationBlock(true);
        }
        else
        {
            menuPanel.SetActive(false);
            saveButtonImage.overrideSprite = null;
            loadButtonImage.overrideSprite = null;
            settingsPanel.SetActive(false);
            FollowingCamera.main.CameraRotationBlock(false);
        }
    }
    public void SaveTerrain()
    {
        saveSystem.Activate(true, true);
        saveButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;
        loadButtonImage.overrideSprite = null;
    }
    public void LoadTerrain()
    {
        saveSystem.Activate(false, true);
        loadButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;
        saveButtonImage.overrideSprite = null;
    }
    public void SettingsButton()
    {
        if (saveSystem.gameObject.activeSelf)
        {
            saveSystem.CloseButton();
            saveButtonImage.overrideSprite = null;
            loadButtonImage.overrideSprite = null;
        }
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
    public void BackToMenu()
    {
        GameMaster.ChangeScene(GameLevel.Menu);
    }

    public void LocalizeTitles()
    {
        transform.GetChild(3).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Menu);
        actionsPanel.transform.GetChild(11).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Play);

        Transform t = menuPanel.transform;
        t.GetChild(0).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Save);
        t.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Load);
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Options);
        t.GetChild(3).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.MainMenu);

        settingsPanel.GetComponent<GameSettingsUI>().LocalizeTitles();
    }
}
