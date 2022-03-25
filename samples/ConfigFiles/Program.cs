using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tmds.Fuse;

namespace ConfigFiles
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!Fuse.CheckDependencies())
            {
                Console.WriteLine(Fuse.InstallationInstructions);
                return;
            }

            var fileSystem = new ConfigFileSystem();

            string mountPoint = $"/tmp/config";
            System.Console.WriteLine($"Mounting filesystem at {mountPoint}");

            Fuse.LazyUnmount(mountPoint);

            // Ensure mount point directory exists
            Directory.CreateDirectory(mountPoint);

            // Add virtual files and their source files
            fileSystem.AddFile("pn.json", "/home/mivansch/repos/tmds/Tmds.Fuse/pn.json");
            fileSystem.AddFile("hello.txt", "/home/mivansch/repos/tmds/Tmds.Fuse/sample.txt");

            try
            {
                using (var mount = Fuse.Mount(mountPoint, fileSystem))
                {
                    await mount.WaitForUnmountAsync();
                }
            }
            catch (FuseException fe)
            {
                Console.WriteLine($"Fuse throw an exception: {fe}");

                Console.WriteLine("Try unmounting the file system by executing:");
                Console.WriteLine($"fuser -kM {mountPoint}");
                Console.WriteLine($"sudo umount -f {mountPoint}");
            }
        }
    }
}
