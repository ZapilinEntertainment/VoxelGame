using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Content {Empty, Plant}

public class BlockSurface : MonoBehaviour {
	public const int INNER_RESOLUTION = 16;
	public Block myBlock {get;private set;}
	public MeshRenderer surfaceRenderer {get;private set;}
	Content[,] map;
	public byte cellsStatus {get; private set;} // -1 is not stated, 1 is full, 0 is empty;

	void Awake() 
	{
		map = new Content[INNER_RESOLUTION,INNER_RESOLUTION];
		for (int i = 0; i< INNER_RESOLUTION;i++) {
			for (int j = 0; j< INNER_RESOLUTION; j++) map[i,j] = Content.Empty;
		}
		cellsStatus = 0;
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
		}
		List<PixelPosByte> freeCells = new List<PixelPosByte>();
		for (byte i = 0; i< INNER_RESOLUTION; i++) {
			for (byte j = 0; j < INNER_RESOLUTION; j++) {
				if (map[i,j] == Content.Empty) freeCells.Add(new PixelPosByte(i,j));
			}
		}
		if (freeCells.Count == 0) {cellsStatus = 1; return PixelPosByte.Empty;}
		else {
			if (freeCells.Count ==map.Length) cellsStatus = 0;
			int pos = (int)(Random.value * (freeCells.Count - 1));
			cell =  freeCells[pos];
		}

		if (cell != PixelPosByte.Empty) {
			map[cell.x, cell.y] = content;
			return cell;
		}
		else return PixelPosByte.Empty;
	}

	public void SetBasement(Block b, MeshRenderer planeRenderer) 
	{
		myBlock = b;
		surfaceRenderer = planeRenderer;
	}

	public void Annihilation() {}

}
