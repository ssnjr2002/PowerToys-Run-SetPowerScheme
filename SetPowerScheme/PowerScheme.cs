using System.Text.RegularExpressions;

namespace Community.PowerToys.Run.Plugin.SetPowerScheme
{
    internal class PowerScheme
    {
        public string? Name { get; set; }
        public string? Guid { get; set; }
        public bool IsActive { get; set; }

        public PowerScheme(string line) 
        {
            var match = Regex.Match(line, 
                @"Power Scheme GUID: (?<guid>[^ ]+)  \((?<name>[^)]+)\)( \*)?");

            if (match.Success)
            {
                Guid = match.Groups["guid"].Value;
                Name = match.Groups["name"].Value;
                IsActive = line.Contains('*');
            }
        }
    }
}
