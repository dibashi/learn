○ 测试用场景，事件脚本

　Assets/Scene/Test 下存放的是测试用场景，Events/Test/ 下存放的是
事件脚本。

　执行测试场景后，对应的测试用事件脚本将被开始执行。

　・TestSimpleEvent 场景
　　test_simple_event.txt 脚本测试
	Tips“事件和Actor”的“事件”中使用的
	简单事件测试

　・TestCondition 场景
　　test_condition.txt 脚本测试
	Tips“事件和Actor”的“事件”中使用的
	游戏内参数测试

　・TestChoice 场景
　　test_choice.txt 脚本测试
	Tips“特殊事件”的“选项指令”中使用的
	选择选项显示事件的测试

　・TestTreasureBox 场景
　　test_treasure_box.txt 脚本测试
	Tips“特殊事件”的“宝箱事件”中使用的

　・TestHouse 场景
　　test_house.txt 脚本测试
	Tips“特殊事件”的“进入屋子的事件”中使用的

　・TestBattle 场景
　　test_battle.txt 脚本测试
	战斗测试

○ Assets/Script/EventManager.cs.simpleEvent

　Tips“事件和Actor”的“尝试执行一个事件”中使用，用于测试执行事件的脚本。
  请将其重命名为"EventManager.cs"使用。
　为了保存原有的文件，推荐按下列步骤进行测试。

　・测试时
　　１．将EventManager.cs 重命名为 EventManager.cs.org
　　２．将EventManager.cs.simpleEvent 重命名为 EventManager.cs

　・还原到最初状态时
　　１．将EventManager.cs 重命名为 EventManager.cs.simpleEvent
　　２．将EventManager.cs.org 重命名为 EventManager.cs

　　如果改变了文件的名字内容没有发生变化，请在脚本上右键点击执行“Reimport”。

○ 调试用修改游戏内参数功能

　游戏中按下 "W" 键，将显示游戏内参数一览。通过在文本输入框中填入值，
　可以替换游戏中的参数值。