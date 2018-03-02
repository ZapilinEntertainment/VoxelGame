using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Content {Empty, Plant, HarvestableResources}

public class BlockSurface : MonoBehaviour {
	public const int INNER_RESOLUTION = 16;
	public Block myBlock {get;private set;}
	public MeshRenderer surfaceRenderer {get;private set;}
	Content[,] map;
	public sbyte cellsStatus {get; private set;} // -1 is not stated, 1 is full, 0 is empty;

	void Awake() 
	{
		map = new Content[INNER_RESOLUTION,INNER_RESOLUTION];
		for (int i = 0; i< INNER_RESOLUTION;i++) {
			for (int j = 0; j< INNER_RESOLUTION; j++) map[i,j] = Content.Empty;
		}
		cellsStatus = 0;
	}

	public static Vector3 GetLocalPosition(PixelPosByte cellPos) {
		float res = BlockSurface.INNER_RESOLUTION;
		float a = cellPos.x, b = cellPos.y;
		return( new Vector3((a/res- 0.5f) * Block.QUAD_SIZE + Block.QUAD_SIZE/ res /2f, 0, (b/res - 0.5f)* Block.QUAD_SIZE + Block.QUAD_SIZE / res / 2f));
	}

	public void ClearCell(PixelPosByte cellPos, Content c) {
		if (cellsStatus == 0) return;
		if (map[cellPos.x, cellPos.y] == c) {
			map[cellPos.x, cellPos.y] = Content.Empty;
			bool isEmpty = true;
			foreach (Content con in map) { if (con != Content.Empty) {isEmpty = false; break;}}
			if (isEmpty) cellsStatus = 0; else cellsStatus = -1;
		}
	}

	/// <summary>
	/// Gets random empty cell and puts the content in it
	/// </summary>
	/// <returns>x and y in PixelPosByte structure format</returns>
	public PixelPosByte PutInCell (Content content) { 
		if (cellsStatus == 1) return PixelPosByte.Empty;

		PixelPosByte cell = PixelPosByte.Empty;
		if (cellsStatus == 0) {
			byte x = (byte)(Random.value * (INNER_RESOLUTION - 1));
			byte y = (byte)(Random.value * (INNER_RESOLUTION - 1));
			cell = new PixelPosByte(x, y);
			cellsStatus = -1;
		}
		else {
			List<PixelPosByte> freeCells = new List<PixelPosByte>();
			for (byte i = 0; i< INNER_RESOLUTION; i++) {
				for (byte j = 0; j < INNER_RESOLUTION; j++) {
					if (map[i,j] == Content.Empty) freeCells.Add(new PixelPosByte(i,j));
				}
			}
			int pos = (int)(Random.value * (freeCells.Count - 1));
			cell =  freeCells[pos];
			if (freeCells.Count == 1) cellsStatus = 0; else cellsStatus = -1;
		}
		map[cell.x, cell.y] = content;
		return cell;
	}

	public List<PixelPosByte> PutInMultipleCells (int count, Content content) { 
		List<PixelPosByte> positions = new List<PixelPosByte>();
		if (cellsStatus == 1) return positions;

		List<PixelPosByte> freeCells = new List<PixelPosByte>();
		for (byte i = 0; i< INNER_RESOLUTION; i++) {
			for (byte j = 0; j < INNER_RESOLUTION; j++) {
				if (map[i,j] == Content.Empty) freeCells.Add(new PixelPosByte(i,j));
			}
		}

		if (freeCells.Count == count) {cellsStatus = 0; return freeCells;}
		else {
			cellsStatus = -1;
			int i = 0;
			while ( i < freeCells.Count && i < count) {
				int pos = (int)(Random.value * (freeCells.Count - 1));
				if (freeCells[pos] == null) {freeCells.RemoveAt(pos); continue;}
				positions.Add(freeCells[pos]); i++; 
				map[freeCells[pos].x, freeCells[pos].y] = content;
			}
			return positions;
		}
	}

	public void SetBasement(Block b, MeshRenderer planeRenderer) 
	{
		myBlock = b;
		surfaceRenderer = planeRenderer;
	}

	public void Annihilation() {
		Destroy(gameObject);
	}

}
