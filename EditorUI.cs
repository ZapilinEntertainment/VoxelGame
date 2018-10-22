using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class EditorUI : MonoBehaviour {
    private enum ClickAction { CreateBlock, DeleteBlock, AddGrassland, DeleteGrassland, MakeSurface, MakeCave, AddLifepower, TakeLifepower}
    
    [SerializeField] GameObject actionsPanel, listPanel, listDownButton, listUpButton;
    [SerializeField] RawImage currentActionIcon, materialButtonImage;
    [SerializeField] Image[] buttonsImages;
    [SerializeField] Text materialName;

    private ClickAction currentAction;
    private bool actionsPanelOpened = false;
    private int chosenMaterialId = ResourceType.STONE_ID, firstInListPos = 0, chosenListPosition = 0;
    private int[] idsArray;
    private bool blockEditMode = true;

    private const int LIST_POSITIONS = 10, LIFEPOWER_PORTION = 100;

    private void Start()
    {
        buttonsImages[(int)currentAction].overrideSprite = PoolMaster.gui_overridingSprite;
        materialButtonImage.uvRect = ResourceType.GetTextureRect(chosenMaterialId);
    }

    public void Click()
    {
        RaycastHit rh;
        if (Physics.Raycast(FollowingCamera.cam.ScreenPointToRay(Input.mousePosition), out rh))
        {
            Transform collided = rh.transform;
            switch (currentAction)
            {
                case ClickAction.CreateBlock:
                    {
                        Block b = collided.parent.gameObject.GetComponent<Block>();
                        if (b == null)
                        {
                            b = collided.parent.parent.gameObject.GetComponent<Block>();
                        }
                        if (b != null)
                        {
                            Vector3Int cpos = new Vector3Int(b.pos.x, b.pos.y, b.pos.z);
                            if (b.type == BlockType.Cube)
                            {
                                float coordsDelta = rh.point.z - b.transform.position.z;
                                if (Mathf.Abs(coordsDelta) >= Block.QUAD_SIZE / 2f)
                                {
                                    if (coordsDelta > 0)
                                    {
                                        cpos.z += 1;
                                        if (cpos.z >= Chunk.CHUNK_SIZE) return;
                                    }
                                    else
                                    {
                                        cpos.z -= 1;
                                        if (cpos.z < 0) return;
                                    }
                                }
                                coordsDelta = rh.point.x - b.transform.position.x;
                                if (Mathf.Abs(coordsDelta) >= Block.QUAD_SIZE / 2f)
                                {
                                    if (coordsDelta > 0)
                                    {
                                        cpos.x += 1;
                                        if (cpos.x >= Chunk.CHUNK_SIZE) return;
                                    }
                                    else
                                    {
                                        cpos.x -= 1;
                                        if (cpos.x < 0) return;
                                    }
                                }
                                coordsDelta = rh.point.y - b.transform.position.y;
                                if (Mathf.Abs(coordsDelta) >= Block.QUAD_SIZE / 2f)
                                {
                                    if (coordsDelta > 0)
                                    {
                                        cpos.y += 1;
                                        if (cpos.y >= Chunk.CHUNK_SIZE) return;
                                    }
                                    else
                                    {
                                        cpos.y -= 1;
                                        if (cpos.y < 0) return;
                                    }
                                }
                                GameMaster.mainChunk.AddBlock(new ChunkPos(cpos.x, cpos.y, cpos.z), BlockType.Cube, chosenMaterialId, true);
                            }
                            else // surface block
                            {
                                GameMaster.mainChunk.ReplaceBlock(b.pos, BlockType.Cube, chosenMaterialId, true);
                            }
                        }
                        break;
                    }
                case ClickAction.DeleteBlock:
                    {
                        Block b = collided.parent.gameObject.GetComponent<Block>();
                        if (b == null)
                        {
                            b = collided.parent.parent.gameObject.GetComponent<Block>();
                        }
                        if (b != null)
                        {
                            Vector3Int cpos = new Vector3Int(b.pos.x, b.pos.y, b.pos.z);
                            if (b.type == BlockType.Surface | b.type == BlockType.Cave)
                            {
                                GameMaster.mainChunk.DeleteBlock(new ChunkPos(cpos.x, cpos.y - 1, cpos.z));
                            }
                            GameMaster.mainChunk.DeleteBlock(new ChunkPos(cpos.x, cpos.y, cpos.z));
                        }
                        break;
                    }
                case ClickAction.AddGrassland:
                    {
                        Block b = collided.parent.gameObject.GetComponent<Block>();
                        if (b == null)
                        {
                            b = collided.parent.parent.gameObject.GetComponent<Block>();
                        }
                        if (b != null)
                        {
                            SurfaceBlock sb = b as SurfaceBlock;
                            if (sb == null & b.pos.y < Chunk.CHUNK_SIZE - 1)
                            {
                                sb = b.myChunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y + 1, b.pos.z), BlockType.Surface, ResourceType.DIRT_ID, true) as SurfaceBlock;
                            }
                            if (sb != null && sb.grassland == null)
                            {
                                Grassland.CreateOn(sb);
                                sb.grassland.AddLifepowerAndCalculate(LIFEPOWER_PORTION);
                            }
                        }
                        break;
                    }
                case ClickAction.DeleteGrassland:
                    {
                        Block b = collided.parent.gameObject.GetComponent<Block>();
                        if (b == null)
                        {
                            b = collided.parent.parent.gameObject.GetComponent<Block>();
                        }
                        if (b != null)
                        {
                            SurfaceBlock sb = b as SurfaceBlock;
                            if (sb != null && sb.grassland != null)
                            {
                                sb.grassland.Annihilation(true);
                            }
                        }
                        break;
                    }
                case ClickAction.MakeSurface:
                    {
                        Block b = collided.parent.gameObject.GetComponent<Block>();
                        if (b == null)
                        {
                            b = collided.parent.parent.gameObject.GetComponent<Block>();
                        }
                        if (b != null )
                        {
                            if (b.type != BlockType.Surface)
                            {
                                if (b.pos.y < Chunk.CHUNK_SIZE - 1) b.myChunk.AddBlock(new ChunkPos(b.pos.x, b.pos.y + 1, b.pos.z), BlockType.Surface, chosenMaterialId, true);
                            }
                            else b.ReplaceMaterial(chosenMaterialId);
                        }
                        break;
                    }
                case ClickAction.MakeCave:
                    {
                        CubeBlock cb = collided.parent.gameObject.GetComponent<CubeBlock>();
                        if (cb != null)
                        {
                            float x = cb.myChunk.CalculateSupportPoints(cb.pos.x, cb.pos.y, cb.pos.z);
                            if (x > 0.5f) cb.myChunk.ReplaceBlock(cb.pos, BlockType.Cave, cb.material_id, true);
                        }
                        break;
                    }
                case ClickAction.AddLifepower:
                    {
                        Block b = collided.parent.gameObject.GetComponent<Block>();
                        if (b == null)
                        {
                            b = collided.parent.parent.gameObject.GetComponent<Block>();
                        }
                        if (b != null)
                        {
                            SurfaceBlock sb = b as SurfaceBlock;
                            if (sb != null && sb.grassland != null) sb.grassland.AddLifepowerAndCalculate(LIFEPOWER_PORTION);
                        }
                        break;
                    }
                case ClickAction.TakeLifepower:
                    {
                        Block b = collided.parent.gameObject.GetComponent<Block>();
                        if (b == null)
                        {
                            b = collided.parent.parent.gameObject.GetComponent<Block>();
                        }
                        if (b != null)
                        {
                            SurfaceBlock sb = b as SurfaceBlock;
                            if (sb != null && sb.grassland != null) sb.grassland.TakeLifepowerAndCalculate(LIFEPOWER_PORTION);
                        }
                        break;
                    }
            }
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
    }

    public void ActionsPanel()
    {
        actionsPanelOpened = !actionsPanelOpened;
        actionsPanel.SetActive(actionsPanelOpened);
    }

    public void MaterialButtonToggle()
    {
        if (listPanel.activeSelf)
        {
            listPanel.SetActive(false);
            listPanel.transform.GetChild(chosenListPosition + 1).GetComponent<Image>().overrideSprite = null;
        }
        else
        {
            if (blockEditMode)
            {
                if (listPanel.transform.childCount == 3)
                {
                    // preparing
                    int listPos = 0;
                    while (listPos < LIST_POSITIONS & listPos < ResourceType.blockMaterials.Length)
                    {
                        RectTransform newButtonTransform = Instantiate(listPanel.transform.GetChild(0).gameObject, listPanel.transform).GetComponent<RectTransform>();
                        newButtonTransform.position += Vector3.down * newButtonTransform.rect.height;
                        int m_id = ResourceType.blockMaterials[listPos].ID;
                        newButtonTransform.GetChild(0).GetComponent<Text>().text = listPos.ToString() + ". " + Localization.GetResourceName(m_id);
                        newButtonTransform.GetComponent<Button>().onClick.AddListener(() => {
                            this.ChangeMaterial(m_id);
                        });
                        if (m_id == chosenMaterialId)
                        {
                            chosenListPosition = listPos;
                            newButtonTransform.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                        }
                        newButtonTransform.gameObject.SetActive(true);
                        listPos++;
                    }
                    firstInListPos = 0;
                    listUpButton.SetActive(false);
                    listDownButton.SetActive(ResourceType.blockMaterials.Length > LIST_POSITIONS);
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
                Transform button = null;
                for (int i = 0; i < LIST_POSITIONS; i++)
                {
                    button = list.GetChild(i + 3);
                    int m_id = ResourceType.blockMaterials[i + 1].ID;
                    button.GetChild(0).GetComponent<Text>().text = Localization.GetResourceName(m_id);
                    Button b = button.GetComponent<Button>();
                    b.onClick.RemoveAllListeners();                    
                    b.onClick.AddListener(() => {
                        this.ChangeMaterial(m_id);
                    });
                }
                listDownButton.SetActive(firstInListPos + LIST_POSITIONS < ResourceType.blockMaterials.Length);
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
                Transform button = null;
                for (int i = 0; i < LIST_POSITIONS; i++)
                {
                    button = list.GetChild(i + 1);
                    int m_id = ResourceType.blockMaterials[firstInListPos + i].ID;
                    button.GetChild(0).GetComponent<Text>().text = Localization.GetResourceName(m_id);
                    Button b = button.GetComponent<Button>();
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() => {
                        this.ChangeMaterial(m_id);
                    });
                }
                listUpButton.SetActive(firstInListPos > 0);
            }
        }
    }

    public void ChangeMaterial(int id)
    {
        chosenMaterialId = id;
        materialButtonImage.uvRect = ResourceType.GetTextureRect(id);
        materialName.text = Localization.GetResourceName(id);
    }
}
