using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//从文件中读取音乐信息
public class SongInfoLoader {
	public SongInfo songInfo;
//以下是文件的输入输出相关处理
	public void ReadCSV( System.IO.TextReader reader, bool isEditorMode=false ){
		string line;

		int		line_number = 0;

		while( (line = reader.ReadLine()) != null ){

			line_number++;

			string[] lineCells = line.Split(',');
			switch( lineCells[0] ){
			case "beatPerSecond":
				songInfo.beatPerSecond=float.Parse(lineCells[1]);
				break;
			case "beatPerBar":
				songInfo.beatPerBar=float.Parse(lineCells[1]);
				break;
			case "scoringUnitSequenceRegion-Begin":
				line_number = ReadCSV_OnBeatAction(reader, line_number);
				break;
			case "stagingDirectionSequenceRegion-Begin":
				ReadCSV_StagingDirection(reader);
				break;
			case "include":
				TextReader textReader;
#if UNITY_EDITOR
				if(isEditorMode){
					textReader  = File.OpenText("Assets/Resources/SongInfo/" + lineCells[1] + ".txt");
				}
				else{
					string data = System.Text.Encoding.UTF8.GetString(
						(Resources.Load("SongInfo/" + lineCells[1]) as TextAsset).bytes
					);
					textReader = new StringReader(data);
				}
#else
				textReader = new StringReader(System.Text.Encoding.UTF8.GetString((Resources.Load("SongInfo/" + lineCells[1]) as TextAsset).bytes));
#endif
				ReadCSV(textReader);
				break;
			}
		}
	}
	private void ReadCSV_StagingDirection( System.IO.TextReader reader ){
		string line;
		float totalBeatCount=0;
		float repeatPosition=0;
		List<StagingDirection> sequence=new List<StagingDirection>();
		while( (line = reader.ReadLine()) != null   ){
			string[] lineCells = line.Split(',');
			switch( lineCells[0] ){
			case "regionParameters":
				totalBeatCount = float.Parse(lineCells[2]);
				repeatPosition = float.Parse(lineCells[3]);
				break;
			case "AllBandMemberDefaultAnimation":
			{
				foreach(GameObject member in GameObject.FindGameObjectsWithTag("BandMember")){
					StagingDirection_SetBandMemberDefaultAnimation defaultAnimationSet
						=new StagingDirection_SetBandMemberDefaultAnimation();
					defaultAnimationSet.triggerBeatTiming = float.Parse(lineCells[1]);
					defaultAnimationSet.m_memberName = member.name;
					defaultAnimationSet.m_animationFromIndex = int.Parse(lineCells[2]);
					defaultAnimationSet.m_animationToIndex = int.Parse(lineCells[3]);
					sequence.Add(defaultAnimationSet);
				}
			}
				break;
			case "SetAllBandMemberAction":
			{
				foreach(GameObject member in GameObject.FindGameObjectsWithTag("BandMember")){
					StagingDirection_SetBandMemberAction actionSet=new StagingDirection_SetBandMemberAction();
					actionSet.triggerBeatTiming = float.Parse(lineCells[1]);
					actionSet.m_memberName = member.name;
					actionSet.m_actionName = lineCells[2];
					sequence.Add(actionSet);
				}
			}
				break;
			case "stagingDirectionSequenceRegion-End":
			{
				for( float repeatOffest=0; repeatOffest < totalBeatCount ; ){
					foreach( StagingDirection stagingDirection in sequence ){
						if(stagingDirection.triggerBeatTiming + repeatOffest > totalBeatCount){
							break;
						}
						StagingDirection cloned = stagingDirection.GetClone() as StagingDirection;
						cloned.triggerBeatTiming += m_stagingDirectoionRegionOffset+repeatOffest;
						songInfo.stagingDirectionSequence.Add( cloned );
					}
					repeatOffest+=repeatPosition;
				}
				m_stagingDirectoionRegionOffset+=totalBeatCount;
			}
				return ;
				// ↑已经有 return 所以这个 break 不会被执行
				//break;
			default:
			{
				StagingDirection stagingDirection
					= StagingDirectionFactory.CreateStagingDirectionFromEnum(
						(StagingDirectionEnum) System.Enum.Parse( typeof(StagingDirectionEnum), lineCells[0] ) 
					);
				if( stagingDirection!=null ){
					stagingDirection.ReadCustomParameterFromString(lineCells);
					stagingDirection.triggerBeatTiming = float.Parse(lineCells[1]);
					sequence.Add(stagingDirection);
				}
			}
				break;
			};
		}
		Debug.LogError("StagingDirectionSequenceRegion.ReadCSV: ParseError - missing stagingDirectionSequenceRegion-End");
	}
	private int		ReadCSV_OnBeatAction( System.IO.TextReader reader , int line_number){
		string line;
		SequenceRegion region = new SequenceRegion();

		List<OnBeatActionInfo> sequence=new List<OnBeatActionInfo>();

		while( (line = reader.ReadLine()) != null   ){

			line_number++;

			string[] lineCells = line.Split(',');
			switch( lineCells[0] ){
			case "regionParameters":
				region.name = lineCells[1];
				region.totalBeatCount = float.Parse(lineCells[2]);
				region.repeatPosition = float.Parse(lineCells[3]);
				break;
			case "scoringUnitSequenceRegion-End":
			{	region.triggerBeatTiming = m_onBeatActionInfoRegionOffset;
				songInfo.onBeatActionRegionSequence.Add(region);
				for (float repeatOffest = 0; repeatOffest < region.totalBeatCount; repeatOffest += region.repeatPosition)
				{
					foreach( OnBeatActionInfo onBeatActionInfo in sequence ){
						if (onBeatActionInfo.triggerBeatTiming + repeatOffest > region.totalBeatCount)
						{
							break;
						}
						OnBeatActionInfo cloned = onBeatActionInfo.GetClone() as OnBeatActionInfo;
						cloned.triggerBeatTiming += m_onBeatActionInfoRegionOffset+repeatOffest;
						songInfo.onBeatActionSequence.Add( cloned );
					}
				}
				m_onBeatActionInfoRegionOffset += region.totalBeatCount;
				return(line_number);
			}
				// ↑已经有 return 所以这个 break 不会被执行
				//break;
			case "SingleShot":
			{
				OnBeatActionInfo onBeatActionInfo = new OnBeatActionInfo();
				if (lineCells[2] != ""){
					onBeatActionInfo.playerActionType
						= (PlayerActionEnum)System.Enum.Parse( typeof(PlayerActionEnum), lineCells[2] );
				}
				else{
					onBeatActionInfo.playerActionType = PlayerActionEnum.HeadBanging;
				}
				onBeatActionInfo.triggerBeatTiming = float.Parse(lineCells[1]);

				// 记录下定义的行号用于调试
				onBeatActionInfo.line_number = line_number;

				sequence.Add(onBeatActionInfo);
			}
				break;
			};

		}
		Debug.LogError("ScoringUnitSequenceRegion.ReadCSV: ParseError - missing ScoringUnitSequenceRegion-End");

		return(line_number);
	}
	private float m_stagingDirectoionRegionOffset=0;
	private float m_onBeatActionInfoRegionOffset=0;
}
