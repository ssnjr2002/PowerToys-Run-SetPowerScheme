using System.Diagnostics;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.SetPowerScheme
{
    internal class PowerCfgHandler
    {
        string lastStdOutput;
        ProcessStartInfo processStartInfo;
        public bool availableOnPath = false;
        public string activeGuid = "";
        public PowerCfgHandler()
        {
            lastStdOutput = "";
            processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "powercfg.exe";
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            CheckPathOnInit();
        }

        private void CheckPathOnInit()
        {
            try
            {
                RunArg("/?");
                availableOnPath = true;
            } 
            catch (Exception ex)
            {
                availableOnPath = false;
                Log.Error(ex.Message, GetType());
            }
        }

        public bool RunArg(string arg)
        {
            Process process = new Process();
            processStartInfo.Arguments = arg;
            process.StartInfo = processStartInfo;
            process.Start();

            lastStdOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Log.Debug("powercfg stdout " + arg + " : " + lastStdOutput, GetType());
            return (process.ExitCode == 0);
        }

        public List<PowerScheme> GetSchemes()
        {
            List<PowerScheme> schemes = new List<PowerScheme>();

            if (RunArg("/list"))
            {
                foreach (var line in lastStdOutput.Split("\n"))
                {
                    if (line.StartsWith("Power Scheme GUID"))
                    {
                        PowerScheme scheme = new PowerScheme(line);
                        schemes.Add(scheme);
                        if (scheme.IsActive && scheme.Guid != null) activeGuid = scheme.Guid;
                    }
                }
            }

            return schemes;
        }

        public bool setActive(PowerScheme scheme)
        {
            if (scheme.Guid != null)
            {
                activeGuid = scheme.Guid;
                return RunArg("/setactive " + scheme.Guid);
            }

            Log.Error("Failed to /setactive, provided Guid is null!", GetType());
            return false;
        }
    }
}
