using UnityEngine;
using System.Collections;

public class ResultControl {
	
	// 斩杀怪物数量的级别范围
	private	const	int		oni_defeat_rank_excellent	= 400;
	private	const	int		oni_defeat_rank_good		= 200;
	private	const	int		oni_defeat_rank_normal		= 100;
	
	// 斩杀怪物评价级别的范围
	private	const	int		evaluation_rank_excellent	= 160;
	private	const	int		evaluation_rank_good		=  80;
	private	const	int		evaluation_rank_normal		=  40;
	
	// 总体评价级别的范围
	private	const	int		result_rank_excellent		= 40;
	private	const	int		result_rank_good			= 32;
	private	const	int		result_rank_normal			= 10;
	
	// 每次斩杀怪物评价的得分数
	private	const	int		evaluation_score_great		= 4;
	private	const	int		evaluation_score_good		= 2;
	private	const	int		evaluation_score_okay		= 1;
	private	const	int		evaluation_score_miss		= 0;
	
	// 每种斩杀怪物评价的点数
	private	const	int		oni_defeat_point_excellent	= 10;
	private	const	int		oni_defeat_point_good		=  8;
	private	const	int		oni_defeat_point_normal		=  5;
	private	const	int		oni_defeat_point_bad		=  3;
	
	// 每种斩杀怪物评价的点数
	private	const	int		evaluation_point_excellent	=  5;
	private	const	int		evaluation_point_good		=  3;
	private	const	int		evaluation_point_normal		=  2;
	private	const	int		evaluation_point_bad		=  1;
	
	// 每种斩杀怪物评价的总点数
	private	const	int		total_rank_excellent		= 15;
	private	const	int		total_rank_good				= 11;
	private	const	int		total_rank_normal			=  7;
	private	const	int		total_rank_bad				=  0;
	
	
	public	int		oni_defeat_score	= 0;
	public	int		evaluation_score	= 0;
	
	public void addOniDefeatScore( int defeat_num )
	{
		oni_defeat_score += defeat_num;
	}
	
	public void addEvaluationScore( int rank )
	{
		switch( rank )
		{
			case (int)SceneControl.EVALUATION.OKAY	: evaluation_score += evaluation_score_okay;	break;
			case (int)SceneControl.EVALUATION.GOOD	: evaluation_score += evaluation_score_good;	break;
			case (int)SceneControl.EVALUATION.GREAT	: evaluation_score += evaluation_score_great;	break;
			case (int)SceneControl.EVALUATION.MISS	: evaluation_score += evaluation_score_miss;	break;
		}
	}
	
	public int getDefeatRank()
	{
		if( oni_defeat_score >= oni_defeat_rank_excellent )		return 3;
		else if( oni_defeat_score >= oni_defeat_rank_good )		return 2;
		else if( oni_defeat_score >= oni_defeat_rank_normal )	return 1;
		else 													return 0;
	}
	
	public int getEvaluationRank()
	{	
		if( evaluation_score >= evaluation_rank_excellent )		return 3;
		else if( evaluation_score >= evaluation_rank_good )		return 2;
		else if( evaluation_score >= evaluation_rank_normal )	return 1;
		else 													return 0;
	}
	
	public	int	getTotalRank()
	{
		int		defeat_point;
		
		if( oni_defeat_score >= oni_defeat_rank_excellent )		defeat_point = oni_defeat_point_excellent;
		else if( oni_defeat_score >= oni_defeat_rank_good )		defeat_point = oni_defeat_point_good;
		else if( oni_defeat_score >= oni_defeat_rank_normal )	defeat_point = oni_defeat_point_normal;
		else 													defeat_point = oni_defeat_point_bad;
		
		int		evaluation_point;
		
		if( evaluation_score >= evaluation_rank_excellent )		evaluation_point = evaluation_point_excellent;
		else if( evaluation_score >= evaluation_rank_good )		evaluation_point = evaluation_point_good;
		else if( evaluation_score >= evaluation_rank_normal )	evaluation_point = evaluation_point_normal;
		else 													evaluation_point = evaluation_point_bad;
	
		int		total_point	= defeat_point + evaluation_point;
		
		if( total_point >= total_rank_excellent )		return 3;
		else if( total_point >= total_rank_good )		return 2;
		else if( total_point >= total_rank_normal )		return 1;
		else 											return 0;
	}
}
