using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFactoryObserver : UIObserver {
    Factory observingFactory;
    [SerializeField]  RawImage inputIcon, outputIcon;
    [SerializeField] Text inputValueString, outputValueString, workflowString;
    [SerializeField] Dropdown recipesDropdown;

    public static UIFactoryObserver InitializeFactoryObserverScript()
    {
        UIFactoryObserver ufo = Instantiate(Resources.Load<GameObject>("UIPrefs/factoryObserver"), UIController.current.rightPanel.transform).GetComponent<UIFactoryObserver>();
        Factory.factoryObserver = ufo;
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
        if (recipes.Length == 1) recipesDropdown.interactable = false;
        else
        {
            recipesDropdown.interactable = true;
            List<Dropdown.OptionData> recipeButtons = new List<Dropdown.OptionData>();
            for (int i = 0; i < recipes.Length; i++)
            {
                recipeButtons.Add(new Dropdown.OptionData(Localization.GetResourceName(recipes[i].input.ID) + " -> " + Localization.GetResourceName(recipes[i].output.ID)));
            }
            recipesDropdown.options = recipeButtons;
        }
        Recipe r = observingFactory.recipe;
        inputIcon.uvRect = ResourceType.GetTextureRect(r.input.ID);
        inputValueString.text = r.inputValue.ToString();
        outputIcon.uvRect = ResourceType.GetTextureRect(r.output.ID);
        outputValueString.text = r.outputValue.ToString();        

        STATUS_UPDATE_TIME = 0.1f; timer = STATUS_UPDATE_TIME;
    }



    override protected void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingFactory == null) SelfShutOff();
        else
        {
            workflowString.text = string.Format("{0:0.##}", observingFactory.workflow * 100) + '%' ;
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
        Building.buildingObserver.ShutOff();
        gameObject.SetActive(false);
    }

    public void SetRecipe(int x)
    {
        observingFactory.SetRecipe(x);
        Recipe r = observingFactory.recipe;
        inputIcon.uvRect = ResourceType.GetTextureRect(r.input.ID);
        inputValueString.text = r.inputValue.ToString();
        outputIcon.uvRect = ResourceType.GetTextureRect(r.output.ID);
        outputValueString.text = r.outputValue.ToString();
    }
}
