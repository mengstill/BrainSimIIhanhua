2022年9月5日
目前BrainSimulator.Modules这个命名空间名称无法更改,因为在 Type t = Type.GetType("BrainSimulator.Modules.Module" + moduleLabel);中命名空间,类名都被限制死了
甚至需要序列化与反序列化的对象也不能修改,因为二者是名称对应关系
使用Settings.settings可以保存配置参数
读取网络的位置还没有确定
软件启动逻辑,首先运行 MainWindow()方法,在这个方法中启动了一个新的线程来运行引擎循环,并且完成窗口初始化时的一系列操作:
组合快捷键的配置,弹出窗口的配置,读取参数来弹出网页或者更新
当窗口打开后则会触发相应的事件,执行此方法:Window_ContentRendered(object sender, EventArgs e)加载上次运行的网络或者新建一个网络
当网络不为空的时候引擎循环可以正常运行


