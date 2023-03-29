
using System.IO.Compression;
using Serilog;
using ShellProgressBar;

namespace MangaArchiver
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.File("logs/Manga_.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true).CreateLogger();

#else
            Log.Logger = new LoggerConfiguration().MinimumLevel.Warning().WriteTo.Console().WriteTo.File("logs/Manga_.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true).CreateLogger();

#endif
            string path;
            if (args.Length >= 1)
            {
                Log.Information("参数存在，使用参数路径");
                Log.Debug($"args[0]:{args[0]}");
                if (!Directory.Exists(args[0]))
                {
                    Log.Fatal($"{args[0]} 不存在");
                    return;
                }
                path = args[0];
            }
            else
            {
                Log.Information("不存在参数，使用当前路径");
                path = Directory.GetCurrentDirectory();
            }
            var dirs = Directory.GetDirectories(path);
            Log.Information($"当前操作目录:{path}");
            foreach (var dir in dirs)
            {
                var pngFiles = Directory.GetFiles(dir);
                var subFiles = Directory.GetDirectories(dir);
                if (pngFiles.All(f => (Path.GetExtension(f).ToLower() == ".png" | Path.GetExtension(f).ToLower() == ".jpeg" | Path.GetExtension(f).ToLower() == ".jpg")) && subFiles.Length == 0)
                {
                    Log.Information($"一共{pngFiles.Length}文件");
                    var progressBarOption = new ProgressBarOptions()
                    {
                        // DisplayTimeInRealTime = true,
                        ForegroundColor = ConsoleColor.Cyan,
                        ForegroundColorDone = ConsoleColor.Green,
                        BackgroundColor = ConsoleColor.DarkBlue,
                        ProgressBarOnBottom = true,
                        BackgroundCharacter = '\u2593',
                        DisableBottomPercentage = true,

                    };

                    using (var progressBar = new ProgressBar(pngFiles.Length, $"[正在处理「{dir.Split(Path.DirectorySeparatorChar).Last().ToLower()}」]", progressBarOption))
                    {
                        char[] processRing = { '\u25CE', '\u25CF' };
                        var zipFileName = dir.Split(Path.DirectorySeparatorChar).Last().ToLower() + ".cbz";
                        if (File.Exists(zipFileName))
                        {
                            Log.Debug($"{zipFileName}已存在，删除...");

                            File.Delete(zipFileName);
                        }
                        using (var archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                        {
                            var totle = (float)pngFiles.Length;
                            foreach (var pngFile in pngFiles.Select((ele, index) => new { Index = index, Ele = ele }))
                            {
                                var pngFileName = Path.GetFileName(pngFile.Ele);
                                archive.CreateEntryFromFile(pngFile.Ele, pngFileName);
                                var index = (float)pngFile.Index + 1;
                                progressBar.Tick($"{processRing[pngFile.Index % 2]}[正在处理「{dir.Split(Path.DirectorySeparatorChar).Last().ToLower()}」][{pngFile.Index + 1}/{pngFiles.Length}][{(index / totle * 100).ToString("0.00")}%]");
                            }
                        }
                    }
                }
                else
                {
                    Log.Warning($"{path}不为漫画目录");

                }
            }
        }
    }
}
