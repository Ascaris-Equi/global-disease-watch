using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

// Screen saver entry point.  /s fullscreen, /p <hwnd> preview, /c settings,
// --window (resizable test window), --render <png> [seconds] [w] [h], --apply (per-user registry), --probe <seconds>.
static class Program {
 [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
 [DllImport("user32.dll",SetLastError=true)] static extern bool SystemParametersInfo(uint action,uint param,IntPtr value,uint flags);
 [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr hwnd,out RECT rect);
 [DllImport("user32.dll")] static extern bool IsWindow(IntPtr hwnd);
 [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hwnd,IntPtr after,int x,int y,int w,int h,uint flags);
 [DllImport("user32.dll")] static extern bool GetLastInputInfo(ref LASTINPUTINFO info);
 [DllImport("kernel32.dll")] static extern uint SetThreadExecutionState(uint flags);
 [StructLayout(LayoutKind.Sequential)] struct RECT {public int Left,Top,Right,Bottom;}
 [StructLayout(LayoutKind.Sequential)] struct LASTINPUTINFO {public uint Size,Time;}

 static System.Threading.Timer testTimer; // kept referenced so the GC cannot collect it
 static uint LastInput(){var li=new LASTINPUTINFO{Size=(uint)Marshal.SizeOf(typeof(LASTINPUTINFO))};GetLastInputInfo(ref li);return li.Time;}

 [STAThread] static int Main(string[] args){
  SetProcessDPIAware();
  string root=Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
  int ri=Array.IndexOf(args,"--root");if(ri>=0&&ri+1<args.Length)root=args[ri+1];
  string mode=args.Length>0?args[0].ToLowerInvariant():"/s";
  // Software rendering (no GPU device at all) is chosen by the supervisor when the display GPU is short of
  // memory or failed recently; it must be set before any WPF object exists.
  bool software=Array.IndexOf(args,"--software")>=0;
  if(software)RenderOptions.ProcessRenderMode=System.Windows.Interop.RenderMode.SoftwareOnly;
  int si=Array.IndexOf(args,"--skip");if(si>=0&&si+1<args.Length)WallView.Skip=args[si+1].ToLowerInvariant();
  int scn=Array.IndexOf(args,"--scene");WallView.Scene=scn>=0&&scn+1<args.Length?args[scn+1].ToLowerInvariant():"full";
  // Software rendering draws every 3D pass on the CPU, so it gets the lighter scenes unless told otherwise:
  // measured on the RTX 5090 host at 1920x1080 this takes the renderer from ~92% to ~28% of one core.
  int li=Array.IndexOf(args,"--lite");WallView.LiteOpts=li>=0&&li+1<args.Length?args[li+1].ToLowerInvariant():software?"clip,nospec,mesh,slice":"";
  // Failures end the process quietly with a code the supervisor understands: 20 render/GPU failure,
  // 21 out of memory, 22 anything else. Details go to data\display-error.log; no Windows error dialog.
  Action<Exception> log=ex=>{foreach(string dir in new[]{Path.Combine(root,"data"),Path.GetTempPath()}){try{File.AppendAllText(Path.Combine(dir,"display-error.log"),DateTime.Now.ToString("s")+" "+ex+"\r\n");return;}catch{}}};
  Func<Exception,int> codeFor=ex=>{
   for(var e=ex;e!=null;e=e.InnerException){
    if(e is OutOfMemoryException)return 21;
    var com=e as COMException;if(com!=null){uint hr=(uint)com.ErrorCode;if((hr&0xFFFF0000u)==0x88980000u||(hr&0xFFFF0000u)==0x88760000u)return 20;}
   }
   return 22;
  };
  AppDomain.CurrentDomain.UnhandledException+=(s,e)=>{var ex=e.ExceptionObject as Exception;log(ex);SetThreadExecutionState(0x80000000);Environment.Exit(codeFor(ex));};
  // Test hooks for the supervisor: exit with a given code after 4 s, or allocate until the memory cap stops us.
  int testExit=Array.IndexOf(args,"--test-exit");if(testExit>=0&&testExit+1<args.Length){int code=Int32.Parse(args[testExit+1]);testTimer=new System.Threading.Timer(_=>Environment.Exit(code),null,4000,-1);}
  if(Array.IndexOf(args,"--test-alloc")>=0)new Thread(()=>{var keep=new List<byte[]>();try{while(true){var b=new byte[64<<20];for(int i=0;i<b.Length;i+=4096)b[i]=1;keep.Add(b);Thread.Sleep(40);}}catch(OutOfMemoryException ex){log(ex);Environment.Exit(21);}}){IsBackground=true}.Start();
  if(mode=="--apply"){
   using(var k=Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop")){k.SetValue("SCRNSAVE.EXE",Path.Combine(root,"MJH-IdleBoard.scr"));k.SetValue("ScreenSaveActive","1");k.SetValue("ScreenSaveTimeOut","300");}
   SystemParametersInfo(15,300,IntPtr.Zero,1);SystemParametersInfo(17,1,IntPtr.Zero,1);return 0;
  }
  if(mode.StartsWith("/c")){MessageBox.Show("Research Wall\n\n空闲 5 分钟后启动，移动鼠标或按键退出。\n硬件、项目、疫情通报与结构数据均由后台任务自动更新。","Research Wall",MessageBoxButton.OK,MessageBoxImage.Information);return 0;}

  var store=new Store(root);store.LoadAll();
  if(mode=="--render"){
   double t=args.Length>2?Double.Parse(args[2],CultureInfo.InvariantCulture):14;
   int w=args.Length>3?Int32.Parse(args[3]):1920,h=args.Length>4?Int32.Parse(args[4]):1080;
   var view=new WallView(store){Width=w,Height=h};
   view.Measure(new Size(w,h));view.Arrange(new Rect(0,0,w,h));
   view.Snap();for(double u=t-3.0;u<t;u+=0.1){view.Tick(u);}for(int i=0;i<3;i++){view.Tick(t+i*.001);}view.Tick(t);view.UpdateLayout();
   var bmp=new RenderTargetBitmap(w,h,96,96,PixelFormats.Pbgra32);bmp.Render(view);
   var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(bmp));using(var f=File.Create(args[1]))enc.Save(f);
   return 0;
  }

  var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};
  app.DispatcherUnhandledException+=(s,e)=>{log(e.Exception);e.Handled=true;SetThreadExecutionState(0x80000000);Environment.Exit(codeFor(e.Exception));};
  var views=new List<WallView>();
  var clock=Stopwatch.StartNew();
  bool polling=false;
  var poll=new DispatcherTimer{Interval=TimeSpan.FromSeconds(3)};
  poll.Tick+=(s,e)=>{
   if(polling)return;polling=true;
   Task.Run(()=>{try{store.Poll();}catch{}}).ContinueWith(_=>app.Dispatcher.BeginInvoke(new Action(()=>{foreach(var v in views)v.Apply();polling=false;})));
  };
  // 24 fps is enough for the slow rotations; 10 fps when rendering in software (3D groups then redraw in turn).
  double fps=software?10:24;int fi=Array.IndexOf(args,"--fps");if(fi>=0&&fi+1<args.Length)fps=Math.Max(1,Math.Min(60,Double.Parse(args[fi+1],CultureInfo.InvariantCulture)));
  double lastFrame=-1,frameGap=1.0/fps-.004;
  EventHandler frame=(s,e)=>{double t=clock.Elapsed.TotalSeconds;if(t-lastFrame<frameGap)return;lastFrame=t;foreach(var v in views)v.Tick(t);};

  if(mode.StartsWith("/p")){
   string id=mode.Contains(":")?mode.Split(':')[1]:(args.Length>1?args[1]:"0");
   IntPtr parent=new IntPtr(Int64.Parse(id));RECT r;GetClientRect(parent,out r);
   var p=new HwndSourceParameters("ResearchWallPreview"){ParentWindow=parent,WindowStyle=0x50000000,PositionX=0,PositionY=0,Width=r.Right,Height=r.Bottom};
   var src=new HwndSource(p);var view=new WallView(store){Width=r.Right,Height=r.Bottom};src.RootVisual=view;views.Add(view);
   var watch=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};watch.Tick+=(s,e)=>{if(!IsWindow(parent))app.Shutdown();};watch.Start();
   CompositionTarget.Rendering+=frame;poll.Start();app.Run();return 0;
  }

  // Only one fullscreen instance; the test window may run alongside a live screen saver.
  bool windowed=mode=="--window",created;
  using(var mutex=new Mutex(!windowed,windowed?"Local\\MJHIdleBoardWindow":"Local\\MJHIdleBoardScreen",out created)){
   if(!created&&!windowed)return 0;
   int timeout=mode=="--probe"&&args.Length>1?Int32.Parse(args[1]):0;
   uint startInput=LastInput();var startCursor=System.Windows.Forms.Cursor.Position;
   Action quit=()=>{SetThreadExecutionState(0x80000000);app.Shutdown();};
   if(windowed){
    var view=new WallView(store);views.Add(view);
    var win=new Window{Title="Research Wall",Width=1280,Height=740,Content=view,Background=view.Background,WindowStartupLocation=WindowStartupLocation.CenterScreen};
    win.Closed+=(s,e)=>quit();win.Show();view.FadeIn();
   }else{
    foreach(var screen in System.Windows.Forms.Screen.AllScreens){
     var b=screen.Bounds;var view=new WallView(store);views.Add(view);
     var win=new Window{WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,ShowInTaskbar=false,Topmost=true,Content=view,Background=view.Background,Cursor=Cursors.None,WindowStartupLocation=WindowStartupLocation.Manual,Left=b.X,Top=b.Y,Width=b.Width,Height=b.Height,ShowActivated=true};
     win.SourceInitialized+=(s,e)=>SetWindowPos(new WindowInteropHelper(win).Handle,new IntPtr(-1),b.X,b.Y,b.Width,b.Height,0x0040);
     win.KeyDown+=(s,e)=>quit();win.MouseDown+=(s,e)=>quit();
     win.Show();view.FadeIn();
    }
    if(views.Count>0)Window.GetWindow(views[0]).Activate();
    Mouse.OverrideCursor=Cursors.None;
    SetThreadExecutionState(0x80000002);
    var guard=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(120)};
    guard.Tick+=(s,e)=>{
     double t=clock.Elapsed.TotalSeconds;
     if(timeout>0&&t>timeout){quit();return;}
     if(t<1.5){startInput=LastInput();startCursor=System.Windows.Forms.Cursor.Position;return;}
     var c=System.Windows.Forms.Cursor.Position;
     if(LastInput()!=startInput||Math.Abs(c.X-startCursor.X)>6||Math.Abs(c.Y-startCursor.Y)>6)quit();
    };
    guard.Start();
   }
   CompositionTarget.Rendering+=frame;poll.Start();
   app.Run();
  }
  return 0;
 }
}
