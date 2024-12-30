using ManagedCommon;
using System.Diagnostics;
using System.Windows.Input;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.SetPowerScheme
{
    public class Main : IPlugin, IContextMenu, IReloadable, IDisposable
    {
        private PluginInitContext _context;
        private string _iconPath;
        private bool _disposed;

        public string Name => Properties.Resources.plugin_name;
        public string Description => Properties.Resources.plugin_description;
        public static string PluginID => "88926194537145688fa20dcb65de1890";

        private PowerCfgHandler _pcfg;
        private List<PowerScheme> _schemes;

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
            _pcfg = new PowerCfgHandler();
            _schemes = new List<PowerScheme>();
        }

        public List<Result> Query(Query query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var results = new List<Result>();

            if (!_pcfg.availableOnPath)
            {
                results.Add(new Result
                {
                    Title = Properties.Resources.notif_error_title,
                    SubTitle = "powercfg.exe not found on PATH",
                    IcoPath = _iconPath,
                });
                return results;
            }

            // Fetch schemes during keyword only
            if (query.Search.Length == 0)
            {
                _schemes = _pcfg.GetSchemes();
            }

            if (_schemes.Count == 0)
            {
                results.Add(new Result
                {
                    Title = Properties.Resources.notif_error_title,
                    SubTitle = "powercfg.exe /list failed to find any power schemes!",
                    IcoPath = _iconPath,
                });
                return results;
            }

            foreach (var scheme in _schemes)
            {
                // Filter out schemes only when a scheme name is typed after keyword
                if (query.Search.Length > 0
                    && (scheme.Name == null
                    || !scheme.Name.StartsWith(query.Search, StringComparison.OrdinalIgnoreCase)))
                { continue; }

                bool isActive = scheme.Guid == _pcfg.activeGuid;

                results.Add(new Result
                {
                    Title = scheme.Name,
                    SubTitle = isActive ? "Currently active" : "Set as active scheme",
                    IcoPath = _iconPath,
                    Action = action =>
                    {
                        if (isActive) { return false; }
                        else if (_pcfg.setActive(scheme))
                        {
                            _context.API.ShowNotification(Properties.Resources.notif_success_title,
                                $"Set {scheme.Name} as the currently active scheme!");
                            return true;
                        }
                        _context.API.ShowNotification(Properties.Resources.notif_error_title,
                            $"Failed to set {scheme.Name} as the currently active scheme!");
                        return false;
                    }
                });
            }

            return results;
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return new List<ContextMenuResult>
            {
                new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Open power options in Control Panel",
                    FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                    Glyph = "\xE713", // Settings
                    // shortcut
                    AcceleratorKey = Key.C,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ =>
                    {
                        OpenPowerOptionsGui();
                        return true;
                    },
                }
            };
        }

        private static void OpenPowerOptionsGui()
        {
            Process openPowerSetting = new Process();
            ProcessStartInfo pwrSetStartInfo = new ProcessStartInfo();
            pwrSetStartInfo.FileName = "control.exe";
            pwrSetStartInfo.Arguments = "/name Microsoft.PowerOptions";
            openPowerSetting.StartInfo = pwrSetStartInfo;
            openPowerSetting.Start();
        }

        private void OnThemeChanged(Theme oldTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _iconPath = _context.CurrentPluginMetadata.IcoPathLight;
            }
            else
            {
                _iconPath = _context.CurrentPluginMetadata.IcoPathDark;
            }
        }

        public void ReloadData()
        {
            if (_context is null)
            {
                return;
            }

            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_context != null && _context.API != null)
                {
                    _context.API.ThemeChanged -= OnThemeChanged;
                }

                _disposed = true;
            }
        }
    }
}
