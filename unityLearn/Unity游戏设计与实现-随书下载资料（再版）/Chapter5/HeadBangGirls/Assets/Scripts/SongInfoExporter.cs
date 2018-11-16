using UnityEngine;
using System.Collections;
using System.IO;

public class SongInfoExporter_CSV {

	// Use this for initialization
	static public void GetOnBeatActionInfo(SongInfo songInfo,TextWriter writer){
		writer.WriteLine("scoringUnitSequenceRegion-Begin");
		float songLength = songInfo.onBeatActionSequence[songInfo.onBeatActionSequence.Count-1].triggerBeatTiming + 1;
		writer.WriteLine("regionParameters,Unified," +
			songLength
			+ "," + songLength);
		foreach(OnBeatActionInfo onBeatActionInfo in songInfo.onBeatActionSequence){
			writer.WriteLine(onBeatActionInfo.GetCustomParameterAsString_CSV());
		}
		writer.WriteLine("scoringUnitSequenceRegion-End");
	}
}
