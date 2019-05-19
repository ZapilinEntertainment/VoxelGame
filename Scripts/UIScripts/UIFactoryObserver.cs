using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIFactoryObserver : UIObserver
{    
#pragma warning disable 0649
    [SerializeField] GameObject limitPanel;
    [SerializeField] RawImage inputIcon, outputIcon;
    [SerializeField] Text inputValueString, outputValueString, workflowString;
    [SerializeField] Dropdown recipesDropdown, modesDropdown;
    [SerializeField] InputField limitInputField;
#pragma warning restore 0649
    private Factory observingFactory;
    private int showingProductionValue;

    public static UIFactoryObserver InitializeFactoryObserverScript()
    {
        UIFactoryObserver ufo = Instantiate(Resources.Load<GameObject>("UIPrefs/factoryObserver"), UIController.current.rightPanel.transform).GetComponent<UIFactoryObserver>();
        Factory.factoryObserver = ufo;
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
        uwb.SetObservingWorkBuilding(observingFactory);

        Recipe[] recipes = observingFactory.GetFactoryRecipes();
        recipesDropdown.enabled = true;
        Recipe r = observingFactory.recipe;
        int positionInDropdown = -1;
        if (recipes.Length == 1) recipesDropdown.interactable = false;
        else
        {
            recipesDropdown.interactable = true;
            List<Dropdown.OptionData> recipeButtons = new List<Dropdown.OptionData>();
            for (int i = 0; i < recipes.Length; i++)
            {
                recipeButtons.Add(new Dropdown.OptionData(Localization.GetResourceName(recipes[i].input.ID) + " -> " + Localization.GetResourceName(recipes[i].output.ID)));
                if (recipes[i].ID == r.ID) positionInDropdown = i;
            }
            recipesDropdown.options = recipeButtons;
        }

        inputIcon.uvRect = ResourceType.GetResourceIconRect(r.input.ID);
        inputValueString.text = r.inputValue.ToString();
        outputIcon.uvRect = ResourceType.GetResourceIconRect(r.output.ID);
        outputValueString.text = r.outputValue.ToString();
        recipesDropdown.value = positionInDropdown;

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
    }

    public void SetRecipe(int x)
    {
        observingFactory.SetRecipe(x);
        Recipe r = observingFactory.recipe;
        inputIcon.uvRect = ResourceType.GetResourceIconRect(r.input.ID);
        inputValueString.text = r.inputValue.ToString();
        outputIcon.uvRect = ResourceType.GetResourceIconRect(r.output.ID);
        outputValueString.text = r.outputValue.ToString();
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
            if (observingFactory.recipe != Recipe.NoRecipe) {
                if (observingFactory.outputResourcesBuffer < Factory.BUFFER_LIMIT)
                {
                    if (observingFactory.isActive & observingFactory.isEnergySupplied)
                    {
                        if (observingFactory.workPaused) workflowString.text = Localization.GetActionLabel(LocalizationActionLabels.WorkStopped);
                        else
                        {
                            float x = observingFactory.workSpeed / observingFactory.workflowToProcess / GameMaster.LABOUR_TICK * observingFactory.recipe.outputValue;
                            workflowString.text = string.Format("{0:0.##}", x) + ' ' + Localization.GetPhrase(LocalizedPhrase.PerSecond);
                        }
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

    public override void LocalizeTitles()
    {
        var options = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoLimit)),
            new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.UpperLimit)),
            new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.IterationsCount))
        };
        modesDropdown.options = options;
    }
}
