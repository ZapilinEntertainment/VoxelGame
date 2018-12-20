using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class EditorUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] GameObject actionsPanel, listPanel, listDownButton, listUpButton, menuPanel, settingsPanel;
    [SerializeField] RawImage currentActionIcon, materialButtonImage;
    [SerializeField] Image[] buttonsImages;
    [SerializeField] Image saveButtonImage, loadButtonImage;
    [SerializeField] Text materialNameTextField;
#pragma warning restore 0649  

    private enum ClickAction { CreateBlock, DeleteBlock, AddGrassland, DeleteGrassland, MakeSurface, MakeCave, AddLifepower, TakeLifepower, CreateLifesource }
    private ClickAction currentAction;
    private LifeSource lifesource;
    private int chosenMaterialId = ResourceType.STONE_ID, firstInListPos = 0, chosenListPosition = 0;
    private int[] idsArray;
    private bool blockEditMode = true, visualBorderDrawn = false;

    private const int LIST_POSITIONS = 10, LIFEPOWER_PORTION = 100, LISTPANEL_DEFAULT_CHILDCOUNT = 3;

    private void Start()
    {
        buttonsImages[(int)currentAction].overrideSprite = PoolMaster.gui_overridingSprite;
        materialButtonImage.uvRect = ResourceType.GetTextureRect(chosenMaterialId);
        materialNameTextField.text = Localization.GetResourceName(chosenMaterialId);
        SaveSystemUI.Check(transform.root);
        SaveSystemUI.current.ingame = true;
        ActionsPanel();

        FollowingCamera.main.ResetTouchRightBorder();
        FollowingCamera.main.CameraRotationBlock(false);
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
                        if (b.type == BlockType.Cube)
                        {
                            ChunkPos cpos;
                            switch (collided.name)
                            {
                                case CubeBlock.FWD_PLANE_NAME: cpos = new ChunkPos(b.pos.x, b.pos.y, b.pos.z + 1); break;
                                case CubeBlock.RIGHT_PLANE_NAME: cpos = new ChunkPos(b.pos.x + 1, b.pos.y, b.pos.z); break;
                                case CubeBlock.BACK_PLANE_NAME: cpos = new ChunkPos(b.pos.x, b.pos.y, b.pos.z - 1); break;
                                case CubeBlock.LEFT_PLANE_NAME: cpos = new ChunkPos(b.pos.x - 1, b.pos.y, b.pos.z); break;
                                case CubeBlock.UP_PLANE_NAME: cpos = new ChunkPos(b.pos.x, b.pos.y + 1, b.pos.z); break;
                                case CubeBlock.DOWN_PLANE_NAME: cpos = new ChunkPos(b.pos.x, b.pos.y - 1, b.pos.z); break;
                                default: cpos = b.pos; break;
                            }
                            GameMaster.realMaster.mainChunk.AddBlock(cpos, BlockType.Cube, chosenMaterialId, true);
                        }
                        else // surface block
                        {
                            GameMaster.realMaster.mainChunk.ReplaceBlock(b.pos, BlockType.Cube, chosenMaterialId, true);
                        }
                        break;
                    }
                case ClickAction.DeleteBlock:
                    {
                        Vector3Int cpos = new Vector3Int(b.pos.x, b.pos.y, b.pos.z);
                        Chunk c = GameMaster.realMaster.mainChunk;
                        Block lowerBlock = c.GetBlock(cpos.x, cpos.y - 1, cpos.z);
                        if ((b.type == BlockType.Surface | b.type == BlockType.Cave) && (lowerBlock.type == BlockType.Cube | lowerBlock.type == BlockType.Cave))
                        {
                            c.DeleteBlock(new ChunkPos(cpos.x, cpos.y - 1, cpos.z));
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
                            sb.grassland.Annihilation(true);
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
                            if (x >= Chunk.SUPPORT_POINTS_ENOUGH_FOR_HANGING) cb.myChunk.ReplaceBlock(cb.pos, BlockType.Cave, cb.material_id, true);
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
            if (chosenListPosition >= 0 & chosenListPosition < LIST_POSITIONS) listPanel.transform.GetChild(chosenListPosition + LISTPANEL_DEFAULT_CHILDCOUNT).GetComponent<Image>().overrideSprite = null;
        }
        else
        {
            if (blockEditMode)
            {
                if (listPanel.transform.childCount == 3)
                {
                    // preparing
                    int listPos = 0;
                    ResourceType[] appliableMaterials = ResourceType.blockMaterials;
                    while (listPos < LIST_POSITIONS & listPos < appliableMaterials.Length)
                    {
                        RectTransform newButtonTransform = Instantiate(listPanel.transform.GetChild(0).gameObject, listPanel.transform).GetComponent<RectTransform>();
                        newButtonTransform.localPosition = new Vector3(newButtonTransform.localPosition.x, newButtonTransform.localPosition.y - listPos * newButtonTransform.rect.height, newButtonTransform.localPosition.z);
                        int m_id = appliableMaterials[listPos].ID;
                        newButtonTransform.GetChild(0).GetComponent<Text>().text = listPos.ToString() + ". " + Localization.GetResourceName(m_id);
                        int arg_listPos = listPos;
                        newButtonTransform.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            this.ChangeMaterial(m_id, arg_listPos);
                        });
                        newButtonTransform.gameObject.SetActive(true);
                        listPos++;
                    }
                    firstInListPos = 0;
                    listUpButton.SetActive(false);
                    listDownButton.SetActive(appliableMaterials.Length > LIST_POSITIONS);

                    chosenListPosition = -1;
                    for (int i = 0; i < appliableMaterials.Length; i++)
                    {
                        if (appliableMaterials[i].ID == chosenMaterialId) chosenListPosition = i;
                    }
                    if (chosenListPosition >= 0 & chosenListPosition < LIST_POSITIONS) listPanel.transform.GetChild(LISTPANEL_DEFAULT_CHILDCOUNT + chosenListPosition).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                }
            }
            listPanel.SetActive(true);
        }
    }
    public void ListDown()
    {
        if (blockEditMode)
        {
            if (firstInListPos + LIST_POSITIONS < ResourceType.blockMaterials.Length)
            {
                firstInListPos++;
                Transform list = listPanel.transform;
                if (chosenListPosition >= 0 & chosenListPosition < LIST_POSITIONS) list.GetChild(LISTPANEL_DEFAULT_CHILDCOUNT + chosenListPosition).GetComponent<Image>().overrideSprite = null;
                chosenListPosition--;
                if (chosenListPosition > 0)
                {
                    list.GetChild(LISTPANEL_DEFAULT_CHILDCOUNT + chosenListPosition).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                }
                Transform button = null;
                for (int i = 0; i < LIST_POSITIONS; i++)
                {
                    button = list.GetChild(i + LISTPANEL_DEFAULT_CHILDCOUNT);
                    int index = firstInListPos + i;
                    int m_id = ResourceType.blockMaterials[index].ID;
                    button.GetChild(0).GetComponent<Text>().text = index.ToString() + ". " + Localization.GetResourceName(m_id);
                    Button b = button.GetComponent<Button>();
                    b.onClick.RemoveAllListeners();
                    int arg_listPos = i;
                    b.onClick.AddListener(() =>
                    {
                        this.ChangeMaterial(m_id, arg_listPos);
                    });
                }
                listDownButton.SetActive(firstInListPos + LIST_POSITIONS < ResourceType.blockMaterials.Length);
                listUpButton.SetActive(firstInListPos > 0);
            }
        }
    }
    public void ListUp()
    {
        if (blockEditMode)
        {
            if (firstInListPos > 0)
            {
                firstInListPos--;
                Transform list = listPanel.transform;
                if (chosenListPosition >= 0 & chosenListPosition < LIST_POSITIONS) list.GetChild(LISTPANEL_DEFAULT_CHILDCOUNT + chosenListPosition).GetComponent<Image>().overrideSprite = null;
                chosenListPosition++;
                if (chosenListPosition < LIST_POSITIONS - 1) list.GetChild(LISTPANEL_DEFAULT_CHILDCOUNT + chosenListPosition).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                Transform button = null;
                for (int i = 0; i < LIST_POSITIONS; i++)
                {
                    button = list.GetChild(i + LISTPANEL_DEFAULT_CHILDCOUNT);
                    int index = firstInListPos + i;
                    int m_id = ResourceType.blockMaterials[index].ID;
                    button.GetChild(0).GetComponent<Text>().text = index.ToString() + ". " + Localization.GetResourceName(m_id);
                    Button b = button.GetComponent<Button>();
                    b.onClick.RemoveAllListeners();
                    int arg_listPos = i;
                    b.onClick.AddListener(() =>
                    {
                        this.ChangeMaterial(m_id, arg_listPos);
                    });
                }
                listUpButton.SetActive(firstInListPos > 0);
                listDownButton.SetActive(firstInListPos + LIST_POSITIONS < ResourceType.blockMaterials.Length);
            }
        }
    }
    public void PlayWithThisTerrain()
    {
        if (visualBorderDrawn) GameMaster.realMaster.mainChunk.HideBorderLine();
        Destroy(transform.root.gameObject);
        GameMaster.realMaster.ChangeModeToPlay();
    }

    public void ChangeMaterial(int id, int pos)
    {
        chosenMaterialId = id;
        materialButtonImage.uvRect = ResourceType.GetTextureRect(id);
        materialNameTextField.text = Localization.GetResourceName(id);
        if (pos + firstInListPos != chosenListPosition)
        {
            if (chosenListPosition >= 0 & chosenListPosition < LIST_POSITIONS) listPanel.transform.GetChild(chosenListPosition + LISTPANEL_DEFAULT_CHILDCOUNT).GetComponent<Image>().overrideSprite = null;
            chosenListPosition = pos;
            listPanel.transform.GetChild(pos + LISTPANEL_DEFAULT_CHILDCOUNT).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
        }
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
        SaveSystemUI.current.Activate(true, true);
        saveButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;
        loadButtonImage.overrideSprite = null;
    }
    public void LoadTerrain()
    {
        SaveSystemUI.current.Activate(false, true);
        loadButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;
        saveButtonImage.overrideSprite = null;
    }
    public void SettingsButton()
    {
        if (SaveSystemUI.current.gameObject.activeSelf)
        {
            SaveSystemUI.current.CloseButton();
            saveButtonImage.overrideSprite = null;
            loadButtonImage.overrideSprite = null;
        }
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
    public void BackToMenu()
    {
        GameMaster.ChangeScene(GameLevel.Menu);
    }
}
