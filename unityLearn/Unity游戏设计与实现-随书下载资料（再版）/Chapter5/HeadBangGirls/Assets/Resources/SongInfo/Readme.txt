这里将对旋律信息设定文件的格式进行说明。

能够设定的信息大概有以下三项。

- 歌曲的基本信息
- 舞台上的表现
- 用于得分高低判断依据的旋律序列数据

通过以下方法能够从多个文件中读取信息。
--------------
** 多个文件的表示方法 **
　从其他文件中读取信息：
　　写法：
　　　include,文件名
　　例：
　　　include,songInfoCSV_Staging.txt
--------------

--------------
**歌曲基本信息的设置方法**

　　旋律的节拍：
　　　写法：
　　　　beatPerSecond , BeatsPerSecond(一秒内的拍子数=BPM/60) 
　　　例：
　　　　beatPerSecond,2.333333

　　一小节的拍子数：
　　　写法：
　　　　beatPerBar , BeatsPerBar(一小节的拍子数) 
　　　例：
　　　　beatPerBar,4
--------------

--------------
**舞台表现的设置方法**

　　概要：
　　　1.可以通过下列步骤来配置舞台表现的各个部分。
　　　　各部分信息按以下方法输入。
********
stagingDirectionSequenceRegion-Begin

....(该部分的基本信息，舞台表现的指定等等)

stagingDirectionSequenceRegion-End
********
　　　2.各部分开始的时间是根据前一部分的结束时间来自动设定的。
　　　
　　　3.可以为各个部分设置为重复。文件中通过节拍指定了表现的出现时机，
　　　　这是通过指定重复段的开始点到哪一节拍来实现的。
　　各部分的基本信息：
　　　写法：
　　　　regionParameters , 该部分名称, 该部分长度（节拍数） , 该部分的重复单位（节拍数）
　　　例：
　　　　regionParameters,Intro,32,4
　　　　※这个例子中该部分长度为32拍，循环单位是4拍，因此32÷4等于8次循环。

　　演出1.焰火：
　　　写法：
　　　　FireBlast , 时间（拍子数） , 绽放的焰火的ID编号( 0 ... 舞台右侧的焰火, 1 ... 舞台左侧的焰火 ) , 焰火数量
　　　例：
　　　　FireBlast,0,0,100

　　演出2.灯光的亮度变化：
　　　注意：
　　　　如果变化前后的亮度相同，则外观上不会有任何变化。
　　　写法：
　　　　LightFade , 时间（拍子数） , 灯光的ID编号( 0 ... 红色的射灯, 1 ... 蓝色的射灯, 2 ... 绿色的射灯 ) , 改变后的亮度
　　　例：
　　　　LightFade,0,0,3

　　演出3.灯光的闪烁：
　　　写法：
　　　　LightFlash , 时间（拍子数）, 灯光的ID编号
　　　例：
　　　　LightFlash,0,0

　　演出4.交换两个灯光的位置（Shuffle）：
　　　注意：
　　　　如果变化前后的射灯亮度相同，则外观上不会有任何变化。
　　　写法：
　　　　LightShuffle , 时间（拍子数） , 一方灯光的ID编号 , 另外一方灯光的ID编号 , 灯光移动的速度
　　　例：
　　　　LightShuffle,0,0,1,10

　　演出5.乐队成员的动作：
　　　写法：
　　　　SetBandMemberAction , 时间（拍子数） , 动作名( actionA, actionB, jump ) , 成员名( Vocalist, Bassist, Guitarist, Drummer )
　　　例：
　　　　SetBandMemberAction,0,jump,Bassist

　　演出6.成员的动作(一次性指定所有成员)：
　　　写法：
　　　　SetAllBandMemberAction , 时间（拍子数） , 动作名( actionA, actionB, jump )
　　　例：
　　　　SetAllBandMemberAction,0,jump

　　演出7.成员的动画：
　　　写法：
　　　　SetBandMemberDefaultAnimation , 时间（拍子数） , 动画开始的帧编号 , 动画结束的帧编号 , 成员名
　　　例：
　　　　SetBandMemberDefaultAnimation,0,2,4,Bassist

　　演出8.成员的动画（一次性指定所有成员）：
　　　写法：
　　　　AllBandMemberDefaultAnimation , 时间（拍子数） , 动画开始的帧编号 , 动画结束的帧编号
　　　例：
　　　　AllBandMemberDefaultAnimation,0,1,1

--------------

--------------
**作为得分评价基准的旋律序列设定方法**

　　概要：
　　　1.和舞台表现类似，对各个部分进行设定。
********
scoringUnitSequenceRegion-Begin

....(各部分的基本信息，舞台表现的设定等等)

scoringUnitSequenceRegion-End
********
　　　2.目前能够设定的只有“是否在恰当的时间点完成了head-banging”评价标准

　　各部分的基本信息（目前和舞台表现部分的基本信息相同）：
　　　写法：
　　　　regionParameters , 该部分名 , 该部分长度（拍子数） , 该部分的循环单位（拍子数）
　　　例：
　　　　regionParameters,Intro,32,4
	　　※该例的部分长度为32拍，循环单位为4，因此会循环32÷4等于8次相同的表现。

　　评价基准1.是否在恰当的时间点完成了head-banging：
　　　写法：
　　　　SingleShot , 时间点(节拍数)
　　　例：
　　　　SingleShot,0
　　　　※该基准意味着如果在该重复段的开始点处完成了一次head-banging将会加分。

--------------