using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FactoryType {Simple, Advanced, Recycler}
public enum FactorySpecialization {Unspecialized, Smeltery}

public class Factory : WorkBuilding {
	public ResourceType inputResource {get; protected set;}
	public float inputCount{get;protected set;}
	public ResourceType outputResource {get; protected set;}
	public float outputCount {get;protected set;}
	public FactoryType factoryType {get;protected set;}
	public FactorySpecialization specialization;
	protected Storage storage;
	protected const float BUFFER_LIMIT = 10;
	protected float inputResourcesBuffer = 0, outputResourcesBuffer = 0;

	void Awake () {
		PrepareWorkbuilding();
		inputResource = ResourceType.Nothing; outputResource = ResourceType.Nothing;
		inputCount = 0; outputCount = 0;
		factoryType = FactoryType.Simple;
		storage = GameMaster.colonyController.storage;
		workflowToProcess = ResourceType.CalculateWorkflowToProcess(inputResource, outputResource);
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		UI.current.AddFactoryToList(this);
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (outputResourcesBuffer >= BUFFER_LIMIT) {
			outputResourcesBuffer -= storage.AddResources(outputResource, outputResourcesBuffer);
		}
		else {
			if (workersCount > 0) {
				workflow += GameMaster.CalculateWorkflow(workersCount, WorkType.Manufacturing);
				if (workflow >= workflowToProcess ) {
					LabourResult();
					workflow -= workflowToProcess;
				}
			}
		}
	}

	override protected void LabourResult() {
		float input = storage.GetResources(inputResource, inputCount);
		inputResourcesBuffer += input;
		if (inputResourcesBuffer >= inputCount) {
			inputResourcesBuffer -= inputCount;
			outputResourcesBuffer += outputCount;
		}
	}
		
	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		UI.current.RemoveFromFactoriesList(this);
	}

}
