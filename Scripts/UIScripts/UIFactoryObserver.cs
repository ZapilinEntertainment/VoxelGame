using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIFactoryObserver : UIObserver, ILocalizable
{    
#pragma warning disable 0649
    [SerializeField] GameObject limitPanel;
    [SerializeField] RawImage inputIcon, inputIcon2, outputIcon;
    [SerializeField] Text inputValueString,inputValueString2, outputValueString, workflowString;
    [SerializeField] Dropdown recipesDropdown, modesDropdown;
    [SerializeField] InputField limitInputField;
#pragma warning restore 0649
    public Factory observingFactory { get; private set; }
    private int showingProductionValue;
    private bool advancedFactoryMode = false, ignoreSetRecipeCall = false;

    public static UIFactoryObserver InitializeFactoryObserverScript()
    {
        UIFactoryObserver ufo = Instantiate(Resources.Load<GameObject>("UIPrefs/factoryObserver"), mycanvas.rightPanel.transform).GetComponent<UIFactoryObserver>();
        ufo.LocalizeTitles();
        return ufo;
    }

    public void SetObservingFactory(Factory f)
    {
        if (f == null)
        {
            SelfShutOff();
            return;
        }
        UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
        if (uwb == null) uwb = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else uwb.gameObject.SetActive(true);
        observingFactory = f; isObserving = true;
        ignoreSetRecipeCall = true;
        advancedFactoryMode = f is AdvancedFactory;
        uwb.SetObservingPlace(observingFactory);

        Recipe[] recipes = observingFactory.GetFactoryRecipes();
        recipesDropdown.enabled = false;       
       
        int positionInDropdown = 0;
        if (recipes.Length == 1 && recipes[0] == Recipe.NoRecipe)
        {
            recipesDropdown.interactable = false;        
        }
        else
        {
            List<Dropdown.OptionData> recipeButtons = new List<Dropdown.OptionData>();
            Recipe r;
            if (advancedFactoryMode)
            {
                AdvancedRecipe ar = (observingFactory as AdvancedFactory).GetRecipe(), arf;
                for (int i = 0; i < recipes.Length; i++)
                {
                    r = recipes[i];
                    arf = r as AdvancedRecipe;
                    if (arf == null)
                    {
                        recipeButtons.Add(new Dropdown.OptionData(Localization.GetResourceName(r.input.ID) + " -> " + Localization.GetResourceName(r.output.ID)));
                    }
                    else
                    {
                        recipeButtons.Add(new Dropdown.OptionData(
                            Localization.GetResourceName(arf.input.ID) + " + " + Localization.GetResourceName(arf.input2.ID) + " -> " + Localization.GetResourceName(arf.output.ID)
                            ));
                    }
                    if (r.ID == ar.ID) positionInDropdown = i;
                }
            }
            else
            {
                Recipe rx = observingFactory.GetRecipe();
                for (int i = 0; i < recipes.Length; i++)
                {
                    r = recipes[i];
                    recipeButtons.Add(new Dropdown.OptionData(Localization.GetResourceName(r.input.ID) + " -> " + Localization.GetResourceName(r.output.ID)));
                    if (r.ID == rx.ID) positionInDropdown = i;
                }
            }
            recipesDropdown.options = recipeButtons;
            recipesDropdown.interactable = true;
        }
        recipesDropdown.value = positionInDropdown;
        RedrawRecipeData();
        recipesDropdown.enabled = true;
        //       
        modesDropdown.value = (int)observingFactory.productionMode;
        if (observingFactory.productionMode == FactoryProductionMode.NoLimit)
        {
            limitPanel.SetActive(false);
        }
        else
        {
            showingProductionValue = observingFactory.productionModeValue;
            limitInputField.text = showingProductionValue.ToString();
            limitPanel.SetActive(true);
        }
        //
        ignoreSetRecipeCall = false;
    }

    public void SetRecipe(int x)
    {
        if (ignoreSetRecipeCall) return;
        observingFactory.SetRecipe(x);
        RedrawRecipeData();
    }
    public void SetProductionMode(int x)
    {
        FactoryProductionMode fmode = (FactoryProductionMode)x;
        observingFactory.SetProductionMode(fmode);
        if (fmode == FactoryProductionMode.NoLimit)
        {
            limitPanel.SetActive(false);
        }
        else
        {
            showingProductionValue = observingFactory.productionModeValue;
            limitInputField.text = showingProductionValue.ToString();
            limitPanel.SetActive(true);
        }
    }

    public void ChangeProductionValue(int delta)
    {
        int x = observingFactory.productionModeValue + delta;
        if (x < 0) x = 0;
        if (x != observingFactory.productionModeValue)
        {
            observingFactory.SetProductionValue(x);
            showingProductionValue = x;
            limitInputField.text = showingProductionValue.ToString();
        }
    }
    public void ProductionInputFieldChanged(string s)
    {
        int x = int.Parse(s); // проверка не нужна, так как поле выставлено на integer
        if (x < 0) { x = 0; limitInputField.text = x.ToString(); }
        if (x != observingFactory.productionModeValue)
        {
            observingFactory.SetProductionValue(x);
            showingProductionValue = x;
            limitInputField.text = showingProductionValue.ToString();
        }
    }

    override public void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingFactory == null) SelfShutOff();
        else
        {
            RedrawRecipeData();
        }
    }

    private void RedrawRecipeData()
    {
        Recipe r;
        RectTransform rt;
        Vector2 v2;
        if (!advancedFactoryMode)
        {
            r = observingFactory.GetRecipe();            
            inputIcon2.gameObject.SetActive(false);
            rt = inputIcon.rectTransform;
            v2 = rt.anchorMax; v2.x = 0.3f;
            rt.anchorMax = v2;
            v2 = rt.anchorMin; v2.x = 0.1f;
            rt.anchorMin = v2;
        }
        else
        {
            var ar = (observingFactory as AdvancedFactory).GetRecipe();
            r = ar;
            inputIcon2.uvRect = ResourceType.GetResourceIconRect(ar.input2.ID);
            inputValueString2.text = ar.inputValue2.ToString();

            rt = inputIcon.rectTransform;
            v2 = rt.anchorMax; v2.x = 0.25f;
            rt.anchorMax = v2;
            v2 = rt.anchorMin; v2.x = 0.1f;
            rt.anchorMin = v2;
            rt = inputIcon2.rectTransform;
            v2 = rt.anchorMax; v2.x = 0.4f;
            rt.anchorMax = v2;
            v2 = rt.anchorMin; v2.x = 0.25f;
            rt.anchorMin = v2;

            inputIcon2.gameObject.SetActive(true);
        }
        inputIcon.uvRect = ResourceType.GetResourceIconRect(r.input.ID);
        inputValueString.text = r.inputValue.ToString();
        outputIcon.uvRect = ResourceType.GetResourceIconRect(r.output.ID);
        outputValueString.text = r.outputValue.ToString();
        // production block
        if (r != Recipe.NoRecipe)
        {
            if (observingFactory.outputResourcesBuffer < Factory.BUFFER_LIMIT)
            {
                if (observingFactory.isActive & observingFactory.isEnergySupplied)
                {
                    if (observingFactory.workPaused) workflowString.text = Localization.GetActionLabel(LocalizationActionLabels.WorkStopped);
                    else workflowString.text = observingFactory.UI_GetInfo();
                }
                else
                {
                    workflowString.text = Localization.GetPhrase(LocalizedPhrase.NoEnergySupply);
                }
            }
            else workflowString.text = Localization.GetPhrase(LocalizedPhrase.BufferOverflow);
            if (observingFactory.productionMode != FactoryProductionMode.NoLimit)
            {
                int pmv = observingFactory.productionModeValue;
                if (showingProductionValue != pmv)
                {
                    showingProductionValue = pmv;
                    limitInputField.text = pmv.ToString();
                }
            }
        }
        else workflowString.text = string.Empty;        
    }

    public Dropdown SYSTEM_GetRecipesDropdown()
    {
        return recipesDropdown;
    }

    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        gameObject.SetActive(false);
    }
    override public void ShutOff()
    {
        isObserving = false;
        observingFactory = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        gameObject.SetActive(false);
    }

    //public void OnDisable()
    //{
    //    Debug.Log(StackTraceUtility.ExtractStackTrace());
   // }

    public void LocalizeTitles()
    {
        var options = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoLimit)),
            new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.UpperLimit)),
            new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.IterationsCount))
        };
        modesDropdown.options = options;
        Localization.AddToLocalizeList(this);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Localization.RemoveFromLocalizeList(this);
    }
}
