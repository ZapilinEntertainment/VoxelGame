﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class EditorUI : MonoBehaviour, IObserverController
{
#pragma warning disable 0649
    [SerializeField] GameObject actionsPanel, listPanel, menuPanel, settingsPanel, touchZone;
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
    private bool visualBorderDrawn = false, listPrepared, touchscreen;
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

    private const float LIFEPOWER_PORTION = 100;

    public Transform GetMainCanvasTransform()
    {
        return GetComponent<RectTransform>();
    }

    private void Start()
    {        
        buttonsImages[(int)currentAction].overrideSprite = PoolMaster.gui_overridingSprite;
        materialButtonImage.uvRect = ResourceType.GetResourceIconRect(chosenMaterialId);
        materialNameTextField.text = Localization.GetResourceName(chosenMaterialId);
        if (saveSystem == null) saveSystem = SaveSystemUI.Initialize(UIController.GetCurrent().GetCurrentCanvasTransform());
        if (!actionsPanel.activeSelf) ActionsPanel();
        
        FollowingCamera.main.ResetTouchRightBorder();
        FollowingCamera.main.CameraRotationBlock(false);
        touchscreen = FollowingCamera.touchscreen;
        touchZone.SetActive(touchscreen);
        LocalizeTitles();        
    }
    private void Update()
    {
        if (!touchscreen)
        {
            if (Input.GetMouseButtonDown(0)) Click();
        }
    }

    public void Click()
    {
        if (menuPanel.activeSelf) return;
        if (FollowingCamera.touchscreen)
        {
            if (Input.touchCount > 1 | FollowingCamera.camRotateTrace > 0) return;
        }
        RaycastHit rh;
        if (Physics.Raycast(FollowingCamera.cam.ScreenPointToRay(Input.mousePosition), out rh) && rh.collider.tag == Chunk.BLOCK_COLLIDER_TAG)
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
                            case Block.FWD_FACE_INDEX:
                                {
                                    if (b.pos.z < Chunk.chunkSize - 1)
                                    {
                                        chunk.AddBlock(b.pos.OneBlockForward(), chosenMaterialId, true, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case Block.RIGHT_FACE_INDEX:
                                {
                                    if (b.pos.x < Chunk.chunkSize - 1)
                                    {
                                        chunk.AddBlock(b.pos.OneBlockRight(), chosenMaterialId, true, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case Block.BACK_FACE_INDEX:
                                {
                                    if (b.pos.z > 0)
                                    {
                                        chunk.AddBlock(b.pos.OneBlockBack(), chosenMaterialId, true, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case Block.LEFT_FACE_INDEX:
                                {
                                    if (b.pos.x  > 0)
                                    {
                                        chunk.AddBlock(b.pos.OneBlockLeft(), chosenMaterialId, true, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case Block.UP_FACE_INDEX:
                                {
                                    if (b.pos.y < Chunk.chunkSize - 1)
                                    {
                                        chunk.AddBlock(b.pos.OneBlockHigher(), chosenMaterialId, true, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case Block.DOWN_FACE_INDEX:
                                {
                                    if (b.pos.y > 0)
                                    {
                                        chunk.AddBlock(b.pos.OneBlockDown(), chosenMaterialId, true, true);
                                        action = true;
                                    }
                                    break;
                                }
                            case 6:
                            case 7:
                                {
                                    b.RebuildBlock(new BlockMaterialsList(chosenMaterialId), true, false, true);
                                    action = true;
                                    break;
                                }
                        }
                    }
                    break;
                case ClickAction.DeleteBlock:
                    {
                        chunk.DeleteBlock(b.pos, false);
                        action = true;
                        break;
                    }
                case ClickAction.AddGrassland:
                    {
                        Plane p = b.FORCED_GetPlane(bh.faceIndex);                        
                        if (p != null && !p.haveGrassland && p.isQuad && p.isSurface)
                        {
                            if (p.materialID != ResourceType.DIRT_ID) p.ChangeMaterial(ResourceType.DIRT_ID, false);
                            Grassland g;
                            if (p.TryCreateGrassland(out g) && g != null)
                            {
                                g.FORCED_AddLifepower(LIFEPOWER_PORTION * 10);
                            }
                        }
                        break;
                    }
                case ClickAction.DeleteGrassland:
                    {
                        Plane p = b.FORCED_GetPlane(bh.faceIndex);
                        if (p!= null && p.haveGrassland) p.extension?.RemoveGrassland();
                        break;
                    }
                case ClickAction.MakeSurface:
                    {
                        Plane p = null;
                        if ( b.TryGetPlane(bh.faceIndex, out p))
                        {
                            p.ChangeMaterial(chosenMaterialId, true);
                        }                        
                        break;
                    }
                case ClickAction.MakeCave:
                    {
                        
                        break;
                    }
                case ClickAction.AddLifepower:
                    {
                        Plane p = b.FORCED_GetPlane(bh.faceIndex);
                        if (p != null && p.haveGrassland) p.GetGrassland()?.FORCED_AddLifepower(LIFEPOWER_PORTION);
                        break;
                    }                    
                case ClickAction.TakeLifepower:
                    {
                        Plane p = b.FORCED_GetPlane(bh.faceIndex);
                        if (p != null && p.haveGrassland) p.GetGrassland()?.Dry();
                        break;
                    }
                case ClickAction.CreateLifesource:
                    {
                        
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
        GameMaster.ReturnToMainMenu();
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
