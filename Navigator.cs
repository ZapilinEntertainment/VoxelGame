using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Point {
	public SurfaceBlock block;
	public PixelPosByte coords;
	public static Point Empty;
	public Point(SurfaceBlock sb, PixelPosByte pos) {
		block = sb; coords = pos;
	}
	static Point() {
		Empty = new Point(null, PixelPosByte.Empty);
	}
	public static bool operator ==(Point lhs, Point rhs) {return lhs.Equals(rhs);}
	public static bool operator !=(Point lhs, Point rhs) {return !lhs.Equals(rhs);}
}

struct RoadPoint {
	public SurfaceBlock block;
	public byte boundMask;
	public RoadPoint(SurfaceBlock sb, byte mask) {
		block = sb; boundMask = mask;
	}
}

public static class Navigator {
	static Chunk myChunk;
	static List<RoadPoint>[] levelMaps;


	static Navigator() {
		levelMaps = new List<RoadPoint>[Chunk.CHUNK_SIZE];
	}

	public static List<PixelPosByte> DoYouKnowDeWay(Point A, Point B) {
		List<SurfaceBlock> wayBlocks = new List<SurfaceBlock>();
		List<PixelPosByte> daWey = new List<PixelPosByte>();
		byte[,] map = new byte[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
		return daWey;
	}

	static void CreateBlockPassabilityMap() {
		
	}

	public static bool ApplyMask (SurfaceBlock sb, byte mask) {
		return true;
	}

	public static void SetChunk (Chunk c) {myChunk = c;}
}
