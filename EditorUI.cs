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
            Transform collided = rh.transform;
            Block b = collided.parent.gameObject.GetComponent<Block>();
            if (b == null)
            {
                b = collided.parent.parent.gameObject.GetComponent<Block>();
            }
            if (b == null) return;
            switch (currentAction)
            {
                case ClickAction.CreateBlock:
                    {
                        //отследить коллизию
                        // добавить блок с соответствующей стороны
                    }
                    break;
                case ClickAction.DeleteBlock:
                    {
                        Vector3Int cpos = new Vector3Int(b.pos.x, b.pos.y, b.pos.z);
                        Chunk c = GameMaster.realMaster.mainChunk;
                        Block lowerBlock = c.GetBlock(cpos.x, cpos.y - 1, cpos.z);
                        if (lowerBlock != null && (lowerBlock.type == BlockType.Cube | lowerBlock.type == BlockType.Cave))
                        {
                            if (b.type == BlockType.Surface | b.type == BlockType.Cave) c.DeleteBlock(new ChunkPos(cpos.x, cpos.y - 1, cpos.z));
                            else
                            {
                                if (b.type == BlockType.Cube)
                                {
                                    c.ReplaceBlock(b.pos, BlockType.Surface, lowerBlock.material_id, true);
                                    Block upperBlock = c.GetBlock(cpos.x, cpos.y + 1, cpos.z);
                                    if (upperBlock != null)
                                    {
                                        if (upperBlock.type == BlockType.Surface) c.DeleteBlock(upperBlock.pos);
                                        else
                                        {
                                            if (upperBlock.type == BlockType.Cave) (upperBlock as CaveBlock).DestroySurface();
                                        }
                                    }
                                    
                                }
                                else c.DeleteBlock(new ChunkPos(cpos.x, cpos.y, cpos.z));
                            }
                        }
                        else c.DeleteBlock(new ChunkPos(cpos.x, cpos.y, cpos.z));
                        break;
                    }
                case ClickAction.AddGrassland:
                    {
                        SurfaceBlock sb = b as SurfaceBlock;
                        if (sb == null & b.pos.y < Chunk.CHUNK_SIZE - 1)
                        {
                            sb = b.myChunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y + 1, b.pos.z), BlockType.Surface, ResourceType.DIRT_ID, true) as SurfaceBlock;
                        }
                        if (sb != null && sb.grassland == null)
                        {
                            if (sb.material_id != ResourceType.DIRT_ID | sb.material_id != ResourceType.FERTILE_SOIL_ID) sb.ReplaceMaterial(ResourceType.DIRT_ID);
                            Grassland.CreateOn(sb);
                            sb.grassland.AddLifepowerAndCalculate(LIFEPOWER_PORTION);
                        }
                        break;
                    }
                case ClickAction.DeleteGrassland:
                    {
                        SurfaceBlock sb = b as SurfaceBlock;
                        if (sb != null && sb.grassland != null)
                        {
                            sb.grassland.Annihilation(true, true);
                        }
                        break;
                    }
                case ClickAction.MakeSurface:
                    {
                        if (b.type != BlockType.Surface)
                        {
                            if (b.pos.y < Chunk.CHUNK_SIZE - 1) b.myChunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y + 1, b.pos.z), BlockType.Surface, chosenMaterialId, true);
                        }
                        else b.ReplaceMaterial(chosenMaterialId);
                        break;
                    }
                case ClickAction.MakeCave:
                    {
                        CubeBlock cb = collided.parent.gameObject.GetComponent<CubeBlock>();
                        if (cb != null)
                        {
                            float x = cb.myChunk.CalculateSupportPoints(cb.pos.x, cb.pos.y, cb.pos.z);
                            if (x >= Chunk.SUPPORT_POINTS_ENOUGH_FOR_HANGING)
                            {
                                Block lb = cb.myChunk.GetBlock(cb.pos.x, cb.pos.y - 1, cb.pos.z); 
                                cb.myChunk.ReplaceBlock(cb.pos, BlockType.Cave, lb != null ? lb.material_id : -1, true);
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
