//
// 地图管理类
// 为了简化程序，这里将方块的大小固定为1，在snap设置中配置为正数1，调整时可以按步长为1变化
//
using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {
	public GameCtrl 	m_gameCtrl;

	private const int 	MAP_ORIGIN_X = 100;
	private const int 	MAP_ORIGIN_Z = 100;

	// 地图方块的种类.
	private const char 	GEM                  = 'c';
	private const char 	WALL                 = '*';
	private const char 	SWORD                = 's';
	private const char 	PLAYER_SPAWN_POINT   = 'p';
	private const char 	TERASURE_SPAWN_POINT = 't';

	private const float	WALL_Y   = 0.0f; 		// 放置墙壁方块的Y坐标.
	private const float	GEM_Y    = 0.5f;  		// 宝石的Y坐标
	public  const float	GEM_SIZE = 0.4f;
	
	// 出现点的种类
	public enum SPAWN_POINT_TYPE {

		BLOCK_SPAWN_POINT_PLAYER = 0,
		BLOCK_SPAWN_POINT_ENEMY1,
		BLOCK_SPAWN_POINT_ENEMY2,
		BLOCK_SPAWN_POINT_ENEMY3,
		BLOCK_SPAWN_POINT_ENEMY4,
		BLOCK_SPAWN_TREASURE,
		BLOCK_SPAWN_POINT_END
	};
	const int SPAWN_POINT_TYPE_NUM = (int)SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_END - (int)SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_PLAYER;
	
	// 地图数据的结构体
	struct MapData {
		public int		width;
		public int		length;
		public int		offset_x;    // data[0][0]表示位置offset_x,offset_z的方块
		public int		offset_z; 
		public char[,]	data;
		public float[,]	height;
		
		public int[,]	gemParticleIndex;
	};
	
	
	private MapData 	m_mapData;
	private Vector3[] 	m_spawnPositions;

	// 地图素材
	private GameObject 		m_items;			// 宝剑道具的父容器对象
	private GameObject 		m_mapObjects;
	private GameObject 		m_mapCollision;
	public AudioChannels 	m_audio;


	public GameObject[] 	m_wallObject;
	public GameObject[] 	m_itemObject;
	public GameObject 		m_wallForCollision;
	
	// 地图的数据
	public TextAsset m_defaultMap;
	public TextAsset[] m_map_texasset;
	
	// 宝石相关
	private ParticleEmitter m_gemEmitter;
	private int		 		m_gemTotalNum;
	private int 			m_gemCurrentNum;
	public AudioClip 		m_gemPickupSe;

	private const int GEM_SCORE = 10;
	
	// 测试用地图
	// Use this for initialization
	void Awake () {
		
	}
	
	public void CreateModel()
	{
		LoadFromAsset(m_defaultMap);
		CreateMap(false,"MapModel",true);
	}
	
	private void SetMapData()
	{
		int stageNo = m_gameCtrl.GetStageNo();
		if (stageNo > m_map_texasset.Length)
			stageNo = (stageNo - 3) % 3 + 3;
		LoadFromAsset(m_map_texasset[stageNo-1]);
	}
	
	public void OnStageStart()
	{
		DeleteMap();
		SetMapData();
		CreateMap(true,"MapCollision",false);
		CreateMap(false,"MapBlocks",false);
		m_gemCurrentNum = m_gemTotalNum;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	private void LoadFromAsset(TextAsset asset)
	{
		m_mapData.offset_x = MAP_ORIGIN_X;
		m_mapData.offset_z = MAP_ORIGIN_Z;
		
		if (asset != null) {

			string txtMapData = asset.text;

			// 将空元素删除的选项
			System.StringSplitOptions	option = System.StringSplitOptions.RemoveEmptyEntries;

			// 用换行符将每行切割开
			string[] lines = txtMapData.Split(new char[] {'\r','\n'},option);

			// 用“,”将每个字符分割开
			char[] spliter = new char[1] {','};

			// 第一行地图的尺寸
			string[]	sizewh = lines[0].Split(spliter,option);
			m_mapData.width  = int.Parse(sizewh[0]);
			m_mapData.length = int.Parse(sizewh[1]);

			char[,] 	mapdata = new char[m_mapData.length,m_mapData.width];

			for(int lineCnt = 0;lineCnt < m_mapData.length;lineCnt++) {

				// 为了保证文本文件中即时指定了特别大的值也不会出问题
				// 进行检测
				if (lines.Length <= lineCnt+1)
					break;

				// 用“,”将每个字符分割开
				string[] data = lines[m_mapData.length-lineCnt].Split(spliter,option);

				for (int col = 0; col < m_mapData.width; col++) {

					if (data.Length <= col)
						break;

					mapdata[lineCnt,col] = data[col][0];
				}
			}
			m_mapData.data = mapdata;
		} else {
			Debug.LogWarning("Map data asset is null");
		}
	}
	
	//
	// 配置宝石和道具
	//
	void SetupGemsAndItems()
	{
		m_mapData.gemParticleIndex = new int[m_mapData.length,m_mapData.width];
		m_gemTotalNum = 0;
		
		// 计算宝石的数量，生成同样数量的粒子效果
		for (int x = 0; x < m_mapData.width; x++) {
			for (int z = 0; z < m_mapData.length; z++) {
				if (IsGem(x,z))
					m_gemTotalNum++;
			}
		}
		m_gemEmitter = GetComponent<ParticleEmitter>() as ParticleEmitter;
		m_gemEmitter.Emit(m_gemTotalNum);
		
		// 粒子特效的配置
		Particle[] gemParticle = m_gemEmitter.particles;
		int gemCnt = 0;
		for (int x = 0; x < m_mapData.width; x++) {
			for (int z = 0; z < m_mapData.length; z++) {
				Vector3	bpos = new Vector3(x + m_mapData.offset_x, GEM_Y, z + m_mapData.offset_z);
				m_mapData.gemParticleIndex[z,x] = -1;
				if (IsGem(x,z)) {
					gemParticle[gemCnt].position = bpos;
					gemParticle[gemCnt].size = GEM_SIZE;
					m_mapData.gemParticleIndex[z,x] = gemCnt;
					gemCnt++;
				}
				
				// 生成道具
				if (m_mapData.data[z,x] == SWORD) {
					GameObject o = (GameObject)Instantiate(m_itemObject[0],bpos,Quaternion.identity);
					o.transform.parent = m_items.transform;
				}
					
					
			}
		}
		m_gemEmitter.particles = gemParticle;
	}
	
	// 是否放置宝石？
	bool	IsGem(int x, int z)
	{
		bool	ret = false;

		// 在“GEM”和怪物的生成位置放置宝石

		switch(m_mapData.data[z,x]) {

			case GEM:
			case '1':
			case '2':
			case '3':
			case '4':
			{
				ret = true;
			}
			break;
		}

		return(ret);
	}

	void CreateMap(bool collisionMode,string mapName,bool modelOnly = false)  // todo: 废除碰撞模式
	{
		m_mapObjects = new GameObject(mapName);
		m_spawnPositions = new Vector3[SPAWN_POINT_TYPE_NUM];
		if (m_items != null)
			Destroy(m_items);
		m_items = new GameObject("Item Folder");
		
		for (int x = 0; x < m_mapData.width; x++) {
			for (int z = 0; z < m_mapData.length; z++) {

				// 方块的坐标
				Vector3		bpos = new Vector3(x + m_mapData.offset_x, 0.0f, z + m_mapData.offset_z);

				// 配置方块模型
				switch (m_mapData.data[z,x]) {

				// 墙壁
				case WALL:
					if (collisionMode) {
						GameObject o = Instantiate(m_wallForCollision, bpos + Vector3.up*0.5f, Quaternion.identity) as GameObject;
						o.transform.parent = m_mapObjects.transform;
					} else {
						GameObject o = Instantiate(m_wallObject[0], bpos + Vector3.up*WALL_Y, Quaternion.identity) as GameObject;
						o.transform.parent = m_mapObjects.transform;
					}
					break;

				// 玩家
				case PLAYER_SPAWN_POINT:
					m_spawnPositions[(int)SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_PLAYER] = bpos;
					break;

				// 宝箱
				case TERASURE_SPAWN_POINT:
					m_spawnPositions[(int)SPAWN_POINT_TYPE.BLOCK_SPAWN_TREASURE] = bpos;
					break;

				// 怪物
				case '1':
				case '2':
				case '3':
				case '4':
					int enemyType = int.Parse(m_mapData.data[z,x].ToString());
					m_spawnPositions[enemyType] = bpos;
					break;

				// 用于检测怪物的AI
				// 在相同位置创建“追赶型”和“伏击型”
				case '5':
					m_spawnPositions[1] = bpos;
					m_spawnPositions[2] = bpos;
					break;

				default:
					
					break;
				}				
			}
		}
		
		// 只生成地图数据
		if (modelOnly)
			return;
		
		Transform[] children = m_mapObjects.GetComponentsInChildren<Transform>();
		m_mapObjects.AddComponent<CombineChildren_Custom>();
		m_mapObjects.GetComponent<CombineChildren_Custom>().Combine();
		Destroy(m_mapObjects.GetComponent<CombineChildren_Custom>());
		
		for (int i = 1; i < children.Length; i++)
			MyDestroy(children[i].gameObject,collisionMode);
		
		if (collisionMode) {
			m_mapObjects.AddComponent<MeshCollider>();
			m_mapObjects.GetComponent<MeshCollider>().sharedMesh = m_mapObjects.GetComponent<MeshFilter>().mesh;
			MyDestroy(m_mapObjects.GetComponent<MeshRenderer>(),collisionMode);
			m_mapCollision = m_mapObjects;
		}
		
		if (!collisionMode)
			SetupGemsAndItems();
	}
	
	private void MyDestroy(Object o,bool makeCollisionMode)
	{
		Destroy(o);
	}
	
			
	
	void DeleteMap()
	{
		if (m_mapObjects != null)
			Destroy(m_mapObjects);
		m_mapObjects = null;

		if (m_mapCollision != null)
			Destroy(m_mapCollision);
		m_mapCollision = null;
		if (m_gemEmitter != null)
			m_gemEmitter.ClearParticles();
		if (m_items != null)
			Destroy(m_items);
		m_items = null;
		
	}
	
	
	//
	// 通过位置获取方块
	//
	int GetBlockFromPos(Vector3 pos)
	{
		int grid_x = Mathf.RoundToInt(pos.x);
		int grid_z = Mathf.RoundToInt(pos.z);
		
		// 修正地图的位置
		grid_x -= m_mapData.offset_x;
		grid_z -= m_mapData.offset_z;
		// 是否在范围内？
		if (grid_x < 0 || grid_z < 0 || grid_x >= m_mapData.width || grid_z >= m_mapData.length)
			return 0;
		return m_mapData.data[grid_z,grid_x];
		
	}	
	
	//---------------------------------------
	// 出现点
	//---------------------------------------
	public Vector3 GetSpawnPoint(SPAWN_POINT_TYPE type)
	{
		int t = (int)type;
		if (t >= m_spawnPositions.Length ) {
			Debug.LogWarning("Spawn Point is not found");
			return new Vector3((float)MAP_ORIGIN_X,0,(float)MAP_ORIGIN_Z);
		}
		
		return m_spawnPositions[t];
	}
	
	public Vector3 GetSpawnPoint(int type)  // TODO: 区别在于上面的函数更难理解
	{
		return m_spawnPositions[type];
	}
	

	public bool PositionToIndex(Vector3 pos,out int x,out int z)
	{
		x = Mathf.RoundToInt(pos.x);
		z = Mathf.RoundToInt(pos.z);
		
		// 修正地图的位置
		x -= m_mapData.offset_x;
		z -= m_mapData.offset_z;
		// 检测范围
		if (x < 0 || z < 0 || x >= m_mapData.width || z >= m_mapData.length)
			return false;
		return true;
		
		
	}
	
	public void PickUpItem(Vector3 p)
	{
		int gx,gz;
		bool ret = PositionToIndex(p,out gx,out gz);
		
		if (ret) {
			int idx = m_mapData.gemParticleIndex[gz,gx];
			if (idx >= 0) {
				Particle[] gemParticle = m_gemEmitter.particles;
				gemParticle[idx].size = 0;
				m_gemEmitter.particles = gemParticle;
				m_mapData.gemParticleIndex[gz,gx] = -1;
				m_audio.PlayOneShot(m_gemPickupSe,1.0f,0);
				Hud.get().AddScore(GEM_SCORE);
				m_gemCurrentNum--;
				if (m_gemCurrentNum <= 0)
					m_gameCtrl.OnEatAll();
			}
		}
	}
	
	public int GetGemRemainNum()
	{
		return m_gemCurrentNum;
	}
}

